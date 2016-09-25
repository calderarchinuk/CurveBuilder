using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CubicBezier3D))]
public class CubicBezierEditor : Editor{

	void OnSceneGUI()
	{
		CubicBezier3D t = target as CubicBezier3D;
		Handles.DrawBezier(t.p0,t.p3,t.p1,t.p2,Color.white,EditorGUIUtility.whiteTexture,3);
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
