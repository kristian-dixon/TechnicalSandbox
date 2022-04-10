// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

Shader "Unlit/MeteorEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVXScale ("UV X Scaling", Float) = 1
        _UVYScale ("UV Y Scaling", Float) = 1

        [HDR] _PrimaryColour("PrimaryColour", Color) = (1, .25, .25, 1)
        [HDR] _SecondaryColour("SecondaryColour", Color) = (1, .5, .5, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        //Cull Off

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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 viewDir : NORMAL1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UVXScale;
            float _UVYScale;
            float4 _PrimaryColour;
            float4 _SecondaryColour;

            // float3 _WorldSpaceCameraPos;

            static const float PI = 3.14159265f;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                o.normal = normalize( mul(unity_ObjectToWorld, float4(v.normal, 0)) );
                o.viewDir = normalize( mul(unity_ObjectToWorld, v.vertex) - _WorldSpaceCameraPos);


                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centerOffset = float2(0.5,0.5) - i.uv;
                float2 uv = float2((atan2(centerOffset.y, centerOffset.x) / PI) * 0.5 + 0.5 , length(centerOffset));

                float2 oguv = uv;

                uv *= float2(_UVXScale, _UVYScale);

                uv.x += _Time.y * 0.1;
                uv.y -= _Time.y * .2;

                uv = abs( frac(uv) * 2.0 - 1.0 );

                // sample the texture
                fixed4 noiseSample = tex2D(_MainTex, uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                //col.xy = (uv.rg);
                clip(noiseSample.r - oguv.y);
                
                float rimStrength = 1 - abs(dot(i.viewDir, i.normal));

                fixed4 rimColour = lerp(_PrimaryColour, _SecondaryColour, rimStrength);

                fixed4 col = rimColour;//fixed4(rimStrength.rrr,1);
                return col;
            }
            ENDCG
        }
    }
}
