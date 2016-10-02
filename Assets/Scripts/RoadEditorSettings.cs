using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[CreateAssetMenu(fileName = "ExtrudeShape", menuName = "ExtrudeShape", order = 1)]
public class RoadEditorSettings : ScriptableObject
{
	public Material roadMaterial;
	public ExtrudeShape extudeShape;

	public List<Intersection>Intersections = new List<Intersection>();
}
