using System;
using UnityEngine;

namespace NathanThus.PerfectAim
{
    public class PerfectAim : MonoBehaviour
    {

        #region Serialized Fields

        [Header("Main Settings")]
        [SerializeField] private Transform _originTransform;
        [SerializeField] private float _maximumVelocity;
        [SerializeField] private ArcStyle _style;
        [SerializeField] private float _heightMax;
        [SerializeField] private float _minimumFlightTime = 1f;
        [SerializeField] private float _desiredFlightTime = 2f;
        [SerializeField] private float _maximumFlightTime = 5f;
        [SerializeField] private Vector2 _windAcceleration;

        [Header("Player Line Visualisation")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField, Range(0, 50), Tooltip("Number of segments for the renderer.")] private int _lineSegments;

        [Header("Debug Visualization")]
        [SerializeField] private Transform _debugTargetPosition; // Set this in the inspector

        #endregion

        #region Properties

        public Vector3 DebugTargetPosition => _debugTargetPosition.position;
        public Vector3 SpawnPosition => _originTransform.position;
        public bool ShowArcInEditor { get => showArcInEditor; set => showArcInEditor = value; }

        #endregion

        #region Fields

        private readonly float _gravityMagnitudeSquared = Mathf.Pow(Physics.gravity.magnitude, 2);
        private Vector3 _physicsAcceleration = Physics.gravity * -1;
        private bool showArcInEditor = true;

        #endregion

        #region Start
        private void Start()
        {
            if (_lineRenderer == null) return;
            _lineRenderer.positionCount = _lineSegments;
            _lineRenderer.useWorldSpace = true;

            if (_originTransform == null) throw new NullReferenceException(nameof(_originTransform));
        }

        #endregion

        #region Public

        /// <summary>
        /// Calculate the velocity to hit a target.
        /// </summary>
        /// <param name="target">The global position of the target.</param>
        /// <returns>The velocity required to hit the target</returns>
        public Vector3 CalculateVelocity(Vector3 target)
        {
            Vector3 deltaPosition = target - _originTransform.position;
            float distance = deltaPosition.magnitude;

            float discriminant = Mathf.Pow(_maximumVelocity, 4) - _gravityMagnitudeSquared * Mathf.Pow(distance, 2);
            if (discriminant < 0) return Vector3.zero; // NO POSSIBLE TRAJECTORIES

            return CalculateLaunchVelocity(deltaPosition, FlightTime(discriminant));
        }

        /// <summary>
        /// Calculate the velocity to hit a target.
        /// </summary>
        /// <param name="target">The global position of the target.</param>
        /// <param name="velocity">The target's velocity.</param>
        /// <returns>The velocity required to hit the target</returns>
        public Vector3 CalculateVelocity(Vector3 target, Vector3 velocity)
        {
            Vector3 deltaPosition = target - _originTransform.position + velocity * _desiredFlightTime;
            float distance = deltaPosition.magnitude;

            float discriminant = Mathf.Pow(_maximumVelocity, 4) - _gravityMagnitudeSquared * Mathf.Pow(distance, 2);
            if (discriminant < 0) return Vector3.zero; // NO POSSIBLE TRAJECTORIES

            return CalculateLaunchVelocity(deltaPosition, FlightTime(discriminant));
        }

        /// <summary>
        /// Calculates the maximum range the system can hit, with the given maximum velocity.
        /// </summary>
        /// <returns>The maximum distance in standard Unity Units.</returns>
        public float CalculateRange()
        {
            float maxDistance = _maximumVelocity * _maximumVelocity / Physics.gravity.magnitude;
            return maxDistance;
        }

        /// <summary>
        /// Set the windspeed variable for CalculateVelocity
        /// </summary>
        /// <param name="windX">The windspeed in the X direction.</param>
        /// <param name="windZ">The windspeed in the Z direction.</param>
        public void SetEnviromentalParameters(float windX, float windZ)
        {
            _windAcceleration = new Vector2(windX, windZ);
        }

        /// <summary>
        /// Show the arc that the projectile will perform, using the given Linerenderer and LaunchVelocity.
        /// </summary>
        /// <param name="launchVelocity">The launch velocity of the projectile.</param>
        public void ShowArc(Vector3 launchVelocity)
        {
            for (int i = 0; i < _lineSegments; i++)
            {
                _lineRenderer.SetPosition(i, CalculatePositionAtTime(_originTransform.position, launchVelocity, i * 0.1f));
            }
        }

        /// <summary>
        /// Calculates the position of the projectile at the a given point in time.
        /// </summary>
        /// <param name="origin">The origin position.</param>
        /// <param name="velocity">The velocity of the projectile.</param>
        /// <param name="time">The time post launch.</param>
        /// <returns>Calculates the position of the projectile, at a given time during flight.</returns>
        public Vector3 CalculatePositionAtTime(Vector3 origin, Vector3 velocity, float time)
        {
            // Kinematic equation: position = origin + velocity * t + 0.5 * t^2 * gravity 
            return origin + velocity * time + 0.5f * Mathf.Pow(time, 2) * GetEnvironmentalAcceleration();
        }

        #endregion

        #region Private

        private Vector3 CalculateLaunchVelocity(Vector3 deltaPosition, float flightTime)
        {
            return deltaPosition / flightTime + GetEnvironmentalAcceleration() * (flightTime / 2.0f);
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
            t = Mathf.Max(t, _minimumFlightTime);
            return t;
        }

        private float FlightTime(float discriminant)
        {
            return _style switch
            {
                ArcStyle.MinimalTrajectory => GetMinimumFlightTime(Mathf.Pow(_maximumVelocity, 2),
                                                                   Mathf.Sqrt(discriminant)),
                ArcStyle.MaximumTrajectory => GetMaximumFlightTime(Mathf.Pow(_maximumVelocity, 2),
                                                                   Mathf.Sqrt(discriminant)),
                ArcStyle.PreciseFlightTime => _desiredFlightTime,
                _ => throw new NotImplementedException()
            };
        }

        private Vector3 GetEnvironmentalAcceleration()
        {
            return new(_windAcceleration.x, _physicsAcceleration.y, _windAcceleration.y);
        }

        #endregion
    }
}
