using UnityEngine;

namespace Templar
{
    internal enum MakePositiveOptions
    {
        InvertFloatCompared,
        FloatCompared,
        InvertFloat
    }
}

namespace Templar.TemplarPhysics
{
    public enum MovementStates
    {
        Slipping,
        Grounded,
        Falling
    }

    internal struct SurfaceTraits
    {
        public Vector3 downslopeVector;
        public Vector3 surfaceNormal;
    }
}