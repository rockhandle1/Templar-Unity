using UnityEngine;
using Templar.TemplarPhysics;

namespace Templar.Player
{
    [RequireComponent(typeof(PhysicsCore))]
    public class CameraRelativePlayer : TemplarPlayerInputs
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
        protected override void LeftStick(Vector2 input)
        {
            if (input.magnitude < deadzone * -1 || input.magnitude > deadzone)
            {
                Movement(new Vector3(input.x, 0, input.y));
            }
            else _physics.CurrentAcceleration = Vector3.zero;
        }

        protected void Movement(Vector3 inputAxis)
        {
            camDirection = _mainCam.transform.right * inputAxis.x + _mainCam.transform.forward * inputAxis.z;
            camDirection = camDirection.normalized;
            float camRotation = _mainCam.transform.rotation.y;
            axisAcceleration = camDirection * _physicsStats.Acceleration * Mathf.Clamp01(inputAxis.magnitude);
            //newAcceleration = new Vector3(axisAcceleration.x / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.z))), 0, axisAcceleration.z / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.x))));
            //float eval = _physicsStats.AccelerationFallOff.Evaluate(axisAcceleration.magnitude / _physicsStats.Acceleration);
            float eval = 1;
            _physics.CurrentAcceleration = axisAcceleration * eval;
            //Debug.Log(_physics.CurrentAcceleration.magnitude);
        }

        private void FixedUpdate()
        {
            if (_physics.CurrentAcceleration != Vector3.zero) _rb.rotation = Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(new Vector3(camDirection.normalized.x, 0, camDirection.normalized.z), Vector3.up), _rotationSpeed * (axisAcceleration.magnitude / _physicsStats.Acceleration) * Time.fixedDeltaTime);
        }
    }
}
