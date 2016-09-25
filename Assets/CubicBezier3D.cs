using UnityEngine;
using System.Collections;

public class CubicBezier3D : MonoBehaviour {

	public Vector3 p0,p1,p2,p3;

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
}
