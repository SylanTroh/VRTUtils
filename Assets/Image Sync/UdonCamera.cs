
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
        public const int POLAROID_RESOLOTION = 128;

        RenderTexture renderTexture;
        [SerializeField] Texture2D blueNoise;
        Color32[] blueNoiseArray;

        [SerializeField] UdonPolaroid polaroid;
        [SerializeField] Camera renderCamera;
        public void Start()
        {
            renderTexture = new RenderTexture(UdonCamera.POLAROID_RESOLOTION, UdonCamera.POLAROID_RESOLOTION, 0, RenderTextureFormat.Default, 0);
            renderTexture.filterMode = FilterMode.Point;
            renderCamera.targetTexture = renderTexture;
            blueNoiseArray = blueNoise.GetPixels32();
        }
        private void RequestReadback()
        {
            VRCAsyncGPUReadback.Request(renderTexture, 0, (IUdonEventReceiver)this);
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
                var px = new Color32[UdonCamera.POLAROID_RESOLOTION * UdonCamera.POLAROID_RESOLOTION];
                Debug.Log("GPU readback success: " + request.TryGetData(px));
                Debug.Log("GPU readback data: " + px[0]);

                //Dither
                for(int i = 1; i < px.Length; i++)
                {
                    px[i] = new Color32(
                        ClampColor(px[i].r - 7 + blueNoiseArray[i].r / 16),
                        ClampColor(px[i].g - 7 + blueNoiseArray[i].g / 16),
                        ClampColor(px[i].b - 7 + blueNoiseArray[i].b / 16),
                        0);
                }
                
                polaroid.SetPicture(ref px);
            }
        }
        public byte ClampColor(int color)
        {
            return (byte)(Math.Min(Math.Max(color, 0), 255));
        }

        public override void OnPickupUseDown()
        {
            if (Networking.IsClogged) return;
            RequestReadback();
        }
    }
}