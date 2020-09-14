Shader "Skybox/FBM_Storm" 
{
	//FBM code taken from the Book of Shaders, mapping to a direction was mainly trial and error for my dissertation, it's a bit gross at the top but it does a decent job of it otherwise
	//TODO:: Look up how to map the shader to a sphere more nicely
	SubShader
	{
		Tags { "Queue" = "Background"  }

		Pass {
			ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define vec2 float2
			#define vec3 float3
			#define vec4 float4
			#define fract frac
			#define mix lerp
			#define mat2 float2x2;


			struct vertexInput {
			   float4 vertex : POSITION;
			   float3 texcoord : TEXCOORD0;
			   float3 normal :NORMAL;

			};

			struct vertexOutput {
			   float4 vertex : SV_POSITION;
			   float3 texcoord : TEXCOORD0;
			   float3 normal : TEXCOORD1;
			   float4 worldPos : TEXCOORD2;
			};

			vertexOutput vert(vertexInput input)
			{
			   vertexOutput output;
			   output.vertex = UnityObjectToClipPos(input.vertex);
			   output.texcoord = input.texcoord;
			   output.normal = input.normal;

			   output.worldPos = mul(unity_ObjectToWorld, input.vertex);
			   return output;
			}

			float random(in vec2 _st) 
			{
				return fract(sin(dot(_st.xy,
					vec2(12.9898, 78.233))) *
					43758.5453123);
			}

			// Based on Morgan McGuire @morgan3d
			// https://www.shadertoy.com/view/4dS3Wd
			float noise(in vec2 _st) {
				vec2 i = floor(_st);
				vec2 f = fract(_st);

				// Four corners in 2D of a tile
				float a = random(i);
				float b = random(i + vec2(1.0, 0.0));
				float c = random(i + vec2(0.0, 1.0));
				float d = random(i + vec2(1.0, 1.0));

				vec2 u = f * f * (3.0 - 2.0 * f);

				return mix(a, b, u.x) +
					(c - a) * u.y * (1.0 - u.x) +
					(d - b) * u.x * u.y;
			}

			#define NUM_OCTAVES 5
			float fbm(in vec2 _st) 
			{
				float v = 0.0;
				float a = 0.5;
				vec2 shift = vec2(100.0, 100.0);
				// Rotate to reduce axial bias
				float2x2 rot = float2x2(cos(0.5), sin(0.5),
					-sin(0.5), cos(0.50));
				for (int i = 0; i < NUM_OCTAVES; ++i) {
					v += a * noise(_st);
					_st = mul(rot, _st * 2.0 + shift);
					a *= 0.5;
				}
				return v;
			}

			float3 SkyboxColour(float3 rayDir, float t)
			{
				rayDir.z *= -1;

				float3 outColour;
				float2 st = rayDir.xy;

				float3 color = float3(0.0, 0, 0);

				float2 q = float2(0., 0);
				q.x = fbm(st + 0.00 * t);
				q.y = fbm(st + float2(1.0, 1));

				st = rayDir.zy;


				float2 r = float2(0., 0);
				r.x = fbm(st + 1.0 * q + float2(1.7, 9.2) + 0.15 * t);
				r.y = fbm(st + 1.0 * q + float2(8.3, 2.8) + 0.126 * t);

				float f = fbm(st + r);

				color = lerp(float3(1, 0.019608, 1),
					float3(0.666667, 0.666667, 0.498039),
					clamp((f * f) * 4.0, 0.0, 1.0));

				color = lerp(color,
					float3(0, 0, 0.164706),
					clamp(length(q), 0.0, 1.0));

				color = lerp(color,
					float3(0.666667, 1, 1),
					clamp(length(r.x), 0.0, 1.0));

				outColour = float4((f * f * f + .6 * f * f + .5 * f) * color, 1.);
				return outColour;
			}


			fixed4 frag(vertexOutput i) : COLOR
			{
				float3 eyeDir = (normalize((i.worldPos - _WorldSpaceCameraPos).xyz));
				fixed3 c = SkyboxColour(eyeDir, _Time.y);

				return fixed4(c, 1);
			}
			ENDCG
		 }
	}
}