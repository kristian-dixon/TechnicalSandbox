Shader "Unlit/PainterShader"
{
    Properties
    {
        _BlendOp("_BlendOp", Float) = 0
    }
    SubShader
    {

        BlendOp [_BlendOp]
        Blend One One

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
                float4 worldPos : TEXCOORD1;
            };

            uniform float4 _BrushPosition;

            v2f vert (appdata v)
            {
                v2f o;

                //Set world position of vertex
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                //Move vertex to UV location (screen space so -1 to 1 instead of normalized) - Also flip if projection is upside down
                float4 uv =  float4(0,0,0,1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * 2.0 - 1.0);
                o.vertex = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float falloff = smoothstep(1.0,0,length(i.worldPos.xyz - _BrushPosition.xyz)) * 0.05;
                return float4(falloff.rrr, 1.0);
            }
            ENDCG
        }
    }
}
