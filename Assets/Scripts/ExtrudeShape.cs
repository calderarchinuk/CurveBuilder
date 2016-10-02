using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ExtrudeShape", menuName = "ExtrudeShape", order = 1)]
public class ExtrudeShape: ScriptableObject
{
	public Vector2[] vert2Ds = new Vector2[]
	{
		new Vector2(-2,0),
		new Vector2(-2,0.1f),
		new Vector2(0,0.1f),
		new Vector2(2,0.1f),
		new Vector2(2,0),
	};

	public Vector2[] normals = new Vector2[]{Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.up};
	public float[] us = new float[]{0,0.2f,0.4f,0.6f,1};
	public int[] lines = new int[]{
		4, 3,
		3, 2,
		2, 1,
		1, 0
	};

	public float UTotalLength()
	{
		float f = 0;
		for(int i = 0; i<vert2Ds.Length-1; i++)
		{
			f += Vector2.Distance(vert2Ds[i],vert2Ds[i+1]);
		}
		return f;
	}
}
