using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;
using VRC.Udon;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SyncedAudioToggle : UdonSharpBehaviour
    {
        [SerializeField] SyncedAudioFade syncedAudioFade;
        SyncedAudioTime syncedAudioTime;
        bool syncTime = false;

        [Header("Fade Settings. Set to 0 for instant toggle.")]
        [SerializeField] float fadeInDuration = 0f;
        [SerializeField] float fadeOutDuration = 0f;
        [Header("Reset Audio To Beginning")]
        [SerializeField] bool reset = false;

        private void Start()
        {
            syncedAudioTime = syncedAudioFade.gameObject.GetComponent<SyncedAudioTime>();
            syncTime = (syncedAudioTime != null);
        }
        public override void Interact()
        {
            ToggleAudio();
        }
        public void ToggleAudio()
        {
            //Fade Out
            if (syncedAudioFade.isPlaying)
            {
                syncedAudioFade.FadeOut(fadeOutDuration);
                return;
            }

            //Fade In
            syncedAudioFade.FadeIn(fadeInDuration);
            if (reset && syncTime)
            {
                syncedAudioTime.SendCustomNetworkEvent(NetworkEventTarget.All, nameof(syncedAudioTime.ResetTime));
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(syncedAudioTime.SyncTime));
            }

        }
    }
}