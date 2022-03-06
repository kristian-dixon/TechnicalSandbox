Shader "Custom/ShellShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _NoiseTex1 ("Noise Texure 1 (RGB)", 2D) = "white" {}
        _NoiseTex2 ("Noise Texure 2 (RGB)", 2D) = "white" {}
        _PaintTex ("Paint (RGBA)", 2D) = "white" {}
        _FlowTex ("FLOW (RG)", 2D) = "white" {}
        _WindStrength("Wind Strength", FLOAT) = 1.0 
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
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

        sampler2D _NoiseTex1;
        sampler2D _NoiseTex2;
        sampler2D _FlowTex;
        sampler2D _PaintTex;
        sampler2D _PaintMaskTex;
        float _WindStrength;

        struct Input
        {
            float2 uv_NoiseTex1;
            float2 uv_NoiseTex2;
            float2 uv_FlowTex;
            float2 uv_PaintTex;
            float4 color : COLOR;
        };


        struct OutputVertex
        {
            float3 position;
            float3 normal;
            float2 uv;
            float4 color;
        };

        
        struct OutputTriangle
        {
            OutputVertex vertex[3];
        };

        struct appdata{
            float4 vertex: POSITION;
            float2 texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
            float3 normal: NORMAL;
            float4 color: COLOR;

            uint id: SV_VertexID;
        };


        #ifdef SHADER_API_D3D11
            StructuredBuffer<OutputTriangle> _TrianglesBuffer;
        #endif

        half _Glossiness;
        half _Metallic;
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
            OutputTriangle tri = _TrianglesBuffer[output.id / 3];
            OutputVertex v = tri.vertex[output.id % 3];

            output.vertex =  float4(v.position, 1);
            output.normal = v.normal;
            output.texcoord = v.uv;
            output.texcoord1 = v.uv;
            output.texcoord2 = v.uv;
            output.color = v.color;

            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color;
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color

            float2 direction = float2(0.1, 0.3) * _Time.y;
            float strength = _WindStrength;
            float2 disp = (tex2D(_FlowTex, IN.uv_FlowTex + direction) * 2.0 - 1.0) * _WindStrength * pow(IN.color.r, 0.5);

            fixed4 c = tex2D (_NoiseTex1, IN.uv_NoiseTex1 + disp); 
            fixed4 c2 =  tex2D (_NoiseTex2, IN.uv_NoiseTex2 + disp);
            c *= c2;

            float mask = tex2D (_PaintMaskTex, IN.uv_PaintTex + disp ).r;


            clip(pow(c.r * mask, 1.0) - IN.color.r);

            c *= _Color * (IN.color.r + 0.9); 

            c = lerp(float4(0.3,0.2,0.05,1) * (c2 * 0.5 + 0.5) , c, 1.0 - pow(1.0 - mask, 2));  

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
