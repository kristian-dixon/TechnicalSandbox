Shader "Hidden/BloomWorld"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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
			#define vec2 float2
			#define vec3 float3
			#define vec4 float4
			#define fract frac
			#define mix lerp
			#define mat2 float2x2;


			#include "UnityCG.cginc"

			uniform float4x4 _FrustumCornersES;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			uniform float4x4 _CameraInvViewMatrix;
			uniform float3 _CameraWS;
			uniform float3 _LightDir;
			uniform float4x4 _MatTorus_InvModel;
			uniform sampler2D _ColorRamp;
			uniform sampler2D _CameraDepthTexture;

			uniform int _ScreenHeight;
			uniform int _ScreenWidth;

			

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 ray : TEXCOORD1;
			};


			// This is the distance field function.  The distance field represents the closest distance to the surface
			// of any object we put in the scene.  If the given point (point p) is inside of an object, we return a
			// negative answer.simpleUnion

			v2f vert(appdata v)
			{
				v2f o;

				//Index passed via custom blit function in RaymarchGeneric
				half index = v.vertex.z;
				v.vertex.z = 0.1;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;

				#if UNITY_UV_STARTS_AT_TOP
						if (_MainTex_TexelSize.y < 0)
							o.uv.y = 1 - o.uv.y;
				#endif

				//Get eyespace view ray (normalized)
				o.ray = _FrustumCornersES[(int)index].xyz;

				//Dividing by z normalizes it in the Z axis. Therefore multiplying it by some number gives the viewspace position of the point on the ray.
				o.ray /= abs(o.ray.z);


				//Transform the ray from eyespace to worldspcae
				//Note: _CameraInvViewMatrix provided by script
				o.ray = mul(_CameraInvViewMatrix, o.ray);

				return o;
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


			fixed4 frag(v2f i) : SV_Target
			{
				float time = fmod(_Time.y * 30, 100);

				fixed3 col = tex2D(_MainTex, i.uv);
				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
				//depth = pow(Linear01Depth(depth), 1);
				depth = LinearEyeDepth(depth);

				float bloomWidth = 10;
				float bloom = smoothstep(time - bloomWidth, time, depth);

				fixed3 bloomCol = mix(SkyboxColour(i.ray, _Time.y), bloom * fixed3(0, .3, 1), step(depth, time + 1));


				col = col * (1-bloom) + bloomCol;

				// Returns final color using alpha blending
				return fixed4(col, 1.0);
			}
			ENDCG
		}
	}
}
