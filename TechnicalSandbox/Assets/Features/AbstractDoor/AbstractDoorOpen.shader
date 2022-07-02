Shader "Unlit/AbstractDoorOpen"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _OpenAmount ("Open", Range(0.0, 1.0)) = 0
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float _OpenAmount;
            float4 _Color;

             // 2D Random
            float random(in float2 st)
            {
                return frac(sin(dot(st.xy,
                                     float2(12.9898, 78.233)))
                             * 43758.5453123);
            }

            // 2D Noise based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            float noise(in float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                // Smooth Interpolation
                float2 u = smoothstep(0., 1., f);

                // Mix 4 coorners percentages
                return lerp(a, b, u.x) +
                        (c - a) * u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;


                float horizontalNoiseStrength = smoothstep(0.01, 0.75, _OpenAmount) * 0.25;
                float dispFromMid = (i.uv.x - 0.5) * 2.0;
                float distFromMid = abs(dispFromMid);
                float side = floor(dispFromMid);


                float wave = noise(float2(distFromMid * 3, side) + float2(_Time. y,0)) * 0.1;
                float distortHorizontal = noise(float2(side, i.uv.y * 100) + float2(0,_Time.y)) * horizontalNoiseStrength;
                distortHorizontal += wave;
                
                float openAmount = _OpenAmount - (distortHorizontal );


                col *= smoothstep(distFromMid - 0.01, distFromMid, openAmount);


                 float vDisp = noise(float2(distFromMid * 20, side) + float2(_Time. y,0)) * 0.05 * distFromMid;

                col *= step(vDisp, i.uv.y);

//                col *= step(verticalDisplacement, _OpenAmount);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
