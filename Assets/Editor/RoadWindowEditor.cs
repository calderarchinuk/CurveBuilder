using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RoadWindowEditor : EditorWindow
{
	Anchor selectedAnchor;
	Vector3 savedPos;
	static RoadEditorSettings settings;

	float RoadMeshScale = 1;

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

		RoadMeshScale = EditorGUILayout.Slider("Curve Secion Multiplier",RoadMeshScale,0.1f,2);

		if (GUILayout.Button("Rebuild All Roads"))
		{
			RebuildAllRoads();
		}

		if (GUILayout.Button("Bake Road Meshes"))
		{
			Bake();
		}

		if (GUILayout.Button("Refresh All Props"))
		{
			RefreshProps();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty(settings);
		}

		EditorGUI.EndDisabledGroup();


	}

	void RefreshProps()
	{
		foreach(var pathmesh in FindObjectsOfType<PathMesh>())
		{
			foreach(var spawners in pathmesh.GetComponents<PlaceOffsetMesh>())
			{
				spawners.Clear();
				spawners.PlacePrefabs();
			}
		}
	}

	void Bake()
	{
		foreach (var pathMesh in FindObjectsOfType<PathMesh>())
		{
			pathMesh.Clear();
			//TODO remove old meshes from asset database!
			pathMesh.Generate();
			//AssetDatabase.CreateAsset(pathMesh.GetComponent<MeshFilter>().mesh,"Assets/RoadMesh/Road"+pathMesh.GetComponent<MeshFilter>().mesh.GetInstanceID().ToString()+".asset");
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
		var curve = go.AddComponent<CubicBezierPath>();
		var mesh = go.AddComponent<PathMesh>();
		mesh.material = settings.roadMaterial;
		mesh.es = settings.extudeShape;

		curve.pts[0] = begin.transform.position;
		curve.pts[1] = begin.transform.position + begin.transform.forward * begin.Power;
		curve.pts[2] = end.transform.position + end.transform.forward * end.Power;
		curve.pts[3] = end.transform.position;

		begin.Curve = curve;
		end.Curve = curve;

		EditorUtility.SetDirty(begin);
		EditorUtility.SetDirty(end);
		UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		Selection.activeGameObject = curve.gameObject;
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
				Selection.activeGameObject = prefab;
				showWindow = false;
			}
		}
		Handles.EndGUI();
	}

	void OnSceneGUI(SceneView sceneView) {
		// Do your drawing here using Handles.

		if (settings != null)
		{
			//redraw this window
			if (showWindow){ShowWindow();}
		}

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

		if (settings != null)
		{
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
			var curve = Selection.activeGameObject.GetComponent<CubicBezierPath>();

			if (curve)
			{
				foreach (var a in FindObjectsOfType<Anchor>())
				{
					if (a.Curve == curve)
					{
						Handles.color = Color.white;
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

	/// <summary>
	/// For each curve, if it can find a beginning and end anchor, update the curve to those anchor points
	/// </summary>
	void RebuildAllRoads()
	{
		Dictionary<CubicBezierPath,Anchor>Curves = new Dictionary<CubicBezierPath,Anchor>();
		foreach (Anchor a in Object.FindObjectsOfType<Anchor>())
		{
			CubicBezierPath curve = a.Curve;
			if (curve == null){continue;}
			if (Curves.ContainsKey(curve))
			{
				//rebuilt
				curve.pts[0] = Curves[curve].transform.position;
				curve.pts[1] = Curves[curve].transform.position + Curves[curve].transform.forward * Curves[curve].Power;
				curve.pts[2] = a.transform.position + a.transform.forward * a.Power;
				curve.pts[3] = a.transform.position;

				//TODO section count should be on the mesh generator
				//curve.SectionCount = (int)(Vector3.Distance(Curves[curve].transform.position,a.transform.position) * RoadMeshScale);

				Curves.Remove(curve);
			}
			else
			{
				Curves.Add(curve,a);
			}
		}
	}
}
