//This is going to contain raymarching code that is pretty much always constant.
//SDF functions will also be included. 

struct Ray
{
    float3 origin;
    float3 dir;
};

//Setup
struct raymarch_appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct raymarch_v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
};

raymarch_v2f raymarch_vert(raymarch_appdata v)
{
    raymarch_v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}


float2 CorrectUVAspect(float2 uv)
{
    float aspect = _ScreenParams.y / _ScreenParams.x;
    float2 sv = 0.5 - uv;
    sv.y *= aspect;
    return sv;
}

Ray CreateStartingRay(float3 cameraPos, float3 fwd, float3 up, float2 uv)
{
    Ray ray;
    ray.origin = cameraPos;

    float3 right = normalize(cross(up, fwd));
    float3 dir = (cameraPos + -fwd) - cameraPos + (uv.x * right + uv.y * up) * 1;
    ray.dir = normalize(-dir);
    return ray;
}

//SDF


float sdSphere(float3 p, float s)
{
    return length(p) - s;
}