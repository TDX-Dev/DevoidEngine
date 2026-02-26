using DevoidGPU;
using System.Diagnostics;
using System.IO;

namespace DevoidEngine.Engine.Core
{
    public class WindowRunState
    {
        public Window window;
        public double fixedAccumulator;
    }

    public class WindowManager
    {
        public int MainThreadID = -1;
        public int UpdateThreadID = -1;

        private readonly List<WindowRunState> windows = new();
        private volatile bool _running = true;

        public static WindowManager Instance { get; } = new WindowManager();
        public bool useVsyncLimiter = false;

        public WindowManager()
        {
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public void RegisterWindow(Window window)
        {
            windows.Add(new WindowRunState { window = window });
        }

        public void RunAllSync()
        {
            const double updateHz = 165;
            const double renderHz = 165;

            double updateStep = 1.0 / updateHz;
            double renderStep = 1.0 / renderHz;

            foreach (var wrs in windows)
                wrs.window.Load();

            Stopwatch timer = Stopwatch.StartNew();

            double nextUpdate = timer.Elapsed.TotalSeconds;
            double nextRender = timer.Elapsed.TotalSeconds;

            while (_running && windows.Count > 0)
            {
                double now = timer.Elapsed.TotalSeconds;

                // -------- UPDATE --------
                if (now >= nextUpdate)
                {
                    for (int i = 0; i < windows.Count; i++)
                    {
                        var wrs = windows[i];

                        wrs.window.Update(updateStep);

                        wrs.fixedAccumulator += updateStep;
                        while (wrs.fixedAccumulator >= updateStep)
                        {
                            wrs.window.FixedUpdate(updateStep);
                            wrs.fixedAccumulator -= updateStep;
                        }
                    }

                    nextUpdate += updateStep;
                }

                // -------- RENDER --------
                if (now >= nextRender)
                {
                    for (int i = windows.Count - 1; i >= 0; i--)
                    {
                        var wrs = windows[i];

                        wrs.window.ProcessEvents();

                        if (wrs.window.IsExiting)
                        {
                            wrs.window.Close();
                            windows.RemoveAt(i);
                            continue;
                        }

                        wrs.window.Render(renderStep);
                    }

                    nextRender += renderStep;
                }

                Thread.Sleep(0); // yield
                _running = windows.Count > 0;
            }
        }


        public void RunAll()
        {
            const double updateHz = 200;
            const double renderHz = 60.0;

            double updateStep = 1.0 / updateHz;
            double renderStep = 1.0 / renderHz;

            foreach (var wrs in windows)
                wrs.window.Load();

            // Shared global timer
            Stopwatch globalTimer = Stopwatch.StartNew();

            double nextRender = globalTimer.Elapsed.TotalSeconds;
            double lastRender = globalTimer.Elapsed.TotalSeconds;

            double nextUpdate = globalTimer.Elapsed.TotalSeconds;
            double lastUpdate = globalTimer.Elapsed.TotalSeconds;

            // ---------------- UPDATE THREAD ----------------
            Thread updateThread = new Thread(() =>
            {
                double accumulator = 0.0;
                double lastTime = globalTimer.Elapsed.TotalSeconds;

                while (_running)
                {
                    double now = globalTimer.Elapsed.TotalSeconds;
                    double frameTime = now - lastTime;
                    lastTime = now;

                    // Prevent spiral of death
                    if (frameTime > 0.25)
                        frameTime = 0.25;

                    accumulator += frameTime;

                    while (accumulator >= updateStep)
                    {
                        for (int i = windows.Count - 1; i >= 0; i--)
                        {
                            windows[i].window.Update(updateStep);
                        }

                        accumulator -= updateStep;
                    }

                    // Yield CPU a little
                    Thread.Sleep(0);
                }
            })
            {
                IsBackground = true,
                Name = "Update Thread"
            };

            updateThread.Start();


            UpdateThreadID = updateThread.ManagedThreadId;


            // ---------------- RENDER / EVENT THREAD ----------------
            double lastRenderTime = globalTimer.Elapsed.TotalSeconds;

            int frameCounter = 0;
            double fpsTimer = 0;
            while (_running)
            {
                if (windows.Count == 0)
                    break;

                double frameStart = globalTimer.Elapsed.TotalSeconds;

                double dt = frameStart - lastRenderTime;
                lastRenderTime = frameStart;

                if (dt > 0.25)
                    dt = 0.25;

                frameCounter++;
                fpsTimer += dt;

                if (fpsTimer >= 1.0)
                {
                    // Console.WriteLine($"FPS: {frameCounter}");
                    frameCounter = 0;
                    fpsTimer -= 1.0;
                }

                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    var wrs = windows[i];
                    var window = wrs.window;

                    window.ProcessEvents();

                    if (window.IsExiting)
                    {
                        window.Close();
                        windows.RemoveAt(i);
                        continue;
                    }

                    window.Render(dt); // Present(1) or Present(0)
                }

                _running = windows.Count > 0;

                // --------- MANUAL FRAME CAP (ONLY IF VSYNC OFF) ---------
                if (!useVsyncLimiter)
                {
                    double frameEnd = globalTimer.Elapsed.TotalSeconds;
                    double elapsed = frameEnd - frameStart;

                    double remaining = renderStep - elapsed;

                    if (remaining > 0)
                    {
                        // Sleep most of it
                        if (remaining > 0.002) // 2ms threshold
                        {
                            Thread.Sleep((int)((remaining - 0.001) * 1000));
                        }

                        // Spin-wait the last ~1ms for precision
                        while (globalTimer.Elapsed.TotalSeconds - frameStart < renderStep)
                        {
                            Thread.SpinWait(10);
                        }
                    }
                }
            }
            _running = false; 
            updateThread.Join();
        }

        ManualResetEventSlim updateStart = new(false);
        ManualResetEventSlim updateDone = new(false);
        double _currentUpdateStep;

        public void RunTicked()
        {
            const int updatesPerFrame = 4;

            foreach (var wrs in windows)
                wrs.window.Load();

            _running = true;

            // ---------------- UPDATE THREAD ----------------
            Thread updateThread = new Thread(() =>
            {
                while (_running)
                {
                    updateStart.Wait();
                    updateStart.Reset();

                    if (!_running)
                        break;

                    double step = Interlocked.CompareExchange(ref _currentUpdateStep, 0, 0);

                    for (int u = 0; u < updatesPerFrame; u++)
                    {
                        for (int i = windows.Count - 1; i >= 0; i--)
                            windows[i].window.Update(step);
                    }

                    updateDone.Set();
                }
            })
            {
                IsBackground = true,
                Name = "Update Thread"
            };

            updateThread.Start();

            // ---------------- RENDER THREAD ----------------
            Stopwatch timer = Stopwatch.StartNew();
            double lastRenderTime = timer.Elapsed.TotalSeconds;

            while (_running)
            {
                if (windows.Count == 0)
                    break;

                double now = timer.Elapsed.TotalSeconds;
                double dt = now - lastRenderTime;
                lastRenderTime = now;

                if (dt > 0.25)
                    dt = 0.25;

                // Split real frame time into 6 equal update steps
                Interlocked.Exchange(ref _currentUpdateStep, dt / updatesPerFrame);

                // ---- Sync: force 6 updates ----
                updateDone.Reset();
                updateStart.Set();
                updateDone.Wait();
                // --------------------------------

                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    var window = windows[i].window;

                    window.ProcessEvents();

                    if (window.IsExiting)
                    {
                        window.Close();
                        windows.RemoveAt(i);
                        continue;
                    }

                    window.Render(dt);
                }

                _running = windows.Count > 0;
            }

            _running = false;
            updateStart.Set();   // release update thread if waiting
            updateThread.Join();
        }

    }
}
