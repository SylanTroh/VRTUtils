
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayAudioSync : UdonSharpBehaviour
{
    [SerializeField] AudioSync audioSync;

    public override void Interact()
    {
        audioSync.Play();
    }
}
