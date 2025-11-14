using System;
using UnityEngine;

namespace NathanThus.PerfectAim
{
    public class PerfectAim : MonoBehaviour
    {

        [SerializeField] private Transform _originTransform;
        [SerializeField] private float _maximumVelocity;
        [SerializeField] private ArcStyle _style;
        [SerializeField] private float _heightMax;
        [SerializeField] private float _mimimumFlightTime = 1f;
        [SerializeField] private float _maximumFlightTime = 5f;
        private Vector3 _physicsAcceleration = Physics.gravity * -1;

        [Header("Debug Visualization")]
        [SerializeField] private Transform _debugTargetPosition; // Set this in the inspector
        public Vector3 DebugTargetPosition => _debugTargetPosition.position;
        public bool ShowArcInEditor { get => showArcInEditor; set { showArcInEditor = value; } }
        private bool showArcInEditor = true;

        private float _gravityMagnitudeSquared = Mathf.Pow(Physics.gravity.magnitude,2); // 9.81

        public Vector3 CalculateVelocity(Vector3 target)
        {
            Vector3 deltaPosition = target - _originTransform.position;
            float deltaPositionMagnitude = deltaPosition.magnitude;

            float discriminant = MathF.Pow(_maximumVelocity, 4) - _gravityMagnitudeSquared * Mathf.Pow(deltaPositionMagnitude, 2);
            if (discriminant < 0) return Vector3.zero; // NO POSSIBLE TRAJECTORIES

            float velocitySquared = MathF.Pow(_maximumVelocity, 2);
            float sqrtDiscriminant = Mathf.Sqrt(discriminant);

            float trajectory = _style switch
            {
                ArcStyle.MimimalTrajectory => GetMinimumTrajectory(velocitySquared, sqrtDiscriminant),
                ArcStyle.MaximumTrajectory => GetMaximumTrajectory(velocitySquared, sqrtDiscriminant),
                _ => throw new NotImplementedException()
            };

            // Calculate launch velocity
            Vector3 launchVelocity = deltaPosition / trajectory + _physicsAcceleration * (trajectory / 2.0f);
            return launchVelocity;
        }

        private float GetMaximumTrajectory(float velocitySquared, float sqrtDiscriminant)
        {
            float t = Mathf.Sqrt((velocitySquared + sqrtDiscriminant) / _gravityMagnitudeSquared);
            t = Mathf.Min(t, _maximumFlightTime);
            return t;
        }

        private float GetMinimumTrajectory(float velocitySquared, float sqrtDiscriminant)
        {

            float t = Mathf.Sqrt((velocitySquared - sqrtDiscriminant) / _gravityMagnitudeSquared);
            t = Mathf.Max(t, _mimimumFlightTime);
            return t;
        }

    }
}
