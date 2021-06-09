using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
   public static void TriangulateQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, List<Vector3> vertices, List<int> indices)
    {
        /*bottomLeft = bottomLeft.normalized;
        bottomRight = bottomRight.normalized;
        topLeft = topLeft.normalized;
        topRight = topRight.normalized;
        */
        vertices.Add(bottomLeft);
        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomRight);


        indices.Add(vertices.Count - 4);
        indices.Add(vertices.Count - 3);
        indices.Add(vertices.Count - 2);

        indices.Add(vertices.Count - 2);
        indices.Add(vertices.Count - 1);
        indices.Add(vertices.Count - 4);
    }

}
