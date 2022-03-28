Shader "Unlit/FragmentParticleShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float4 color : COLOR0;
        };

        struct Particle
        {
            float3 position;
            float3 velocity;
        };
       

        
        

        struct appdata{
            float4 vertex: POSITION;
            uint id: SV_VertexID;
            float3 normal: NORMAL;
            float4 color : COLOR;
        };

        #ifdef SHADER_API_D3D11
            StructuredBuffer<Particle> _ParticleBuffer;
        #endif

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata output, out Input o)
        {
            #ifdef SHADER_API_D3D11
            Particle p = _ParticleBuffer[output.id];

            output.vertex = float4(p.position, 1);
            output.color =  float4(abs(p.velocity), 1);
            o.color = float4(abs(p.velocity), 1);

            
            output.normal = p.velocity;

            UNITY_INITIALIZE_OUTPUT(Input, o);
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            o.Albedo = float4(1,1,1,1);
            //o.Emission = IN.color * 10;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
