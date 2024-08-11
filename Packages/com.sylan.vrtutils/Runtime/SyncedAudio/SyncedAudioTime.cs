using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedAudioTime : UdonSharpBehaviour
    {
        AudioSource audioSource;

        [UdonSynced] float time = 0f;

        void Start()
        {
            //Initialize Variables
            audioSource = GetComponent<AudioSource>();
        }
        private void OnEnable()
        {
            if (audioSource == null) return;
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(SyncTime));
            SyncTime();
        }
        public override void OnPreSerialization()
        {
            time = audioSource.time;
        }
        public override void OnDeserialization()
        {
            //Desync Buffer
            if(Mathf.Abs(audioSource.time - time) > 1f)
            {
                audioSource.time = time;
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RequestSerialization();
        }
        public void SyncTime()
        {
            RequestSerialization();
        }
        public void ResetTime()
        {
            if (audioSource == null) return;
            audioSource.time = 0f;
        }
    }
}
