using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcaVisualization : MonoBehaviour
{
    public GameObject a;
    public GameObject b;

    public float VelALength = 1f;
    public float RadiusA = 5f;
    public float VelBLength = 2f;
    public float RadiusB = 6f;
    public float TimeHorizon = 1f;

    private float InverseTimeHorizon => 1 / TimeHorizon;

    private void Update()
    {
        var aPosition = a.transform.position;
        var aVelocity = a.transform.forward * VelALength;
        DrawHelper.DrawVector(aPosition, aVelocity + aPosition, Color.blue);
        DrawHelper.DrawVector(Vector3.zero, aVelocity, Color.blue);
        
        var bPosition = b.transform.position;
        var bVelocity = b.transform.forward * VelBLength;
        DrawHelper.DrawVector(bPosition, bVelocity + bPosition, Color.red);
        DrawHelper.DrawVector(Vector3.zero, bVelocity, Color.red);

        var relativeVelocity = aVelocity - bVelocity;
        DrawHelper.DrawVector(Vector3.zero, relativeVelocity, Color.yellow);
        DrawHelper.DrawVector(bVelocity + bPosition, bVelocity + relativeVelocity + bPosition, Color.yellow);
        DrawHelper.DrawVector(bVelocity, bVelocity + relativeVelocity, Color.yellow);

        var relativePosition = bPosition - aPosition;
        relativePosition *= InverseTimeHorizon;
        DrawHelper.DrawVector(aPosition, bPosition, Color.white);
        DrawHelper.DrawVector(Vector3.zero, relativePosition, Color.white);

        var w = relativeVelocity - relativePosition;
        // DrawHelper.DrawVector(Vector3.zero, w, Color.magenta);
        DrawHelper.DrawVector(relativePosition, relativePosition + w, Color.magenta);
        // DrawHelper.DrawVector(aPosition, aPosition + w, Color.magenta);

        float dotProduct = Vector3.Dot(w, relativePosition);
        float combinedRadius = RadiusA + RadiusB;

        if (dotProduct < 0f && dotProduct > combinedRadius * w.magnitude)
        {
            Vector3 u = (combinedRadius * InverseTimeHorizon - w.magnitude) * w.normalized;
            DrawHelper.DrawVector(aVelocity, aVelocity + u, Color.green);
        }
        else
        {
            float distSq = relativePosition.sqrMagnitude;
            float a = distSq;
            float b = Vector3.Dot(relativePosition, relativeVelocity);
            float c = AbsSq(relativeVelocity) - AbsSq(Vector3.Cross(relativePosition, relativeVelocity)) / (distSq - combinedRadius);
            float t = (b + Mathf.Sqrt(b * b - a * c)) / a;
            w = relativeVelocity - t * relativePosition;
            Vector3 unitW = w.normalized;
            Vector3 u = (combinedRadius * t - w.magnitude) * unitW;
            
            DrawHelper.DrawVector(aVelocity + aPosition, aVelocity + u + aPosition, Color.green);
            DrawHelper.DrawVector(relativeVelocity, relativeVelocity + u, Color.green);
        }
    }

    private float AbsSq(Vector3 vector)
    {
        return Vector3.Dot(vector, vector);
    }

    private void OnDrawGizmos()
    {
        if (a == null || b == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(a.transform.position, RadiusA);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(b.transform.position, RadiusB);
        
        Gizmos.color = Color.white;
        float radiusSum = RadiusA + RadiusB;
        radiusSum *= InverseTimeHorizon;
        Vector3 relativePosition = (b.transform.position - a.transform.position) * InverseTimeHorizon;
        Gizmos.DrawWireSphere(
            relativePosition,
            radiusSum);

        var angle = Mathf.Acos(radiusSum / relativePosition.magnitude);
        angle *= Mathf.Rad2Deg;
        var cut1 = Quaternion.AngleAxis(angle, Vector3.up) * -relativePosition.normalized;
        cut1 *= radiusSum;
        var cut2 = Quaternion.AngleAxis(-angle, Vector3.up) * -relativePosition.normalized;
        cut2 *= radiusSum;
        Gizmos.DrawLine(Vector3.zero, cut1 + relativePosition);
        Gizmos.DrawLine(Vector3.zero, cut2 + relativePosition);
        
        Vector3 relativeVelocity = a.transform.forward * VelALength - b.transform.forward * VelBLength;
        Gizmos.color = Vector3.Distance(relativeVelocity, relativePosition) < radiusSum ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(relativeVelocity, 0.1f);
    }
}

public static class DrawHelper
{
    public static void DrawVector(Vector3 start, Vector3 end, Color color)
    {
        Vector3 direction = end - start;
        Quaternion dirRot = Quaternion.LookRotation(direction, Vector3.up);

        Vector3 left = new Vector3(-1, 0f, -1).normalized;
        Vector3 right = new Vector3(1f, 0f, -1).normalized;

        left = dirRot * left;
        right = dirRot * right;
        
        Debug.DrawLine(start, end, color, Time.deltaTime);
        Debug.DrawLine(end, end + left, color, Time.deltaTime);
        Debug.DrawLine(end, end + right, color, Time.deltaTime);
    }
}