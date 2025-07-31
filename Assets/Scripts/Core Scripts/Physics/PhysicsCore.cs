using UnityEngine;
using System.Collections.Generic;

namespace Templar.TemplarPhysics
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsCore : MonoBehaviour
    {
        Rigidbody rb;
        Collider col;
        float steepness = 0;
        float _gravity;
        float cachedAirResistance;
        public Vector3 CurrentVelocity { get; private set; } = new();

        Vector3 counterVelocity;

        [SerializeField] public PhysicsStats Stats;

        [HideInInspector] public Vector3 CurrentAcceleration { get; set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            //slowDownFactor = (acceleration * 0.1f / airResistance);
            cachedAirResistance = Stats.AirResistance;
            _gravity = Stats.Gravity;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            if (rb == null) throw new System.NullReferenceException("Rigidbody is null");
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
            Vector3[] returnedVectors = SlopeDirectionToVelocity();
            float directionDot = Vector3.Dot(CurrentAcceleration.normalized, returnedVectors[0].normalized);
            Vector3 forceDirection = Vector3.Lerp(CurrentAcceleration.normalized, returnedVectors[0] * directionDot, 0.99f).normalized;
            DrawRays(returnedVectors[0], forceDirection);
            VelocityUpdate(Time.fixedDeltaTime, forceDirection, returnedVectors[0]);
        }

        bool _grounded;
        Vector3[] SlopeDirectionToVelocity()
        {
            int numberOfRays = 20;
            float spacing = 180 / (numberOfRays / 2);
            List<Vector3> raycastDirections = new();
            List<RaycastHit> hits = new();

            Vector3 downslopeVector;
            Vector3 surfaceNormal;

            for (int i = 0; i < numberOfRays / 2; i++)
            {
                float angle = i * spacing;
                Vector3 direction = Quaternion.Euler(0, angle - 90, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, 0.25f));
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, col.bounds.extents.y + 0.04f);
                hits.Add(hit);
            }

            for (int i = 0; i < numberOfRays / 2; i++)
            {
                float angle = i * spacing;
                Vector3 direction = Quaternion.Euler(0, angle - 90, 0) * transform.forward;
                raycastDirections.Add(Vector3.Lerp(Vector3.down, direction, 0.5f));
                Physics.Raycast(rb.position, raycastDirections[i], out RaycastHit hit, col.bounds.extents.y + 1f);
                hits.Add(hit);
            }

            Physics.Raycast(rb.position, Vector3.down, out RaycastHit downHit, col.bounds.extents.y + 1f);
            hits.Add(downHit);

            surfaceNormal = FindCentrePoint(hits);
            downslopeVector = Vector3.ProjectOnPlane(-Vector3.up, surfaceNormal);

            if (downHit.collider != null && downHit.distance < 1f)
            {
                rb.position = downHit.point + new Vector3(0, 1f, 0);
            }

            if (downHit.collider != null && downHit.distance < 1.2f) _grounded = true;

            else _grounded = false;

            steepness = Vector3.Dot(Vector3.up, -downslopeVector.normalized);

            Vector3[] returnVectors = new Vector3[3];
            returnVectors[0] = downslopeVector;
            returnVectors[1] = surfaceNormal;

            foreach (Vector3 raycastDirection in raycastDirections)
            {
                Debug.DrawRay(rb.position, raycastDirection * 5, Color.yellow);
            }

            return returnVectors;
        }

        void DrawRays(Vector3 downslopeVector, Vector3 forceDirection)
        {
            Debug.DrawRay(rb.position, forceDirection * (CurrentAcceleration.magnitude / Stats.Acceleration) * (1 + (Stats.AccelerationFallOff.Evaluate(CurrentAcceleration.magnitude) * 100)), Color.blue);
            Debug.DrawRay(rb.position, downslopeVector * 5.0f, Color.red);
        }

        void VelocityUpdate(float deltaTime, Vector3 accelerationDirection, Vector3 downslope)
        {
            //Debug.Log(steepness);
            Debug.Log(Vector3.Angle(accelerationDirection, downslope));
            if (steepness < Stats.maxSteepnessThreshold || Vector3.Angle(accelerationDirection, downslope) <= 90)
            {
                rb.AddForce(CurrentAcceleration.magnitude * accelerationDirection * deltaTime, ForceMode.Acceleration);
            }

            if (_grounded)
            {
                if(steepness > 0.1f && CurrentAcceleration.magnitude < 1000)
                {
                    Stats.AirResistance = 3;
                }

                else
                {
                    _gravity = Stats.Gravity - (Stats.Gravity * (steepness * 5 / 10));
                    Stats.AirResistance = cachedAirResistance;
                }

                _gravity = 0;
            }

            else
            {
                _gravity = Stats.Gravity;
                Stats.AirResistance = cachedAirResistance;
            }

            CurrentVelocity = rb.linearVelocity;
            counterVelocity = CurrentVelocity * -1 / Stats.AirResistance * 1000;

            rb.AddForce(counterVelocity * deltaTime, ForceMode.Acceleration);
            rb.AddForce(Vector3.down * _gravity * rb.mass * deltaTime, ForceMode.Acceleration);
            if (Mathf.Min(new Vector2(CurrentVelocity.x, CurrentVelocity.z).magnitude, 0.01f) < 0.01f) rb.linearVelocity = new Vector3(0, CurrentVelocity.y, 0);
            //Debug.Log(counterVelocity * deltaTime);
            //Debug.Log(currentVelocity);
            //Debug.Log(Input.GetAxisRaw("Horizontal"));
        }
    }
}