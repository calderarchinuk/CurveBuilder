using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaceOffsetMesh : MonoBehaviour
{
	CubicBezier3D curve;
	public int steps = 8;
	public bool left = true;
	public bool right = true;
	public float offset = 4;

	public GameObject Prefab;

	public void Start()
	{
		
	}

	public void Clear()
	{
		//
	}

	void OnDrawGizmos()
	{
		if (curve == null)
			curve = GetComponent<CubicBezier3D>();

		for (int i = 0; i<steps; i++)
		{
			float t = (float)i/(float)steps;
			Gizmos.color = Color.red;

			Vector3[] pts = new Vector3[]{curve.p0,curve.p1,curve.p2,curve.p3};

			if (left)
			{
				Gizmos.DrawLine(curve.GetPoint(pts,t),curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.left) * offset);
			}
			if (right)
			{
				Gizmos.DrawLine(curve.GetPoint(pts,t),curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.right) * offset);
			}
		}
	}

	List<OrientedPoint> GetSpawnPoints()
	{
		List<OrientedPoint> returnlist = new List<OrientedPoint>();

		if (curve == null)
			curve = GetComponent<CubicBezier3D>();

		for (int i = 0; i<steps; i++)
		{
			float t = (float)i/(float)steps;

			Vector3[] pts = new Vector3[]{curve.p0,curve.p1,curve.p2,curve.p3};

			if (left)
			{
				returnlist.Add(new OrientedPoint(curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.left) * offset,
					Quaternion.LookRotation(curve.GetPoint(pts,t) - curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.left) * offset)));
			}
			if (right)
			{
				returnlist.Add(new OrientedPoint(curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.right) * offset,
					Quaternion.LookRotation(curve.GetPoint(pts,t) - curve.GetPoint(pts,t) + curve.GetNormal3D(pts,t,Vector3.right) * offset)));
			}
		}
		return returnlist;
	}

	[ContextMenu("debug spawn")]
	public void PlacePrefabs()
	{
		#if UNITY_EDITOR
		foreach (var point in GetSpawnPoints())
		{
			var prefab = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(Prefab);
			prefab.transform.position = point.position;
			prefab.transform.rotation = point.rotation;
		}
		#endif
	}
}
