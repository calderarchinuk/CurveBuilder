using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PathMesh))]
public class PathMeshEditor : Editor{

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		var b = target as PathMesh;

		if (GUI.changed)
		{
			//b.ClearPath();
		}

		if (b.SectionCount < 1)
		{
			b.SectionCount = 1;
		}

		if (GUILayout.Button("Clear Mesh"))
		{
			b.ClearMesh();
		}

		GUIContent buildmesh = new GUIContent("Rebuild Mesh");
		EditorGUI.BeginDisabledGroup(b.ExtrudeShape == null);
		if (b.ExtrudeShape == null)
		{
			buildmesh.tooltip = "Must set an Extrude Shape!";
		}
		if (GUILayout.Button(buildmesh))
		{
			b.ClearPath();
			b.ClearMesh();
			b.Generate();
		}
		EditorGUI.EndDisabledGroup();
	}
}
