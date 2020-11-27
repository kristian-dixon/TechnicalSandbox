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

//SDF -- Credit : https://iquilezles.org/www/articles/distfunctions/distfunctions.htm
float sdSphere(float3 p, float s)
{
    return length(p) - s;
}

float sdBox(float3 p, float3 b)
{
    //This works because symmetry is cool so the abs(p) bit means we only care about the top right quadrant
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}




float sdSierpinskiTetrahedron(float3 p, float3 Offset, float Scale)
{
    float3 z = p;
    int n = 0;
    while (n < 16)
    {
        //z = mul(rot, z);

        if (z.x + z.y < 0)
        {
            z.xy = -z.yx;
                        //col.rgb += float3(0.57 / 16.0, 0.2/ 16.0,0) ;
        } // fold 1

        if (z.x + z.z < 0)
        {
            z.xz = -z.zx;
                        //col.rgb += float3(0.57 / 16.0, 0.2 / 16.0, 0);

        } // fold 2

        if (z.y + z.z < 0)
        {
            z.zy = -z.yz;
                        //col.bg += float2(0.3 / 16.0, 0.75 / 8.0);
        } // fold 3	
        z = z * Scale - Offset * (Scale - 1.0);
        n++;
    }
    return (length(z)) * pow(Scale, -float(n));
}

//Basically above but with rotation per iteration
float3 sdRotatingSierpinski(float3 p, float3 Offset, float Scale, float3x3 Rot)
{
    float3 z = p;
    int n = 0;
    while (n < 16)
    {
        z = mul(Rot, z);

        if (z.x + z.y < 0)
        {
            z.xy = -z.yx;
                        //col.rgb += float3(0.57 / 16.0, 0.2/ 16.0,0) ;
        } // fold 1

        if (z.x + z.z < 0)
        {
            z.xz = -z.zx;
                        //col.rgb += float3(0.57 / 16.0, 0.2 / 16.0, 0);

        } // fold 2

        if (z.y + z.z < 0)
        {
            z.zy = -z.yz;
                        //col.bg += float2(0.3 / 16.0, 0.75 / 8.0);
        } // fold 3	
        z = z * Scale - Offset * (Scale - 1.0);
        n++;
    }
    return (length(z)) * pow(Scale, -float(n));
}