using UnityEngine;

public enum MovementStates
{
    Slipping,
    Grounded,
    Falling
}

public enum MakePositiveOptions
{
    InvertFloatCompared,
    FloatCompared,
    InvertFloat
}

namespace Templar.TemplarPhysics
{
    public partial class PhysicsCore : MonoBehaviour
    {
        private enum ReturnVectors
        {
            downslopeVector,
            surfaceNormal
        }
    }
}