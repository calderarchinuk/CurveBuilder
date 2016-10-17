using UnityEngine;
using System.Collections;

//used by the roadbuilder to rebuild curves to correct intersection points

public class Anchor : MonoBehaviour
{
	public CubicBezier3D Curve;
	//public bool LockCurveToAnchor = true;

	public float Power = 10;

	private static Mesh _anchorMesh;
	public static Mesh AnchorMesh
	{
		get
		{
			if (_anchorMesh == null)
			{
				_anchorMesh = Resources.Load<Mesh>("AnchorMesh");
			}
			return _anchorMesh;

		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position,transform.forward * Power + transform.position);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position,transform.right + transform.position);
		Gizmos.DrawLine(transform.position,-transform.right + transform.position);
	}
}
