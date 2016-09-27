using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierMesh : MonoBehaviour
{
	public bool UniqueMesh = true;
	public ExtrudeShape es;
	public Material material;
	CubicBezier3D curve;

	void Start()
	{
		if (UniqueMesh && GetComponent<UniqueMesh>()==null)gameObject.AddComponent<UniqueMesh>();

		if (GetComponent<MeshFilter>()==null)
		{
			gameObject.AddComponent<MeshFilter>();
		}
		if (GetComponent<MeshRenderer>()==null)
		{
			gameObject.AddComponent<MeshRenderer>();
			if (material != null)
				GetComponent<MeshRenderer>().material = material;
		}
		curve = GetComponent<CubicBezier3D>();
		if (!curve.UpdateCurve)
		{
			this.enabled = false;
		}
		EvalutateAndExtrude();
	}

	void Update ()
	{
		EvalutateAndExtrude();
	}

	void EvalutateAndExtrude()
	{
		List<OrientedPoint> path = curve.EvaluatePoints();
		Extrude(GetComponent<MeshFilter>().mesh,es,path.ToArray());
	}

	void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
	{
		int vertsInShape = shape.vert2Ds.Length;
		int segments = path.Length - 1;
		int edgeLoops = path.Length;
		int vertCount = vertsInShape * edgeLoops;
		int triCount = shape.lines.Length * segments;
		int triIndexCount = triCount * 3;
		int[] lines = shape.lines;

		int[] triangleIndices 	= new int[ triIndexCount ];
		Vector3[] vertices 	= new Vector3[ vertCount ];
		Vector3[] normals 		= new Vector3[ vertCount ];
		Vector2[] uvs 			= new Vector2[ vertCount ];

		for( int i = 0; i < path.Length; i++ ) {
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ ) {
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
				normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
				uvs[id] = new Vector2( shape.us[j], i / ((float)edgeLoops) ); //TODO use calclengthtable to fix stretching here!
			}
		}

		int ti = 0;
		for( int i = 0; i < segments; i++ ) {
			int offset = i * vertsInShape;
			for ( int l = 0; l < lines.Length; l += 2 ) {
				int a = offset + lines[l] + vertsInShape;
				int b = offset + lines[l];
				int c = offset + lines[l+1];
				int d = offset + lines[l+1] + vertsInShape;
				triangleIndices[ti] = a; 	ti++;
				triangleIndices[ti] = b; 	ti++;
				triangleIndices[ti] = c; 	ti++;
				triangleIndices[ti] = c; 	ti++;
				triangleIndices[ti] = d; 	ti++;
				triangleIndices[ti] = a; 	ti++;
			}
		}

		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangleIndices;
		mesh.normals = normals;
		mesh.uv = uvs;

	}
}
