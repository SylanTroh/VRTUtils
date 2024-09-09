Shader "Sylan/Dithering"
{
    Properties
    {
        _MainTex ("InputTex", 2D) = "white" {}
        _BlueNoise ("BlueNoise", 2D) = "white" {}
    }

     SubShader
     {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            UNITY_DECLARE_TEX2D(_MainTex);
            UNITY_DECLARE_TEX2D(_BlueNoise);
            half4 _BlueNoise_TexelSize;

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float ratio = _CustomRenderTextureWidth * _BlueNoise_TexelSize.x ;
                return UNITY_SAMPLE_TEX2D(_MainTex, IN.localTexcoord.xy) + ((UNITY_SAMPLE_TEX2D(_BlueNoise, IN.localTexcoord.xy * ratio) - 0.5) / 16) ;
            }
            ENDCG
        }
    }
}