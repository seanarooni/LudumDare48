using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

        public GameObject sliderParent;
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


        [SerializeField] private AudioSource sfxSource;
        
        [Header("General Music Sources")]
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource gameMusicSource;
        [SerializeField] private AudioSource menuMusicSource;
        
        [Header("Clip Pools")]
        [SerializeField] private AudioClip[] bubbleClips;
        [SerializeField] private AudioClip[] collisionClips;
        
        private void Awake()
        {
            Debug.Assert(ambienceSource != null);
            Debug.Assert(gameMusicSource != null);
            Debug.Assert(menuMusicSource != null);


            // Debug.Assert(bubbleClips.Length > 0);
            // Debug.Assert(collisionClips.Length > 0);
            
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
            ToggleAudioControls();
            TitleScreen();
        }

        public void QuitGameButton()
        {
            Application.Quit();
        }

        public void RestartGame()
        {
            TitleScreen();
            SceneManager.LoadScene(0);
        }

        public void PlayCollisionSound()
        {
            sfxSource.Play();
        }

        public void TitleScreen()
        {
            StartCoroutine(FadeIn(menuMusicSource));
            StartCoroutine(FadeOut(ambienceSource));
            StartCoroutine(FadeOut(gameMusicSource));
        }

        public void GameMode()
        {
            if (sliderParent.activeInHierarchy)
            {
                ToggleAudioControls();
            }
            
            StartCoroutine(FadeOut(menuMusicSource));
            StartCoroutine(FadeOut(ambienceSource));
            StartCoroutine(FadeIn(gameMusicSource));
        }

        public void StartGameMusic()
        {
            gameMusicSource.Play();
        }

        public void AmbienceMode()
        {
            StartCoroutine(FadeOut(menuMusicSource));
            StartCoroutine(FadeIn(ambienceSource));
            StartCoroutine(FadeOut(gameMusicSource));

        }
        
        
        
        public AudioClip GetBubbleClip()
        {
            return bubbleClips[Random.Range(0, bubbleClips.Length)];
        }
        
        public AudioClip GetCollisionCli()
        {
            return collisionClips[Random.Range(0, collisionClips.Length)];
        }
        
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

        public void ToggleAudioControls()
        {
            sliderParent.SetActive(!sliderParent.activeInHierarchy);
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TitleScreen();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GameMode();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AmbienceMode();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
                PlayCollisionSound();
            
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleAudioControls();
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
