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

            // Indirection Pool, // 1 / S, // Lookup coordinates
            float4 tree_lookup(uniform sampler3D IndirPool, uniform float3 invS, uniform float N, float3 M)     
            {    
                float4 I = float4(0.0, 0.0, 0.0, 0.0);    
                float3 MND = M;      
                
                for (float i=0; i < 8; i++) 
                { 
                    // fixed # of iterations    
                    float3 P;      
                    // compute lookup coords. within current node      
                    P = (MND + floor(0.5 + I.xyz * 255.0)) * invS;      
                    // access indirection pool      
                    if (I.w < 0.9)                   
                        // already in a leaf?          
                        I =(float4)tex3D(IndirPool,P);
                    // no, continue to next depth     
                    
                    if (I.w > 0.9)    
                        // a leaf has been reached          
                        break;         
                    if (I.w < 0.1) // empty cell        
                        discard;      
                    // compute pos within next depth grid        
                        MND = MND * N;    
                }    
                return (I);  
            } 


            float4 map(float3 pos, float3 dir) {

            }


            /*
            //Sample position = bottom left corner of quad. Position = center of voxel.
            fixed4 traverseTree(Ray ray, float3 samplePosition, float3 position, float scale) 
            {
                int startX = 0; //ray.origin.x > 0 ? 1 : 0;
                int startY = 0; //ray.origin.y > 0 ? 1 : 0;
                int startZ = 0; //ray.origin.z > 0 ? 1 : 0;

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
                    
                    return treeValue;
                    


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
            */
            fixed4 trace(float2 uv) {
                Ray ray = CreateStartingRay(_CameraPos, _CameraFwd, _CameraUp, uv);
                
                fixed4 col = lerp(fixed4(0, 0, 0.5,1), fixed4(0, 0, 1, 1), max(0, dot(ray.dir, float3(0, 1, 0))));
                
                //Unit cube intersection
                float neighbourStepSize = 1 / 255.0f;
//                float3 treeValue = tex3D(_TreeTex, float3(neighbourStepSize,0,0)).xyz;



                if (aabbIntersect(ray, float3(0, 0, 0), float3(1, 1, 1))) 
                {
                    //float4 treeVal = traverseTree(ray, float3(0, 0, 0), float3(0, 0, 0), 1);
                    fixed4 col = fixed4(0, 0, 0, 0);
                    for (int i = 0; i < 200; i++) {

                        float4 surf = map(ray.origin);
                        if (dist < 0.01) {
                            return col;
                        }

                        ray.origin += ray.dir * dist;
                    }
                    
                    return fixed4(1, 1, 1, 1);
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
