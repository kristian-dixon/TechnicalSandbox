// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "TriplanarMapping/Texture"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale("Scale", Float) = 1
    }
    SubShader
        {
            Tags { "RenderType" = "Opaque" }
            //LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows vertex:vert

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
                float3 worldPos;
                float3 normal;
                float4 grabPos;
            };

            void vert(inout appdata_full v, out Input o) {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.normal = normalize((mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz));

                float4 pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
            }

            sampler2D _BackgroundTexture;

            half _Glossiness;
            half _Metallic;
            float _Scale;
            fixed4 _Color;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            #define vec2 float2
            #define vec3 float3
            #define vec4 float4
            #define fract frac
            #define mix lerp

            fixed4 Image(float2 uv) {
                
                return tex2D(_MainTex, uv);
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Albedo comes from a texture tinted by color
                float scale = _Scale;

                fixed4 dx = Image(frac(IN.worldPos.zy * scale)); //* _Color;
                fixed4 dy = Image(frac(IN.worldPos.xz * scale));//
                fixed4 dz = Image(frac(IN.worldPos.xy * scale));// tex2D(_MainTex, IN.worldPos.xy) * _Color;

                float3 normal = abs(IN.normal);
                fixed4 c = normal.x * dx + normal.y * dy + normal.z * dz;

                o.Albedo = c * _Color;

                //o.Emission = fixed3(1 - dot(normalize(_WorldSpaceCameraPos - IN.worldPos), IN.normal), 0,0);


                // Metallic and smoothness come from slider variables
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                
                //o.Albedo = IN.normal;
            }
            ENDCG
    }
}
