Shader "Unlit/Sjhell"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;



            struct OutputVertex
            {
                float3 position;
                float3 normal;
                float2 uv;
                float4 color;
            };

            
            struct OutputTriangle
            {
                OutputVertex vertexA;
                OutputVertex vertexB;
                OutputVertex vertexC;
            };

            StructuredBuffer<OutputTriangle> _TrianglesBuffer;


            v2f vert (uint vertexID : SV_VertexID)
            {
                v2f o;
                OutputTriangle tri = _TrianglesBuffer[vertexID / 3];
                OutputVertex v;

                int vert = vertexID % 3;
                if(vert == 0)
                {
                    //o.vertex = UnityObjectToClipPos(float3(-1, triIdx,-1));
                    v = tri.vertexA;
                }
                else if(vert == 1)
                {
                    //o.vertex = UnityObjectToClipPos(float3(0,triIdx,1));
                    v = tri.vertexB;
                }
                else
                {
                    //o.vertex = UnityObjectToClipPos(float3(1,triIdx,-1));
                    v = tri.vertexC;

                }




                
                o.vertex = UnityObjectToClipPos(v.position);
                o.uv = v.uv;
                o.color = v.color;
                
                

                
                
               // o.color = float4(triIdx / 200.0f, triIdx == 199 ? 1 : 0 ,0,1);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                float avg = (col.r + col.g + col.b) / 3.0f;
                 
 
                clip(col.r - i.color.r );

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
