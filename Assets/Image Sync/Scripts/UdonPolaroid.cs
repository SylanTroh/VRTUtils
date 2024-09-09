using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.VRTUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

    public class UdonPolaroid : UdonSharpBehaviour
    {
        Animator animator;
        int TakePictureAnimationID;

        Texture2D texture;
        Material material;

        [UdonSynced] int resolution;
        [UdonSynced] public byte[] imageDataRG;
        [UdonSynced] public byte[] imageDataB;

        public void Start()
        {
            //Initialize Animator
            animator = GetComponent<Animator>();
            TakePictureAnimationID = Animator.StringToHash("TakePicture");

            material = GetComponent<MeshRenderer>().material;
        }

        private void InitImage()
        {
            //Create and Initialize Image Texture
            texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
        }
        public void SetImage(ref Color32[] px, int resolution)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            this.resolution = resolution;

            //Quantize Image
            Color32ArrayToBytes(ref px, ref imageDataRG, ref imageDataB);
            SendCustomEventDelayedFrames(nameof(ShowImage), 1);

            RequestSerialization();
        }

        public void ShowImage()
        {
            InitImage();
            var px = BytesToColor32Array(ref imageDataRG, ref imageDataB);
            SetMaterial(px);
            TakePicture();
        }

        void SetMaterial(Color32[] px)
        {
            texture.SetPixels32(px);
            texture.Apply();
            material.SetTexture("_MainTex", texture);
        }

        public override void OnDeserialization()
        {
            ShowImage();
        }

        public void Color32ArrayToBytes(ref Color32[] colorArray, ref byte[] imageDataRG, ref byte[] imageDataB)
        {
            // Step 1: Quantize colors and remove alpha
            imageDataRG = new byte[colorArray.Length];
            imageDataB = new byte[(int)Mathf.Ceil(colorArray.Length / 2.0f)];

            for (int i = 0; i < imageDataRG.Length; i++)
            {
                // Quantize to 4 bits per channel (0-15)
                byte r = (byte)((colorArray[i].r) >> 4);
                byte g = (byte)((colorArray[i].g) >> 4);

                // Pack two 4-bit values into each byte
                imageDataRG[i] = (byte)((r << 4) | g);
            }

            for (int i = 0; i < imageDataB.Length; i++)
            {
                var colorIndex = 2 * i;
                // Quantize to 4 bits per channel (0-15)
                byte b1 = (byte)((colorArray[colorIndex].b) >> 4);
                byte b2 = (byte)((colorArray[colorIndex + 1].b) >> 4);

                // Pack two 4-bit values into each byte
                imageDataB[i] = (byte)((b1 << 4) | b2);
            }
        }

        public Color32[] BytesToColor32Array(ref byte[] imageDataRG, ref byte[] imageDataB)
        {
            // Step 2: Convert quantized byte array back to Color32 array
            Color32[] colorArray = new Color32[resolution * resolution];
            for (int i = 0; i < colorArray.Length; i++)
            {
                int blueIndex = i / 2;
                bool blueHalf = (i % 2) == 1;

                byte packedRG = imageDataRG[i];
                byte packedB = imageDataB[blueIndex];

                // Unpack and dequantize
                byte r = (byte)((packedRG >> 4) * 17); // * 17 to spread 0-15 to 0-255
                byte g = (byte)((packedRG & 0x0F) * 17);
                byte b;
                if (blueHalf) b = (byte)((packedB & 0x0F) * 17);
                else b = (byte)((packedB >> 4) * 17);

                colorArray[i] = new Color32(r, g, b, 255); // Alpha is always 255
            }

            return colorArray;
        }
        public void TakePicture()
        {
            animator.Play(TakePictureAnimationID);
        }
    }
}