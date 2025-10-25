using Codice.CM.Common;
using UnityEngine;

namespace Templar.TemplarPhysics
{
    [CreateAssetMenu(fileName = "PhysicsStats", menuName = "Scriptable Objects/PhysicsStats")]
    public class PhysicsStats : ScriptableObject
    {
        [field: Header("Suspension")]
        [field: SerializeField, Min(1), Tooltip("How high the player should hover above the ground.\n\nIt is recommended to set this to a value higher than 1 for some ground clearance")] public float SuspensionDistance { get; private set; } = 1;
        [field: SerializeField, Min(1), Tooltip("Amount of upwards force applied")] public float SpringForce { get; private set; } = 4000;
        [field: SerializeField, Min(1), Tooltip("Spring damping.\n\nHigher value is less damping")] public float Damping { get; private set; } = 1.5f;
        
        [field: Header("Acceleration")]
        [field: SerializeField, Tooltip("How much acceleration should decrease over input magnitude\n\nCan be used to fine tune speed at lower input ranges without messing with top speed")] public float AccelerationFallOff { get; private set; }
        [field: SerializeField, Tooltip("Think of this as the force at which the object is pushed forward")] public float Acceleration { get; private set; } = 3000;

        [field: Header("Counter Forces & Limiters")]
        [field: SerializeField, Tooltip("Will only reach top speed if there is enough acceleration to overcome the air resistance. Set to -1 to disable")] public float TopSpeed { get; private set; } = -1;
        [field: SerializeField, Min(1), Tooltip("As speed is gained, air resistance increases, slowing down the player's acceleration. Lower value is more air resistance")] public float AirResistance { get; private set; } = 3;
        [field: SerializeField] public float Gravity { get; private set; } = 500;
        [field: SerializeField, Min(0), Tooltip("Allows the player to move around in the air at a fraction of it's original speed")] public float ControlInAir { get; private set; } = 0.5f;

        [field: Header("Slipping")]
        [field: SerializeField, Tooltip("Multiplier of gravity, applied when player is slipping")] public float SlipForce { get; private set; } = 10;
        [field: SerializeField, Min(0), Tooltip("How many seconds before player stops slipping")] public float SlipTimer { get; private set; } = 0.5f;
        [field: SerializeField, Range(0, 0.94f), Tooltip("The steepest slope the player can climb\n\n0 is flat ground, 1 is 90 degrees")] public float MaxSteepnessThreshold { get; private set; } = 0.7f;

        [field: Header("Terrain Detection\n(Cannot be edited at runtime)")]
        [field: SerializeField, Min(1), Tooltip("Number of primary (inner) raycasts for terrain detection\n\nMore rays is more accurate at the cost of performance")] public int PrimaryRaysCount { get; private set; } = 10;
        [field: SerializeField, Min(1), Tooltip("Number of secondary (outer) raycasts for terrain detection\n\nMore rays is more accurate at the cost of performance")] public int SecondaryRaysCount { get; private set; } = 4;
        [field: SerializeField, Range(0, 1), Tooltip("Angle as a percentage of 90 degrees")] public float PrimaryRaycastAngle { get; private set; } = 0.25f;
        [field: SerializeField, Range(0, 1), Tooltip("Angle as a percentage of 90 degrees")] public float SecondaryRaycastAngle { get; private set; } = 0.5f;

        [field: Header("Operations")]
        [field: SerializeField, Tooltip("Layer used for physics calculations")] public LayerMask PhysicsLayer { get; private set; }
        [field: SerializeField, Min(1), Tooltip("Scalar for various forces that are too small otherwise")] public int GlobalScalar { get; private set; } = 1000;
        [field: SerializeField, Tooltip("A percentage of current speed is used to calculate downwards force on slopes to prevent jitter.\n\nThis value may be used to cap the percentage of speed used in this calculation"), Range(0, 1)] public float SlopeJitterPrevention { get; private set; } = 0.5f;
    }
}
