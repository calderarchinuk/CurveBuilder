using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//https://www.youtube.com/watch?v=o9RK6O2kOKo
//https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gc41ce114c_1_31

public class Road : MonoBehaviour
{
	public List<Anchor> Anchors = new List<Anchor>();
	public ExtrudeShape ExtrudeShape;
	public int SectionCount = 20;

	void Start()
	{
		CubicBezier3D cb = GetComponent<CubicBezier3D>();
		if (cb == null)
		{
			cb = gameObject.AddComponent<CubicBezier3D>();
			cb.p0 = Anchors[0].transform.position;
			cb.p1 = Anchors[0].transform.forward * Anchors[0].Power + Anchors[0].transform.position;
			cb.p2 = Anchors[1].transform.forward * Anchors[1].Power + Anchors[1].transform.position;
			cb.p3 = Anchors[1].transform.position;
		}




		if (GetComponent<BezierMesh>()==null)gameObject.AddComponent<BezierMesh>();
		if (GetComponent<UniqueMesh>()==null)gameObject.AddComponent<UniqueMesh>();

		if (GetComponent<MeshFilter>()==null)gameObject.AddComponent<MeshFilter>();
		if (GetComponent<MeshRenderer>()==null)gameObject.AddComponent<MeshRenderer>();
		GetComponent<MeshRenderer>().material = Resources.Load<Material>("RoadMat");
	}
}
