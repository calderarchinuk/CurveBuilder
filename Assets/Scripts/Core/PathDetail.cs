using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//places a mesh at intervals along a path

public class PathDetail : MonoBehaviour
{
	IPath curve;
	public GameObject DetailPrefab;

	public bool LeftSide = true;
	public bool RightSide = true;
	public float OffsetDistance = 4;
	public bool SnapPrefabToGround;

	//interval
	public int Steps = 8;
	public bool SpawnByDistance = false;
	public float DistanceSteps = 10;

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
			curve = GetComponent<IPath>();

		Gizmos.matrix = transform.localToWorldMatrix;

		if (SpawnByDistance)
		{
			float totalDistance = curve.GetDistance(Steps);
			float distance = totalDistance; 

			if (DistanceSteps > 1)
			{
				while(distance > 0)
				{
					distance -= DistanceSteps;
					Vector3 startPoint = curve.GetPoint(distance/totalDistance);
					Gizmos.color = Color.red;
					Gizmos.DrawLine(startPoint,startPoint + Vector3.up*10);
				}
			}

		}
		else
		{
			for (int i = 1; i<Steps+1; i++)
			{
				float t = (float)i/((float)Steps+1);
				Gizmos.color = Color.red;

				Vector3 startPoint = curve.GetPoint(t);
				Vector3 normal = curve.GetNormal3D(t,Vector3.up);
				Vector3 tangent = curve.GetTangent(t);
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
			curve = GetComponent<IPath>();

		for (int i = 1; i<Steps+1; i++)
		{
			float t = (float)i/((float)Steps+1);

			Vector3 startPoint = transform.rotation * curve.GetPoint(t) + transform.position;
			Vector3 normal = curve.GetNormal3D(t,Vector3.up);
			Vector3 tangent = curve.GetTangent(t);

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

	public void PlacePrefabs()
	{
		#if UNITY_EDITOR
		if (DetailPrefab == null)
		{
			Debug.LogWarning("cannot place prefabs. prefab not set!");
			return;
		}

		foreach (var point in GetSpawnPoints())
		{
			var prefab = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(DetailPrefab);
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
