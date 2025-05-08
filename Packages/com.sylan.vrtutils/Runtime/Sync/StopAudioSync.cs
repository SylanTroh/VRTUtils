using UdonSharp;
using UnityEngine;

namespace Sylan.VRTUtils.Sync
{
    public class StopAudioSync : UdonSharpBehaviour
    {
        [SerializeField] private AudioSync audioSync;

        public override void Interact()
        {
            audioSync.Stop();
        }
    }
}