using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Sylan.VRTUtils.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AudioSync : UdonSharpBehaviour
    {
        [SerializeField] private AudioSource targetAudioSource; // Reference to animator component.

        [SerializeField] [Tooltip("Add Delay to Compensate for Lag")]
        private float delay;

        [SerializeField] private bool loop;
        [SerializeField] private int loopStartSamples;
        [SerializeField] private int loopEndSamples;

        [UdonSynced] private bool _isPlaying;
        private int _loopLengthSamples;

        private float _sendTime;

        // This Forces Reserialization with a new value each time
        // ReSharper disable once NotAccessedField.Local
        [UdonSynced] private float _syncedSendTime;

        private void Start()
        {
            _loopLengthSamples = loopEndSamples - loopStartSamples;
        }

        private void Update()
        {
            if (!loop) return;
            if (targetAudioSource.timeSamples >= loopEndSamples) targetAudioSource.timeSamples -= _loopLengthSamples;
        }

        public override void OnDeserialization(DeserializationResult result)
        {
            _sendTime = result.sendTime;
            SyncAudio();
        }

        private void SyncAudio()
        {
            var clip = targetAudioSource.clip;
            if (clip == null) return;

            Debug.Log("[AudioSync] SendTime: " + _sendTime + ", CurrentTime: " + Time.realtimeSinceStartup);

            //Not Currently Playing
            if (!_isPlaying)
            {
                Debug.Log("[AudioSync] Not Playing");
                targetAudioSource.Stop();
                return;
            }

            //Account for Delay
            var startTime = Time.realtimeSinceStartup - _sendTime - delay;
            if (startTime < 0f)
            {
                SendCustomEventDelayedSeconds(nameof(PlayDelayed), -startTime);
                return;
            }

            var timeSamples = (int)startTime * targetAudioSource.clip.frequency;
            if (loop)
                while (timeSamples >= loopEndSamples)
                    timeSamples -= _loopLengthSamples;
            if (timeSamples > targetAudioSource.clip.samples)
            {
                Debug.Log("[AudioSync] Finished Playing");
                targetAudioSource.Stop();
                return;
            }

            targetAudioSource.Play();
            targetAudioSource.timeSamples = timeSamples;
        }

        public void Play()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            _isPlaying = true;
            _syncedSendTime = Time.realtimeSinceStartup;

            // Play immediately if no delay, otherwise schedule
            if (delay <= 0f) targetAudioSource.Play();
            else SendCustomEventDelayedSeconds(nameof(PlayDelayed), delay);

            RequestSerialization();
            Debug.Log("[AudioSync] CurrentTime: " + Time.realtimeSinceStartup);
        }

        public void Stop()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            _isPlaying = false;
            _syncedSendTime = Time.realtimeSinceStartup;

            // Stop immediately if no delay, otherwise schedule
            if (delay <= 0f) targetAudioSource.Stop();
            else SendCustomEventDelayedSeconds(nameof(StopDelayed), delay);

            RequestSerialization();
            Debug.Log("[AudioSync] CurrentTime: " + Time.realtimeSinceStartup);
        }

        public void PlayDelayed()
        {
            targetAudioSource.Play();
        }

        public void StopDelayed()
        {
            targetAudioSource.Stop();
        }
    }
}