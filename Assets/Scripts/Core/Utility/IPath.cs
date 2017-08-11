using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the path type interface. used by linear and curved paths

public interface IPath
{
	List<OrientedPoint> EvaluatePoints(int sectionCount);
	float GetDistance(int sectionCount);
	Vector3 GetPoint(float normalDist );
	Vector3 GetTangent(float normalDist ) ;
	Vector3 GetNormal3D(float normalDist, Vector3 up);
}
