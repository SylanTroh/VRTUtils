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
        int FadeInAnimationID;
        public Texture2D texture;
        public Material material;

        [UdonSynced] public string imageData;

        public void Start()
        {
            animator = GetComponent<Animator>();
            FadeInAnimationID = Animator.StringToHash("PhotoFadeIn");
            texture = new Texture2D(UdonCamera.POLAROID_RESOLOTION, UdonCamera.POLAROID_RESOLOTION, TextureFormat.ARGB32,false);
            texture.filterMode = FilterMode.Point;
            material = GetComponent<MeshRenderer>().material;
            material.SetTexture("_MainTex", texture);
        }
        public void FadePhotoIn()
        {
            animator.Play(FadeInAnimationID);
        }

        public void SetPicture(ref Color32[] px)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            imageData = ConvertColor32ArrayToBinaryString(ref px);
            RequestSerialization();
            px = ConvertBinaryStringToColor32Array(imageData);
            texture.SetPixels32(px);
            texture.Apply();
            material.SetTexture("_MainTex", texture);
        }

        public override void OnDeserialization()
        {
            var px = ConvertBinaryStringToColor32Array(imageData);
            texture.SetPixels32(px);
            texture.Apply();
            material.SetTexture("_MainTex", texture);
        }

        public string ConvertColor32ArrayToBinaryString(ref Color32[] colorArray)
        {
            // Step 1: Quantize colors and remove alpha
            byte[] quantizedArray = new byte[colorArray.Length * 2]; // 2 bytes per Color32 (no alpha)
            for (int i = 0; i < colorArray.Length; i++)
            {
                int byteIndex = i * 2;
                // Quantize to 4 bits per channel (0-15)
                byte r = (byte)((colorArray[i].r) >> 4);
                byte g = (byte)((colorArray[i].g) >> 4);
                byte b = (byte)((colorArray[i].b) >> 4);

                // Pack two 4-bit values into each byte
                quantizedArray[byteIndex] = (byte)((r << 4) | g);
                quantizedArray[byteIndex + 1] = (byte)(b << 4);

            }

            // Step 2: Convert byte array to Base64 string
            string binaryString = Convert.ToBase64String(quantizedArray);

            return binaryString;
        }

        public Color32[] ConvertBinaryStringToColor32Array(string binaryString)
        {
            // Step 1: Convert Base64 string back to byte array
            byte[] quantizedArray = Convert.FromBase64String(binaryString);

            // Step 2: Convert quantized byte array back to Color32 array
            Color32[] colorArray = new Color32[UdonCamera.POLAROID_RESOLOTION * UdonCamera.POLAROID_RESOLOTION];
            for (int i = 0; i < colorArray.Length; i++)
            {
                int byteIndex = i * 2;
                byte packedRG = quantizedArray[byteIndex];
                byte packedB = quantizedArray[byteIndex + 1];

                // Unpack and dequantize
                byte r = (byte)((packedRG >> 4) * 17); // * 17 to spread 0-15 to 0-255
                byte g = (byte)((packedRG & 0x0F) * 17);
                byte b = (byte)((packedB >> 4) * 17);

                colorArray[i] = new Color32(r, g, b, 255); // Alpha is always 255
            }

            return colorArray;
        }
    }
}