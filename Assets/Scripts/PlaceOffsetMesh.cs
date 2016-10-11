﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//based on intervals. should also have option for placing based on distance

public class PlaceOffsetMesh : MonoBehaviour
{
	CubicBezier3D curve;
	public GameObject Prefab;

	public bool LeftSide = true;
	public bool RightSide = true;
	public float OffsetDistance = 4;
	public bool SnapPrefabToGround;

	//interval
	public int Steps = 8;
	public bool SpawnByDistance = false;
	public float DistanceSteps = 10;

	[ContextMenu("debug clear")]
	public void Clear()
	{
		List<GameObject>toDestroy = new List<GameObject>();
		for(int i = 0; i< transform.childCount; i++)
		{
			toDestroy.Add(transform.GetChild(i).gameObject);
		}
		foreach (var v in toDestroy)
		{
			DestroyImmediate(v);
		}
	}

	void OnDrawGizmos()
	{
		if (curve == null)
			curve = GetComponent<CubicBezier3D>();

		if (SpawnByDistance)
		{
			Vector3[] pts = new Vector3[]{curve.p0,curve.p1,curve.p2,curve.p3};
			float totalDistance = curve.GetDistance();
			float distance = totalDistance; 

			if (DistanceSteps > 1)
			{
				while(distance > 0)
				{
					distance -= DistanceSteps;
					Vector3 startPoint = curve.GetPoint(pts,distance/totalDistance);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(startPoint,startPoint + Vector3.up*10);
				}
			}

		}
		else
		{
			Vector3[] pts = new Vector3[]{curve.p0,curve.p1,curve.p2,curve.p3};
			for (int i = 1; i<Steps+1; i++)
			{
				float t = (float)i/((float)Steps+1);
				Gizmos.color = Color.red;

				Vector3 startPoint = curve.GetPoint(pts,t);
				Vector3 normal = curve.GetNormal3D(pts,t,Vector3.up);
				Vector3 tangent = curve.GetTangent(pts,t);
				if (LeftSide)
				{
					Vector3 outPoint = Vector3.Cross(tangent,normal) * OffsetDistance;
					Gizmos.DrawLine(startPoint,startPoint + outPoint);
				}
				if (RightSide)
				{
					Vector3 outPoint = Vector3.Cross(normal,tangent) * OffsetDistance;
					Gizmos.DrawLine(startPoint,startPoint + outPoint);
				}
			}
		}
	}

	List<OrientedPoint> GetSpawnPoints()
	{
		List<OrientedPoint> returnlist = new List<OrientedPoint>();

		if (curve == null)
			curve = GetComponent<CubicBezier3D>();

		for (int i = 1; i<Steps+1; i++)
		{
			float t = (float)i/((float)Steps+1);

			Vector3[] pts = new Vector3[]{curve.p0,curve.p1,curve.p2,curve.p3};

			Vector3 startPoint = curve.GetPoint(pts,t);
			Vector3 normal = curve.GetNormal3D(pts,t,Vector3.up);
			Vector3 tangent = curve.GetTangent(pts,t);

			if (LeftSide)
			{
				returnlist.Add(new OrientedPoint(startPoint+Vector3.Cross(tangent,normal) * OffsetDistance,
					Quaternion.LookRotation(Vector3.Cross(tangent,normal))));
			}
			if (RightSide)
			{
				returnlist.Add(new OrientedPoint(startPoint+Vector3.Cross(normal,tangent) * OffsetDistance,
					Quaternion.LookRotation(Vector3.Cross(normal,tangent))));
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
			prefab.transform.parent = transform;
			if (SnapPrefabToGround)
			{
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast(point.position,Vector3.down,out hit, 1000f))
				{
					prefab.transform.position = hit.point;
				}
			}
		}
		#endif
	}
}
