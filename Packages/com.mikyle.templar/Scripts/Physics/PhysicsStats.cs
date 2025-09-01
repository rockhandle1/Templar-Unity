using UnityEngine;

namespace Templar.TemplarPhysics
{
    [CreateAssetMenu(fileName = "PhysicsStats", menuName = "Scriptable Objects/PhysicsStats")]
    public class PhysicsStats : ScriptableObject
    {
        [Header("Suspension")]
        [SerializeField, Min(1), Tooltip("How high the player should hover above the ground.\n\nIt is recommended to set this to a value higher than 1 for some ground clearance")] public float SuspensionDistance = 5;
        [Header("Acceleration")]
        [SerializeField, Tooltip("How much acceleration should decrease over time\n\nCan be used to fine tune speed at lower input ranges without messing with top speed")] public float AccelerationFallOff;
        [SerializeField, Tooltip("Think of this as the force at which the object is pushed forward")] public float Acceleration = 100;

        [Header("Counter Forces & Limiters")]
        [SerializeField, Tooltip("Will only reach top speed if there is enough acceleration to overcome the air resistance. Set to -1 to disable")] public float TopSpeed = -1;
        [SerializeField, Tooltip("The steepest slope the player can climb\n\n0 is flat ground, 1 is 90 degrees"), Range(0, 0.94f)] public float maxSteepnessThreshold = 0.7f;
        [SerializeField, Tooltip("As speed is gained, air resistance increases, slowing down the player's acceleration. Lower value is more air resistance"), Min(1)] public float AirResistance = 13;
        [SerializeField] public float Gravity = 10;
        [SerializeField, Tooltip("Allows the player to move around in the air at a fraction of it's original speed"), Min(0)] public float ControlInAir = 0.5f;
    }
}
