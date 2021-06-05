Shader "Custom/Building"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _EmissiveColor("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
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

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _EmissiveColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float random(float2 st) {
            return frac(sin(dot(st.xy,
                float2(12.9898, 78.233))) *
                43758.5453123);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 colour = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            float light = 0;

            float2 uv = IN.uv_MainTex;
            uv.x = (uv.x * 2) - 1.0;

            float3 worldCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            
            float3 localPos = IN.worldPos - worldCenter;
            float3 localNormal = normalize(mul(unity_WorldToObject, float4(IN.worldNormal, 0)).xyz);

            float2 id = floor(localPos.xy);
            uv.x = frac((localPos.x));
            uv.y = frac((localPos.y));

            float windowsZ = 1.0 - (step(0.2, uv.x) - step(0.8, uv.x)) * 
                                    (step(0.2, uv.y) - step(0.8, uv.y));
            windowsZ *= abs(dot(float3(0, 0, 1), localNormal));
            light = (1 - windowsZ) * random(id);
            
            id = floor(localPos.zy);
            uv.x = frac((localPos.z));
            uv.y = frac((localPos.y));
            float windowsX = 1.0 - (step(0.2, uv.x) - step(0.8, uv.x)) *
                (step(0.2, uv.y) - step(0.8, uv.y));
            windowsX *= abs(dot(float3(1, 0, 0), localNormal));
            light *= (1 - windowsX) * random(id);

            float roofPrevention = step(0.3, localNormal.y);

            float window = (windowsZ + windowsX);
            //window *= 1 - roofPrevention;

            colour = lerp(_Color * (window), _Color, roofPrevention);
            
            
            // Albedo comes from a texture tinted by color
            o.Albedo = colour;
            // Metallic and smoothness come from slider variables
            o.Emission = _EmissiveColor * light * (1 - roofPrevention);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
