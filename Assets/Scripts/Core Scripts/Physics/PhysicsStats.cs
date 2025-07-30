using UnityEngine;

namespace Templar.TemplarPhysics
{
    [CreateAssetMenu(fileName = "AccelerationStats", menuName = "Scriptable Objects/AccelerationStats")]
    public class PhysicsStats : ScriptableObject
    {
        [Header("Acceleration")]
        [SerializeField, Tooltip("How much acceleration should decrease over time")] public AnimationCurve AccelerationFallOff;
        [SerializeField, Tooltip("Think of this as the force at which the object is pushed forward")] public float Acceleration = 100;

        [Header("Counter Forces & Limiters")]
        [SerializeField, Tooltip("Will only reach top speed if there is enough acceleration to overcome the air resistance. Set to -1 to disable")] public float TopSpeed = -1;
        [SerializeField, Tooltip("The steepest slope the player can climb"), Range(0, 1)] public float maxSteepnessThreshold = 0.7f;
        [SerializeField, Tooltip("Lower is more. Value of 0 is invalid. negative values are tailwinds (there is no other resistance so it will result in exponential acceleration)")] public float AirResistance = 13;
        [SerializeField] public float Gravity = 10;
    }
}
