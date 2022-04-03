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
            float4x4 _IP;
            float4x4 _IV;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (o.vertex.xy / _ScreenParams.xy);
                o.viewDir = normalize(WorldSpaceViewDir( v.vertex));
                o.worldPos =  v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 positionNDeviceCoords = i.vertex.xy / _ScreenParams.xy;



                float depth = DECODE_EYEDEPTH(tex2Dlod(_CameraDepthTexture, float4(positionNDeviceCoords.xy, 0, 0)));
                float depth01 = Linear01Depth(tex2Dlod(_CameraDepthTexture, float4(positionNDeviceCoords.xy, 0, 0)));


                float3 viewDir = normalize(WorldSpaceViewDir(i.worldPos));

                float3 worldPos = _WorldSpaceCameraPos - (viewDir * depth) ;

                fixed4 col = fixed4(abs(ceil(worldPos) * 0.1),1);

                col.r = col.r % col.b;
                col.gb = (0.0).rr;

                if(depth01 > 0.9999){
                    col = float4(0,0,0,1);
                }
                // sample the texture
                //col.rg = i.vertex.xy / i.uv;
                //col.rg = positionNDeviceCoords;
                
                return col;
            }
            ENDCG
        }
    }
}
