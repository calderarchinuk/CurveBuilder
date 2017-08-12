using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RoadWindowEditor : EditorWindow
{
	Anchor selectedAnchor;
	Vector3 savedPos;
	public static RoadEditorSettings Settings;
	public bool IntersectionsFoldout;
	public Vector2 IntersectionCanvas;

	int SelectedRoadTile = 0;

	int TileBrowserGridColumns = 3;

	bool AutoAddIntersect = false;
	bool placingIntersection = false;
	Vector3 lastIntersectionPos;

	[MenuItem ("Window/RoadEditor")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		RoadWindowEditor window = (RoadWindowEditor)EditorWindow.GetWindow (typeof (RoadWindowEditor));
		window.Show();
	}

	void OnGUI ()
	{
		if (GUILayout.Button("New Road Settings"))
		{
			NewRoadSettings();
		}
		Settings = (RoadEditorSettings)EditorGUILayout.ObjectField("Road Editor Settings",Settings,typeof(RoadEditorSettings),false);

		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

		if (Settings == null)
		{
			var path = AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString("LastRoadSettingsGUID"));
			Settings = AssetDatabase.LoadAssetAtPath<RoadEditorSettings>(path);
			if (Settings == null){return;}
		}

		Repaint();

		Settings.automaticallyBuildRoads = EditorGUILayout.Toggle("Automaticall Build Roads",Settings.automaticallyBuildRoads);
		Settings.roadsHaveMeshColliders = EditorGUILayout.Toggle("Automaticall Add Mesh Colliders",Settings.roadsHaveMeshColliders);
		Settings.roadMaterial = (Material)EditorGUILayout.ObjectField("Road Material",Settings.roadMaterial,typeof(Material),false);
		Settings.extudeShape = (ExtrudeShape)EditorGUILayout.ObjectField("Extrude Shape",Settings.extudeShape,typeof(ExtrudeShape),false);
		Settings.heightOffset = EditorGUILayout.Slider("Intersection Height Offset",Settings.heightOffset,0,1);

		AutoAddIntersect = EditorGUILayout.Toggle("Auto Add Intersect",AutoAddIntersect);

		IntersectionsFoldout = EditorGUILayout.Foldout(IntersectionsFoldout,"Edit Intersections");



		if (IntersectionsFoldout)
		{
			ListHeader(Settings.Intersections,"Intersections");
			IntersectionCanvas = EditorGUILayout.BeginScrollView(IntersectionCanvas);
			for (int i = 0; Settings.Intersections.Count > i;i++)
			{
				GUILayout.Space(2);
				GUILayout.BeginHorizontal();
				GUILayout.Label(AssetPreview.GetAssetPreview(Settings.Intersections[i].Prefab),GUILayout.Width(64),GUILayout.Height(64));
				GUILayout.BeginVertical();
				Settings.Intersections[i].Name = EditorGUILayout.TextField("Name",Settings.Intersections[i].Name);
				Settings.Intersections[i].Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab",Settings.Intersections[i].Prefab,typeof(GameObject),false);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				//Intersections[i].Prefabs = editoruig
			}
			EditorGUILayout.EndScrollView();
		}
		else
		{
			List<Texture2D> intersectionImages = new List<Texture2D>();
			for(int i = 0; i<Settings.Intersections.Count;i++)
			{
				intersectionImages.Add(AssetPreview.GetAssetPreview(Settings.Intersections[i].Prefab));
			}
			SelectedRoadTile = GUILayout.SelectionGrid(SelectedRoadTile,intersectionImages.ToArray(),TileBrowserGridColumns);
		}

		if (GUILayout.Button("Recalculate All Curves"))
		{
			foreach(var intersection in Object.FindObjectsOfType<Intersection>())
			{
				intersection.ForceAnchorDirections();
			}
		}

		/*if (GUILayout.Button("Rebuild All Roads"))
		{
			foreach(var intersection in Object.FindObjectsOfType<Intersection>())
			{
				intersection.ForceAnchorDirections();
				intersection.RecalculateAllAnchoredPaths();
			}
			//RebuildAllRoads();
		}*/

		EditorGUI.BeginDisabledGroup(Selection.gameObjects.Length == 0);
		if (GUILayout.Button("Rebuild Meshes - Selected Roads And Intersections"))
		{
			foreach(var v in Selection.gameObjects)
			{
				var intersection = v.GetComponent<Intersection>();
				if (intersection)
				{
					intersection.RebuildAllAnchoredPaths(false);
				}
				var road = v.GetComponent<PathMesh>();
				if (road)
				{
					road.Rebuild();
				}
			}
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button("Rebuild Meshes - All"))
		{
			EditorUtility.DisplayProgressBar("Rebuild All Road Meshes","Generating and saving meshes for all roads",0);

			var allIntersections = Object.FindObjectsOfType<Intersection>();
			for (int i = 0; i<allIntersections.Length;i++)
			{
				EditorUtility.DisplayProgressBar("Rebuild All Road Meshes","Generating and saving meshes for all roads",i/allIntersections.Length);
				allIntersections[i].RebuildAllAnchoredPaths(false);
			}
			AssetDatabase.SaveAssets();
			EditorUtility.ClearProgressBar();
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		if (GUILayout.Button("Refresh All Props"))
		{
			foreach(var spawners in FindObjectsOfType<PathDetail>())
			{
				spawners.Clear();
				spawners.PlacePrefabs();
			}
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		}

		if (GUI.changed)
		{
			//this refreshes the sceneview somehow
			EditorUtility.SetDirty(Settings);
		}

		EditorGUI.EndDisabledGroup();
	}

	void NewRoadSettings()
	{
		RoadEditorSettings asset = ScriptableObject.CreateInstance<RoadEditorSettings>();

		AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
		AssetDatabase.SaveAssets();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = asset;
		Settings = asset;
	}

	// Window has been selected
	void OnFocus() {
		// Remove delegate listener if it has previously
		// been assigned.
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		// Add (or re-add) the delegate.
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;


		if (Settings != null)
		{
			var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Settings));
			EditorPrefs.SetString("LastRoadSettingsGUID",guid);
		}
	}

	void OnDestroy() {
		// When the window is destroyed, remove the delegate
		// so that it will no longer do any drawing.

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
	}

	void Clear()
	{
		selectedAnchor = null;
	}

	public void ListHeader(List<IntersectionType> _list, string _label)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(_label);
		if (_list.Count > 0)
		{
			if (GUILayout.Button("-",GUILayout.Width(50)))
				_list.RemoveAt(_list.Count-1);
		}
		if (GUILayout.Button("+",GUILayout.Width(50)))
			_list.Add(new IntersectionType());
		GUILayout.EndHorizontal();
	}

	void BuildRoad(Anchor begin, Anchor end)
	{
		if (begin == null || end == null){return;}

		GameObject go = new GameObject("Road");
		Undo.RegisterCreatedObjectUndo(go,"Created Road");
		var curve = go.AddComponent<CubicBezierPath>();
		var mesh = go.AddComponent<PathMesh>();
		mesh.material = Settings.roadMaterial;
		mesh.ExtrudeShape = Settings.extudeShape;

		curve.pts[0] = begin.transform.position;
		curve.pts[1] = begin.transform.position + begin.transform.forward * begin.Power;
		curve.pts[2] = end.transform.position + end.transform.forward * end.Power;
		curve.pts[3] = end.transform.position;

		begin.Path = go;
		end.Path = go;

		EditorUtility.SetDirty(begin);
		EditorUtility.SetDirty(end);

		if (Settings.automaticallyBuildRoads)
		{
			mesh.Rebuild();
		}
		if (Settings.roadsHaveMeshColliders)
		{
			go.AddComponent<MeshCollider>();
		}

		UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
		//Selection.activeGameObject = curve.gameObject;
	}

	Mesh dummyMesh;

	void OnSceneGUI(SceneView sceneView)
	{
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

		if (Settings == null)
		{
			return;
		}
			
		if (e.type == EventType.keyDown)
		{
			if (e.keyCode == KeyCode.Escape)
			{
				//Clear();
				selectedAnchor = null;
				placingIntersection = false;
			}

			//if (e.keyCode == KeyCode.I)
			//	showWindow = !showWindow;
			//gui popup window
		}

		foreach (var a in FindObjectsOfType<Anchor>())
		{
			if (a.Path != null)
			{
				//a.Curve.DrawCurve();
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
				else if (selectedAnchor != a)
				{
					BuildRoad(selectedAnchor,a);
					//Clear();
					selectedAnchor = null;
				}
			}
		}
			
		if (!placingIntersection)
		{
			savedPos = hitPoint;
		}
		Handles.color = Color.blue;
		Handles.DrawWireDisc(savedPos,Vector3.up,5);

		Debug.DrawRay(lastIntersectionPos,Vector3.up * 20,Color.red);

		if (selectedAnchor != null)
		{
			Handles.DrawDottedLine(selectedAnchor.transform.position,savedPos,3);
		}

		if (AutoAddIntersect && selectedAnchor != null)
		{
			//TODO bool for debug mesh on reroute intersections (without meshfilters)
			var prefab = Settings.Intersections[SelectedRoadTile].Prefab;
			var meshfilter = prefab.GetComponentInChildren<MeshFilter>();
			Mesh mesh = null;
			if (meshfilter == null)
			{
				if (dummyMesh == null)
				{
					dummyMesh = new Mesh();
				}
				mesh = dummyMesh;
			}
			else
			{
				mesh = meshfilter.sharedMesh;
			}

			if (mesh != null)
			{
				if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.mouseDown)
				{
					if (!placingIntersection)
					{
						placingIntersection = true;
						lastIntersectionPos = savedPos;
						//start placing
					}
					else
					{
						//spawn prefab

						var intersection = (GameObject)PrefabUtility.InstantiatePrefab(Settings.Intersections[SelectedRoadTile].Prefab);
						Undo.RegisterCreatedObjectUndo(intersection,"Created Intersection");
						intersection.transform.position = savedPos + Settings.heightOffset*Vector3.up;

						intersection.transform.position = lastIntersectionPos;

						Quaternion rot = Quaternion.LookRotation(savedPos-hitPoint);
						intersection.transform.rotation = rot;

						Anchor closestAnchor = null;
						float closestDistance = 999;
						foreach(var v in intersection.GetComponentsInChildren<Anchor>())
						{
							Vector3 pos = v.transform.position;
							Vector3 dir = v.transform.forward;

							Handles.DrawLine(pos,savedPos + dir*10);
							float dist = Vector3.Distance(pos,selectedAnchor.transform.position);
							if (dist < closestDistance)
							{
								closestDistance = dist;
								closestAnchor = v;
							}
						}

						BuildRoad(selectedAnchor,closestAnchor);
						placingIntersection = false;
						selectedAnchor = null;

						Selection.activeGameObject = intersection;
					}

					Event.current.Use();
				}
				else if (placingIntersection)
				{
					//rotate from savedPos toward mouse position
					Quaternion rot = Quaternion.LookRotation(savedPos-hitPoint);

					//TODO this doesn't draw scaled intersections
					Graphics.DrawMesh(mesh,savedPos + Settings.heightOffset * Vector3.up,rot,Settings.roadMaterial,0);
					Anchor closestAnchor = null;
					float closestDistance = 999;
					foreach(var v in prefab.GetComponentsInChildren<Anchor>())
					{
						Vector3 pos = savedPos + (rot * v.transform.position);
						Vector3 dir = (rot *v.transform.position) + (rot * v.transform.forward);

						Handles.DrawLine(pos,savedPos + dir*10);
						float dist = Vector3.Distance(pos,selectedAnchor.transform.position);
						if (dist < closestDistance)
						{
							closestDistance = dist;
							closestAnchor = v;
						}
					}
					if (closestAnchor != null)
					{
						UnityEditor.Handles.DrawBezier(
							selectedAnchor.transform.position,
							savedPos + (rot * closestAnchor.transform.position),
							selectedAnchor.transform.position + selectedAnchor.transform.forward * 10,
							savedPos + (rot * closestAnchor.transform.position) + (rot *closestAnchor.transform.position) + (rot * closestAnchor.transform.forward) * 10,
							Color.cyan,
							UnityEditor.EditorGUIUtility.whiteTexture,
							2
						);
					}
				}
				else
				{
					Graphics.DrawMesh(mesh,hitPoint,Quaternion.identity,Settings.roadMaterial,0);
				}
				EditorUtility.SetDirty(Settings);
			}
		}
		else if (AutoAddIntersect)
		{
			//TODO bool for debug mesh on reroute intersections (without meshfilters)
			var prefab = Settings.Intersections[SelectedRoadTile].Prefab;
			var meshfilter = prefab.GetComponentInChildren<MeshFilter>();
			Mesh mesh = null;
			if (meshfilter == null)
			{
				if (dummyMesh == null)
				{
					dummyMesh = new Mesh();
				}
				mesh = dummyMesh;
			}
			else
			{
				mesh = meshfilter.sharedMesh;
			}

			Graphics.DrawMesh(mesh,hitPoint,Quaternion.identity,Settings.roadMaterial,0);
			EditorUtility.SetDirty(Settings);

			if (Event.current.isMouse && Event.current.button == 0 && Event.current.type == EventType.mouseDown)
			{
				if (hit.collider != null)
				{
					if (hit.collider.GetComponentInParent<Intersection>() != null || hit.collider.GetComponent<IPath>() != null)
					{
						//TODO insert tool - click to split a path and place an intersection
						//atm you clicked on some other road tool thing, so you probably want to select that
						AutoAddIntersect = false;
						return;
					}
				}

				if (Settings == null || Settings.Intersections[SelectedRoadTile] == null || Settings.Intersections[SelectedRoadTile].Prefab == null)
				{
					return;
				}
				//just click and place intersection quickly
				var intersection = (GameObject)PrefabUtility.InstantiatePrefab(Settings.Intersections[SelectedRoadTile].Prefab);
				Undo.RegisterCreatedObjectUndo(intersection,"Created Intersection");
				intersection.transform.position = savedPos + Settings.heightOffset*Vector3.up;
				Event.current.Use();
				Selection.activeGameObject = intersection;
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			}
		}
	}
}
