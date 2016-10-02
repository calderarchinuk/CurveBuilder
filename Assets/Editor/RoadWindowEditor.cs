using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RoadWindowEditor : EditorWindow
{
	//List<Intersection>Intersections = new List<Intersection>();

	//Junction selectedJunction;
	//Junction selectedJunction2;
	Anchor selectedAnchor;
	//Material roadMaterial;
	//ExtrudeShape extrudeShape;
	Vector3 savedPos;
	//Anchor selectedAnchorB;
	RoadEditorSettings settings;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("Window/RoadEditor")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		RoadWindowEditor window = (RoadWindowEditor)EditorWindow.GetWindow (typeof (RoadWindowEditor));
		window.Show();
	}

	void OnGUI ()
	{
		//GUILayout.BeginHorizontal();

		if (GUILayout.Button("New"))
		{
			RoadEditorSettings asset = ScriptableObject.CreateInstance<RoadEditorSettings>();

			AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();

			Selection.activeObject = asset;
			settings = asset;
		}
		settings = (RoadEditorSettings)EditorGUILayout.ObjectField("Road Editor Settings",settings,typeof(RoadEditorSettings),false);


		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

		//GUILayout.EndHorizontal();
		if (settings == null){return;}
		EditorGUI.BeginDisabledGroup(settings == null);

		//GUILayout.Label ("Settings", EditorStyles.boldLabel);

		settings.roadMaterial = (Material)EditorGUILayout.ObjectField("Road Material",settings.roadMaterial,typeof(Material),false);
		settings.extudeShape = (ExtrudeShape)EditorGUILayout.ObjectField("Extrude Shape",settings.extudeShape,typeof(ExtrudeShape),false);

		ListHeader(settings.Intersections,"Intersections");
		for (int i = 0; settings.Intersections.Count > i;i++)
		{
			GUILayout.Space(10);
			settings.Intersections[i].Name = EditorGUILayout.TextField("Name",settings.Intersections[i].Name);
			settings.Intersections[i].Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab",settings.Intersections[i].Prefab,typeof(GameObject),false);
			//Intersections[i].Prefabs = editoruig
		}

		//selectedJunction = (Junction)EditorGUILayout.ObjectField(selectedJunction,typeof(Junction),true);
		//selectedJunction2 = (Junction)EditorGUILayout.ObjectField(selectedJunction2,typeof(Junction),true);

		//bool valid = selectedJunction != null && selectedJunction2 != null && selectedJunction != selectedJunction2;

		/*EditorGUI.BeginDisabledGroup(!valid);
		if (GUILayout.Button("Generate Road"))
		{
			BuildRoad();
		}
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button("Clear"))
		{
			Clear();
		}*/

		EditorGUI.EndDisabledGroup();
	}

	// Window has been selected
	void OnFocus() {
		// Remove delegate listener if it has previously
		// been assigned.
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		// Add (or re-add) the delegate.
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}

	void OnDestroy() {
		// When the window is destroyed, remove the delegate
		// so that it will no longer do any drawing.
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	void Clear()
	{
		selectedAnchor = null;
		showWindow = false;
	}

	public void ListHeader(List<Intersection> _list, string _label)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(_label);
		if (_list.Count > 0)
		{
			if (GUILayout.Button("-",GUILayout.Width(50)))
				_list.RemoveAt(_list.Count-1);
		}
		if (GUILayout.Button("+",GUILayout.Width(50)))
			_list.Add(new Intersection());
		GUILayout.EndHorizontal();
	}

	void BuildRoad(Anchor begin, Anchor end)
	{
		if (begin == null || end == null){return;}

		GameObject go = new GameObject("Road");
		var curve = go.AddComponent<CubicBezier3D>();
		var mesh = go.AddComponent<BezierMesh>();
		mesh.material = settings.roadMaterial;
		mesh.es = settings.extudeShape;


		curve.SectionCount = (int)Vector3.Distance(begin.transform.position,end.transform.position) * 2;

		curve.p0 = begin.transform.position;
		curve.p1 = begin.transform.position + begin.transform.forward * begin.Power;
		curve.p2 = end.transform.position + end.transform.forward * end.Power;
		curve.p3 = end.transform.position;

		begin.Curve = curve;
		end.Curve = curve;

		/*if (selectedAnchorA != null && selectedAnchorB != null && selectedAnchorA != selectedAnchorB)
		{
			BuildAnchoredRoad();
			return;
		}

		if (selectedJunction == null || selectedJunction2 == null || selectedJunction == selectedJunction2){return;}
		if (selectedJunction.GetComponentInChildren<Anchor>() == null){return;}
		if (selectedJunction2.GetComponentInChildren<Anchor>() == null){return;}

		Anchor highestDotAnchorA = null;
		float highestDot = -1;

		Vector3 abDirection = (selectedJunction.transform.position-selectedJunction2.transform.position).normalized;
		Vector3 baDirection = (selectedJunction2.transform.position-selectedJunction.transform.position).normalized;

		//go through each anchor child of the junction
		foreach (var a in selectedJunction.GetComponentsInChildren<Anchor>())
		{
			if (Vector3.Dot(a.transform.forward,baDirection)> highestDot)
			{
				highestDot = Vector3.Dot(a.transform.forward,baDirection);
				highestDotAnchorA = a;
			}
		}
		//pick the one with the highest dot
		//save

		Anchor highestDotAnchorB = null;
		highestDot = -1;
		foreach (var a in selectedJunction2.GetComponentsInChildren<Anchor>())
		{
			if (Vector3.Dot(a.transform.forward,abDirection)> highestDot)
			{
				highestDot = Vector3.Dot(a.transform.forward,abDirection);
				highestDotAnchorB = a;
			}
		}

		GameObject go = new GameObject("road");
		//Road r = go.AddComponent<Road>();
		//r.Anchors.Add(highestDotAnchorA);
		//r.Anchors.Add(highestDotAnchorB);

		//go through each anchor child of the junction
		//pick the one with the highest dot

		//create a road between the two anchors with high dots*/
	}
	bool showWindow;
	void ShowWindow()
	{
		Handles.BeginGUI();
		foreach (var v in settings.Intersections)
		{
			if (GUILayout.Button(v.Name))
			{
				var prefab = (GameObject)PrefabUtility.InstantiatePrefab(v.Prefab);
				prefab.transform.position = savedPos;
			}
		}
		Handles.EndGUI();
	}

	void OnSceneGUI(SceneView sceneView) {
		// Do your drawing here using Handles.

		if (showWindow){ShowWindow();}

		Event e = Event.current;
		Ray r = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

		RaycastHit hit = new RaycastHit();
		Plane zeroplane = new Plane(Vector3.up,Vector2.zero);
		float zeroplaneDistance = 0;
		Vector3 hitPoint = Vector3.zero;

		if (Physics.Raycast(r,out hit, 1000))
		{
			Debug.DrawRay(hit.point,Vector3.up);
			hitPoint = hit.point;
		}
		else if (zeroplane.Raycast(r,out zeroplaneDistance))
		{
			hitPoint = r.GetPoint(zeroplaneDistance);
		}


		if (e.type == EventType.keyDown)
		{
			if (e.keyCode == KeyCode.Escape)
			{
				Clear();
			}

			if (e.keyCode == KeyCode.I)
			showWindow = !showWindow;
			//gui popup window
		}

		foreach (var a in FindObjectsOfType<Anchor>())
		{
			if (a.Curve != null)
			{
				a.Curve.DrawCurve();
				continue;
			}
			//Handles.DrawWireDisc(j.transform.position,j.transform.up,4);
			if (Handles.Button(a.transform.position+a.transform.forward * 3.5f,Quaternion.LookRotation(Vector3.up),1,1,Handles.CircleCap))
			{
				if (selectedAnchor == null)
				{
					selectedAnchor = a;
					break;
				}
				else
				{
					BuildRoad(selectedAnchor,a);
					Clear();
				}
			}
		}

		if (!showWindow)
		{
			savedPos = hitPoint;
		}
		Handles.color = Color.blue;
		Handles.DrawWireDisc(savedPos,Vector3.up,5);

		if (selectedAnchor != null)
		{
			Handles.DrawDottedLine(selectedAnchor.transform.position,savedPos,3);
		}
	}

	void DrawAnchor(Anchor anchor)
	{

	}
		
	void DrawCurve(CubicBezier3D curve)
	{
		/*
		if (r.Anchors.Count != 2){return;}
		if (r.Anchors[0] == null || r.Anchors[1] == null){return;}
		Anchor start = r.Anchors[0];
		Anchor end = r.Anchors[1];

		Handles.DrawBezier(
			start.transform.position,
			end.transform.position,
			start.transform.forward * start.Power + start.transform.position,
			end.transform.forward * end.Power + end.transform.position,
			Color.white,
			null,
			5
		);*/
	}
}
