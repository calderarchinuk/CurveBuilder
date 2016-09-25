using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor( typeof( Road ) )]
public class RoadViewer : Editor
{
	void OnSceneGUI( )
	{
		Road r = target as Road;

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