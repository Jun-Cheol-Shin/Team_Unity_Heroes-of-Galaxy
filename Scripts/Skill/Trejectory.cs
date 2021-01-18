using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trejectory : MonoBehaviour
{
    public Vector3 start;
    public Vector3 target;

    Vector3 center = Vector3.zero;
    Vector3 theArc = Vector3.zero;

    public float rotationSpeed = 4.04f;

    LineRenderer lineRenderer = null;

    public void TrejectMethod()
    {
        if(lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        center = (start + target) * 0.5f;
        center.y -= 4.0f;

        //Quaternion targetRotation = Quaternion.LookRotation(center - start);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Vector3 RelCenter = start - center;
        Vector3 aimRelCenter = target - center;


        lineRenderer.positionCount = 0;
        for(float index = 0.0f, interval = -0.0417f; interval < 1.0f;)
        {
            theArc = Vector3.Slerp(RelCenter, aimRelCenter, interval += 0.0417f);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition((int)index++, theArc + center);
        }
    }
}
