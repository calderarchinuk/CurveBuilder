using UnityEngine;
using System.Collections;

public static class Extensions
{
	public static float Sample( this float[] fArr, float t){
		int count = fArr.Length;
		if(count == 0){
			Debug.LogError("Unable to sample array - it has no elements" );
			return 0;
		}
		if(count == 1)
			return fArr[0];
		float iFloat = t * (count-1);
		int idLower = Mathf.FloorToInt(iFloat);
		int idUpper = Mathf.FloorToInt(iFloat + 1);
		if( idUpper >= count )
			return fArr[count-1];
		if( idLower < 0 )
			return fArr[0];
		return Mathf.Lerp( fArr[idLower], fArr[idUpper], iFloat - idLower);
	}
}
