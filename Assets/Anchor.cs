using UnityEngine;
using System.Collections;


public class Anchor : MonoBehaviour
{
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
	//public Transform Target;
	public float Power = 12;

	void OnDrawGizmos()
	{
		//Gizmos.DrawWireMesh(
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position,transform.forward * Power + transform.position);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position,transform.right + transform.position);
		Gizmos.DrawLine(transform.position,-transform.right + transform.position);
	}
}
