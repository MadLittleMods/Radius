using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class UtilityMethods : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	
	// Usage: ParseEnum<AudioBase.AudioType>("Music")
	public static T ParseEnum<T>( string value )
	{
		return (T) Enum.Parse( typeof( T ), value, true );
	}


	// --------------------------------------------------------------
	// --------------------------------------------------------------

	public static Dictionary<string, string> ParseSeparatedStrToDict(string separatedString)
	{
		// separatedString should be formatted as: "key:value~anotherkey:anothervalue~again:andagain"
		//Debug.Log(separatedString);
		var valueDictionary = new Dictionary<string, string>();
		
		MatchCollection matches = Regex.Matches(separatedString, @"([^~:;]*):([^~:;]*)");
		foreach (Match match in matches)
		{
			//Debug.Log(match.Groups[1].Value + " : " + match.Groups[2].Value);
			valueDictionary[WWW.UnEscapeURL(match.Groups[1].Value)] = WWW.UnEscapeURL(match.Groups[2].Value);
		}
		
		return valueDictionary;
	}
	
	public static string GenerateSeparatedString(Dictionary<string, string> valueDictionary)
	{
		return string.Join("~", valueDictionary.Select(x => WWW.EscapeURL(x.Key) + ":" + WWW.EscapeURL(x.Value)).ToArray());
	}



	// --------------------------------------------------------------
	// --------------------------------------------------------------
	
	public static Vector3 RotateX(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.x, point.y*Mathf.Cos(radRotation) - point.z*Mathf.Sin(radRotation), point.y*Mathf.Sin(radRotation) + point.z*Mathf.Cos(radRotation));
	}
	public static Vector3 RotateY(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.z*Mathf.Sin(radRotation) + point.x*Mathf.Cos(radRotation), point.y, point.z*Mathf.Cos(radRotation) - point.x*Mathf.Sin(radRotation));
	}
	public static Vector3 RotateZ(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.x*Mathf.Cos(radRotation) - point.y*Mathf.Sin(radRotation), point.x*Mathf.Sin(radRotation) + point.y*Mathf.Cos(radRotation), point.z);
	}
	
	public static Vector3 RotateXYZ(Vector3 point, Quaternion rotation)
	{
		Vector3 eulerRotation = rotation.eulerAngles;
		Debug.Log("rotation: " + eulerRotation);
		
		Vector3 adjustedPoint = point;
		
		if(eulerRotation.x != 0)
			adjustedPoint = RotateX(point, eulerRotation.x);
		
		if(eulerRotation.y != 0)
			adjustedPoint = RotateY(point, eulerRotation.y);
		
		if(eulerRotation.z != 0)
			adjustedPoint = RotateZ(point, eulerRotation.z);
		
		return adjustedPoint;
	}
}
