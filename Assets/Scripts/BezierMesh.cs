using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierMesh : MonoBehaviour
{
	MeshFilter _meshFilter;
	MeshFilter mf
	{
		get
		{
			_meshFilter = this.GetOrAddComponent<MeshFilter>();
			return _meshFilter;
		}
	}

	CubicBezier3D _curve;
	CubicBezier3D curve
	{
		get
		{
			_curve = this.GetOrAddComponent<CubicBezier3D>();
			return _curve;
		}
	}

	int ownerID; // To ensure they have a unique mesh
	Mesh _mesh;
	protected Mesh mesh
	{
		get
		{
			bool isOwner = ownerID == gameObject.GetInstanceID();
			if( mf.sharedMesh == null || !isOwner )
			{
				mf.sharedMesh = _mesh = new Mesh();
				ownerID = gameObject.GetInstanceID();
				_mesh.name = "Mesh [" + ownerID + "]";
			}
			return _mesh;
		}
	}

	public ExtrudeShape es;
	public Material material;

	public int SectionCount = 20;

	public void Generate()
	{
		Clear();

		List<OrientedPoint> path = curve.EvaluatePoints(SectionCount);

		if (path.Count == 0)
		{
			Debug.LogWarning("path length is 0 points!",this);
			return;
		}
			
		Extrude(mesh,es,path.ToArray());
	}



	public void Clear()
	{
		#if UNITY_EDITOR
		MeshFilter m = GetComponent<MeshFilter>();
		if (m != null && m.sharedMesh != null)
		{
			AssetDatabase.DeleteAsset("Assets/RoadMesh/"+m.sharedMesh.name+".asset");
			AssetDatabase.Refresh();
			m.sharedMesh = null;
		}
		//DestroyImmediate(GetComponent<MeshRenderer>());
		//DestroyImmediate(GetComponent<MeshFilter>());
		//DestroyImmediate(GetComponent<MeshCollider>());
		#endif
	}

	void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
	{
		if (shape == null)
		{
			Debug.LogWarning("Shape is null!",gameObject);
			return;
		}
		if (mesh == null)
		{
			Debug.LogWarning("mesh is null!",gameObject);
			return;
		}

		int vertsInShape = shape.vert2Ds.Length;
		int segments = path.Length - 1;
		int edgeLoops = path.Length;
		int vertCount = vertsInShape * edgeLoops;
		int triCount = shape.lines.Length * segments;
		int triIndexCount = triCount * 3;
		int[] lines = shape.lines;

		int[] triangleIndices 	= new int[ triIndexCount ];
		Vector3[] vertices 		= new Vector3[ vertCount ];
		Vector3[] normals 		= new Vector3[ vertCount ];
		Vector2[] uvs 			= new Vector2[ vertCount ];

		for( int i = 0; i < edgeLoops; i++ )
		{
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ )
			{
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
				normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
				//uvs[id] = new Vector2( shape.us[j], i / es.UTotalLength() ); //TODO use calclengthtable to fix stretching here!
				uvs[id] = new Vector2( shape.us[j], i / curve.GetDistance(edgeLoops) );
			}
		}

		int ti = 0;
		for( int i = 0; i < segments; i++ )
		{
			int offset = i * vertsInShape;
			for ( int l = 0; l < lines.Length; l += 2 )
			{
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
		MeshUtility.Optimize(mesh);
	}

	//TODO proper calculations on uvs
	public void CalcLengthTableInfo(float[] arr, CubicBezier3D curve)
	{
		arr[0] = 0f;
		float totalLength = 0f;
		Vector3 prev = curve.pts[0];
		for( int i = 1; i < arr.Length; i++ )
		{
			float t = ( (float)i ) / ( arr.Length - 1 );

			List<OrientedPoint> oriented = curve.EvaluatePoints(arr.Length);
			List<Vector3> points = new List<Vector3>();
			foreach(var v in oriented)
			{
				points.Add(v.position);
			}

			Vector3 pt = curve.GetPoint( t );
			float diff = ( prev - pt ).magnitude;
			totalLength += diff;
			arr[i] = totalLength;
			prev = pt;
		}
	}
}
