using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Anchor))]
public class AnchorEditor : Editor{

	void OnSceneGUI()
	{
		//Anchor a = target as Anchor;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		Anchor a = target as Anchor;
		EditorGUI.BeginDisabledGroup(a.Curve == null);

		if (GUILayout.Button("Snap Curve to Anchor"))
		{
			a.Curve.p3 = a.transform.position;
			a.Curve.p2 = a.transform.forward * a.Power + a.transform.position;
			OnSceneGUI();
		}

		EditorGUI.EndDisabledGroup();
	}
}
