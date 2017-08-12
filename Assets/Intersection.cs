using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//used by the road manager to quickly rebuild roads from this intersection

public class Intersection : MonoBehaviour
{
	public void ForceAnchorDirections()
	{
		foreach(var anchor in GetComponentsInChildren<Anchor>())
		{
			if (anchor.Path == null)
			{
				continue;
			}

			anchor.ForceDirection(true);
		}
	}

	/// <summary>
	/// Fixes all anchor directions
	/// Rebuilds all anchored path meshes
	/// </summary>
	public void RebuildAllAnchoredPaths(bool rebuildAssetDatabase)
	{
		ForceAnchorDirections();
		foreach(var anchor in GetComponentsInChildren<Anchor>())
		{
			if (anchor.Path == null)
			{
				continue;
			}

			var mesh = anchor.Path.GetComponent<PathMesh>();
			if (mesh != null)
			{
				mesh.Rebuild();
			}
		}
		if (rebuildAssetDatabase)
		{
			#if UNITY_EDITOR
			UnityEditor.AssetDatabase.SaveAssets();
			#endif
		}
	}

	public void OnDrawGizmosSelected()
	{
		foreach(var anchor in GetComponentsInChildren<Anchor>())
		{
			anchor.DrawCustomGizmos();
		}
	}
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Intersection))]
[UnityEditor.CanEditMultipleObjects]
public class IntersectionEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		Intersection intersection = (Intersection)target;

		if (GUILayout.Button("Recalculate Curves"))
		{
			intersection.ForceAnchorDirections();
		}

		if (GUILayout.Button("Rebuild Meshes"))
		{
			intersection.RebuildAllAnchoredPaths(true);
		}

		UnityEditor.EditorGUI.BeginDisabledGroup(true);
		if (GUILayout.Button("Rebuild Details"))
		{
			//TODO rebuild details for connected paths
			Debug.Log("TODO - rebuild details along roads");
		}
		UnityEditor.EditorGUI.EndDisabledGroup();

		if (UnityEditor.Selection.gameObjects.Length > 1){return;}

		int sliderId = 0;
		foreach(var anchor in intersection.GetComponentsInChildren<Anchor>())
		{
			GUILayout.BeginHorizontal();

			GUI.color = GetColor(sliderId);
			GUIContent swatch = new GUIContent(UnityEditor.EditorGUIUtility.whiteTexture);
			UnityEditor.EditorGUILayout.LabelField(swatch,GUILayout.Width(8));
			anchor.SetGizmoColor(GUI.color);
			GUI.color = Color.white;

			anchor.Power = UnityEditor.EditorGUILayout.Slider("Anchor " + sliderId,anchor.Power,0f,50f);

			UnityEditor.EditorGUI.BeginDisabledGroup(null == anchor.Path);
			if (GUILayout.Button(new GUIContent("R","Next Road")))
			{
				UnityEditor.Selection.activeGameObject = anchor.Path;
			}

			if (GUILayout.Button(new GUIContent("I","Next Intersection")))
			{
				Debug.Log("search for next intersection");
				foreach(var nextIntersection in FindObjectsOfType<Intersection>())
				{
					if (nextIntersection == target){continue;}

					foreach(var nextAnchor in nextIntersection.GetComponentsInChildren<Anchor>())
					{
						if (nextAnchor.Path != null && nextAnchor.Path == anchor.Path)
						{
							UnityEditor.Selection.activeGameObject = nextIntersection.gameObject;
							break;
						}
					}
				}
			}
			UnityEditor.EditorGUI.EndDisabledGroup();

			sliderId ++;
			GUILayout.EndHorizontal();
		}
		if (GUI.changed)
		{
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			foreach(var anchor in intersection.GetComponentsInChildren<Anchor>())
			{
				UnityEditor.EditorUtility.SetDirty(anchor);
			}
			UnityEditor.EditorUtility.SetDirty(intersection);
		}
	}

	public Color GetColor(int id)
	{
		switch(id)
		{
			case 0: return Color.red;
			case 1: return Color.green;
			case 2: return Color.blue;
			case 3: return Color.yellow;
			case 4: return Color.magenta;
			case 5: return Color.black;
		}
		return Color.white;
	}
}

#endif
