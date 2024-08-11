using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedAudioFade : UdonSharpBehaviour
    {
        [HideInInspector] public AudioSource audioSource;

        [UdonSynced, FieldChangeCallback(nameof(FadeDurationSync))] float _fadeDurationSync = 1e-5f;
        [UdonSynced, FieldChangeCallback(nameof(Volume))] float _volume = 0f;
        public bool isPlaying = false;

        //The time to start lerping volume from
        float fadeStartTime = 0f;

        //Volume to FadeIn to
        float maxVolume = 0f;
        //Volume at the beginning of a fade
        float startVolume = 0f;
        //Volume at the end of a fade
        float targetVolume = 0f;

        void Start()
        {
            //Initialize Variables
            audioSource = GetComponent<AudioSource>();
            maxVolume = audioSource.volume;
            Volume = audioSource.volume;
        }
        private void OnEnable()
        {
            if (audioSource == null) return;
            if (audioSource.playOnAwake) Volume = maxVolume;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestSerialization));
        }
        void Update()
        {
            //Handle Audio Fading
            if (audioSource.volume != Volume)
            {
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, (Time.time - fadeStartTime) / FadeDurationSync);
            }
        }
        public float FadeDurationSync
        {
            get => _fadeDurationSync;
            set
            {
                _fadeDurationSync = value;
                //Fade must be non-negative
                if (_fadeDurationSync <= 0f) _fadeDurationSync = 1e-5f;
                RequestSerialization();
            }
        }
        public float Volume
        {
            get => _volume;
            set
            {
                fadeStartTime = Time.time;
                startVolume = _volume;
                targetVolume = value;
                _volume = Mathf.Clamp01(value);
                isPlaying = _volume == 0f ? false : true;
                RequestSerialization();
            }
        }

        public void Play()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            FadeDurationSync = 0f;
            Volume = maxVolume;
        }
        public void Stop()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            FadeDurationSync = 0f;
            Volume = 0f;
        }
        public void FadeIn(float fadeDuration)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            FadeDurationSync = fadeDuration;
            Volume = maxVolume;
        }
        public void FadeOut(float fadeDuration)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            FadeDurationSync = fadeDuration;
            Volume = 0f;
        }
    }
}
