using UnityEngine;
using UnityEngine.AI;

// ReSharper disable once CheckNamespace
namespace AudioFramework
{
    public class EnemyHearingComponent : MonoBehaviour
    {
        public Vector3 LastKnownPosition => _lastingKnownPosition;
        
        [SerializeField] private float hearingStrength = 1f;
        [SerializeField] private AnimationCurve hearingFalloffCurve;

        private Vector3 _lastingKnownPosition;
        private bool _playerInRange;

        private NavMeshAgent _navMeshAgent;
        private SphereCollider _collider;

        private GameObject _player;
        private Animator _playerAnim;

        private Transform _transform;
        
        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _collider = GetComponent<SphereCollider>();
            _playerAnim = GetComponent<Animator>();
            _player = GameObject.Find("Player"); //TODO: replace
            _playerAnim = _player.GetComponent<Animator>();
            _transform = transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) //TODO: replace 
            {
                _playerInRange = true;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) //TODO: replace 
            {
                _playerInRange = false;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player")) return;
            //TODO: if playing is moving

            var playerPosition = _player.transform.position;
            if (CalculatePathLength(playerPosition) <= _collider.radius)
            {
                _lastingKnownPosition = playerPosition;
            }
        }

        private float CalculatePathLength(Vector3 target)
        {
            // return Vector3.Distance(_transform.position, target);
            var path = new NavMeshPath();

            if (_navMeshAgent.enabled)
            {
                _navMeshAgent.CalculatePath(target, path);
            }
            
            var allPoints = new Vector3[path.corners.Length + 2];
            allPoints[0] = _transform.position;
            allPoints[allPoints.Length - 1] = target;

            for (var i = 0; i < path.corners.Length; i++)
            {
                allPoints[i + 1] = path.corners[i];
            }

            var pathLength = 0f;

            for (var i = 0; i < allPoints.Length-1; i++)
            {
                pathLength += Vector3.Distance(allPoints[i], allPoints[i + 1]);
            }

            return pathLength;
        }
    }
}