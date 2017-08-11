using UnityEngine;
using System.Collections;

//used by the roadbuilder to rebuild curves to correct intersection points

public class Anchor : MonoBehaviour
{
	public GameObject Path;
	public float Power = 20;

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

	/// <summary>
	/// rebuilds the curve using this anchor's position and the other anchor's position
	/// </summary>
	public void ForceDirection (bool updateOther)
	{
		var curve = Path.GetComponent<CubicBezierPath>();
		if (curve == null) {return;}

		if (Vector3.Distance(curve.pts[0],transform.position) < Vector3.Distance(curve.pts[3],transform.position))
		{
			curve.pts[0] = transform.position;
			curve.pts[1] = curve.pts[0] + transform.forward * Power;
		}
		else
		{
			curve.pts[3] = transform.position;
			curve.pts[2] = curve.pts[3] + transform.forward * Power;
		}

		if (!updateOther){return;}

		foreach(var otherAnchor in FindObjectsOfType<Anchor>())
		{
			if (otherAnchor == this){continue;}
			if (otherAnchor.Path == Path)
			{
				otherAnchor.ForceDirection(false);
				break;
			}
		}
	}

	Color LastColor;
	public void SetGizmoColor(Color color)
	{
		LastColor = color;
	}

	public void DrawCustomGizmos()
	{
		Gizmos.color = LastColor;
		Gizmos.DrawLine(transform.position,transform.forward * Power + transform.position);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position,transform.right + transform.position);
		Gizmos.DrawLine(transform.position,-transform.right + transform.position);
	}
}
