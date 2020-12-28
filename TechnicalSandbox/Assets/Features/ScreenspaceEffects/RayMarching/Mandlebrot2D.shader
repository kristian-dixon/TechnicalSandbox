///This doesn't necessarily belong in the raymarching folder but it'll eventually lead to raymarched fractals.
///Also slightly because I copied and pasted the glow spheres file :)

Shader "PostProcessing/Raymarching/Mandlebrot2D"
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
            uniform float3 _ConstantOffset;
            uniform float3 _VelocityOffset;

            sampler2D _MainTex;

            fixed4 frag (raymarch_v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
            
                //Get aspect ratio corrected UV coordinates & centre the coordinate system about the middle of the screen.
                float2 uv = CorrectUVAspect(i.uv);
                float2 constantOffset = CorrectUVAspect(_ConstantOffset.xy);
                uv *= 5;// *= (sin(_Time.y) + float2(1,1)) * 0.5;

                //As I understand it this is a escape velocity thing where each iteration checks to see if the point has managed to
                //exceed some value and if so it'll be coloured a specific colour on a ramp. Each loop squares the current val and then adds a constant
                float2 velocity = uv;

                float2 constant = uv;//float2(0.25, 0.5) + constantOffset * 0.1 + float2(0, sin(_Time.x) * 0.001);// *1.5; //Uncomment for Julia

                for (int i = 0; i < 1000; i++) {
                    float x = (velocity.x * velocity.x - velocity.y * velocity.y) + constant.x;
                    float y = (velocity.y * velocity.x + velocity.x * velocity.y) + constant.y;

                    if (x * x + y * y > 4)
                        break;
                    velocity.x = x;
                    velocity.y = y;
                }

                float t = smoothstep(0, 100, i) - smoothstep(1000 * 0.5, 1000, i);

                float background = step(t, 1000) * 0.0005;
                t = t - background;
                col = fixed4(t + background, background, background, 1);

                return col;
            }
            ENDCG
        }
    }
}
