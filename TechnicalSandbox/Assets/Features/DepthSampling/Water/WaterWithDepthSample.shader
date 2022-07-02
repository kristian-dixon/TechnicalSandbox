// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/WaterVolume"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        ZWrite Off
        Pass
        {
        Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float3 camForward : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _MainTex_ST;
            float4 _CameraDepthTexture_ST;
            float4x4 _IP;
            float4x4 _IV;
            float4x4 _IVP;

            fixed4 _ScaledScreenParams;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos((v.vertex));
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = o.vertex.xy;
                o.camForward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
                
             

                //o.screenPos = o.vertex;
                return o;
            }

          
            fixed4 frag (v2f i) : SV_Target
            {
                float2 positionNDC = i.vertex.xy / _ScreenParams.xy;
                float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(positionNDC))).r;



                if(depth / _ProjectionParams.z > 0.99){ 
                clip(-1);
                    return float4(0,0,0,1);
                }

                float3 viewDirUN = _WorldSpaceCameraPos - i.worldPos;
                float dp = dot(viewDirUN, i.camForward); 

                float3 viewDir = viewDirUN / dp;
                float3 worldPos = viewDir * depth + _WorldSpaceCameraPos;

                float dist = saturate(pow(length(worldPos - i.worldPos) / 10., 5));               
    
                return fixed4(0,0.4,1 * (1- dist), 1);///dist);
            }
            ENDCG
        }
    }
}
