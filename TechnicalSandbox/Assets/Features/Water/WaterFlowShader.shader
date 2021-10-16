Shader "Custom/WaterFlowShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Noise0 ("Noise 1 (RGB)", 2D) = "black" {}
        _Noise1 ("Noise 2 (RGB)", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Noise0;
        sampler2D _Noise1;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _Color2;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed sampleWater(float2 uv)
        {
            float time = _Time.y;
            
            // Albedo comes from a texture tinted by color
            fixed4 noise0 = 1 - tex2D (_Noise0, (uv * 2 + float2(time * -0.021, time * -0.1) + float2(0.1 * sin(time + uv.x * 6.28), 0))); //* _Color;
            fixed4 noise1 = 1 - tex2D (_Noise1, (uv * 2 + float2(time * 0.5, time * 0.35) + float2(0.03 * cos(time + uv.x * 6.28 * 2), 0))); //* _Color;
            
            return (noise1.r * pow(noise0.r, 2));
        }

        float3 FindNormal(float2 uv, float u)
        {
            float2 offsets[4];
            offsets[0] = float2(-u, 0);
            offsets[1] = float2(u, 0);
            offsets[2] = float2(0, -u);
            offsets[3] = float2(0, u);
               
            float heights[4];
            for(int i = 0; i < 4; i++)
            {
                heights[i] = sampleWater(uv + offsets[i]);
            }
               
            float2 step = float2(1.0, 0.0);
               
            float3 va = normalize( float3(step.xy, heights[1]-heights[0]) );
            float3 vb = normalize( float3(step.yx, heights[3]-heights[2]) );
               
            return cross(va,vb).rbg; 
               
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed water = sampleWater(IN.uv_MainTex);
            fixed4 c = lerp( _Color, _Color2, water);

            o.Normal = FindNormal(IN.uv_MainTex, 0.1);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
