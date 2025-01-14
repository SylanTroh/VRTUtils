
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.VRTUtils
{    
    public class StopAudioSync : UdonSharpBehaviour
    {
        [SerializeField] AudioSync audioSync;

        public override void Interact()
        {
            audioSync.Stop();
        }
    }
}
