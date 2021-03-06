﻿using UnityEngine;
using System.Collections;
using System;
using System.Linq;

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

	/// <summary>
	/// Gets or add a component. Usage example:
	/// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
	/// </summary>
	static public T GetOrAddComponent<T> (this Component child) where T: Component {
		T result = child.GetComponent<T>();
		if (result == null) {
			result = child.gameObject.AddComponent<T>();
		}
		return result;
	}

	public static T[] GetInterfaces<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
		var mObjs = gObj.GetComponents<MonoBehaviour>();

		return (from a in mObjs where a.GetType().GetInterfaces().Any(k => k == typeof(T)) select (T)(object)a).ToArray();
	}

	public static T GetInterface<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface) throw new SystemException("Specified type is not an interface!");
		return gObj.GetInterfaces<T>().FirstOrDefault();
	}
}
