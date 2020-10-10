Shader "Geometry/NewGeometryShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Extrusion("Extrusion", Range(-10,10)) = 1
        _BaseColor("Base Colour", Color) = (0.5,0.5,0.5,1)
        _TipColor("Tip Colour", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                //float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Extrusion;

            fixed4 _BaseColor;
            fixed4 _TipColor;

            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal = v.normal;
                return o;
            }

            [maxvertexcount(9)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;

                float3 normal = normalize(normalize(IN[0].normal) + normalize(IN[1].normal) + normalize(IN[2].normal));
                float4 vtxCentre = ((IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0f) + float4(normal,0) * _Extrusion;
                vtxCentre = UnityObjectToClipPos(vtxCentre);

                for (int i = 0; i < 3; i++)
                {
                    o.vertex = UnityObjectToClipPos(IN[i].vertex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    o.color = _BaseColor;
                    triStream.Append(o);
                 
                    o.vertex = UnityObjectToClipPos(IN[fmod(i + 1, 3)].vertex);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    triStream.Append(o);

                    o.vertex = vtxCentre;
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    o.color = _TipColor;

                    triStream.Append(o);

                    triStream.RestartStrip();
                }

            }

            fixed4 frag(g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color; //fixed4(0.9,0.9,0.9,1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
        ENDCG
    }
    }
}