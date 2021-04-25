// using UnityEngine;
// using UnityEngine.Audio;
//
// // ReSharper disable once CheckNamespace
// namespace AudioFramework
// {
//     public class EnemySoundComponent : MonoBehaviour
//     {
//         private AudioSource _chatterSource;
//         private AudioSource _alertSource;
//         private AudioSource _typingSource;
//         
//         private void Awake()
//         {
//             _chatterSource = gameObject.AddComponent<AudioSource>();
//             _alertSource = gameObject.AddComponent<AudioSource>();
//             _typingSource = gameObject.AddComponent<AudioSource>();
//             
//             // _chatterSource.spatialBlend = 1f;
//             // _alertSource.spatialBlend = 1f;
//             // _typingSource.spatialBlend = 1f;
//
//             _chatterSource.loop = false;
//             _alertSource.loop = false;
//             _typingSource.loop = true;
//
//             _chatterSource.playOnAwake = false;
//             _alertSource.playOnAwake = false;
//             _typingSource.playOnAwake = false;
//         }
//
//         private void Start()
//         {
//             var mixer = Resources.Load("SpaceOfficeMixer") as AudioMixer;
//             Debug.Assert(mixer != null);
//             
//             _chatterSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX/Chatter")[0];
//             _alertSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX/Chatter")[0];
//             _typingSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX/Typing")[0];
//             
//             Debug.Assert(_chatterSource.outputAudioMixerGroup != null);
//             Debug.Assert(_alertSource.outputAudioMixerGroup != null);
//             Debug.Assert(_typingSource.outputAudioMixerGroup != null);
//             
//             PlayTypingSound();
//         }
//         
// #if UNITY_EDITOR
//
//         private void Update()
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha5))
//             {
//                 PlayChatterSound();
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha6))
//             {
//                 PlayAlertSound();
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha7))
//             {
//                 PlayTypingSound();
//             }
//             if (Input.GetKeyDown(KeyCode.Alpha8))
//             {
//                 StopTypingSound();
//             }
//         }
//
// #endif
//         
//         public void PlayChatterSound()
//         {
//             _chatterSource.PlayOneShot(AudioManager.Instance.GetChatterClip());
//         }
//
//         public void PlayAlertSound()
//         {
//             _chatterSource.PlayOneShot(AudioManager.Instance.GetAlertClip());
//         }
//
//         public void PlayTypingSound()
//         {
//             _typingSource.clip = AudioManager.Instance.GetTypingClip();
//             _typingSource.Play();
//         }
//
//         public void StopTypingSound()
//         {
//             _typingSource.Stop();
//         }
//     }
// }