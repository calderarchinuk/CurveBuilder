using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Anchor))]
public class AnchorEditor : Editor{

	void OnSceneGUI()
	{
		Anchor a = target as Anchor;

		if (a.Curve != null)
		{
			Handles.color = Color.red;
			if (a.AnchorPoint == Anchor.AnchorPointType.Start)
			{
				Handles.DrawDottedLine(a.transform.position,a.Curve.p0,4);
			}
			else
			{
				Handles.DrawDottedLine(a.transform.position,a.Curve.p3,4);
			}

			if (a.LockCurveToAnchor)
			{
				//TODO move point and relevant tangent as this moves
			}
		}
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		Anchor a = target as Anchor;
		EditorGUI.BeginDisabledGroup(a.Curve == null);

		if (GUILayout.Button("Snap Curve to Anchor"))
		{
			a.Curve.p3 = a.transform.position;
			a.Curve.p2 = a.transform.forward * 10 + a.transform.position;
			OnSceneGUI();
		}

		EditorGUI.EndDisabledGroup();
	}
}
