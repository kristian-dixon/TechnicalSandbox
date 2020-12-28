///A quick implementation to get back up to speed with Raymarching. Simple infinitely repeating spheres that have a bloom effect.
// Future improvements -- The bloom is currently fixed into each sphere's domain, allowing the bloom to pass through each boundary 
// would potentially make it cooler :)

Shader "PostProcessing/Raymarching/GlowSphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex raymarch_vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Raymarch.cginc"


            uniform float3 _CameraFwd;
            uniform float3 _CameraUp;
            uniform float3 _CameraPos;
            sampler2D _MainTex;

            float4 surfaceMin(float4 a, float4 b) {
                if (a.a < b.a) {
                    return a;
                }
                else {
                    return b;
                }
            }
          
            float random(float3 st) {
                return frac(sin(dot(st.xyz,
                    float3(12.9898, 78.233, 37.312))) *
                    43758.5453123);
            }

            float4 map(float3 p) {
                p *= 0.1;
                float3 cell = floor(p);//floor((p + float3(5,0,0))) * 10;
                float rngR = random(cell);
                float rngG = random(cell.yxz);
                float rngB = random(cell.zyx);

                float rngS = random(cell.zyx);

                /*p.x += sin(_Time.y  + p.y * 2);
                p.y += cos(_Time.y + p.z * 0.2);
                p.z += -cos(_Time.y + p.x * 1);
                */

                p = frac(p);

                //p = frac(p);// sign(p)* fmod(abs(p), float3(1, 1, 1));

                float4 s1 = float4(0.1,0.5,0.25, sdSphere(p - float3(0.5, 0.5, 0.5), 0.1));//float4(rngR,rngG,rngB,sdSphere(p + float3(0.5,0.5,0.5), abs(sin(_Time.y * 2 + rngS) * 0.25 )));
                return s1; 
            }

            fixed4 trace(float2 uv) {
                Ray ray = CreateStartingRay(_CameraPos, _CameraFwd, _CameraUp, uv);
                
                fixed4 col = fixed4(0, 0, 0, 0);
                for (int i = 0; i < 200; i++) {

                    float4 surf = map(ray.origin);
                    float dist = surf.w;
                    col += (abs(sin(_Time.y)) * 0.1 + 0.5)/ dist * float4(surf.rgb, 1) * 0.001;

                    if (dist < 0.01) {
                        return col;
                    }

                    ray.origin += ray.dir * dist;
                }

                
                col.a = 1;
                
                return col;//fixed4(1, 0, 0, 1);
            }

            fixed4 frag (raymarch_v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
            
                //Get aspect ratio corrected UV coordinates & centre the coordinate system about the middle of the screen.
                float2 uv = CorrectUVAspect(i.uv);
                col = trace(uv);

                return col;
            }
            ENDCG
        }
    }
}
