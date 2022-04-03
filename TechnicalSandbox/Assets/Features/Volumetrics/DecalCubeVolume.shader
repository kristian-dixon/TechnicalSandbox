// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DecalCubeVolume"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off
        Pass
        {
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
                float3 viewDir : NORMAL;
                float2 uv : TEXCOORD0;

                float4 worldPos : TEXCOORD1;
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
                o.uv = (o.vertex.xy / _ScreenParams.xy);
                o.viewDir = normalize(WorldSpaceViewDir( v.vertex));
                o.worldPos =  v.vertex;
                return o;
            }

            float4 ComputeClipSpacePosition(float2 positionNDC, float deviceDepth)
            {
                float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);

            #if UNITY_UV_STARTS_AT_TOP
                // Our world space, view space, screen space and NDC space are Y-up.
                // Our clip space is flipped upside-down due to poor legacy Unity design.
                // The flip is baked into the projection matrix, so we only have to flip
                // manually when going from CS to NDC and back.
                positionCS.y = -positionCS.y;
            #endif

                return positionCS;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 positionNDC = i.vertex.xy / _ScreenParams.xy;
                float depth01 = (tex2D(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(positionNDC))).r;

                float4 posCS = ComputeClipSpacePosition(positionNDC, depth01);

                //float4 hposWS = mul(_IP, posCS);
                //hposWS = mul(UNITY_MATRIX_I_V, hposWS);
                float4 hposWS = mul(_IVP, posCS);
                //float4 ws = mul(UNITY_MATRIX_I_V, float4(hposWS.xyz, hposWS.w));

                float3 ws = hposWS.xyz / hposWS.w; 

               // ws.g = 0;
               // ws = hposWS;
                //float3 viewDir = normalize(WorldSpaceViewDir(i.worldPos));
                //float3 worldPos = _WorldSpaceCameraPos - (viewDir * depth) ;

                //fixed4 col = fixed4(abs(ceil(worldPos) * 0.1),1);
                fixed4 col = fixed4(frac(ws.xyz), 1);
                //col.r = col.r % col.b;
                //col.gb = (0.0).rr;
                col.gb = float2(0,0);
                #if UNITY_REVERSED_Z
                    if(depth01 < 0.0001) return half4(0,0,0,1);
                #else
                    if(depth01 > 0.9999) return half4(0,0,0,1);
                #endif
                // sample the texture
                //col.rg = i.vertex.xy / i.uv;
                //col.rg = positionNDeviceCoords;
                
                return col;
            }
            ENDCG
        }
    }
}
