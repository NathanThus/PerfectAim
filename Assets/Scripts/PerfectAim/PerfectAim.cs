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
        [SerializeField] private float _desiredFlightTime = 2f;
        [SerializeField] private float _maximumFlightTime = 5f;
        [SerializeField]private Vector2 _windAcceleration;
        private Vector3 _physicsAcceleration = Physics.gravity * -1;

        [Header("Debug Visualization")]
        [SerializeField] private Transform _debugTargetPosition; // Set this in the inspector
        public Vector3 DebugTargetPosition => _debugTargetPosition.position;
        public bool ShowArcInEditor { get => showArcInEditor; set { showArcInEditor = value; } }
        private bool showArcInEditor = true;
        private readonly float _gravityMagnitudeSquared = Mathf.Pow(Physics.gravity.magnitude,2);

        public Vector3 CalculateVelocity(Vector3 target)
        {
            Vector3 deltaPosition = target - _originTransform.position;
            float distance = deltaPosition.magnitude;

            float discriminant = MathF.Pow(_maximumVelocity, 4) - _gravityMagnitudeSquared * Mathf.Pow(distance, 2);
            if (discriminant < 0) return Vector3.zero; // NO POSSIBLE TRAJECTORIES

            float flightTime = FlightTime(discriminant);
            
            return deltaPosition / flightTime + GetEnvironmentalAcceleration() * (flightTime / 2.0f);
        }

        public Vector3 CalculateVelocity(Vector3 target, Vector3 velocity)
        {
            Vector3 deltaPosition = target - _originTransform.position + velocity * _desiredFlightTime;
            float distance = deltaPosition.magnitude;

            float discriminant = MathF.Pow(_maximumVelocity, 4) - _gravityMagnitudeSquared * Mathf.Pow(distance, 2);
            if (discriminant < 0) return Vector3.zero; // NO POSSIBLE TRAJECTORIES

            return deltaPosition / _desiredFlightTime + GetEnvironmentalAcceleration() * (_desiredFlightTime / 2.0f);
        }

        private float GetMaximumFlightTime(float velocitySquared, float sqrtDiscriminant)
        {
            float t = Mathf.Sqrt((velocitySquared + sqrtDiscriminant) / _gravityMagnitudeSquared);
            t = Mathf.Min(t, _maximumFlightTime);
            return t;
        }

        private float GetMinimumFlightTime(float velocitySquared, float sqrtDiscriminant)
        {
            float t = Mathf.Sqrt((velocitySquared - sqrtDiscriminant) / _gravityMagnitudeSquared);
            t = Mathf.Max(t, _mimimumFlightTime);
            return t;
        }

        private float FlightTime(float discriminant)
        {
            return _style switch
            {
                ArcStyle.MimimalTrajectory => GetMinimumFlightTime(MathF.Pow(_maximumVelocity, 2),
                                                                   Mathf.Sqrt(discriminant)),
                ArcStyle.MaximumTrajectory => GetMaximumFlightTime(MathF.Pow(_maximumVelocity, 2),
                                                                   Mathf.Sqrt(discriminant)),
                ArcStyle.PreciseFlightTime => _desiredFlightTime,
                _ => throw new NotImplementedException()
            };
        }

        private Vector3 GetEnvironmentalAcceleration()
        {
            return new(_windAcceleration.x, _physicsAcceleration.y, _windAcceleration.y);
        }
    }
}
