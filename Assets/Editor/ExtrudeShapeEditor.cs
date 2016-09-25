using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ExtrudeShape))]
public class ExtrudeShapeEditor : Editor {

	public override void OnInspectorGUI ()
	{
		//TODO set correct number of Us, normal, lines based on vert2d count
		base.OnInspectorGUI ();
	}

	void OnEnable()
	{
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}

	void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
	}

	void OnSceneGUI(SceneView sceneView)
	{
		ExtrudeShape es = target as ExtrudeShape;

		for (int i = 0; i<es.vert2Ds.Length;i++)
		{
			Vector3 inPos = new Vector3(es.vert2Ds[i].x,es.vert2Ds[i].y,0);
			inPos = Handles.PositionHandle(inPos,Quaternion.identity);
			es.vert2Ds[i] = new Vector2(inPos.x,inPos.y);
		}

		Vector3[] vert3ds= new Vector3[es.vert2Ds.Length];
		for (int i = 0; i<es.vert2Ds.Length;i++)
		{
			vert3ds[i] = new Vector3(es.vert2Ds[i].x,es.vert2Ds[i].y,0);
		}

		Handles.color = Color.blue;
		for (int i = 0; i<es.normals.Length;i++)
		{
			Handles.DrawDottedLine(AsVector3(es.vert2Ds[i]),AsVector3(es.normals[i]) + AsVector3(es.vert2Ds[i]),3);
			//vert3ds[i] = new Vector3(es.vert2Ds[i].x,es.vert2Ds[i].y,0);
		}
		Handles.color = Color.white;

		Handles.DrawPolyLine(vert3ds);
	}

	Vector3 AsVector3(Vector2 vector2)
	{
		return new Vector3(vector2.x,vector2.y,0);
	}
}
