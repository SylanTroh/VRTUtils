using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AnimationSync : UdonSharpBehaviour
{

    [SerializeField] Animator targetAnimator; // Reference to animator component.
    [UdonSynced] bool isPlaying = false;
    [UdonSynced] float time;
    float sendTime = 0f;
    [UdonSynced] int stateID;

    public override void OnDeserialization(DeserializationResult result)
    {
        sendTime = result.sendTime;
        SyncAnim();
    }

    public void SyncAnim()
    {   
        if (targetAnimator == null) return;
        AnimatorClipInfo[] currentAnims = targetAnimator.GetCurrentAnimatorClipInfo(0);
        if (currentAnims == null) return;
        AnimatorStateInfo stateinfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        if(stateinfo.shortNameHash != stateID)
        {
            targetAnimator.Play(stateID);
            SendCustomEventDelayedFrames(nameof(SyncAnim), 2);
            return;
        }
        AnimationClip currentClip = currentAnims[0].clip;
        if (currentClip == null) return;
        float animationSeconds = currentClip.length;
        int stateHash = targetAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash;
        float time = Time.realtimeSinceStartup;
        Debug.Log("[AnimationSync] SendTime: " + sendTime + ", CurrentTime: " + time + " StateID: " + stateID + " AnimationSeconds: " + animationSeconds);
        targetAnimator.Play(stateID, 0, (time - sendTime) / animationSeconds);
    }

    public void PlayState(int id)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isPlaying = true;
        stateID = id;
        time = Time.realtimeSinceStartup;
        targetAnimator.Play(stateID);
        RequestSerialization();
    }
}
