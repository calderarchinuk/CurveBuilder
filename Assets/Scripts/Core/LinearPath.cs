using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a linear path

public class LinearPath : MonoBehaviour, IPath
{
	public Vector3[] pts = new Vector3[]{Vector3.zero,Vector3.forward};

	//returns all points along the path
	public List<OrientedPoint> EvaluatePoints (int sectionCount)
	{
		var path = new List<OrientedPoint>();

		var forward = Quaternion.LookRotation(pts[1]-pts[0]);

		for (int i = 0; i<sectionCount;i++)
		{
			var pos = Vector3.Lerp(pts[0],pts[1],(float)i/(sectionCount-1));
			path.Add(new OrientedPoint(pos,forward));
		}

		return path;
	}

	//gets the total distance of all the line sections up to this count
	public float GetDistance (int sectionCount)
	{
		List<OrientedPoint> oriented = EvaluatePoints(sectionCount);

		return Vector3.Distance(pts[0],pts[1]) * (float)sectionCount/oriented.Count;
	}

	public Vector3 GetPoint (float normalDist)
	{
		return Vector3.Lerp(pts[0],pts[1],normalDist);
	}

	public Vector3 GetTangent (float normalDist)
	{
		var forward = pts[1]-pts[0];
		//Vector3 binormal = Vector3.Cross( Vector3.up, forward ).normalized;
		//var tangent = Vector3.Cross(forward,Vector3.up);
		return forward.normalized;
		//tangent is the same for the entire length
		//return tangent.normalized;
	}
	public Vector3 GetNormal3D (float normalDist, Vector3 up)
	{
		Vector3 tangent = GetTangent( normalDist );
		Vector3 binormal = Vector3.Cross( up, tangent ).normalized;
		return Vector3.Cross( tangent, binormal );
	}
}
