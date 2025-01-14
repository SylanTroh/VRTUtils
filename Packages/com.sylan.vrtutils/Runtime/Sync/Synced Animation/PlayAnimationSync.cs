
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayAnimationSync : UdonSharpBehaviour
    {
        [SerializeField] string stateName;
        int stateID;
        [SerializeField] AnimationSync animationSync;

        private void Start()
        {
            stateID = Animator.StringToHash(stateName);
        }
        public override void Interact()
        {
            animationSync.PlayState(stateID);
        }
    }
}