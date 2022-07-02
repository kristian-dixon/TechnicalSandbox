Shader "Unlit/VertexAnimUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MorphTex ("Animation Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 morphUV : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MorphTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                
                float pixelWidth = 1.0 / 100.0;

                 
                float3 col = tex2Dlod(_MorphTex, float4(v.morphUV.x + pixelWidth / 2.0, _Time.y * (1/24.0) * 4.0, 0,0)).rgb;
                float4 vtx = float4(((col.rgb * 2.0) - 1.0) * 10, 1);
                
                o.vertex = UnityObjectToClipPos(vtx);// + v.vertex);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.morphUV, _MainTex);



                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

               // col.xy = frac(i.uv);

               // col.z = frac(_Time.y + col.x * 10);
                // apply fog

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
