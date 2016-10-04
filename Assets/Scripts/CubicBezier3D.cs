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
public class CubicBezier3D : MonoBehaviour {

	public int SectionCount = 20;
	public Vector3 p0;
	public Vector3 p1 = Vector3.forward*1;
	public Vector3 p2 = Vector3.forward*2;
	public Vector3 p3 = Vector3.forward*3;


	public bool UpdateCurve = false;

	//TODO if this curve isn't being update, cache these points
	public List<OrientedPoint> EvaluatePoints()
	{
		Vector3[] pts = new Vector3[]{p0,p1,p2,p3};
		List<OrientedPoint> path = new List<OrientedPoint>();
		for (int i = 0; i<= SectionCount; i++)
		{
			path.Add(new OrientedPoint(GetPoint(pts,i/(float)SectionCount),GetOrientation3D(pts,i/(float)SectionCount,Vector3.up)));
		}
		return path;
	}

	public Vector3 GetPoint( Vector3[] pts, float t ) {
		float omt = 1f-t;
		float omt2 = omt * omt;
		float t2 = t * t;
		return	pts[0] * ( omt2 * omt ) +
			pts[1] * ( 3f * omt2 * t ) +
			pts[2] * ( 3f * omt * t2 ) +
			pts[3] * ( t2 * t );
	}

	public Vector3 GetTangent( Vector3[] pts, float t ) {
		float omt = 1f-t;
		float omt2 = omt * omt;
		float t2 = t * t;
		Vector3 tangent = 
			pts[0] * ( -omt2 ) +
			pts[1] * ( 3 * omt2 - 2 * omt ) +
			pts[2] * ( -3 * t2 + 2 * t ) +
			pts[3] * ( t2 );
		return tangent.normalized;
	}
	public Vector3 GetNormal2D( Vector3[] pts, float t ) {
		Vector3 tng = GetTangent( pts, t );
		return new Vector3( -tng.y, tng.x, 0f );
	}

	public Vector3 GetNormal3D( Vector3[] pts, float t, Vector3 up ) {
		Vector3 tng = GetTangent( pts, t );
		Vector3 binormal = Vector3.Cross( up, tng ).normalized;
		return Vector3.Cross( tng, binormal );
	}
	public Quaternion GetOrientation2D( Vector3[] pts, float t ) {
		Vector3 tng = GetTangent( pts, t );
		Vector3 nrm = GetNormal2D( pts, t );
		return Quaternion.LookRotation( tng, nrm );
	}

	public Quaternion GetOrientation3D( Vector3[] pts, float t, Vector3 up ) {
		Vector3 tng = GetTangent( pts, t );
		Vector3 nrm = GetNormal3D( pts, t, up );
		return Quaternion.LookRotation( tng, nrm );
	}

	Color editorColor = new Color(1f,1f,0.5f,0.3f);
	public void DrawCurve()
	{
		#if UNITY_EDITOR

		UnityEditor.Handles.DrawBezier(
			p0,
			p3,
			p1,
			p2,
			editorColor,
			UnityEditor.EditorGUIUtility.whiteTexture,
			5
		);

		#endif
	}

	void OnDrawGizmos()
	{
		
	}
}
