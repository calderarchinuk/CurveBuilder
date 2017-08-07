using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubicBezierPath))]
public class CubicBezierEditor : Editor
{
	void OnSceneGUI()
	{
		var c = (CubicBezierPath)target;

		if (c.pts == null || c.pts.Length == 0){return;}

		var lastMatrix = UnityEditor.Handles.matrix;
		UnityEditor.Handles.matrix = c.transform.localToWorldMatrix;

		Handles.color = Color.cyan;

		c.pts[0] = Handles.PositionHandle(c.pts[0],Quaternion.identity);
		c.pts[1] = Handles.PositionHandle(c.pts[1],Quaternion.identity);
		c.pts[2] = Handles.PositionHandle(c.pts[2],Quaternion.identity);
		c.pts[3] = Handles.PositionHandle(c.pts[3],Quaternion.identity);

		Handles.DrawLine(c.pts[0],c.pts[1]);
		Handles.DrawLine(c.pts[2],c.pts[3]);

		UnityEditor.Handles.DrawBezier(
			c.pts[0],
			c.pts[3],
			c.pts[1],
			c.pts[2],
			Color.blue,
			UnityEditor.EditorGUIUtility.whiteTexture,
			2
		);

		UnityEditor.Handles.matrix = lastMatrix;
	}
}
