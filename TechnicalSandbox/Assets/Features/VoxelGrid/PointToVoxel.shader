Shader "Geometry/PointToVoxel"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        //_Scale("Scale", Float) = 1
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
            #include "UnityLightingCommon.cginc" 

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            struct g2f
            {
                //float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;


            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.color = v.color;
                return o;
            }

            [maxvertexcount(36)]
            void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
            {
                
                //return;

                g2f o;
                float4 voxelColour = float4(1, 1, 1, 1);
                //float4 vtxCentreClip = UnityObjectToClipPos(vtxCentre);
                o.color = voxelColour;
                float _Scale = IN[0].color.w;

                float4 center = IN[0].vertex;
                
                //Front face
                o.normal = float3(0, 0, -1);

                o.vertex = center + (float4(-1,-1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                //
                o.vertex = center + (float4(1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                triStream.RestartStrip();


                //Back face
                o.normal = float3(0, 0, 1);

                o.vertex = center + (float4(-1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                //
                o.vertex = center + (float4(1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                triStream.RestartStrip();

                

                //Right
                o.normal = float3(1, 0, 0);

                o.vertex = center + (float4(1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                o.vertex = center + (float4(1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();


                //Left
                o.normal = float3(-1, 0, 0);

                o.vertex = center + (float4(-1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                o.vertex = center + (float4(-1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                //Top
                o.normal = float3(0, 1, 0);

                o.vertex = center + (float4(-1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                o.vertex = center + (float4(1, 1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, 1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                //Bottom
                o.normal = float3(0, -1, 0);

                o.vertex = center + (float4(-1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();

                o.vertex = center + (float4(1, -1, -1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);

                o.vertex = center + (float4(-1, -1, 1, 0) * _Scale * 0.5);
                o.vertex = UnityObjectToClipPos(o.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                triStream.Append(o);
                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.color; //fixed4(0.9,0.9,0.9,1);
                
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                fixed3 diff = nl * _LightColor0;

                diff += ShadeSH9(half4(i.normal, 1));
                
                col *= fixed4(diff,1);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
        ENDCG
    }
    }
}