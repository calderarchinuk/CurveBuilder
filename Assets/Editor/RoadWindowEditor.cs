using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RoadWindowEditor : EditorWindow
{
	Anchor selectedAnchor;
	Vector3 savedPos;
	static RoadEditorSettings settings;

	[MenuItem ("Window/RoadEditor")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		RoadWindowEditor window = (RoadWindowEditor)EditorWindow.GetWindow (typeof (RoadWindowEditor));
		window.Show();
	}

	void OnGUI ()
	{
		//GUILayout.BeginHorizontal();

		if (GUILayout.Button("New Road Settings"))
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

		if (GUILayout.Button("Rebuild All Roads"))
		{
			RebuildAllRoads();
		}

		if (GUILayout.Button("Bake Road Meshes"))
		{
			Bake();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(settings);
		}

		EditorGUI.EndDisabledGroup();


	}

	void Bake()
	{
		foreach(CubicBezier3D curve in FindObjectsOfType<CubicBezier3D>())
		{
			var beziermesh = curve.GetComponent<BezierMesh>();
			if (!beziermesh){continue;}
			beziermesh.Clear();
			//scrap old meshes!
			beziermesh.Start();
			AssetDatabase.CreateAsset(beziermesh.GetComponent<MeshFilter>().mesh,"Assets/RoadMesh/Road"+beziermesh.GetComponent<MeshFilter>().mesh.GetInstanceID().ToString()+".asset");
		}
		AssetDatabase.SaveAssets();
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

		EditorUtility.SetDirty(begin);
		EditorUtility.SetDirty(end);
		UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
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
				showWindow = false;
			}
		}
		Handles.EndGUI();
	}

	void OnSceneGUI(SceneView sceneView) {
		// Do your drawing here using Handles.

		if (settings == null)
		{
			//redraw this window
			return;
		}
		if (showWindow){ShowWindow();}

		Event e = Event.current;
		Ray r = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

		RaycastHit hit = new RaycastHit();
		Plane zeroplane = new Plane(Vector3.up,Vector2.zero);
		float zeroplaneDistance = 0;
		Vector3 hitPoint = Vector3.zero;

		if (Physics.Raycast(r,out hit, 1000))
		{
			//Debug.DrawRay(hit.point,Vector3.up);
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

		if (Selection.activeGameObject != null)
		{
			var curve = Selection.activeGameObject.GetComponent<CubicBezier3D>();

			if (curve)
			{
				foreach (var a in FindObjectsOfType<Anchor>())
				{
					if (a.Curve == curve)
					{
						
						Vector3 value = Handles.Slider(a.transform.position + Vector3.up,a.transform.forward * a.Power);

						float delta = Vector3.Distance(value+Vector3.down,a.transform.position);
						if (delta > 0.01f)
						{
							a.Power = delta;
							//Debug.Log(a.Power);
							EditorUtility.SetDirty(a);
							UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
						}
						Handles.Label(a.transform.position + Vector3.up,"Power "+a.Power);
					}
				}
			}
		}
	}

	void RebuildAllRoads()
	{
		Dictionary<CubicBezier3D,Anchor>Curves = new Dictionary<CubicBezier3D,Anchor>();
		foreach (Anchor a in Object.FindObjectsOfType<Anchor>())
		{
			
			CubicBezier3D curve = a.Curve;
			if (curve == null){continue;}
			if (Curves.ContainsKey(curve))
			{
				//rebuilt
				curve.p0 = Curves[curve].transform.position;
				curve.p1 = Curves[curve].transform.position + Curves[curve].transform.forward * Curves[curve].Power;
				curve.p2 = a.transform.position + a.transform.forward * a.Power;
				curve.p3 = a.transform.position;
				Curves.Remove(curve);
			}
			else
			{
				Curves.Add(curve,a);
			}
		}
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
