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
				#if UNITY_EDITOR
				AssetDatabase.CreateAsset(_mesh,"Assets/RoadMesh/Mesh"+ownerID+".asset");
				#endif
			}
			return _mesh;
		}
	}

	public ExtrudeShape es;
	public Material material;
	public float MaterialScale = 16;

	public int SectionCount = 20;

	private List<OrientedPoint> path;

	public void Generate()
	{
		Clear();

		path = curve.EvaluatePoints(SectionCount);

		if (path.Count == 0)
		{
			Debug.LogWarning("path length is 0 points!",this);
			return;
		}
			
		Extrude(mesh,es,path.ToArray());
	}

	public void ClearPath()
	{
		path = null;
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

		float totalDistance = GetTotalLengthOfCurve();

		for( int i = 0; i < edgeLoops; i++ )
		{
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ )
			{
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
				normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
				float v = GetVAtSection(totalDistance,i) * MaterialScale;

				uvs[id] = new Vector2(shape.us[j], v);
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
		#if UNITY_EDITOR
		MeshUtility.Optimize(mesh);
		#endif
	}

	//return 0-1 coords for v based on distance of curve
	public float GetVAtSection(float totallengthofcurve,int section)
	{
		if (path == null)
			path = curve.EvaluatePoints(SectionCount);

		Vector3 prev = path[0].position;
		float myLength = 0;

		if (section == path.Count)
		{
			return 1;
		}

		for( int i = 0; i < section+1; i++ )
		{
			float diff = ( prev - path[i].position ).magnitude;
			myLength += diff;
			prev = path[i].position;
		}
		return myLength / totallengthofcurve;
	}

	float GetVRounded(float totaldistance)
	{
		return totaldistance%MaterialScale;
	}

	//return real distance of curve
	public float GetTotalLengthOfCurve()
	{
		Vector3 prev = curve.pts[0];
		float totalLength = 0;
		for( int i = 0; i < path.Count; i++ )
		{
			float diff = ( prev - path[i].position ).magnitude;
			totalLength += diff;
			prev = path[i].position;
		}
		return totalLength;
	}

	void OnDrawGizmos()
	{
		if (es == null){return;}

		if (path == null)
			path = curve.EvaluatePoints(SectionCount);

		Gizmos.matrix = transform.localToWorldMatrix;
		//float totaldist = GetTotalLengthOfCurve();

		//Handles.color = Color.red;
		//UnityEditor.Handles.Label(path[0].position + Vector3.up * 2, totaldist.ToString());



		for (int j = 0; j<path.Count; j++)
		{
			for (int i = 0; i < es.vert2Ds.Length-1; i++)
			{
				Gizmos.DrawLine(path[j].LocalToWorld(es.vert2Ds[i]),path[j].LocalToWorld(es.vert2Ds[i+1]));
			}

			//Handles.color = Color.green;
			//UnityEditor.Handles.Label(path[j].position,GetVAtSection(totaldist,j).ToString());
			//float normalizedDist = (float)j/((float)path.Count-1);
			;
		}
	}
}
