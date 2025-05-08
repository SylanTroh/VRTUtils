using UdonSharp;
using UnityEngine;

namespace Sylan.VRTUtils.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayAnimationSync : UdonSharpBehaviour
    {
        [SerializeField] private string stateName;
        [SerializeField] private AnimationSync animationSync;
        private int _stateID;

        private void Start()
        {
            _stateID = Animator.StringToHash(stateName);
        }

        public override void Interact()
        {
            animationSync.PlayState(_stateID);
        }
    }
}