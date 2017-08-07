using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

//extrudes a shape along a path

public class PathMesh : MonoBehaviour
{
	MeshFilter _meshFilter;
	MeshFilter mf
	{
		get
		{
			_meshFilter = this.GetOrAddComponent<MeshFilter>();
			this.GetOrAddComponent<MeshRenderer>();
			return _meshFilter;
		}
	}

	IPath _path;
	IPath path
	{
		get
		{
			if (_path == null)
			{
				_path = GetComponent<IPath>();
			}
			//_path = this.GetOrAddComponent<IPath>();
			return _path;
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
				System.IO.Directory.CreateDirectory(Application.dataPath+"/GeneratedMesh");
				AssetDatabase.CreateAsset(_mesh,"Assets/GeneratedMesh/Mesh"+ownerID+".asset");
				#endif
			}
			return _mesh;
		}
	}

	public ExtrudeShape ExtrudeShape;
	public Material material;
	public float MaterialScale = 16;

	public int SectionCount = 20;

	private List<OrientedPoint> pathPoints;

	public void Generate()
	{
		ClearMesh();

		pathPoints = path.EvaluatePoints(SectionCount);

		if (pathPoints.Count == 0)
		{
			Debug.LogWarning("path length is 0 points!",this);
			return;
		}
			
		Extrude(mesh,ExtrudeShape,pathPoints.ToArray());
	}

	public void ClearPath()
	{
		pathPoints = null;
	}

	public void ClearMesh()
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
			Debug.LogWarning("Mesh is null!",gameObject);
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

		float totalDistance = GetTotalLengthOfPath();

		for( int i = 0; i < edgeLoops; i++ )
		{
			int offset = i * vertsInShape;
			for( int j = 0; j < vertsInShape; j++ )
			{
				int id = offset + j;
				vertices[id] = path[i].LocalToWorld( shape.vert2Ds[j] );
				normals[id] = path[i].LocalToWorldDirection( shape.normals[j] );
				float v = GetVAtSection(totalDistance,i);

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

	//return 0-X coords for v based on distance of curve
	public float GetVAtSection(float totallengthofcurve,int section)
	{
		//1m is 0-1
		//then /MaterialScale

		if (pathPoints == null)
			pathPoints = path.EvaluatePoints(SectionCount);

		Vector3 prev = pathPoints[0].position;
		float myLength = 0;

		if (section == pathPoints.Count)
		{
			return 1;
		}

		for( int i = 0; i < section+1; i++ )
		{
			float diff = ( prev - pathPoints[i].position ).magnitude;
			myLength += diff;
			prev = pathPoints[i].position;
		}

		return myLength/MaterialScale;// / totallengthofcurve;
	}

	float GetVRounded(float totaldistance)
	{
		return totaldistance%MaterialScale;
	}

	//return real distance of path
	public float GetTotalLengthOfPath()
	{
		Vector3 prev = path.GetPoint(0);
		float totalLength = 0;
		for( int i = 0; i < pathPoints.Count; i++ )
		{
			float diff = ( prev - pathPoints[i].position ).magnitude;
			totalLength += diff;
			prev = pathPoints[i].position;
		}
		return totalLength;
	}

	void OnDrawGizmos()
	{
		if (ExtrudeShape == null){return;}

		if (pathPoints == null)
			pathPoints = path.EvaluatePoints(SectionCount);

		Gizmos.matrix = transform.localToWorldMatrix;
		//float totaldist = GetTotalLengthOfCurve();

		//Handles.color = Color.red;
		//UnityEditor.Handles.Label(path[0].position + Vector3.up * 2, totaldist.ToString());

		Gizmos.color = Color.green;

		for (int j = 0; j<pathPoints.Count; j++)
		{
			for (int i = 0; i < ExtrudeShape.vert2Ds.Length-1; i++)
			{
				Gizmos.DrawLine(pathPoints[j].LocalToWorld(ExtrudeShape.vert2Ds[i]),pathPoints[j].LocalToWorld(ExtrudeShape.vert2Ds[i+1]));
			}

			//Handles.color = Color.green;
			//UnityEditor.Handles.Label(path[j].position,GetVAtSection(totaldist,j).ToString());
			//float normalizedDist = (float)j/((float)path.Count-1);
			;
		}

		pathPoints = null;
	}
}
