Shader "Unlit/TimePortalClockface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Progress;

            #define PI 3.14159


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }


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


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                float2 cUV = float2(0.5,0.5) - i.uv;

                float2 rUV = float2(length(cUV), (atan2(-cUV.y,cUV.x) + PI) / (2 * PI));
                float2 rUV2 = float2(length(cUV), (atan2(cUV.y,-cUV.x) + PI) / (2 * PI));

                float d1 = noise(rUV * 10 + float2(_Time.y, _Time.y)) * 2.0 - 1.0;
                float d2 = noise(rUV2 * 10 + float2(_Time.y, _Time.y)) * 2.0 - 1.0;

                float mask = pow(abs(atan2(-cUV.y,cUV.x)) / PI, 5);

                float distortion = d1;// + d2;//lerp(d2, d1, mask) ; //smoothstep( lerp(d2,d1, rUV.y);
                d1 *= 1.0 - mask;
                d2 *= mask;
                distortion = d1 + d2;//lerp(d2, d1, mask) ; //smoothstep( lerp(d2,d1, rUV.y);

                distortion *= pow(smoothstep(0.7,0.99, _Progress), 10) * 0.5 + 0.01;


                fixed4 col = tex2D(_MainTex, i.uv + normalize(cUV) * distortion ) ;

                
                float r = step(col.r, _Progress) * step(0.001,col.r);
                float b = step(col.g, _Progress) * col.b;
                
                //return float4( , 0, 0,1);
                return max(r,b);

            }
            ENDCG
        }
    }
}
