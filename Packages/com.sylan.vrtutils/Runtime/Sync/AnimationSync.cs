using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Sylan.VRTUtils.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AnimationSync : UdonSharpBehaviour
    {
        [SerializeField] [Tooltip("The Animator to Sync")]
        private Animator targetAnimator; // Reference to animator component.

        [SerializeField] [Tooltip("Add Delay to Compensate for Lag")]
        private float delay;

        private float _sendTime;
        [UdonSynced] private int _stateID;

        // This Forces Reserialization with a new value each time
        // ReSharper disable once NotAccessedField.Local
        [UdonSynced] private float _syncedSendTime;

        public override void OnDeserialization(DeserializationResult result)
        {
            _sendTime = result.sendTime;
            SyncAnim();
        }

        public void SyncAnim()
        {
            if (targetAnimator == null) return;

            //Account for Delay
            var startTime = Time.realtimeSinceStartup - _sendTime - delay;
            if (startTime < 0f)
            {
                SendCustomEventDelayedSeconds(nameof(PlayDelayed), -startTime);
                return;
            }

            //Play Proper State
            if (targetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != _stateID)
            {
                targetAnimator.Play(_stateID);
                SendCustomEventDelayedFrames(nameof(SyncAnim), 2);
                return;
            }

            //Get Animation Clip
            var currentAnims = targetAnimator.GetCurrentAnimatorClipInfo(0);
            if (currentAnims == null) return;
            if (currentAnims[0].clip == null) return;

            //Calculate Offset
            var animationSeconds = currentAnims[0].clip.length;

            Debug.Log("[AnimationSync] SendTime: " + _sendTime + ", CurrentTime: " + Time.realtimeSinceStartup +
                      " StateID: " + _stateID + " AnimationSeconds: " + animationSeconds);
            targetAnimator.Play(_stateID, 0, startTime / animationSeconds);
        }

        public void PlayState(int id)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            _stateID = id;
            _syncedSendTime = Time.realtimeSinceStartup;

            // Play immediately if no delay, otherwise schedule
            if (delay <= 0f) targetAnimator.Play(_stateID);
            else SendCustomEventDelayedSeconds(nameof(PlayDelayed), delay);

            RequestSerialization();
        }

        public void PlayDelayed()
        {
            targetAnimator.Play(_stateID);
        }
    }
}