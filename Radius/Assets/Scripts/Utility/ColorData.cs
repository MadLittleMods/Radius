/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ColorData.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorData
{
	private Color Color;

	public ColorData(Color color)
	{
		this.Color = color;
	}

	public float r()
	{
		return this.Color.r;
	}
	public float g()
	{
		return this.Color.g;
	}
	public float b()
	{
		return this.Color.b;
	}
	public float a()
	{
		return this.Color.a;
	}

	public Vector4 ToVector4()
	{
		return (Vector4)this.Color;
	}
	public Vector3 ToVector3()
	{
		return (Vector3)(Vector4)this.Color;
	}
	public Dictionary<string, float> ToDictionary()
	{
		return new Dictionary<string, float>
		{
			{ "r", this.Color.r },
			{ "g", this.Color.g },
			{ "b", this.Color.b },
			{ "a", this.Color.a }
		};
	}

	public static Color ToColor(Dictionary<string, float> colorDictionary)
	{
		return new Color(colorDictionary["r"], colorDictionary["g"], colorDictionary["b"], colorDictionary["a"]);
	}

	public static Color Color255ToColor1(int red, int green, int blue, float alpha)
	{
		return new Color(Mathf.Clamp((float)red/255, 0, 1), Mathf.Clamp((float)green/255, 0, 1), Mathf.Clamp((float)blue/255, 0, 1), Mathf.Clamp(alpha, 0, 1));
	}
}
