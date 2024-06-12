Shader "Unlit/High_SSVEP"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1) // Base Color
        _MinLuminance ("Min Luminance", Range(0, 1)) = 0.2 // Minimum luminance level
        _MaxLuminance ("Max Luminance", Range(0, 1)) = 1.0 // Maximum luminance level
        _Frequency ("Frequency", Float) = 1.0 // Frequency in Hz
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _MinLuminance;
            float _MaxLuminance;
            float _Frequency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the time-based luminance flicker
                float flicker = sin(_Time.y * _Frequency * 2.0 * Mathf.PI);
                float luminance = lerp(_MinLuminance, _MaxLuminance, (flicker + 1.0) / 2.0);
                return _Color * luminance;
            }
            ENDCG
        }
    }
}
