using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierMesh : MonoBehaviour {

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
				uvs[id] = new Vector2( shape.us[j], i / ((float)edgeLoops) ); //use calclengthtable to fix stretching here!
			}
		}

		/*for( int i = 0; i < path.Length; i++ ) {
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ ) {
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j].point );
				normals[id] = path[i].LocalToWorldDirection( shape.vert2Ds[j].normal );
				uvs[id] = new Vector2( shape.vert2Ds[j].uCoord, i / ((float)edgeLoops) );
			}
		}*/
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

	void CalcLengthTableInfo(float[] arr, CubicBezier3D bezier)
	{
		/*arr[0] = 0f;
		float totalLength = 0f;
		Vector3 prev = bezier.p0;
		for( int i = 1; i < arr.Length; i++ ) {
			float t = ( (float)i ) / ( arr.Length - 1 );
			Vector3 pt = bezier.GetPoint( t );
			float diff = ( prev - pt ).magnitude;
			totalLength += diff;
			arr[i] = totalLength;
			prev = pt;
		}*/
	}

	Road r;
	ExtrudeShape es;
	void Start () {
		r = GetComponent<Road>();
		es = r.ExtrudeShape;
	}
	
	// Update is called once per frame
	void Update ()
	{
		CubicBezier3D cb = GetComponent<CubicBezier3D>();
		List<OrientedPoint> path = new List<OrientedPoint>();
		Vector3[] pts = new Vector3[]{cb.p0,cb.p1,cb.p2,cb.p3};
		for (int i = 0; i<= r.SectionCount; i++)
		{
			//cb.GetPoint(new Vector3[]{cb.p0,cb.p3,cb.p1,cb.p2}
			path.Add(new OrientedPoint(cb.GetPoint(pts,i/(float)r.SectionCount),cb.GetOrientation3D(pts,i/(float)r.SectionCount,Vector3.up)));
		}
		Extrude(GetComponent<MeshFilter>().mesh,es,path.ToArray());
	}
}
