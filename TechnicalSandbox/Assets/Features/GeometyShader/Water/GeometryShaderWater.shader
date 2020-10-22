Shader "Geometry/Water"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Extrusion("Extrusion", Range(-10,10)) = 1
        _AnimSpeed("AnimSpeed", Range(0, 10)) = 1
        _Wavelength("Wavelength", range(0, 10)) = 1
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
            #pragma target 4.6
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag


            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" 


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
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float4 colour : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Extrusion;

            fixed4 _BaseColor;
            fixed4 _TipColor;
            float _Wavelength;
            float _AnimSpeed;

            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal = v.normal;
                return o;
            }

            g2f VertexOutput(float4 position, float3 normal, float4 colour)
            {
                g2f o;
                o.vertex = UnityObjectToClipPos(position);
                o.normal = UnityObjectToWorldNormal(normal);
                o.colour = colour;
                return o;
            }


            float WaveHeight(float4 vertex) {

                return (sin((_AnimSpeed * _Time.y + vertex.x * _Wavelength)) * cos(_AnimSpeed * _Time.y + (vertex.z + .2345))) * _Extrusion;



                return sin((_AnimSpeed * _Time.y + vertex.x * _Wavelength)) * _Extrusion;
            }

            float4 WaveColour(float waveHeight) {
                //Wave is currently in the range of extrusion * (-1 to 1)
                float waveHeightUnscaled = waveHeight / _Extrusion;
                float waveHeightNormalized = waveHeightUnscaled * 0.5f + 0.5f;

                float factor = pow(waveHeightNormalized, 5);

                return lerp(_BaseColor, _TipColor, factor);
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float4 vertex1 = IN[0].vertex;
                vertex1.y += WaveHeight(vertex1);

                float4 vertex2 = IN[1].vertex;
                vertex2.y += WaveHeight(vertex2);

                float4 vertex3 = IN[2].vertex;
                vertex3.y += WaveHeight(vertex3);

                float3 normal = normalize(cross((vertex1.xyz - vertex2.xyz), (vertex1.xyz - vertex3.xyz)));


                triStream.Append(VertexOutput(vertex1, normal, WaveColour(vertex1.y)));
                triStream.Append(VertexOutput(vertex2, normal, WaveColour(vertex2.y)));
                triStream.Append(VertexOutput(vertex3, normal, WaveColour(vertex3.y)));
            }

            fixed4 frag(g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.colour;
                
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                fixed3 diff = nl * _LightColor0;
                
                col *= fixed4(diff,1);
                return col;
            }
        ENDCG
    }
    }
}