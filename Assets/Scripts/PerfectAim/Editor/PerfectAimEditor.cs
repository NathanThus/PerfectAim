using UnityEngine;

namespace NathanThus.PerfectAim.Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PerfectAim))]
    public class PerfectAimEditor : Editor
    {
        private const int ARC_SEGMENTS = 50;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PerfectAim perfectAimModule = (PerfectAim)target;

            EditorGUILayout.Space();
            perfectAimModule.ShowArcInEditor = EditorGUILayout.Toggle("Show Arc Gizmo", perfectAimModule.ShowArcInEditor);

            if (GUILayout.Button("Refresh Arc Preview"))
            {
                SceneView.RepaintAll();
            }
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        static void DrawArcGizmo(PerfectAim perfectAimModule, GizmoType gizmoType)
        {
            if (!perfectAimModule.ShowArcInEditor) return;

            Vector3 origin = perfectAimModule.transform.position;
            Vector3 targetPos = perfectAimModule.DebugTargetPosition;

            // Draw target position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPos, 0.3f);

            // Get the launch velocity
            Vector3 launchVelocity = perfectAimModule.CalculateVelocity(targetPos);

            if (launchVelocity.magnitude == 0 || float.IsInfinity(launchVelocity.magnitude))
            {
                // Draw dotted line to show invalid trajectory
                Gizmos.color = Color.red;
                DrawDottedLine(origin, targetPos);
                return;
            }

            // Calculate flight time
            float gravity = Mathf.Abs(Physics.gravity.y);
            float totalTime = 2f * launchVelocity.y / gravity;

            // Draw the arc
            Vector3 previousPoint = origin;
            Gizmos.color = Color.green;

            for (int i = 1; i <= ARC_SEGMENTS; i++)
            {
                float t = i / (float)ARC_SEGMENTS * totalTime;
                Vector3 point = origin + launchVelocity * t + 0.5f * t * t * Physics.gravity;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Draw origin point
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(origin, 0.3f);

            // Draw landing point
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(previousPoint, 0.25f);
        }

        static void DrawDottedLine(Vector3 start, Vector3 end)
        {
            int segments = 20;
            Vector3 dir = (end - start) / segments;
            for (int i = 0; i < segments; i += 2)
            {
                Gizmos.DrawLine(start + dir * i, start + dir * (i + 1));
            }
        }

        private void OnSceneGUI()
        {
            PerfectAim perfectAimModule = (PerfectAim)target;

            Vector3 origin = perfectAimModule.transform.position;
            Vector3 targetPos = perfectAimModule.DebugTargetPosition;

            // Draw target position
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, targetPos, Quaternion.identity, 0.3f, EventType.Repaint);
            Handles.Label(targetPos + Vector3.up * 0.5f, "Target");

            // Get the launch velocity
            Vector3 launchVelocity = perfectAimModule.CalculateVelocity(targetPos);

            if (launchVelocity.magnitude == 0 || float.IsInfinity(launchVelocity.magnitude))
            {
                Handles.Label(origin + Vector3.up * 2, "Invalid Trajectory!");
                // Draw straight line to show the attempted path
                Handles.color = Color.red;
                Handles.DrawDottedLine(origin, targetPos, 4f);
                return;
            }

            // Calculate flight time
            float totalTime = CalculateFlightTime(launchVelocity);

            // Draw the arc
            Vector3 previousPoint = origin;
            Handles.color = Color.green;

            for (int i = 1; i <= ARC_SEGMENTS; i++)
            {
                float t = i / ARC_SEGMENTS * totalTime;
                Vector3 point = CalculatePositionAtTime(origin, launchVelocity, t);

                Handles.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            // Draw origin point
            Handles.color = Color.blue;
            Handles.SphereHandleCap(0, origin, Quaternion.identity, 0.3f, EventType.Repaint);
            Handles.Label(origin + Vector3.up * 0.5f, "Origin");

            // Draw velocity vector at origin
            Handles.color = Color.yellow;
            Handles.DrawLine(origin, origin + launchVelocity.normalized * 2f);
            Handles.Label(origin + launchVelocity.normalized * 2.5f,
                $"V: {launchVelocity.magnitude:F2} m/s\nTime: {totalTime:F2}s");

            // Draw where the arc actually lands
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, previousPoint, Quaternion.identity, 0.25f, EventType.Repaint);
            float distanceToTarget = Vector3.Distance(previousPoint, targetPos);
            
            Handles.Label(previousPoint, $"Landing\nError: {distanceToTarget:F2}m");
            //+ Vector3.up * 0.5f
        }

        private float CalculateFlightTime(Vector3 launchVelocity)
        {
            // Simple approximation: time = 2 * vy / g
            float gravity = Mathf.Abs(Physics.gravity.y);
            return 2f * launchVelocity.y / gravity;
        }

        private Vector3 CalculatePositionAtTime(Vector3 origin, Vector3 velocity, float time)
        {
            // Kinematic equation: position = origin + velocity * t + 0.5 * gravity * t^2
            return origin + velocity * time + 0.5f * time * time * Physics.gravity;
        }
    }
}
