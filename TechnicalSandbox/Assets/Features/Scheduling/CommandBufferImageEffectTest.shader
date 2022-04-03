Shader "Hidden/CommandBufferImageEffectTest"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                //Move vertex to UV location (screen space so -1 to 1 instead of normalized) - Also flip if projection is upside down
                float4 uv =  float4(0,0,0,1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * 2.0 - 1.0);
                o.vertex = uv;
                return o;
            }

            sampler2D _MainTexed;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTexed, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
