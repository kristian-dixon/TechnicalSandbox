// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AppendExample/BufferShader"
{
    SubShader
    {
        Pass
        {
            Cull Off 
            Fog { Mode off }

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" 

            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            struct Vertex
            {
                float3 position;
                int lookupValue;
            };

            struct TableLookup
            {
                int entry[16];
            };

            uniform StructuredBuffer<Vertex> buffer;
            uniform StructuredBuffer<TableLookup> lookupTable;
            uniform Buffer<float3> cubePoints;

            uniform float3 col;

            struct v2g
            {
                float4 pos : SV_POSITION;
                int lookupValue : CUSTOM;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
            };

            v2g vert(uint id : SV_VertexID)
            {
                v2g OUT;
                OUT.pos = float4(buffer[id].position, 1);
                OUT.lookupValue = buffer[id].lookupValue;
                return OUT;
            }

            [maxvertexcount(16)]
            void geom(point v2g IN[1], inout TriangleStream<g2f> stream)
            {
                g2f o;
                float4 pos = IN[0].pos;
                int lookupVal = IN[0].lookupValue;
                for (int i = 0; i < 16 ; i+=3) {
                    int entry = lookupTable[lookupVal].entry[i];
                    if (entry == -1) return;
                    
                    //
                    float4 v1 = (pos + float4(cubePoints[entry],0));
                    entry = lookupTable[lookupVal].entry[i+1];
                    float4 v2 = (pos + float4(cubePoints[entry], 0));
                    entry = lookupTable[lookupVal].entry[i + 2];
                    float4 v3 = (pos + float4(cubePoints[entry], 0));

                    float3 n = cross(normalize(v1.xyz - v2.xyz), normalize(v1.xyz - v3.xyz));
                    o.normal = UnityObjectToWorldNormal(n);

                    o.pos = UnityObjectToClipPos(v1);
                    stream.Append(o);
                    o.pos = UnityObjectToClipPos(v2);
                    stream.Append(o);
                    o.pos = UnityObjectToClipPos(v3);
                    stream.Append(o);

                    //
                }
            }

            float4 frag(g2f IN) : COLOR
            {
                half nl = max(0, dot(IN.normal, _WorldSpaceLightPos0.xyz));
                fixed3 diff = nl * _LightColor0;

                diff += ShadeSH9(half4(IN.normal, 1));

                return float4(1,1,1,1) * fixed4(diff, 1);
            }

            ENDCG
        }
    }
}