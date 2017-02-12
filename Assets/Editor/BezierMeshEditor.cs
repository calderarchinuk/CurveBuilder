using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BezierMesh))]
public class BezierMeshEditor : Editor{

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();
		BezierMesh b = target as BezierMesh;

		if (GUI.changed)
		{
			b.ClearPath();
		}

		if (b.SectionCount < 1)
		{
			b.SectionCount = 1;
		}

		if (GUILayout.Button("Clear Mesh"))
		{
			b.Clear();
		}
		if (GUILayout.Button("Build Mesh"))
		{
			b.Clear();

			Anchor savedAnchor = null;
			foreach (Anchor a in Object.FindObjectsOfType<Anchor>())
			{
				CubicBezier3D curve = a.Curve;
				if (curve != b.GetComponent<CubicBezier3D>()){continue;}
				if (savedAnchor != null)
				{
					//rebuilt
					curve.pts[0] = savedAnchor.transform.position;
					curve.pts[1] = savedAnchor.transform.position + savedAnchor.transform.forward * savedAnchor.Power;
					curve.pts[2] = a.transform.position + a.transform.forward * a.Power;
					curve.pts[3] = a.transform.position;
					break;
				}
				else
				{
					savedAnchor = a;
				}
			}

			b.Generate();
		}
	}
}
