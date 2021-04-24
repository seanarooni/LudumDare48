using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

namespace AudioFramework
{
    public class SoundEffectComponent : MonoBehaviour
    {
        [SerializeField] private string soundName;
        [SerializeField] private bool playOnAwake = false;
        [SerializeField] private bool loop = false;
        [SerializeField] private bool cutSelf = false;
        [SerializeField] private int cut = -1;
        [SerializeField] private int cutBy = -1;
        [SerializeField] private AudioClip[] soundClips;
        public float samplePickValue = 0f;

        public float SamplePickValue
        {
            get => samplePickValue;
            set => samplePickValue = Mathf.Clamp01(value);
        }
        
        private AudioSource _audioSource;

        private int roundRobinCurrentIndex;

        public enum SampleModes
        {
            Single,
            SamplePick,
            RoundRobin,
            Random
        }

        public SampleModes currentSampleMode = SampleModes.Random;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            // _audioSource.spatialBlend = 0.0f;
            _audioSource.loop = loop;
            _audioSource.playOnAwake = false;
        }

        private void Start()
        {
            Debug.Assert(soundClips.Length > 0);
            
            var mixer =
                Resources.Load("LD48Mixer") as AudioMixer; //TODO: set path to sound mixer in global settings file.
            Debug.Assert(mixer != null);

            _audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups($"SFX/{soundName}")[0];
            roundRobinCurrentIndex = 0;
            if (playOnAwake)
                Play();
        }
        
#if UNITY_EDITOR

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Play();
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
               _audioSource.Stop();
            }
        }

#endif

        private AudioClip GetClip()
        {
            throw new NotImplementedException();
        }
        
        private int CalculateSamplePickMode()
        {
            var count = soundClips.Length;
            var share = 1f / count; 
            for (var i = 1; i <= count; i++)
            {
                if (samplePickValue <= share * i)
                    return i - 1;
            }

            return 0;
        }
        
        private void Play()
        {
            AudioClip clip;
            
            switch (currentSampleMode)
            {
                case SampleModes.Single:
                    clip = soundClips[0];
                    break;
                case SampleModes.SamplePick:
                    clip = soundClips[CalculateSamplePickMode()];
                    break;
                case SampleModes.RoundRobin:
                    clip = soundClips[roundRobinCurrentIndex];
                    roundRobinCurrentIndex++;
                    if (roundRobinCurrentIndex >= soundClips.Length)
                        roundRobinCurrentIndex = 0;
                    break;
                case SampleModes.Random:
                    clip = soundClips[UnityEngine.Random.Range(0, soundClips.Length)];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (cutSelf)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }
            else
                _audioSource.PlayOneShot(clip);
        }

        private void PlaySingle()
        {
            _audioSource.PlayOneShot(GetClip());
        }

        private void PlayLooping()
        {
            _audioSource.clip = GetClip();
            _audioSource.Play();
        }
    }
}