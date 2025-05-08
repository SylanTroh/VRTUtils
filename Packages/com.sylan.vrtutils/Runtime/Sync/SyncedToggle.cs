using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.VRTUtils.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedToggle : UdonSharpBehaviour
    {
        [SerializeField] [UdonSynced] private bool toggle;
        public GameObject[] toggleObjects;

        public override void Interact()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            toggle = !toggle;
            Toggle();
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            Toggle();
        }

        public void Toggle()
        {
            foreach (var toggleObject in toggleObjects)
                if (toggleObject != null)
                    toggleObject.SetActive(toggle);
        }

        public void SendToggle()
        {
            Interact();
        }
    }
}