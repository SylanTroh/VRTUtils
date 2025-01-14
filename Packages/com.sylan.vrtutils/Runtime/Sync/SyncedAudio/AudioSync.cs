
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AudioSync : UdonSharpBehaviour
    {
        [SerializeField] AudioSource targetAudioSource; // Reference to animator component.
        [UdonSynced] bool isPlaying = false;
        [UdonSynced] float forceSync;
        float sendTime = 0f;

        [SerializeField] private bool loop;
        [SerializeField] private int loopStartSamples;
        [SerializeField] private int loopEndSamples;
        private int loopLengthSamples;

        public override void OnDeserialization(DeserializationResult result)
        {
            sendTime = result.sendTime;

            SyncAudio();
        }

        public void SyncAudio()
        {
            AudioClip clip = targetAudioSource.clip;
            if (clip == null) return;
            float localtime = Time.realtimeSinceStartup;
            Debug.Log("[AudioSync] SendTime: " + sendTime + ", CurrentTime: " + localtime);
            if (!isPlaying)
            {
                Debug.Log("[AudioSync] Not Playing");
                targetAudioSource.Stop();
                return;
            }
            var offset = (localtime - sendTime);
            var timesamples = (int)offset * targetAudioSource.clip.frequency;
            if (loop) while (timesamples >= loopEndSamples) timesamples -= loopLengthSamples;
            if (timesamples > targetAudioSource.clip.samples)
            {
                Debug.Log("[AudioSync] Finished Playing");
                targetAudioSource.Stop();
                return;
            }
            targetAudioSource.Play();
            targetAudioSource.timeSamples = timesamples;
        }

        public void Play()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isPlaying = true;
            forceSync = Time.realtimeSinceStartup;
            targetAudioSource.Play();
            RequestSerialization();
            Debug.Log("[AudioSync] CurrentTime: " + forceSync);
        }

        public void Stop()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isPlaying = false;
            forceSync = Time.realtimeSinceStartup;
            targetAudioSource.Stop();
            RequestSerialization();
            Debug.Log("[AudioSync] CurrentTime: " + forceSync);
        }

        private void Start()
        {
            loopLengthSamples = loopEndSamples - loopStartSamples;
        }

        private void Update()
        {
            if (!loop) return;
            if (targetAudioSource.timeSamples >= loopEndSamples) { targetAudioSource.timeSamples -= loopLengthSamples; }
        }
    }
}