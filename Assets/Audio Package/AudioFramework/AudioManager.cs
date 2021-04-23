using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum SuspicionLevels
{
    None,
    Low,
    High
}

public enum Songs
{
    Menu,
    Death,
    Level,
    Radio
}

namespace AudioFramework
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
    
        public AudioMixer currentMixer;
    
        public Slider soundEffectsVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider masterVolumeSlider;
        
        [Header("Default Audio Settings")] 
        [SerializeField] private float defaultVolume = 1f;
        [SerializeField] private float defaultMusicVolume = 1f;
        // [SerializeField] private float crossFadeSpeed = 1f;
        [SerializeField] private float fadeInSpeed = 1f;
        [SerializeField] private float fadeOutSpeed = 1f;

        private float _currentSfxVolume;
        private float _currentMusicVolume;
        private float _currentMasterVolume;
        private bool _isFading;
        private Coroutine _activeCoroutine;
        
        [Header("General Music Sources")]
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource heightenedAwarenessSource;
        [SerializeField] private AudioSource themeMusicSource;
        [SerializeField] private AudioSource deathMusicSource;

        [Header("Sneaking Theme")]
        [SerializeField] private AudioSource melodySource;
        [SerializeField] private AudioSource chordsSource;
        [SerializeField] private AudioSource stringsSource;
        [SerializeField] private AudioSource alarmSource;

        [Header("Clip Pools")]
        [SerializeField] private AudioClip[] chatterClips;
        [SerializeField] private AudioClip[] alertClips;
        [SerializeField] private AudioClip[] typingClips;
        [SerializeField] private AudioClip[] footstepClips;
        
        private void Awake()
        {
            Debug.Assert(ambienceSource != null);
            Debug.Assert(heightenedAwarenessSource != null);
            Debug.Assert(themeMusicSource != null);
            Debug.Assert(deathMusicSource != null);
            
            Debug.Assert(melodySource != null);
            Debug.Assert(chordsSource != null);
            Debug.Assert(stringsSource != null);
            Debug.Assert(alarmSource != null);
            
            Debug.Assert(chatterClips.Length > 0);
            Debug.Assert(alertClips.Length > 0);
            Debug.Assert(typingClips.Length > 0);
            Debug.Assert(footstepClips.Length > 0);

            //B-B-B-Bonus Feature!: ensure there's only 1 event system object in the scene.
            var eventSystems = FindObjectsOfType<EventSystem>();
            if (eventSystems.Length > 1)
            {
                Debug.LogWarning("multiple event systems found in scene.");
                foreach (var eventSystem in eventSystems)
                {
                    Debug.Log("event system: ", eventSystem);
                }
            }

            //Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.LogWarning("multiple audio managers in scene, destroying", this);
                Destroy(this);
                return;
            }

            _currentSfxVolume = defaultVolume;
            _currentMusicVolume = defaultMusicVolume;
            _currentMasterVolume = defaultVolume;
        
            soundEffectsVolumeSlider.value = _currentSfxVolume;
            musicVolumeSlider.value = _currentMusicVolume;
            masterVolumeSlider.value = _currentMasterVolume;
        }

        private void Start()
        {
            SetSuspicionLevel(SuspicionLevels.None);
        }
        
        public void SetSuspicionLevel(SuspicionLevels level)
        {
            switch (level)
            {
                case SuspicionLevels.None:
                    //play melody & chords,
                    //mute strings & alarm
                    StartCoroutine(FadeIn(melodySource));
                    StartCoroutine(FadeIn(chordsSource));
                    StartCoroutine(FadeOut(stringsSource));
                    StartCoroutine(FadeOut(alarmSource));
                    break;
                case SuspicionLevels.Low:
                    //play melody chords and strings
                    //mute alarm
                    StartCoroutine(FadeIn(melodySource));
                    StartCoroutine(FadeIn(chordsSource));
                    StartCoroutine(FadeIn(stringsSource));
                    StartCoroutine(FadeOut(alarmSource));
                    break;
                case SuspicionLevels.High:
                    //play chords strings and alarm
                    //mute melody
                    StartCoroutine(FadeOut(melodySource));
                    StartCoroutine(FadeIn(chordsSource));
                    StartCoroutine(FadeIn(stringsSource));
                    StartCoroutine(FadeIn(alarmSource));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
        
        public AudioClip GetChatterClip()
        {
            return chatterClips[Random.Range(0, chatterClips.Length)];
        }
        
        public AudioClip GetAlertClip()
        {
            return alertClips[Random.Range(0, alertClips.Length)];
        }
        
        public AudioClip GetFootstepsClip()
        {
            return footstepClips[Random.Range(0, footstepClips.Length)];
        }
        
        public AudioClip GetTypingClip()
        {
            return typingClips[Random.Range(0, typingClips.Length)];
        }
        
        // public void EnterHeightenedAwarenessState()
        // {
        //     if (_isFading)
        //         StopCoroutine(_activeCoroutine);
        //     
        //     //swap ambience music clip to 
        //     _activeCoroutine = StartCoroutine(CrossFade(ambienceSource, heightenedAwarenessSource));
        // }
        //
        // public void EndEnemyAwarenessHeightenedState()
        // {
        //     if (_isFading)
        //         StopCoroutine(_activeCoroutine);
        //
        //     _activeCoroutine = StartCoroutine(CrossFade(heightenedAwarenessSource, ambienceSource));
        // }

        public void OnSFXVolumeAdjust(float value)
        {
            // soundEffectsVolumeSlider.value = value;
            currentMixer.SetFloat("sfxVol", Mathf.Log(value) * 20);
        }
    
        public void OnMusicVolumeAdjust(float value)
        {
            // musicVolumeSlider.value = value;
            currentMixer.SetFloat("musicVol", Mathf.Log(value) * 20);
        }
    
        public void OnMasterVolumeAdjust(float value)
        {
            // masterVolumeSlider.value = value;
            currentMixer.SetFloat("masterVol", Mathf.Log(value) * 20);
        }
#if UNITY_EDITOR
        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.F))
            // {
            //     // StartCoroutine(CrossFade(ambienceSource, heightenedAwarenessSource));
            //     EnterHeightenedAwarenessState();
            // }
            //
            // if (Input.GetKeyDown(KeyCode.G))
            // {
            //     // StartCoroutine(CrossFade(heightenedAwarenessSource, ambienceSource));
            //     EndEnemyAwarenessHeightenedState();
            // }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetSuspicionLevel(SuspicionLevels.None);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetSuspicionLevel(SuspicionLevels.Low);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetSuspicionLevel(SuspicionLevels.High);
            }
        }
#endif

        private IEnumerator FadeOut(AudioSource fade)
        {
            while (true)
            {
                if (fade.volume >= 0f)
                    fade.volume -= fadeOutSpeed * Time.deltaTime;

                if (fade.volume <= 0f)
                    break;

                yield return null;
            }
        }

        private IEnumerator FadeIn(AudioSource fade)
        {
            while (true)
            {
                if (fade.volume < 1)
                    fade.volume += fadeInSpeed * Time.deltaTime;

                if (fade.volume >= 1f)
                    break;

                yield return null;
            }
        }

        // private IEnumerator CrossFade(AudioSource fadeFrom, AudioSource fadeTo)
        // {
        //     _isFading = true;
        //     fadeTo.volume = 0f;
        //     fadeTo.Play();
        //     fadeTo.volume = 0f;
        //
        //     yield return null;
        //
        //     while (true)
        //     {
        //         var check = 0;
        //         if (fadeTo.volume < 1)
        //         {
        //             fadeTo.volume += crossFadeSpeed * Time.deltaTime;
        //         }
        //         else
        //         {
        //             check++;
        //         }
        //
        //         if (fadeFrom.volume > 0f)
        //         {
        //             fadeFrom.volume -= crossFadeSpeed * Time.deltaTime;
        //         }
        //         else
        //         {
        //             check++;
        //         }
        //
        //         if (check == 2)
        //             break;
        //         
        //         yield return null;
        //     }
        //
        //     _isFading = false;
        //     // Debug.Log("fade complete");
        // }
    }
}
