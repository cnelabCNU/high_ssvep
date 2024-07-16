Shader "Custom/StereoExample"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                int eyeIndex = unity_StereoEyeIndex;
                half4 color = tex2D(_MainTex, i.uv);

                // Example: Apply different colors for left and right eyes
                if (eyeIndex == 0)
                {
                    // Left eye specific processing
                    color.r += 0.5;
                }
                else if (eyeIndex == 1)
                {
                    // Right eye specific processing
                    color.g += 0.5;
                }

                // Pass eyeIndex value to a C# script
                // For example, you can set a static variable in your script
                MyScript.eyeIndex = eyeIndex;

                return color;
            }
            ENDCG
        }
    }
}