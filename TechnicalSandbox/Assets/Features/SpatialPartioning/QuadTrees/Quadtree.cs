using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadtree : MonoBehaviour
{
    public float rootScale = 50;
    public int maxDepth = 10;
    public int numberPoints;
    
    List<Vector2> points;

    // Start is called before the first frame update
    void Start()
    {
        points = new List<Vector2>();
        for(int i = 0; i < numberPoints; i++)
        {
            points.Add(new Vector2(Random.Range(0, rootScale), Random.Range(0, rootScale)));
        }

        Debug.Log("Points" + points.Count);
    }

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        if (points == null) return;


        Run(0, points, Vector2.one * rootScale / 2, rootScale);

        for(int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawSphere(points[i], 0.25f);
        }
    }

    void Run(int currentDepth, List<Vector2> pointsInSegment, Vector2 centre, float size)
    {
        if (currentDepth > maxDepth)
            return;


        Gizmos.DrawWireCube(centre, Vector3.one * size);
        if (pointsInSegment.Count <= 1) return;

        //This approach needs changing to make it more kind to memory. 

        //Split points into their correect segments
        var tl = new List<Vector2>();
        var tr = new List<Vector2>();
        var bl = new List<Vector2>();
        var br = new List<Vector2>();

        var tlCentre = centre + new Vector2(-size / 4f, -size / 4f);
        var trCentre = centre + new Vector2(size / 4f, -size / 4f);
        var blCentre = centre + new Vector2(-size / 4f, size / 4f);
        var brCentre = centre + new Vector2(size / 4f, size / 4f);

        for(int i = 0; i < pointsInSegment.Count; i++)
        {
            var distFromCenter = tlCentre - pointsInSegment[i];
            if(Mathf.Abs(distFromCenter.x) < size / 4f && Mathf.Abs(distFromCenter.y) < size / 4f)
            {
                tl.Add(pointsInSegment[i]);
                continue;
            }

            distFromCenter = trCentre - pointsInSegment[i];
            if (Mathf.Abs(distFromCenter.x) < size / 4f && Mathf.Abs(distFromCenter.y) < size / 4f)
            {
                tr.Add(pointsInSegment[i]);
                continue;
            }

            distFromCenter = blCentre - pointsInSegment[i];
            if (Mathf.Abs(distFromCenter.x) < size / 4f && Mathf.Abs(distFromCenter.y) < size / 4f)
            {
                bl.Add(pointsInSegment[i]);
                continue;
            }

            distFromCenter = brCentre - pointsInSegment[i];
            if (Mathf.Abs(distFromCenter.x) < size / 4f && Mathf.Abs(distFromCenter.y) < size / 4f)
            {
                br.Add(pointsInSegment[i]);
                continue;
            }
        }

        Run(currentDepth + 1, tl, tlCentre, size / 2f);

        Run(currentDepth + 1, tr, trCentre, size / 2f);

        Run(currentDepth + 1, bl, blCentre, size / 2f);

        Run(currentDepth + 1, br, brCentre, size / 2f);
    }

}
