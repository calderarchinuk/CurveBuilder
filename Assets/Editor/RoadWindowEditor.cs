using UnityEngine;
using System.Collections;
using UnityEditor;

public class RoadWindowEditor : EditorWindow
{

	Junction selectedJunction;
	Junction selectedJunction2;
	Anchor selectedAnchorA;
	Anchor selectedAnchorB;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("Window/RoadEditor")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		RoadWindowEditor window = (RoadWindowEditor)EditorWindow.GetWindow (typeof (RoadWindowEditor));
		window.Show();
	}

	void OnGUI ()
	{
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);

		selectedJunction = (Junction)EditorGUILayout.ObjectField(selectedJunction,typeof(Junction),true);
		selectedJunction2 = (Junction)EditorGUILayout.ObjectField(selectedJunction2,typeof(Junction),true);

		bool valid = selectedJunction != null && selectedJunction2 != null && selectedJunction != selectedJunction2;

		EditorGUI.BeginDisabledGroup(!valid);
		if (GUILayout.Button("Generate Road"))
		{
			BuildRoad();
		}
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button("Clear"))
		{
			Clear();
		}
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
		selectedJunction = null;
		selectedJunction2 = null;
		selectedAnchorB = null;
		selectedAnchorA = null;
	}

	void BuildAnchoredRoad()
	{
		GameObject go = new GameObject("road");
		Road r = go.AddComponent<Road>();
		r.Anchors.Add(selectedAnchorA);
		r.Anchors.Add(selectedAnchorB);
	}

	void BuildRoad()
	{
		if (selectedAnchorA != null && selectedAnchorB != null && selectedAnchorA != selectedAnchorB)
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
		Road r = go.AddComponent<Road>();
		r.Anchors.Add(highestDotAnchorA);
		r.Anchors.Add(highestDotAnchorB);

		//go through each anchor child of the junction
		//pick the one with the highest dot

		//create a road between the two anchors with high dots
	}

	void OnSceneGUI(SceneView sceneView) {
		// Do your drawing here using Handles.

		Event e = Event.current;
		if (e.type == EventType.keyDown)
		{
			if (e.keyCode == KeyCode.C)
			{
				Clear();
			}
			if (e.keyCode == KeyCode.B)
			{
				BuildRoad();
			}
		}

		foreach (var j in FindObjectsOfType<Junction>())
		{
			//Handles.DrawWireDisc(j.transform.position,j.transform.up,4);
			if (Handles.Button(j.transform.position,Quaternion.LookRotation(Vector3.up),4,4,Handles.CircleCap))
			{
				if (Event.current.shift)
				{
					if (j == selectedJunction)
					{
						selectedJunction = null;
					}
					if (j == selectedJunction2)
					{
						selectedJunction2 = null;
					}
				}
				else
				{
					if (selectedJunction == null)
					{
						selectedJunction = j;
					}
					else if (j != selectedJunction)
					{
						selectedJunction2 = j;
					}
				}
			}
		}

		foreach (var a in FindObjectsOfType<Anchor>())
		{
			//Handles.DrawWireDisc(j.transform.position,j.transform.up,4);
			if (Handles.Button(a.transform.position+a.transform.forward * 3.5f,Quaternion.LookRotation(Vector3.up),1,1,Handles.CircleCap))
			{
				if (Event.current.shift)
				{
					if (a == selectedAnchorA)
					{
						selectedAnchorA = null;
					}
					if (a == selectedAnchorB)
					{
						selectedAnchorB = null;
					}
				}
				else
				{
					if (selectedAnchorA == null)
					{
						selectedAnchorA = a;
					}
					else if (a != selectedAnchorB)
					{
						selectedAnchorB = a;
					}
				}
			}

			//TODO put these along the outside of the ring
			//Handles.DrawWireDisc(a.transform.position+a.transform.forward * 3.5f,Vector3.up,1);
		}


		foreach (var r in FindObjectsOfType<Road>())
		{
			DrawRoad(r);
		}

		if (selectedAnchorA != null)
		{
			Handles.color = new Color( 0, 0, 1, 0.4f );
			Handles.DrawSolidDisc(selectedAnchorA.transform.position+selectedAnchorA.transform.forward * 3.5f,Vector3.up,1);

			Vector3 outvalue = selectedAnchorA.transform.position;

			outvalue = Handles.Slider(outvalue + Vector3.up,selectedAnchorA.transform.forward);

			/*outvalue = Handles.Slider2D(outvalue + Vector3.up,
				selectedAnchorA.transform.forward,
				selectedAnchorA.transform.forward,
				selectedAnchorA.transform.forward,
				5,
				Handles.ArrowCap,
				0.001f,
				true
			);*/
			outvalue += Vector3.down;

			float delta = (outvalue-selectedAnchorA.transform.position).magnitude;

			if (delta > 0)
			{

				//outvalue - selectedAnchorA.transform.position 



				if (Vector3.Dot(selectedAnchorA.transform.forward + selectedAnchorA.transform.position,selectedAnchorA.transform.position)>Vector3.Dot(selectedAnchorA.transform.forward + selectedAnchorA.transform.position,outvalue))
				{
					delta = -delta;
				}

				Debug.Log(delta);

				selectedAnchorA.Power = delta;
			}

			//Vector3 value = Handles.Slider(selectedAnchorA.transform.position + Vector3.up,selectedAnchorA.transform.forward * selectedAnchorA.Power);
			//selectedAnchorA.Power += (value - selectedAnchorA.transform.position).magnitude;
		}

		if (selectedAnchorB != null)
		{
			Handles.color = new Color( 0, 0, 1, 0.1f );
			Handles.DrawSolidDisc(selectedAnchorB.transform.position+selectedAnchorB.transform.forward * 3.5f,Vector3.up,1);
		}

		if (selectedJunction != null)
		{
			Handles.color = new Color( 0, 1, 0, 0.4f );
			Handles.DrawSolidDisc(selectedJunction.transform.position,Vector3.up,4);
		}

		if (selectedJunction2 != null)
		{
			Handles.color = new Color( 0, 1, 0, 0.1f );
			Handles.DrawSolidDisc(selectedJunction2.transform.position,Vector3.up,4);
		}
		Handles.color = Color.white;


		Handles.BeginGUI();

		// Do your drawing here using GUI.
		Handles.EndGUI();    
	}

	void DrawRoad(Road r)
	{
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
		);
	}
}
