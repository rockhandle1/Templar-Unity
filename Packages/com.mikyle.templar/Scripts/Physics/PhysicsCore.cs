using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

namespace Templar.TemplarPhysics
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public partial class PhysicsCore : MonoBehaviour
    {
        Rigidbody rb;
        [SerializeField, Tooltip("A non trigger collider that is used for collisions"), InspectorName("Collider")] Collider col;
        float steepness = 0;
        public MovementStates State { get; private set; }
        public Vector3 CurrentVelocity { get; private set; } = new();
        public IReadOnlyList<RaycastHit> CurrentFooting => hits;

        Vector3 counterVelocity;

        [SerializeField] public PhysicsStats Stats;

        [HideInInspector] public Vector3 CurrentAcceleration { get; set; }

        private float slopeRaycastsL1, slopeRaycastsL2;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (col == null || col.isTrigger) throw new System.NullReferenceException("No physics collider found. Please ensure a non trigger collider is attached to the gameobject");
            if (rb == null) throw new System.NullReferenceException("Rigidbody is null");
            if (Stats == null) throw new System.NullReferenceException("No physics stats provided");
            if (Stats.TopSpeed > 0) rb.maxLinearVelocity = Stats.TopSpeed;

            slopeRaycastsL1 = CalculateRayLength(Stats.PrimaryRaycastAngle);
            slopeRaycastsL2 = CalculateRayLength(Stats.SecondaryRaycastAngle);
        }

        Vector3 FindCentrePoint(List<RaycastHit> hits)
        {
            Vector3 sum = Vector3.zero;
            float totalWeight = 0;

            foreach (RaycastHit hit in hits)
            {
                //Reject vector if it is a wall. Fixes player being able to climb walls
                if (Vector3.Dot(Vector3.up, -Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized) > 0.95f) continue;

                float distance = Vector3.Distance(transform.position, hit.point);
                float weight = 1 / (distance + 1);

                sum += hit.normal * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? sum / totalWeight : Vector3.zero;
        }

        Vector3 forceDirection;
        Vector3 unadjustedForceDirection;
        Dictionary<ReturnVectors, Vector3> returnedVectors = new();
        void FixedUpdate()
        {
            returnedVectors.Clear();
            SlopeDirectionToVelocity();
            unadjustedForceDirection = Vector3.ProjectOnPlane(CurrentAcceleration.normalized, returnedVectors[ReturnVectors.surfaceNormal]);
            forceDirection = Vector3.Lerp(unadjustedForceDirection, -returnedVectors[ReturnVectors.surfaceNormal].normalized, 0.25f);

#if UNITY_EDITOR
            DrawRays(returnedVectors[ReturnVectors.downslopeVector], forceDirection);
#endif
            UpdateMovementState(CurrentFooting);
            VelocityUpdate(Time.fixedDeltaTime, forceDirection, returnedVectors[ReturnVectors.downslopeVector]);
#if UNITY_EDITOR
            Debug.Log(rb.linearVelocity.magnitude);
#endif
        }

        void Suspension(RaycastHit downHit)
        {
            if (downHit.distance < Stats.SuspensionDistance)
            {
                rb.linearVelocity += Vector3.up * Stats.SuspensionDistance * (downHit.distance / Stats.SuspensionDistance) * 2;
                //rb.AddForce(Vector3.up * Stats.SuspensionDistance * (downHit.distance / Stats.SuspensionDistance) * 50, ForceMode.Acceleration);
            }
        }

        float slipTimer = 0;
        void UpdateMovementState(IReadOnlyList<RaycastHit> hits)
        {
            bool hasCloseHit = hits.Any(hit => hit.distance < Stats.SuspensionDistance + 0.2f);

            if (hasCloseHit)
            {
                if (State == MovementStates.Slipping && (steepness > 0.7f * Stats.maxSteepnessThreshold || slipTimer < Stats.SlipTimer))
                {
                    slipTimer += Time.fixedDeltaTime;
                    return;
                }

                slipTimer = 0;

                if (steepness > Stats.maxSteepnessThreshold)
                {
                    State = MovementStates.Slipping;
                    return;
                }

                State = MovementStates.Grounded;
                return;
            }

            State = MovementStates.Falling;
            slipTimer = 0;
        }

        float CalculateRayLength(float angle)
        {
            //Yes, it is intentionally slightly longer than it needs to be to touch a flat ground. why? because sometimes it needs to touch a slope
            return col.bounds.extents.y + Mathf.Sqrt(Mathf.Pow(Mathf.Tan((angle * 90) * Mathf.Deg2Rad) * Stats.SuspensionDistance, 2) + Mathf.Pow(Stats.SuspensionDistance, 2));
        }

        private readonly List<Vector3> raycastDirections = new();
        private readonly List<RaycastHit> hits = new();

        //More rays will give a more accurate slope direction and grounded detection at the cost of performance
        void SlopeDirectionToVelocity()
        {
            float spacing = 180 / Stats.SecondaryRaysCount;
            Vector3 downslopeVector, surfaceNormal;

            raycastDirections.Clear();
            hits.Clear();
            for (int i = 0; i < Stats.PrimaryRaysCount; i++)
            {
                float angle = i * (360 / Stats.PrimaryRaysCount);
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, Stats.PrimaryRaycastAngle));
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, slopeRaycastsL1, Stats.PhysicsLayer, QueryTriggerInteraction.Ignore);
                if (hit.collider == null) continue;
                hits.Add(hit);
            }

            Physics.Raycast(rb.position, Vector3.down, out RaycastHit downHit, col.bounds.extents.y + Stats.SuspensionDistance, Stats.PhysicsLayer, QueryTriggerInteraction.Ignore);
            if (downHit.collider != null) hits.Add(downHit);

            Suspension(downHit);

            for (int i = Stats.PrimaryRaysCount; i < Stats.PrimaryRaysCount + Stats.SecondaryRaysCount; i++)
            {
                float angle = (i - Stats.PrimaryRaysCount) * spacing;
                Vector3 direction = Quaternion.Euler(0, (Mathf.Rad2Deg * angle) + 270, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, Stats.SecondaryRaycastAngle));
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, slopeRaycastsL2, Stats.PhysicsLayer, QueryTriggerInteraction.Ignore);
                if (hit.collider == null) continue;
                hits.Add(hit);
            }

            surfaceNormal = FindCentrePoint(hits).normalized;
            downslopeVector = Vector3.ProjectOnPlane(-Vector3.up, surfaceNormal);

            steepness = Vector3.Dot(Vector3.up, -downslopeVector.normalized);

            
            returnedVectors[ReturnVectors.downslopeVector] = downslopeVector;
            returnedVectors[ReturnVectors.surfaceNormal] = surfaceNormal;

#if UNITY_EDITOR
            foreach (Vector3 raycastDirection in raycastDirections)
            {
                Debug.DrawRay(rb.position, raycastDirection * (col.bounds.extents.y + Mathf.Sqrt(Mathf.Pow(Mathf.Tan(Stats.PrimaryRaycastAngle * 90) * Stats.SuspensionDistance, 2) + Mathf.Pow(Stats.SuspensionDistance, 2))), Color.yellow);
            }
#endif
        }

        void DrawRays(Vector3 downslopeVector, Vector3 forceDirection)
        {
            Debug.DrawRay(rb.position, (CurrentAcceleration.magnitude / Stats.Acceleration) * 10 * forceDirection, Color.blue);
            Debug.DrawRay(rb.position, downslopeVector * 5.0f, Color.red);
        }

        void VelocityUpdate(float deltaTime, Vector3 accelerationDirection, Vector3 downslope)
        {
            //Debug.Log(steepness);
            //Debug.Log(State);
            float inputAmount = CurrentAcceleration.magnitude / Stats.Acceleration;
            Vector3 acceleration = (2 - ((Vector3.Dot(accelerationDirection, unadjustedForceDirection)) / 2)) * CurrentAcceleration.magnitude * accelerationDirection;
            float damping = CurrentVelocity.y * -1 / Stats.AirResistance * Stats.GlobalScalar;
            switch (State)
            {
                case MovementStates.Falling:
                    acceleration = (Vector3.down * Stats.Gravity * 10) + (acceleration * Stats.ControlInAir);
                    break;

                case MovementStates.Slipping:
                    acceleration += Stats.SlipForce * Stats.Gravity * downslope.normalized;
                    break;

                case MovementStates.Grounded:
                    damping = CurrentVelocity.y * -1 / 1.5f * Stats.GlobalScalar;
                    //acceleration += (-returnedVectors[ReturnVectors.surfaceNormal].normalized * Stats.Gravity * rb.mass);
                    break;
            }

            CurrentVelocity = rb.linearVelocity;
            counterVelocity = -CurrentVelocity / Stats.AirResistance * Stats.GlobalScalar;
            counterVelocity.y = damping;
            //counterVelocity = CurrentVelocity * -1 / (Stats.AirResistance / (1 + (State == MovementStates.Falling || State == MovementStates.Slipping ? 1 : 0))) * 1000;
            rb.AddForce((acceleration + counterVelocity) * deltaTime, ForceMode.Acceleration);
            //rb.AddForce(CurrentAcceleration.magnitude * accelerationDirection * deltaTime, ForceMode.Acceleration);

            //if (State == MovementStates.Falling) rb.AddForce(Vector3.down * Stats.Gravity * 10 * rb.mass * deltaTime, ForceMode.Acceleration);
            if (Mathf.Min(new Vector2(CurrentVelocity.x, CurrentVelocity.z).magnitude, 0.01f) < 0.01f) rb.linearVelocity = new Vector3(0, CurrentVelocity.y, 0);
            //Debug.Log(counterVelocity * deltaTime);
            //Debug.Log(currentVelocity);
        }
    }
}