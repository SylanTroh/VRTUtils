
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace UdonSharp.Examples.Utilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SyncedToggle : UdonSharpBehaviour
    {
        [SerializeField, UdonSynced] bool toggle = false;
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
            foreach (GameObject toggleObject in toggleObjects)
            {
                if (toggleObject != null)
                {
                    toggleObject.SetActive(toggle);
                }
            }
        }

        public void SendToggle()
        {
            Interact();
        }
    }
}
