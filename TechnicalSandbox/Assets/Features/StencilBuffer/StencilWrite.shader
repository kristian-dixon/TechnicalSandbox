Shader "Unlit/StencilWrite"
{
    SubShader
    {
        Tags { "RenderType" = "StencilWrite" "Queue" = "Geometry-3" }
        ColorMask 0
        cull front

        Stencil
        {
            ref 1
            Comp Always
            Pass replace
        }

        CGPROGRAM
        #pragma surface surf Lambert
        struct Input
        {
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = fixed4(1,1,1,1);
        }
        ENDCG
    }
}
