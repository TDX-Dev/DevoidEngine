using DevoidEngine.Attributes;
using DevoidEngine.Serialization;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Core
{
    [DevoidDataClass]
    public class Transform3D
    {
        public event Action? TransformChanged;

        public Vector3 LocalPosition
        {
            get => _localPosition;
            set
            {
                if (_localPosition != value)
                {
                    _localPosition = value;
                    MarkDirty();
                    //Console.WriteLine("Marked Dirty");
                }
            }
        }

        public Vector3 EulerAngles
        {
            get
            {
                if (_eulerDirty)
                {
                    _eulerValue = MathHelper.QuaternionToEuler(_localRotation);
                    _eulerDirty = false;
                }

                return _eulerValue;
            }
            set
            {
                _eulerValue = value;
                _localRotation = MathHelper.EulerToQuaternion(value);
                MarkDirty();
            }
        }

        public Quaternion LocalRotation
        {
            get => _localRotation;
            set
            {
                if (_localRotation != value)
                {
                    _localRotation = value;
                    _eulerDirty = true;
                    MarkDirty();
                }
            }
        }

        public Vector3 LocalScale
        {
            get => _localScale;
            set
            {
                if (_localScale != value)
                {
                    _localScale = value;
                    MarkDirty();
                }
            }
        }
        public Vector3 Position
        {
            get => WorldMatrix.Translation;
            set
            {
                if (_parent != null)
                {
                    Matrix4x4.Invert(_parent.WorldMatrix, out var invParent);
                    Vector3 local = Vector3.Transform(value, invParent);
                    _localPosition = local;
                }
                else
                {
                    _localPosition = value;
                }

                MarkDirty();
            }
        }
        public Quaternion Rotation
        {
            get
            {
                if (_parent == null)
                    return _localRotation;

                return _parent.Rotation * _localRotation;
            }
            set
            {
                if (_parent != null)
                {
                    Quaternion invParent = Quaternion.Inverse(_parent.Rotation);
                    _localRotation = invParent * value;
                }
                else
                {
                    _localRotation = value;
                }

                MarkDirty();
            }
        }

        public Vector3 Scale
        {
            get
            {
                if (_parent == null)
                    return _localScale;

                return _parent.Scale * _localScale;
            }
            set
            {
                if (_parent != null)
                    _localScale = value / _parent.Scale;
                else
                    _localScale = value;

                MarkDirty();
            }
        }
        public Matrix4x4 LocalMatrix => Matrix4x4.CreateScale(_localScale) * Matrix4x4.CreateFromQuaternion(_localRotation) * Matrix4x4.CreateTranslation(_localPosition);

        public Matrix4x4 WorldMatrix
        {
            get
            {
                if (_dirty)
                    RecalculateWorldMatrix();

                return _worldMatrix;
            }
        }

        [DontSerialize]
        public Vector3 Forward => Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, Rotation));
        [DontSerialize]
        public Vector3 Up => Vector3.Normalize(Vector3.Transform(Vector3.UnitY, Rotation));
        [DontSerialize]
        public Vector3 Right => Vector3.Normalize(Vector3.Transform(Vector3.UnitX, Rotation));
        public Transform3D? Parent => _parent;
        public List<Transform3D> Children => _children;

        private Transform3D? _parent;
        private readonly List<Transform3D> _children;

        private Vector3 _localPosition = Vector3.Zero;
        private Quaternion _localRotation = Quaternion.Identity;
        private Vector3 _localScale = Vector3.One;

        private Vector3 _prevLocalPosition;
        private Quaternion _prevLocalRotation;
        private Vector3 _prevLocalScale;

        private Matrix4x4 _worldMatrix = Matrix4x4.Identity;

        private Vector3 _eulerValue;
        private bool _eulerDirty = true;
        private bool _dirty = true;

        public bool HasMoved = false;

        private Matrix4x4 _interpolatedWorldMatrix;
        private uint _interpolatedFrame = uint.MaxValue;

        public Transform3D()
        {
            _children = [];
        }

        public void SetParent(Transform3D? newParent, bool keepWorld = false)
        {
            if (_parent == newParent)
                return;

            Transform3D? current = newParent;

            while (current != null)
            {
                if (current == this)
                    return;

                current = current._parent;
            }

            Matrix4x4 oldWorld = WorldMatrix;

            _parent?._children.Remove(this);

            _parent = newParent;
            _parent?._children.Add(this);

            if (keepWorld)
            {
                if (_parent != null)
                {
                    Matrix4x4.Invert(_parent.WorldMatrix, out var invParent);
                    // This was changed from:
                    Matrix4x4 local = oldWorld * invParent;
                    //Matrix4x4 local = invParent * oldWorld;
                    Decompose(local);
                }
                else
                {
                    Decompose(oldWorld);
                }
            }

            MarkDirty();
        }

        private void RecalculateWorldMatrix()
        {
            if (_parent != null)
                _worldMatrix = LocalMatrix * _parent.WorldMatrix;
            else
                _worldMatrix = LocalMatrix;

            _dirty = false;
        }

        internal void MarkDirty()
        {
            _dirty = true;
            HasMoved = true;

            foreach (var child in _children)
                child.MarkDirty();
        }
        internal void ClearDirty()
        {
            if (!HasMoved)
                return;

            HasMoved = false;

            TransformChanged?.Invoke();
        }
        internal void CapturePrevious()
        {
            _prevLocalPosition = _localPosition;
            _prevLocalRotation = _localRotation;
            _prevLocalScale = _localScale;
        }

        internal void InitializeInterpolation()
        {
            _prevLocalPosition = _localPosition;
            _prevLocalRotation = _localRotation;
            _prevLocalScale = _localScale;

            _interpolatedWorldMatrix = WorldMatrix;
            _interpolatedFrame = uint.MaxValue;
        }
        public Matrix4x4 GetGlobalTransformInterpolated(uint frameIndex, float alpha)
        {

            if (_interpolatedFrame == frameIndex)
                return _interpolatedWorldMatrix;

            Matrix4x4 local;

            Vector3 pos = Vector3.Lerp(_prevLocalPosition, _localPosition, alpha);
            Quaternion rot = Quaternion.Slerp(_prevLocalRotation, _localRotation, alpha);
            Vector3 scale = Vector3.Lerp(_prevLocalScale, _localScale, alpha);

            local = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(pos);
            Matrix4x4 result;

            // i hate life
            if (_parent != null)
            {
                Matrix4x4 parentGlobal;

                if (_parent._interpolatedFrame == frameIndex)
                {
                    parentGlobal = _parent._interpolatedWorldMatrix;
                }
                else
                {
                    parentGlobal = _parent.GetGlobalTransformInterpolated(frameIndex, alpha);
                }

                result = local * parentGlobal;
            }
            else
            {
                result = local;
            }

            _interpolatedWorldMatrix = result;
            _interpolatedFrame = frameIndex;

            return result;
        }
        private void Decompose(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(
                matrix,
                out Vector3 scale,
                out Quaternion rotation,
                out Vector3 translation
            );

            _localPosition = translation;
            _localRotation = rotation;
            _localScale = scale;
        }
    }
}