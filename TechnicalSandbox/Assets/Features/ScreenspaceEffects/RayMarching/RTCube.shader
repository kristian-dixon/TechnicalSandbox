///A quick implementation to get back up to speed with Raymarching. Simple infinitely repeating spheres that have a bloom effect.
// Future improvements -- The bloom is currently fixed into each sphere's domain, allowing the bloom to pass through each boundary 
// would potentially make it cooler :)

Shader "PostProcessing/Raytracing/Cube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TreeTex ("Tree Texture", 3D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex raymarch_vert
            #pragma fragment frag

            #pragma exclude_renderers d3d11_9x
            #pragma exclude_renderers d3d9

            #include "UnityCG.cginc"
            #include "Raymarch.cginc"
            
            sampler3D _TreeTex;
            float4 _TreeTex_ST;
            uniform float3 _CameraFwd;
            uniform float3 _CameraUp;
            uniform float3 _CameraPos;
           
            

            sampler2D _MainTex;

            bool aabbIntersect(Ray ray, float3 position, float3 scale) {
                float x0 = ((position.x + -scale.x) - ray.origin.x) / ray.dir.x;
                float x1 = ((position.x + scale.x) - ray.origin.x) / ray.dir.x;
                float xMin = min(x0, x1);
                float xMax = max(x0, x1);

                float y0 = ((position.y + -scale.y) - ray.origin.y) / ray.dir.y;
                float y1 = ((position.y + scale.y) - ray.origin.y) / ray.dir.y;
                float yMin = min(y0, y1);
                float yMax = max(y0, y1);

                float z0 = ((position.z + -scale.z) - ray.origin.z) / ray.dir.z;
                float z1 = ((position.z + scale.z) - ray.origin.z) / ray.dir.z;
                float zMin = min(z0, z1);
                float zMax = max(z0, z1);

                if (  max(xMin, max(yMin, zMin)) < min(xMax, min(yMax, zMax))  )
                {
                    return true;
                }

                return false;
            }



           
            //Sample position = bottom left corner of quad. Position = center of voxel.
            fixed4 traverseTree3(Ray ray, float3 samplePosition, float3 position, float scale)
            {
                int startX = ray.origin.x > 0 ? 1 : 0;
                int startY = ray.origin.y > 0 ? 1 : 0;
                int startZ = ray.origin.z > 0 ? 1 : 0;

                float neighbourStepSize = 1 / 255.0f;

                //This is shit
                float4 texVals[8] = {
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 1)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 1))
                };

                //Getting around loop unrolling in the most horrible possible way
                int x = startX; int y = startX; int z = startX;

                float3 boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }

                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                }

                x = startX; y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                }

                x = startX; y = startY; z = 1 - startZ * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                  
                }

                y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                 
                }

                x = startX;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                 
                }

                return fixed4(0, 0, 0, 1);
            }




            //Sample position = bottom left corner of quad. Position = center of voxel.
            fixed4 traverseTree2(Ray ray, float3 samplePosition, float3 position, float scale)
            {
                int startX = ray.origin.x > 0 ? 1 : 0;
                int startY = ray.origin.y > 0 ? 1 : 0;
                int startZ = ray.origin.z > 0 ? 1 : 0;

                float neighbourStepSize = 1 / 255.0f;

                //This is shit
                float4 texVals[8] = {
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 1)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 1))
                };

                //Getting around loop unrolling in the most horrible possible way
                int x = startX; int y = startX; int z = startX;

                float3 boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX; y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX; y = startY; z = 1 - startZ * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree3(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                return fixed4(0, 0, 0, 1);
            }


            //Sample position = bottom left corner of quad. Position = center of voxel.
            fixed4 traverseTree(Ray ray, float3 samplePosition, float3 position, float scale) 
            {
                int startX = ray.origin.x > 0 ? 1 : 0;
                int startY = ray.origin.y > 0 ? 1 : 0;
                int startZ = ray.origin.z > 0 ? 1 : 0;

                float neighbourStepSize = 1 / 255.0f;

                //This is shit
                float4 texVals[8] = {
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 0)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 0)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 0, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 0, neighbourStepSize * 1)),

                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 0, neighbourStepSize * 1, neighbourStepSize * 1)),
                    tex3D(_TreeTex, samplePosition + float3(neighbourStepSize * 1, neighbourStepSize * 1, neighbourStepSize * 1))
                };

                /*for (int x = startX; x > -1 && x < 2; x += 1 - startX * 2)
                {
                    for (int y = startY; y > -1 && y < 2; y += 1 - startY * 2)
                    {
                        for (int z = startZ; z > -1 && z < 2; z += 1 - startZ * 2)
                        {
                            

                        }
                    }
                }*/

                //Getting around loop unrolling in the most horrible possible way
                int x = startX; int y = startY; int z = startZ;

                float3 boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x += 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX; y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX; y = startY; z = 1 - startZ * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = 1 - startX * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                y = 1 - startY * 2 - startY * 2;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                x = startX;
                boundingCentre = position + float3(-0.5 + 1 * x, -0.5 + 1 * y, -0.5 + 1 * z) * scale;
                if (aabbIntersect(ray, boundingCentre, scale * 0.5)) {
                    float4 treeValue = texVals[x + y * 2 + z * 4];
                    if (treeValue.w > 0.5)
                    {
                        return treeValue;
                    }


                    fixed4 childVal = traverseTree2(ray, treeValue.xyz, boundingCentre, scale * 0.5);
                    if (childVal.w > 0.5 && childVal.r > 0.1)
                    {
                        return childVal;
                    }
                }

                return fixed4(0, 0, 0, 1);
            }

            fixed4 trace(float2 uv) {
                Ray ray = CreateStartingRay(_CameraPos, _CameraFwd, _CameraUp, uv);
                
                fixed4 col = lerp(fixed4(0, 0, 0.5,1), fixed4(0, 0, 1, 1), max(0, dot(ray.dir, float3(0, 1, 0))));
                
                //Unit cube intersection
                float neighbourStepSize = 1 / 255.0f;
                float3 treeValue = tex3D(_TreeTex, float3(neighbourStepSize,0,0)).xyz;
                
               
                //if (aabbIntersect(ray, float3(0, 0, 0), float3(1, 1, 1))) 
                {
                    float4 treeVal = traverseTree(ray, float3(0, 0, 0), float3(0, 0, 0), 1);

                    if (treeVal.r > 0.1)
                    {
                        return treeVal;
                    }
                }

                
                return col;//fixed4(1, 0, 0, 1);
            }

            fixed4 frag (raymarch_v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
            
                //Get aspect ratio corrected UV coordinates & centre the coordinate system about the middle of the screen.
                float2 uv = CorrectUVAspect(i.uv);
                col = trace(uv);

                return col;
            }
            ENDCG
        }
    }
}
