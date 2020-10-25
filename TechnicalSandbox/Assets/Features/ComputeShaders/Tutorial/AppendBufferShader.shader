// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AppendExample/BufferShader"
{
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            struct Vertex
            {
                float3 position;
                float4 colour;
            };

            uniform StructuredBuffer<Vertex> buffer;
            uniform float3 col;

            struct v2f
            {
                float4  pos : SV_POSITION;
                float4 col : TEXCOORD0;
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(float4(buffer[id].position, 1));
                OUT.col = buffer[id].colour;
                return OUT;
            }

            float4 frag(v2f IN) : COLOR
            {
                return float4(IN.col.xyz, 1) + float4(1,1,1,1) * 0.1;
            }

            ENDCG
        }
    }
}