Shader "Unlit/Rays"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _MainTex2 ("Turbulance Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
     
        Blend SrcAlpha OneMinusSrcAlpha 
        Cull Off
        ZWrite Off
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL0;
                float3 worldPos : POSITION1;
            };

            sampler2D _MainTex;
            sampler2D _MainTex2;
            float4 _MainTex_ST;
            float4 _MainTex2_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv, _MainTex2);
                o.normal = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col2 = tex2D(_MainTex2, float2(0.005 * -_Time.y, 0.05 * -_Time.y) + i.uv1 * float2(0.5,0.1));
                float2 distortion = (col2.gb * 2 - float2(1,1)) * 0.95;

                fixed4 col = tex2D(_MainTex, i.uv);// + distortion);
                //col.rgb = i.normal;

                float edgeMask = 1 - pow(1 - abs(dot(normalize(i.normal), normalize(_WorldSpaceCameraPos - i.worldPos))), 3);

                col.rgb = edgeMask.rrr;
                col.a *= edgeMask;
                return col;
            }
            ENDCG
        }
    }
}
