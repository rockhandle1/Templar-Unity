using UnityEngine;
using System.Collections.Generic;

namespace Templar.TemplarPhysics
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public partial class PhysicsCore : MonoBehaviour
    {
        Rigidbody rb;
        Collider col;
        float steepness = 0;
        public MovementStates State { get; private set; }
        public Vector3 CurrentVelocity { get; private set; } = new();

        Vector3 counterVelocity;

        [SerializeField] public PhysicsStats Stats;

        [HideInInspector] public Vector3 CurrentAcceleration { get; set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            if (rb == null) throw new System.NullReferenceException("Rigidbody is null");
            if (Stats == null) throw new System.NullReferenceException("No physics stats provided");
            if (Stats.TopSpeed > -1) rb.maxLinearVelocity = Stats.TopSpeed;
        }

        Vector3 FindCentrePoint(List<RaycastHit> hits)
        {
            Vector3 sum = Vector3.zero;
            float totalWeight = 0;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null) continue;
                float distance = Vector3.Distance(transform.position, hit.point);
                float weight = 1 / (distance + 1);

                sum += hit.normal * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? sum / totalWeight : Vector3.zero;
        }

        void FixedUpdate()
        {
            Dictionary<ReturnVectors, Vector3> returnedVectors = SlopeDirectionToVelocity();

            Vector3 forceDirection = Vector3.ProjectOnPlane(CurrentAcceleration.normalized, returnedVectors[ReturnVectors.surfaceNormal]);
            DrawRays(returnedVectors[ReturnVectors.downslopeVector], forceDirection);
            VelocityUpdate(Time.fixedDeltaTime, forceDirection, returnedVectors[ReturnVectors.downslopeVector]);
        }

        void Suspension(RaycastHit downHit)
        {
            if (downHit.collider != null && downHit.distance < Stats.SuspensionDistance)
            {
                rb.AddForce(Vector3.up * Stats.SuspensionDistance * 1000 * (downHit.distance / Stats.SuspensionDistance) * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }

        void UpdateMovementState(List<RaycastHit> hits)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.distance < Stats.SuspensionDistance + 0.2f)
                {
                    State = MovementStates.Grounded;
                    break;
                }

                State = MovementStates.Falling;
            }
        }

        Dictionary<ReturnVectors, Vector3> SlopeDirectionToVelocity()
        {
            //More rays will give a more accurate slope direction and grounded detection at the cost of performance
            int numberOfRays = 20;
            float spacing = 180 / (numberOfRays / 2);
            List<Vector3> raycastDirections = new();
            List<RaycastHit> hits = new();

            Vector3 downslopeVector, surfaceNormal;

            for (int i = 0; i < numberOfRays / 2; i++)
            {
                float angle = i * (360 / (numberOfRays / 2));
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, 0.25f));
                //a is 0.25 & b is suspensionDistance
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, col.bounds.extents.y + Mathf.Sqrt(Mathf.Pow(0.25f, 2) + Mathf.Pow(Stats.SuspensionDistance, 2)));
                hits.Add(hit);
            }

            Physics.Raycast(rb.position, Vector3.down, out RaycastHit downHit, col.bounds.extents.y + Stats.SuspensionDistance);
            hits.Add(downHit);

            UpdateMovementState(hits);
            Suspension(downHit);

            for (int i = 0; i < numberOfRays / 2; i++)
            {
                float angle = i * spacing;
                Vector3 direction = Quaternion.Euler(0, angle - 90, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, 0.5f));
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, col.bounds.extents.y + Mathf.Sqrt(Mathf.Pow(0.5f, 2) + Mathf.Pow(Stats.SuspensionDistance, 2)));
                hits.Add(hit);
            }

            surfaceNormal = FindCentrePoint(hits);
            downslopeVector = Vector3.ProjectOnPlane(-Vector3.up, surfaceNormal);

            steepness = Vector3.Dot(Vector3.up, -downslopeVector.normalized);

            Dictionary<ReturnVectors, Vector3> returnVectors = new();
            returnVectors[ReturnVectors.downslopeVector] = downslopeVector;
            returnVectors[ReturnVectors.surfaceNormal] = surfaceNormal;

            foreach (Vector3 raycastDirection in raycastDirections)
            {
                Debug.DrawRay(rb.position, raycastDirection * 5, Color.yellow);
            }

            return returnVectors;
        }

        void DrawRays(Vector3 downslopeVector, Vector3 forceDirection)
        {
            Debug.DrawRay(rb.position, forceDirection * (CurrentAcceleration.magnitude / Stats.Acceleration) * 10, Color.blue);
            Debug.DrawRay(rb.position, downslopeVector * 5.0f, Color.red);
        }

        void VelocityUpdate(float deltaTime, Vector3 accelerationDirection, Vector3 downslope)
        {
            //Debug.Log(steepness);
            float angle = Vector3.Angle(accelerationDirection, downslope);

            if (State != MovementStates.Falling)
            {
                if (steepness < Stats.maxSteepnessThreshold) State = MovementStates.Grounded;
                else State = MovementStates.Slipping;
            }

            //Debug.Log(State);
            float inputAmount = CurrentAcceleration.magnitude / Stats.Acceleration;
            Vector3 acceleration = CurrentAcceleration.magnitude * accelerationDirection;
            float damping = CurrentVelocity.y * -1 / Stats.AirResistance * 1000;
            switch (State)
            {
                case MovementStates.Falling:
                    acceleration = (Vector3.down * Stats.Gravity * 10 * rb.mass) + (acceleration * Stats.ControlInAir);
                    break;

                case MovementStates.Slipping:
                    acceleration += downslope.normalized * Stats.Gravity * 10 * rb.mass;
                    break;

                case MovementStates.Grounded:
                    damping = CurrentVelocity.y * -1 / 3 * 1000;
                    break;
            }

            CurrentVelocity = rb.linearVelocity;
            counterVelocity = CurrentVelocity * -1 / Stats.AirResistance * 1000;
            //counterVelocity.y = damping;
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