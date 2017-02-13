using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//https://www.youtube.com/watch?v=o9RK6O2kOKo
//https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gc41ce114c_1_31

//http://blog.meltinglogic.com/2013/12/how-to-generate-procedural-racetracks/
//http://gamedev.stackexchange.com/questions/75182/how-can-i-create-or-extrude-a-mesh-along-a-spline

/// <summary>
/// hold the description of the curve and the functions to evaluate this curve
/// </summary>
public class CubicBezierPath : MonoBehaviour, IPath
{
	public Vector3[] pts = new Vector3[4]{Vector3.zero,Vector3.forward*1,Vector3.forward*2,Vector3.forward*3};

	//TODO if this curve isn't being update, cache these points
	public List<OrientedPoint> EvaluatePoints(int sectionCount)
	{
		List<OrientedPoint> path = new List<OrientedPoint>();
		for (int i = 0; i<= sectionCount; i++)
		{
			path.Add(new OrientedPoint(GetPoint(i/(float)sectionCount),GetOrientation3D(i/(float)sectionCount,Vector3.up)));
		}
		return path;
	}

	//gets the total distance of all the line sections up to this section count
	public float GetDistance(int sectionCount)
	{
		float totalLength = 0f;
		Vector3 prev = pts[0];
		List<OrientedPoint> oriented = EvaluatePoints(sectionCount);
		List<Vector3> points = new List<Vector3>();
		for( int i = 0; i < sectionCount; i++ )
		{
			float t = ( (float)i ) / sectionCount;

			foreach(var v in oriented)
			{
				points.Add(v.position);
			}

			Vector3 pt = GetPoint( t );
			float diff = ( prev - pt ).magnitude;
			totalLength += diff;
			prev = pt;
		}
		return totalLength;
	}

	public Vector3 GetPoint(float normalDist ) {
		float omNormalDist = 1f-normalDist;
		float omNormalDistSqr = omNormalDist * omNormalDist;
		float normalDistSqr = normalDist * normalDist;

		return
			pts[0] * ( omNormalDistSqr * omNormalDist ) +
			pts[1] * ( 3f * omNormalDistSqr * normalDist ) +
			pts[2] * ( 3f * omNormalDist * normalDistSqr ) +
			pts[3] * ( normalDistSqr * normalDist );
	}

	public Vector3 GetTangent(float normalDist ) {
		float omNormalDist = 1f-normalDist;
		float omNormalDistSqr = omNormalDist * omNormalDist;
		float normalDistSqr = normalDist * normalDist;
		Vector3 tangent = 
			pts[0] * ( -omNormalDistSqr ) +
			pts[1] * ( 3 * omNormalDistSqr - 2 * omNormalDist ) +
			pts[2] * ( -3 * normalDistSqr + 2 * normalDist ) +
			pts[3] * ( normalDistSqr );
		return tangent.normalized;
	}

	public Vector3 GetNormal2D(float normalDist ) {
		Vector3 tangent = GetTangent( normalDist );
		return new Vector3( -tangent.y, tangent.x, 0f );
	}

	public Vector3 GetNormal3D(float normalDist, Vector3 up ) {
		Vector3 tangent = GetTangent( normalDist );
		Vector3 binormal = Vector3.Cross( up, tangent ).normalized;
		return Vector3.Cross( tangent, binormal );
	}
	public Quaternion GetOrientation2D(float normalDist ) {
		Vector3 tangent = GetTangent( normalDist );
		Vector3 normal = GetNormal2D( normalDist );
		return Quaternion.LookRotation( tangent, normal );
	}

	public Quaternion GetOrientation3D(float normalDist, Vector3 up ) {
		Vector3 tangent = GetTangent( normalDist );
		Vector3 normal = GetNormal3D( normalDist, up );
		return Quaternion.LookRotation( tangent, normal );
	}

	void OnDrawGizmos()
	{
		DrawCurve();
	}

	#if UNITY_EDITOR
	Color editorColor = new Color(0.5f,1f,0.5f,0.3f);
	#endif
	public void DrawCurve()
	{
		#if UNITY_EDITOR

		Gizmos.matrix = transform.localToWorldMatrix;
		//UnityEditor.Handles.matrix = transform.localToWorldMatrix;

		Gizmos.color = editorColor;
		Gizmos.DrawRay(pts[0],Vector3.up);
		Gizmos.DrawRay(pts[3],Vector3.up);

		var lastMatrix = UnityEditor.Handles.matrix;
		UnityEditor.Handles.matrix = transform.localToWorldMatrix;

		UnityEditor.Handles.DrawBezier(
			pts[0],
			pts[3],
			pts[1],
			pts[2],
			editorColor,
			UnityEditor.EditorGUIUtility.whiteTexture,
			5
		);

		UnityEditor.Handles.matrix = lastMatrix;

		#endif
	}
}
