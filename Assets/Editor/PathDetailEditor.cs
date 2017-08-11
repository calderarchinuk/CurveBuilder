using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PathDetail))]
public class PathDetailEditor : Editor{

	void OnSceneGUI()
	{
		
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		PathDetail o = target as PathDetail;

		//place prefabs
		GUIContent placeContent = new GUIContent("Place Detail Prefabs");
		EditorGUI.BeginDisabledGroup(o.DetailPrefab == null);
		if (o.DetailPrefab == null)
		{
			placeContent.tooltip = "Detail Prefab not assigned!";
		}
		if (GUILayout.Button(placeContent))
		{
			o.PlacePrefabs();
		}
		EditorGUI.EndDisabledGroup();

		//remove children that match detail prefabs
		if (GUILayout.Button("Remove Prefab Children"))
		{
			for(int i = o.transform.childCount-1; i>=0; i--)
			{
				if (o.transform.GetChild(i).name == o.DetailPrefab.name)
				{
					DestroyImmediate(o.transform.GetChild(i).gameObject);
				}
			}
		}
	}
}
