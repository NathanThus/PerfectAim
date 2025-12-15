using UnityEngine;

namespace NathanThus.PerfectAim.Demo
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private Rigidbody _projectilePrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _destructionTime = 10f;

        /// <summary>
        /// Launch projectile with a velocity supplied externally.
        /// </summary>
        public void LaunchProjectile(Vector3 velocity)
        {
            if (_projectilePrefab == null || _spawnPoint == null)
            {
                Debug.Log("ProjectileLauncher: Missing prefab or spawn point.");
                return;
            }

            Rigidbody proj = Instantiate(_projectilePrefab, _spawnPoint.position, _spawnPoint.rotation);
            proj.linearVelocity = velocity;
            Destroy(proj.gameObject, _destructionTime);
        }
    }
}
