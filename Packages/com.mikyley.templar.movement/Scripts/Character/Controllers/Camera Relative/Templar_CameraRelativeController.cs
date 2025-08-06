using UnityEngine;
using Templar.TemplarPhysics;

namespace Templar.Player
{
    [RequireComponent(typeof(PhysicsCore))]
    public class Templar_CameraRelativePlayer : MonoBehaviour
    {
        [SerializeField] float _rotationSpeed;
        PhysicsCore _physics;
        Camera _mainCam;
        PhysicsStats _physicsStats;
        Rigidbody _rb;
        Vector3 camDirection;
        Vector3 axisAcceleration;

        private void Awake()
        {
            _mainCam = Camera.main;
            _physics = GetComponent<PhysicsCore>();
            _physicsStats = _physics.Stats;
            _rb = GetComponent<Rigidbody>();
        }

        protected void Movement(Vector2 inputAxis)
        {
            Vector3 v3InputAxis = new Vector3(inputAxis.x, 0, inputAxis.y);
            camDirection = _mainCam.transform.right * v3InputAxis.x + _mainCam.transform.forward * v3InputAxis.z;
            camDirection = camDirection.normalized;
            float camRotation = _mainCam.transform.rotation.y;
            axisAcceleration = camDirection * _physicsStats.Acceleration * Mathf.Clamp01(v3InputAxis.magnitude);
            //newAcceleration = new Vector3(axisAcceleration.x / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.z))), 0, axisAcceleration.z / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.x))));
            //float eval = _physicsStats.AccelerationFallOff.Evaluate(axisAcceleration.magnitude / _physicsStats.Acceleration);
            float eval = 1;
            _physics.CurrentAcceleration = axisAcceleration * eval;
            //Debug.Log(_physics.CurrentAcceleration.magnitude);
        }

        private void UpdateRotation()
        {
            if (_physics.CurrentAcceleration != Vector3.zero) _rb.rotation = Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(new Vector3(camDirection.normalized.x, 0, camDirection.normalized.z), Vector3.up), _rotationSpeed * (axisAcceleration.magnitude / _physicsStats.Acceleration) * Time.fixedDeltaTime);
        }

        protected virtual void FixedUpdate()
        {
            UpdateRotation();
        }
    }
}
