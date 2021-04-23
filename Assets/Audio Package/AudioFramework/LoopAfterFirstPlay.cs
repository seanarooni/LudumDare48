using System.Collections;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AudioFramework
{
    public class LoopAfterFirstPlay : MonoBehaviour
    {
        [SerializeField] private AudioClip loopingClip;
        
        private void Awake()
        {

            StartCoroutine(PlayAudioAfter());
        }

        private IEnumerator PlayAudioAfter()
        {
            var audioSource = GetComponent<AudioSource>();
            Debug.Assert(audioSource != null);
            audioSource.loop = false;
            
            yield return new WaitForSecondsRealtime(audioSource.clip.length);
            
            audioSource.clip = loopingClip;
            audioSource.Play();
            audioSource.loop = true;
        }
    }
}