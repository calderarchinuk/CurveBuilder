using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CubicBezier3D))]
public class CubicBezierEditor : Editor{

	void OnSceneGUI()
	{
		CubicBezier3D t = target as CubicBezier3D;
		/*
		Handles.DrawBezier(t.p0,t.p3,t.p1,t.p2,Color.white,EditorGUIUtility.whiteTexture,3);

		t.p0 = Handles.PositionHandle(t.p0,Quaternion.identity);
		t.p1 = Handles.PositionHandle(t.p1,Quaternion.identity);
		t.p2 = Handles.PositionHandle(t.p2,Quaternion.identity);
		t.p3 = Handles.PositionHandle(t.p3,Quaternion.identity);

		Handles.color = Color.green;
		Handles.DrawLine(t.p0,t.p1);
		Handles.DrawLine(t.p2,t.p3);
		*/
		var points = t.EvaluatePoints();

		for (int i = 0; i<points.Count; i++)
		{
			Handles.DrawLine(points[i].position,points[i].position+Vector3.up);
		}
	}
}
