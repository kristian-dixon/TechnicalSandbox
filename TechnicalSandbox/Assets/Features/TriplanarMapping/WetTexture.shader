// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "TriplanarMapping/DrippyTexture"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Scale("Scale", Float) = 1
    }
    SubShader
        {
            Tags { "RenderType" = "Opaque" }
            //LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows vertex:vert

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
                float3 worldPos;
                float3 normal;
            };

            void vert(inout appdata_full v, out Input o) {
                UNITY_INITIALIZE_OUTPUT(Input, o);
                o.normal = normalize((mul(unity_ObjectToWorld, float4(v.normal, 0)).xyz));

                float4 pos = UnityObjectToClipPos(v.vertex);
                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate

            }


            half _Glossiness;
            half _Metallic;
            float _Scale;
            fixed4 _Color;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            #define vec2 float2
            #define vec3 float3
            #define vec4 float4
            #define fract frac
            #define mix lerp
            
            float N(float x)
            {
                return fract(sin(x * 4124.6713) * 7253.163);
            }


            //Taken from the art of code - https://www.youtube.com/watch?v=52TMliAWocY
            vec2 Rain(vec2 uv, float t)
            {
                t *= 40.;

                //Essentially figuring out the scale of the grids that will contain a raindrop -- closer to 0 the bigger the cell will be on that e 
                vec2 a = vec2(3., 1.);
                vec2 st = uv * a;

                //Assign an ID for each cell used to get local random values
                vec2 id = floor(st);

                //Downwards scrolling of texture
                st.y += t * .22;

                //n used to get random vertical offset along the X axis grids
                float n = N(id.x);
                st.y += n;
                uv.y += n;
                id = floor(st);
                st = fract(st) - .5; //Visualize st to gain intuition of how this works


                t += fract(sin(id.x * 76.34 + id.y * 1453.7) * 768.34) * 6.283;

                float y = -sin(t + sin(t + sin(t) * .5)) * .4;
                float x = fract(sin(id.x * 76.34 + id.y * 1453.7) * 768.34) * 0.5 - .25;

                vec2 p1 = vec2(x, y);

                //Gradient for slide down in cell
                vec2 o1 = (st - p1) / a;

                //d becomes the circular part of the raindrop
                float d = length(o1);
                float m1 = smoothstep(.07, .0, d); //The raindrop with a bit of a cleaner shape thanks to smoothstep (also important it inverts the colour sneakily :))

                //This is for followup drips to make a fake trail, it'll be a couple of tiny circles
                vec2 o2 = (fract(uv * a.x * vec2(1., 2.)) - .5) / vec2(1., 2.) - vec2(x, 0.);
                d = length(o2);
                float m2 = smoothstep(.3 * (.5 - st.y), .0, d) * smoothstep(-.1, .1, st.y - p1.y); //Additional smoothstep for fadeout
                //if(st.x > .46) m1 = 1.;  if(st.y > .49) m1 = 1.;

                //This essentially allows the dots to be drawn cleanly together
                return vec2(m1 * o1 * 20. + m2 * o2 * 10.);
            }

            vec3 Outside(vec2 uv, float t)
            {
                vec2 rainDistort = Rain(uv * 5., t) * .5;
                rainDistort += Rain(uv * 7., t) * .5;

                //Bit of a sine wave for wibblyness
                uv.x += sin(uv.y * 70.) * .0005;
                uv.y += sin(uv.x * 170.) * .00135;

                return vec3(rainDistort.xy, 0);
            }

            fixed4 Image(float2 uv) {
                float t = _Time.y * 0.1;
                vec2 rainDistort = Rain(uv * 5., t) * .5;
                rainDistort += Rain(uv * 7., t) * .5;
                return fixed4(rainDistort, 0, 0);//fixed4(uv.x, uv.y, 0, 1);
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Albedo comes from a texture tinted by color
                float scale = _Scale;

                fixed4 dx = Image(frac(IN.worldPos.zy * scale)); //* _Color;
                fixed4 dy = Image(frac(IN.worldPos.xz * scale));//
                fixed4 dz = Image(frac(IN.worldPos.xy * scale));// tex2D(_MainTex, IN.worldPos.xy) * _Color;

                float3 normal = abs(IN.normal);
                fixed4 c = normal.x * dx + normal.y * dy + normal.z * dz;

                o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;;

                //o.Emission = fixed3(1 - dot(normalize(_WorldSpaceCameraPos - IN.worldPos), IN.normal), 0,0);

                // Metallic and smoothness come from slider variables
                o.Metallic = c;
                o.Smoothness = c;
                
                //o.Albedo = IN.normal;
            }
            ENDCG
    }
}
