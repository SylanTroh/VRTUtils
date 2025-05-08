using UdonSharp;
using UnityEngine;

namespace Sylan.VRTUtils.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayAudioSync : UdonSharpBehaviour
    {
        [SerializeField] private AudioSync audioSync;

        public override void Interact()
        {
            audioSync.Play();
        }
    }
}