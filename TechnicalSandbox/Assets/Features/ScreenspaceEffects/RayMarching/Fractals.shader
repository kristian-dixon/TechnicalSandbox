///A quick implementation to get back up to speed with Raymarching. Simple infinitely repeating spheres that have a bloom effect.
// Future improvements -- The bloom is currently fixed into each sphere's domain, allowing the bloom to pass through each boundary 
// would potentially make it cooler :)

Shader "PostProcessing/Raymarching/Fractals"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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

            float3x3 rotX(float angle) {
                return float3x3(1,0,0,
                                0, cos(angle), -sin(angle),
                                0, sin(angle), cos(angle));
            }

            float3x3 rotY(float angle) {
                return float3x3(cos(angle), 0, sin(angle),
                    0, 1, 0,
                    -sin(angle), 0, cos(angle));
            }

            float3x3 rotZ(float angle) {
                return float3x3(
                    cos(angle), -sin(angle),0,
                    sin(angle), cos(angle), 0,
                    0,0,1);
            }

            float4 map(float3 p) {


                //Bounding sphere
                float len = length(p);//


                if (len > 10) {
                    //return float4(0, 0, 0, len);
                }

                float3 col = float3(0.5, 0.5, 0.5);
                float3x3 rot = rotZ(5 * 0.01);
                float d = sdSphere(p, 10);// float3(10, 10, 1000));
                float scale = 0.2;
                //p = mul(rot, p);
                

                for (int m = 0; m < 4; m++)
                {

                    float3 a = fmod(abs(p) * scale, 2.0) - 1.0;
                    scale *= 3.0;
                    float3 r = abs(1.0 - 3.0 * abs(a));
                    float da = max(r.x, r.y);
                    float db = max(r.y, r.z);
                    float dc = max(r.z, r.x);
                    float c = (min(da, min(db, dc)) - 1.0) / scale;

                    d = max(d, c);

                }

                /*float3x3 rot = mul(rotZ(_Time.y), mul(rotY(_Time.y), rotX((_Time.y))));
                float dist = sdRotatingSierpinski(p, float3(1, 1, 1), 2, rot);*/
                //return length(z) * pow(Scale, float(-n));
                return float4(col, d);

            }


            fixed4 trace(float2 uv) {
                Ray ray = CreateStartingRay(_CameraPos, _CameraFwd, _CameraUp, uv);

                fixed4 col = fixed4(0, 0, 0, 0);
                for (int i = 0; i < 500; i++) {

                    float4 surf = map(ray.origin);
                    float dist = surf.w;

                    if (dist < 0.001) {
                        return float4(surf.xyz, 1) *pow(1 - i / 500.0f, 8);
                    }
                    
                    ray.origin += ray.dir * dist;
                }

                
                col.a = 1;

                return col;//fixed4(1, 0, 0, 1);
            }

            fixed4 frag(raymarch_v2f i) : SV_Target
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
