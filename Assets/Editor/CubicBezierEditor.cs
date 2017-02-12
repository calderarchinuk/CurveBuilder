using UnityEngine;
using System.Collections;
using UnityEditor;

//[CustomEditor(typeof(CubicBezier3D))]
public class CubicBezierEditor : Editor{

	void OnSceneGUI()
	{
		

		//CubicBezier3D t = target as CubicBezier3D;

		/*for (int i = 0; i<t.pts.Length; i++)
		{
			t.pts[i] = Handles.PositionHandle(t.pts[i],Quaternion.identity);
		}*/

		//Handles.draw(t.pts[0],t.pts[3]+Vector3.up);

		//return;
		/*
		Handles.DrawBezier(t.p0,t.p3,t.p1,t.p2,Color.white,EditorGUIUtility.whiteTexture,3);

		

		Handles.color = Color.green;
		Handles.DrawLine(t.p0,t.p1);
		Handles.DrawLine(t.p2,t.p3);

		var points = t.EvaluatePoints(10);

		for (int i = 0; i<points.Count; i++)
		{
			Handles.DrawLine(points[i].position,points[i].position+Vector3.up);
		}*/
	}
}
