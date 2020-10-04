Shader "Hidden/FadeToSkyboxFogEffect"
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
			uniform sampler2D _SkyTexture;

			uniform int _ScreenHeight;
			uniform int _ScreenWidth;
			uniform float _WorldRadius;
			

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
				o.ray = mul(_CameraInvViewMatrix, o.ray);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float time = _WorldRadius; //fmod(_Time.y * 30, 100);

				fixed3 col = tex2D(_MainTex, i.uv);
				float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
				depth = LinearEyeDepth(depth);

				float bloomWidth = 100;
				float bloom = smoothstep(time, time + bloomWidth, depth);

				fixed3 skyColour = tex2D(_SkyTexture, i.uv);
				fixed3 bloomCol = mix(col, skyColour,  pow(bloom, 2));
				col = bloomCol;

				// Returns final color using alpha blending
				return fixed4(col, 1.0);
			}
			ENDCG
		}
	}
}
