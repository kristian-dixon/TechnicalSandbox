Shader "Geometry/TeleportEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Gradient("Gradient", 2D) = "black" {}
        _Extrusion("Extrusion", Range(-10,10)) = 1
        //_BaseColor("Base Colour", Color) = (0.5,0.5,0.5,1)
        //_TipColor("Tip Colour", Color) = (1,1,1,1)
        _Progress("Progress", Range(0.0,1.0)) = 0.5
        _Direction("Direction", Vector) = (0,1,0)
    }
        
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" 

            float remap01(float x)
            {
                return (x * 2.0) - 1.0;
            }

            float remapNeg11(float x)
            {
                return (x * 2.0) - 1.0;
            }



            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct g2f
            {
                //float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float4 barycentricCoords : COLOR0;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Gradient;
            float4 _Gradient_ST;
            float _Extrusion;
            float _Progress;
            float3 _Direction;

            v2g vert(appdata v )
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal = v.normal;
                o.uv = v.uv;
                o.uv2 = v.uv2;
                return o;
            }

            float random(float3 st) {
                return frac(sin(dot(st.xyz,
                    float3(12.9898, 78.233, 37.312))) *
                    43758.5453123);
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream, uint triangleID : SV_PrimitiveID)
            {
                g2f o;

                fixed col = tex2Dlod(_Gradient,  float4((IN[0].uv2 + IN[1].uv2 + IN[2].uv2) / 3.0, 0,0)).r;
                
                float3 startPoint = float3(-2, 0, 0);
                float3 endPoint = float3(2, 0, 0);
                float3 dir = normalize(endPoint - startPoint);

                float3 normal = normalize(cross(IN[0].vertex - IN[1].vertex, IN[0].vertex - IN[2].vertex).xyz); //   float3(0, 0, 0);
                float dirRatio = (dot(dir, normal) + 1) / 2.0; 
                float4 center = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3.0;
                o.normal = UnityObjectToWorldNormal(normal);

                float rng = random(float3(triangleID, 0, 0));
                float rng2 = random(float3( 0, triangleID, 0));


                //float progressCurved = pow(min(1, dirRatio + _Progress), 10 * rng2);

                float positionCurve = smoothstep(0.25, 0.75, _Progress + (rng2) * 0.15 + 0.05 * dirRatio);
                float3 rootPosition = startPoint + (endPoint - startPoint) * positionCurve;  

                float extrusionCurve = pow(cos(3.14 * remap01(_Progress) / 2.0) , 3.5);
                float3 extrusion = (normal * _Extrusion * extrusionCurve * rng);

                
                float3 position = rootPosition + extrusion;

                float4x4 translation = float4x4(1, 0, 0, position.x, 0, 1, 0, position.y, 0, 0, 1, position.z, 0, 0, 0, 0);

                float4x4 tform = translation;// * distortion;

                float4 coords[3];
                coords[0] = float4(1,0,0,0);
                coords[1] = float4(0,1,0,0);
                coords[2] = float4(0,0,1,0);


                for (int i = 0; i < 3; i++)
                {
                    //Scaling the triangle
                    float progress = remap01(_Progress);
                    float scalingCurve = pow(abs(sin(3.145 * progress / 2.0)), 1.5 + (remap01(rng) * 1)); //pow(max(0.0, abs(progress) * 2.0 - 1.0), 2);
                    float triScaleAmount = 0.45 + 0.55 * scalingCurve;

                    float4 pos = center + (IN[i].vertex - center) * triScaleAmount; // mul(tform, IN[i].vertex);
                    
                    pos = mul(tform, pos);

                    o.vertex = UnityObjectToClipPos(pos);
                    o.uv = IN[i].uv;
                    o.barycentricCoords = coords[i]; 
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    triStream.Append(o);
                }
                
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i ) : SV_Target
            {
                // sample the texture
                
                fixed4 col =  1 - tex2D(_MainTex, i.uv);
                
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                fixed3 diff = nl * _LightColor0;

                diff += ShadeSH9(half4(i.normal, 1));
                
                col *= fixed4(diff,1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
        

                float4 edgeGlow = float4(0,0,0,1);
                float smallestEdge = min(i.barycentricCoords.x, min(i.barycentricCoords.y, i.barycentricCoords.z));

                if(smallestEdge < 0.05)// && i.barycentricCoords.x > 0.49)            
                {
                    edgeGlow = float4(0,0.2,1,1);
                }

                float progress = remap01(_Progress);
                    
                float colourCurve = 1.0 - pow(max(0.0, abs(progress) * 2.0 - 1.0), 4);
                col = lerp(col, edgeGlow, colourCurve);
                return col;
            }
        ENDCG
    }
    }
}