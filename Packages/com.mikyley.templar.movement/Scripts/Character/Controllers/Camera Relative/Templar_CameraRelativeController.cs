using UnityEngine;
using Templar.TemplarPhysics;

namespace Templar.Player
{
    [RequireComponent(typeof(PhysicsCore))]
    public class Templar_CameraRelativePlayer : MonoBehaviour
    {
        [SerializeField] float _rotationSpeed;
        PhysicsCore _physics;
        [SerializeField, Tooltip("Primary camera that the player controls\n\nIf value is not set, will attempt to find main camera")] Camera _gameCamera;
        PhysicsStats _physicsStats;
        Rigidbody _rb;
        Vector3 camDirection;
        Vector3 axisAcceleration;
        bool _rotationEnabled;
        public bool RotationActive { get => _rotationEnabled; set => _rotationEnabled = value; }
        public Camera GameCamera { get => _gameCamera; set => _gameCamera = value; }
        public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }

        public void SetRotation(Quaternion rotation) => _rb.rotation = rotation;

        private void Awake()
        {
            if(_gameCamera == null) _gameCamera = Camera.main;
            if(_gameCamera == null) throw new System.NullReferenceException("Could not find a game camera");
            _physics = GetComponent<PhysicsCore>();
            _physicsStats = _physics.Stats;
            _rb = GetComponent<Rigidbody>();
        }

        protected void Movement(Vector2 inputAxis)
        {
            Vector3 v3InputAxis = new Vector3(inputAxis.x, 0, inputAxis.y);
            camDirection = _gameCamera.transform.right * inputAxis.x + _gameCamera.transform.forward * inputAxis.y;
            camDirection = camDirection.normalized;
            float camRotation = _gameCamera.transform.rotation.y;
            float fallenOffAccel = _physicsStats.Acceleration * Mathf.Clamp01(inputAxis.magnitude) / (1 + (Mathf.Clamp01(inputAxis.magnitude) / _physicsStats.Acceleration * _physicsStats.AccelerationFallOff));
            //Debug.Log(fallenOffAccel);
            axisAcceleration = camDirection * fallenOffAccel;
            //newAcceleration = new Vector3(axisAcceleration.x / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.z))), 0, axisAcceleration.z / (1 + Mathf.Clamp01(SharedFunctions.MakePositive(inputAxis.x))));
            //float eval = _physicsStats.AccelerationFallOff.Evaluate(axisAcceleration.magnitude / _physicsStats.Acceleration);
            _physics.CurrentAcceleration = axisAcceleration;
            //Debug.Log(_physics.CurrentAcceleration.magnitude);
        }

        private void UpdateRotation()
        {
            if (_physics.CurrentAcceleration != Vector3.zero) _rb.rotation = Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(new Vector3(camDirection.normalized.x, 0, camDirection.normalized.z), Vector3.up), _rotationSpeed * (axisAcceleration.magnitude / _physicsStats.Acceleration) * Time.fixedDeltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (!_rotationEnabled) return;
            UpdateRotation();
        }
    }
}
