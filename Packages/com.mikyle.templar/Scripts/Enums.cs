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

    public partial class PhysicsCore : MonoBehaviour
    {
        private enum ReturnVectors
        {
            downslopeVector,
            surfaceNormal
        }
    }
}