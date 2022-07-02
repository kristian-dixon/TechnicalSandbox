using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PipeGenerator : MonoBehaviour
{
    MeshFilter filter;
    Mesh mesh;

    public int circleResolution = 6;
    public float radius = 1f;
    public float endCapPadding = 0.2f;

    private void OnValidate()
    {
        Debug.Log("HELLO");
    }

    private void Update()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).hasChanged)
            {
                transform.GetChild(i).hasChanged = false;
                return;
            }
        }
    }

    void GenerateMesh()
    {
        mesh = new Mesh();

    }

    private void OnDrawGizmos()
    {
        var childCount = transform.childCount;

        for (int i = 1; i < childCount; i++)
        {
            var previousTransform = transform.GetChild(i - 1);
            var currentTransform = transform.GetChild(i);
            Vector3 lineDirection; float lineLength;
            GetLineInfo(previousTransform, currentTransform, out lineDirection, out lineLength);

            Vector3 lineNormal = GetLineNormal(lineDirection);
            var lineTangent = Vector3.Cross(lineDirection, lineNormal).normalized;

            var startCapPosition = previousTransform.position + (lineDirection * (radius + endCapPadding));
            var endCapPosition = currentTransform.position - (lineDirection * (radius + endCapPadding));

            var startStraightPoints = GetCirclePoints(startCapPosition, lineNormal, lineTangent, radius, circleResolution);
            var endStraightPoints = GetCirclePoints(endCapPosition, lineNormal, lineTangent, radius, circleResolution);


            Gizmos.DrawLine(startStraightPoints[circleResolution - 1], startStraightPoints[0]);
            Gizmos.DrawLine(endStraightPoints[circleResolution - 1], endStraightPoints[0]);
            Gizmos.DrawLine(startStraightPoints[0], endStraightPoints[0]);
            for (int j = 1; j < startStraightPoints.Length; j++)
            {
                Gizmos.DrawLine(startStraightPoints[j - 1], startStraightPoints[j]);
                Gizmos.DrawLine(endStraightPoints[j - 1], endStraightPoints[j]);
                Gizmos.DrawLine(startStraightPoints[j], endStraightPoints[j]);
            }


            if (i + 1 < childCount)
            {
                var nextTransform = transform.GetChild(i + 1);
                Vector3 nextLineDirection = Vector3.zero;
                var nextLength = 0f;

                GetLineInfo(currentTransform, nextTransform, out nextLineDirection, out nextLength);
                var nextNormal = GetLineNormal(nextLineDirection); var nextTangent = Vector3.Cross(nextLineDirection, nextNormal).normalized;

                //Recalculated for curve
                var currentEndCapPoints = GetCirclePoints(currentTransform.position - lineDirection * (radius), lineNormal, lineTangent, radius * 0.9f, circleResolution);
                var nextEndCapPoints = GetCirclePoints(currentTransform.position + nextLineDirection * (radius), nextNormal, nextTangent, radius * 0.9f, circleResolution);
                
                for(int j = 0; j < circleResolution; j++)
                {
                    var interA = Vector3.Dot((currentEndCapPoints[j] - nextEndCapPoints[j]), lineNormal);
                    var interB = Vector3.Dot(nextLineDirection, lineNormal);

                    //Assuming it intersects for now
                    var dist = interA / interB;



                    var a = currentEndCapPoints[j];
                    var b = nextEndCapPoints[j] + nextLineDirection * dist;
                    var c = nextEndCapPoints[j];

                    Vector3 prevPos = a;

                    for (int k = 0; k < 4; k++)
                    {
                        var d = Vector3.Lerp(a, b, k / 4f);
                        var e = Vector3.Lerp(b, c, k / 4f);
                        var p = Vector3.Lerp(d, e, k / 4f);

                        Gizmos.DrawLine(prevPos, p);
                        prevPos = p;
                    }
                    Gizmos.DrawLine(prevPos, c);


                }


                

            }
        }
    }

    private static void GetLineInfo(Transform previousTransform, Transform currentTransform, out Vector3 lineDirection, out float lineLength)
    {
        var line = currentTransform.position - previousTransform.position;
        lineDirection = line.normalized;
        lineLength = line.magnitude;
    }

    private static Vector3 GetLineNormal(Vector3 lineDirection)
    {
        Vector3 spatialUp = Vector3.up;
        if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(spatialUp, lineDirection)), 1)) { spatialUp = Vector3.forward; }

        var lineRight = Vector3.Cross(lineDirection, spatialUp).normalized;
        return lineRight;
    }

    private static Vector3[] GetCirclePoints(Vector3 position, Vector3 normal, Vector3 tangent, float radius, int resolution)
    {
        var output = new Vector3[resolution];
        float anglePerIteration = Mathf.Deg2Rad * (360f / resolution);

        for (int j = 0; j < resolution; j++)
        {
            Vector3 startPoint = position + normal * Mathf.Cos(j * anglePerIteration) * radius + tangent * Mathf.Sin(j * anglePerIteration) * radius;
            output[j] = startPoint;
        }
        return output;
    }
}
