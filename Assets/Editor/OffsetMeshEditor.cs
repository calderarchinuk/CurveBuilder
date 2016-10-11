using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlaceOffsetMesh))]
public class PlaceOffsetMeshEditor : Editor{

	void OnSceneGUI()
	{
		
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		PlaceOffsetMesh o = target as PlaceOffsetMesh;

		if (GUILayout.Button("Place"))
		{
			o.PlacePrefabs();
		}
	}
}
