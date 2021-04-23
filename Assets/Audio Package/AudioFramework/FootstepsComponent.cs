using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AudioFramework
{
    public class FootstepsComponent : MonoBehaviour
    {
        [SerializeField] private AudioClip[] defaultFootsteps;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Debug.Assert(_audioSource != null);
        }

        private void Step()
        {
            var clip = defaultFootsteps[Random.Range(0, defaultFootsteps.Length)];
            _audioSource.PlayOneShot(clip);
        }

        private void AlertEnemies()
        {
            
        }
    }
}