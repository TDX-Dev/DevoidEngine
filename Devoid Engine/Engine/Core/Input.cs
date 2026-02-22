using System.Numerics;
using System.Threading;

namespace DevoidEngine.Engine.Core
{
    public static class Input
    {
        private static InputSnapshot _snapshotFront = new();
        private static InputSnapshot _snapshotBack = new();

        // ===============================
        // RAW THREAD-SAFE ACCUMULATORS
        // (Written by Render Thread)
        // ===============================

        private static float _rawMouseDeltaX;
        private static float _rawMouseDeltaY;

        private static float _rawScrollX;
        private static float _rawScrollY;

        private static readonly object _lock = new();

        private static readonly HashSet<Keys> _rawKeysDown = new();
        private static readonly HashSet<MouseButton> _rawMouseDown = new();

        // ===============================
        // GAME-FACING STATE
        // (Read by Update Thread)
        // ===============================

        public static Vector2 MousePosition { get; private set; }
        public static Vector2 MouseDelta { get; private set; }
        public static Vector2 MouseScrollDelta { get; private set; }

        private static readonly HashSet<Keys> keysDown = new();
        private static readonly HashSet<Keys> keysPressedThisFrame = new();
        private static readonly HashSet<Keys> keysReleasedThisFrame = new();

        private static readonly HashSet<MouseButton> mouseDown = new();
        private static readonly HashSet<MouseButton> mousePressedThisFrame = new();
        private static readonly HashSet<MouseButton> mouseReleasedThisFrame = new();


        static Input()
        {
            _snapshotFront.Keys = new HashSet<Keys>();
            _snapshotFront.Mouse = new HashSet<MouseButton>();

            _snapshotBack.Keys = new HashSet<Keys>();
            _snapshotBack.Mouse = new HashSet<MouseButton>();
        }

        // ===============================
        // RENDER THREAD CALLS THESE
        // ===============================

        private static void AtomicAdd(ref float target, float value)
        {
            float initial, computed;
            do
            {
                initial = target;
                computed = initial + value;
            }
            while (Interlocked.CompareExchange(ref target, computed, initial) != initial);
        }


        public static void OnMouseMove(MouseMoveEvent e)
        {
            MousePosition = e.position;

            AtomicAdd(ref _rawMouseDeltaX, e.delta.X);
            AtomicAdd(ref _rawMouseDeltaY, e.delta.Y);

        }

        public static void OnMouseWheel(MouseWheelEvent e)
        {

            AtomicAdd(ref _rawScrollX, e.Offset.X);
            AtomicAdd(ref _rawScrollY, e.Offset.Y);

        }

        public static void OnMouseButton(MouseButtonEvent e)
        {
            lock (_lock)
            {
                if (e.IsPressed)
                {
                    _rawMouseDown.Add(e.Button);
                }
                else
                {
                    _rawMouseDown.Remove(e.Button);
                }
            }
        }

        public static void OnKeyDown(Keys key)
        {
            lock (_lock)
            {
                _rawKeysDown.Add(key);
            }
        }

        public static void OnKeyUp(Keys key)
        {
            lock (_lock)
            {
                _rawKeysDown.Remove(key);
            }
        }

        // ===============================
        // UPDATE THREAD CALLS THIS
        // ===============================

        /// <summary>
        /// Call ONCE at start of update frame.
        /// </summary>
        public static void Update()
        {
            var snap = _snapshotFront;

            MouseDelta = snap.MouseDelta;
            MouseScrollDelta = snap.MouseScroll;

            keysDown.Clear();
            foreach (var k in snap.Keys)
                keysDown.Add(k);

            // BEFORE clearing pressed/released
            keysPressedThisFrame.Clear();
            keysReleasedThisFrame.Clear();

            // Detect transitions
            foreach (var key in keysDown)
            {
                if (!_snapshotBack.Keys.Contains(key))
                    keysPressedThisFrame.Add(key);
            }

            foreach (var key in _snapshotBack.Keys)
            {
                if (!keysDown.Contains(key))
                    keysReleasedThisFrame.Add(key);
            }

            mousePressedThisFrame.Clear();
            mouseReleasedThisFrame.Clear();

            mouseDown.Clear();
            foreach (var m in snap.Mouse)
                mouseDown.Add(m);

            // Detect mouse transitions
            foreach (var m in mouseDown)
            {
                if (!_snapshotBack.Mouse.Contains(m))
                    mousePressedThisFrame.Add(m);
            }

            foreach (var m in _snapshotBack.Mouse)
            {
                if (!mouseDown.Contains(m))
                    mouseReleasedThisFrame.Add(m);
            }

            _snapshotBack = snap;
        }

        public static void Publish()
        {
            var newSnapshot = new InputSnapshot
            {
                MouseDelta = new Vector2(
                    Interlocked.Exchange(ref _rawMouseDeltaX, 0f),
                    Interlocked.Exchange(ref _rawMouseDeltaY, 0f)),

                MouseScroll = new Vector2(
                    Interlocked.Exchange(ref _rawScrollX, 0f),
                    Interlocked.Exchange(ref _rawScrollY, 0f)),

                Keys = new HashSet<Keys>(),
                Mouse = new HashSet<MouseButton>()
            };

            lock (_lock)
            {
                foreach (var k in _rawKeysDown)
                    newSnapshot.Keys.Add(k);

                foreach (var m in _rawMouseDown)
                    newSnapshot.Mouse.Add(m);
            }

            Interlocked.Exchange(ref _snapshotFront, newSnapshot);
        }

        // ===============================
        // GAME QUERIES
        // ===============================

        public static bool GetKey(Keys key) => keysDown.Contains(key);
        public static bool GetKeyDown(Keys key) => keysPressedThisFrame.Contains(key);
        public static bool GetKeyUp(Keys key) => keysReleasedThisFrame.Contains(key);

        public static bool GetMouse(MouseButton btn) => mouseDown.Contains(btn);
        public static bool GetMouseDown(MouseButton btn) => mousePressedThisFrame.Contains(btn);
        public static bool GetMouseUp(MouseButton btn) => mouseReleasedThisFrame.Contains(btn);

        public static Vector2 MoveAxis
        {
            get
            {
                Vector2 axis = Vector2.Zero;

                if (GetKey(Keys.W)) axis.Y += 1;
                if (GetKey(Keys.S)) axis.Y -= 1;
                if (GetKey(Keys.D)) axis.X += 1;
                if (GetKey(Keys.A)) axis.X -= 1;

                if (axis.LengthSquared() > 1f)
                    axis = Vector2.Normalize(axis);

                return axis;
            }
        }

        public static bool JumpPressed => GetKeyDown(Keys.Space);
    }
}
