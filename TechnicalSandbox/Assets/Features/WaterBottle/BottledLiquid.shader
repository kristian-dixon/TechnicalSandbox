Shader "Custom/BottledLiquid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _WaterHeight ("Smoothness", Range(0,1)) = 0.5
        _WaterDirection ("WaterDir", Vector) = (0,1,0)
    }


  
    SubShader
    {
        Tags {"Queue" = "Geometry" "RenderType"="Opaque" }
        Cull off
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _WaterHeight;
        fixed4 _Color;
        float3 _WaterDirection;
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        //Get world vertex position
        //Get world object position
        //subtract vertex.xz from object.xz & normalize(maybe)    
    
        //Multiply (0,1,0) by world matrix to get upwards orientation
        void vert (inout appdata_full v) {
          v.vertex.xyz -= v.normal * 0.05;

      }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 waterCenter = mul(unity_ObjectToWorld,float4(0,_WaterHeight, 0, 1));
            float4 objPos = mul(unity_WorldToObject, float4(IN.worldPos, 1));
            float4 posWorld = mul(unity_ObjectToWorld, float4(objPos.x, IN.uv_MainTex.y, objPos.z,1));
                        

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float3 height = (waterCenter - posWorld).xyz;
            float proj = dot(height, normalize(_WaterDirection));

            //float waterLevel = step(0, _WaterHeight);
            //float waterLevel = step(posWorld.y, waterCenter.y);
            o.Albedo = _Color;// * height.y;// * 10);
            o.Emission = _Color;
            clip(proj);
           // o.Albedo = float4(height,1);
            o.Smoothness = 0.08;
            
            o.Alpha = c.a;
        }
        ENDCG

///
        
        Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
        Cull back

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _WaterHeight;
        fixed4 _Color;
        float3 _WaterDirection;
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        //Get world vertex position
        //Get world object position
        //subtract vertex.xz from object.xz & normalize(maybe)    
    
        //Multiply (0,1,0) by world matrix to get upwards orientation

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 waterCenter = mul(unity_ObjectToWorld,float4(0,_WaterHeight, 0, 1));
            float4 objPos = mul(unity_WorldToObject, float4(IN.worldPos, 1));
            float4 posWorld = mul(unity_ObjectToWorld, float4(objPos.x, IN.uv_MainTex.y, objPos.z,1));
                        

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;


            o.Albedo = float4(1,1,1,.2);
    
            o.Alpha = 0.3;
            o.Smoothness = 0.8;
        }
        ENDCG
        
    }
    FallBack "Diffuse"
}
