Shader "Unlit/MultipassFur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FurStrength  ("Float", float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);// * 0.1;

                half3 worldNormal = normalize(i.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                half4 diff = nl * _LightColor0;

                
                diff.rgb += ShadeSH9(half4(worldNormal,1));
                col *= diff;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        cull off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        //Fur
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
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

            struct v2g
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float depth : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _FurStrength;

            v2g vert (appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            [maxvertexcount(60)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;
                
                float y = 0;
                for(int j = 0; j < 20; j++)
                {
                    //y += 0.025f; 
                    y += _FurStrength;
                    for(int i = 0; i < 3; i++)
                    {
                        o.vertex = mul(UNITY_MATRIX_VP,  mul(unity_ObjectToWorld, IN[i].vertex) + float4(IN[i].normal,0) * y);
                        UNITY_TRANSFER_FOG(o,o.vertex);
                        o.uv = TRANSFORM_TEX(IN[i].uv, _MainTex);
                        o.depth = j;
                        triStream.Append(o);
                    }
                    triStream.RestartStrip();
                } 
            }

            #define vec2 float2
            #define fract frac
            #define mix lerp

            //Perlin Noise function pulled from book of shaders :)
            // 2D Random
            float random (in vec2 st) {
                return fract(sin(dot(st.xy,
                vec2(12.9898,78.233)))
                * 43758.5453123);
            }

            // 2D Noise based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            float noise (in vec2 st) {
                vec2 i = floor(st);
                vec2 f = fract(st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + vec2(1.0, 0.0));
                float c = random(i + vec2(0.0, 1.0));
                float d = random(i + vec2(1.0, 1.0));

                // Smooth Interpolation

                // Cubic Hermine Curve.  Same as SmoothStep()
                vec2 u = f*f*(3.0-2.0*f);
                // u = smoothstep(0.,1.,f);

                // Mix 4 coorners percentages
                return mix(a, b, u.x) +
                (c - a)* u.y * (1.0 - u.x) +
                (d - b) * u.x * u.y;
            }





            fixed4 frag (g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float strn = pow(1.0 - (i.depth / 20.0), 2);

                col.a = noise(i.uv * 200 + random(float2(i.depth,0)))*1.0;
                col.a *= noise(i.uv * 10 + i.depth * 0.0)*0.55;
                col.a *= strn;
                //col.rgb = col.aaa;
                //col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
