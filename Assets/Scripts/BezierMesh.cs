using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierMesh : MonoBehaviour
{
	//public bool UniqueMesh = true;
	public ExtrudeShape es;
	public Material material;
	CubicBezier3D curve;

	public void Start()
	{
		//if (UniqueMesh && GetComponent<UniqueMesh>()==null)gameObject.AddComponent<UniqueMesh>();

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

		if (GetComponent<MeshCollider>() == null)
		{
			gameObject.AddComponent<MeshCollider>();
		}
		mesh.Optimize();
	}

	//unique mesh
	[HideInInspector] int ownerID; // To ensure they have a unique mesh
	MeshFilter _mf;
	MeshFilter mf { // Tries to find a mesh filter, adds one if it doesn't exist yet
		get{
			_mf = _mf == null ? GetComponent<MeshFilter>() : _mf;
			_mf = _mf == null ? gameObject.AddComponent<MeshFilter>() : _mf;
			return _mf;
		}
	}

	Mesh _mesh;
	protected Mesh mesh { // The mesh to edit
		get{
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

	public void Clear()
	{
		DestroyImmediate(GetComponent<MeshRenderer>());
		DestroyImmediate(GetComponent<MeshFilter>());
		DestroyImmediate(GetComponent<MeshCollider>());
		this.enabled = true;
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
		if (shape == null)
		{
			Debug.LogWarning("Shape is Null!",gameObject);
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
		Vector3[] vertices 	= new Vector3[ vertCount ];
		Vector3[] normals 		= new Vector3[ vertCount ];
		Vector2[] uvs 			= new Vector2[ vertCount ];

		for( int i = 0; i < path.Length; i++ ) {
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ ) {
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
				normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
				uvs[id] = new Vector2( shape.us[j], i / es.UTotalLength() ); //TODO use calclengthtable to fix stretching here!
				//uvs[id] = new Vector2( shape.us[j], i / es.us.Sample(j) ); //TODO use calclengthtable to fix stretching here!


				//i is the path. j is the drawn shape?
				//Vector2 uv = new Vector2();
				//uv.x = shape.us[j];
				//uv.y = i / es.us.Sample(j);  //TODO use calclengthtable to fix stretching here!
				//uv.y = CalcLengthTableInfo(shape.us,curve);

				//uvs[id] = uv;
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

	//TODO proper calculations on uvs
	public void CalcLengthTableInfo(float[] arr, CubicBezier3D curve)
	{
		arr[0] = 0f;
		float totalLength = 0f;
		Vector3 prev = curve.p0;
		for( int i = 1; i < arr.Length; i++ ) {
			float t = ( (float)i ) / ( arr.Length - 1 );

			List<OrientedPoint> oriented = curve.EvaluatePoints();
			List<Vector3> points = new List<Vector3>();
			foreach(var v in oriented)
			{
				points.Add(v.position);
			}

			Vector3 pt = curve.GetPoint(points.ToArray(), t );
			float diff = ( prev - pt ).magnitude;
			totalLength += diff;
			arr[i] = totalLength;
			prev = pt;
		}
	}
}
