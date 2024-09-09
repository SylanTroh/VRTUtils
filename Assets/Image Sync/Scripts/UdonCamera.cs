
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class UdonCamera : UdonSharpBehaviour
    {
        int _resolution;
        public const float PHOTO_LOCKOUT = 5;

        Animator animator;
        int TakePictureAnimationID;
        int ClickAnimationID;

        [SerializeField] UdonPolaroid polaroid;
        [SerializeField] Camera renderCamera;
        [SerializeField] CustomRenderTexture customRenderTexture;

        float lastPhotoTime = -PHOTO_LOCKOUT;
        public void Start()
        {
            //Initialize Animator
            animator = GetComponent<Animator>();
            TakePictureAnimationID = Animator.StringToHash("TakePicture");
            ClickAnimationID = Animator.StringToHash("Click");

            //Set Image Resolution
            _resolution = customRenderTexture.width;

            //Disable Camera
            renderCamera.enabled = false;
        }

        public byte ClampColor(int color)
        {
            return (byte)(Math.Min(Math.Max(color, 0), 255));
        }
        public byte ClampColor(float color)
        {
            return ClampColor((int) Mathf.Round(color));
        }
        public void UpdateCustomRenderTexture()
        {
            customRenderTexture.Initialize();
            customRenderTexture.Update();
        }
        public void RequestGPUReadback()
        {
            VRCAsyncGPUReadback.Request(customRenderTexture, 0, (IUdonEventReceiver)this);
        }
        public override void OnPickupUseDown()
        {
            if (Time.time - lastPhotoTime < 2f) return;
            if (Networking.IsClogged || Time.time - lastPhotoTime < PHOTO_LOCKOUT)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Click));
                return;
            }

            //Take Picture
            lastPhotoTime = Time.time;
            renderCamera.Render();
            SendCustomEventDelayedFrames(nameof(UpdateCustomRenderTexture), 1);
            SendCustomEventDelayedFrames(nameof(RequestGPUReadback), 2);
            //Play Sound
            //TakePicture();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TakePicture));
        }
        public void TakePicture()
        {
            animator.Play(TakePictureAnimationID);
        }
        public void Click()
        {
            animator.Play(ClickAnimationID);
        }
        public override void OnAsyncGpuReadbackComplete(VRCAsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error!");
                return;
            }
            else
            {
                var px = new Color32[_resolution * _resolution];
                Debug.Log("[UdonCamera] GPU readback success: " + request.TryGetData(px));

                polaroid.SetImage(ref px, _resolution);
            }
        }
    }
}