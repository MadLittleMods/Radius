/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ExtensionMethods.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods 
{
	// Some various extension for various objects
	// Like dictionary, list tostrings
	// enum parsing, dictionary utilities..


	// Convert Dictionary to string
	public static string ToDebugString<TKey, TValue> (this IDictionary<TKey, TValue> dictionary)
	{
		return "{" + string.Join(", ", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray()) + "}";
	}
	public static string ToDebugString<TKey, TValue> (this ReadOnlyDictionary<TKey, TValue> dictionary)
	{
		return "{" + string.Join(", ", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToString()).ToArray()) + "}";
	}

	public static string ToDebugStringWithList<TKey, TValue> (this IDictionary<TKey, List<TValue>> dictionary)
	{
		return "{" + string.Join(", ", dictionary.Select(kv => kv.Key.ToString() + "=" + kv.Value.ToDebugString()).ToArray()) + "}";
	}

	// Convert List to string
	public static string ToDebugString<T>(this IList<T> list)
	{
		return list.Count.ToString() + "`[" + string.Join("; ", list.Select(i => i.ToString()).ToArray()) + "]";
	}

	// Dictionary GetValueOrDefault
	public static TValue GetValueOrDefault<TKey, TValue>
		(this IDictionary<TKey, TValue> dictionary, 
		 TKey key,
		 TValue defaultValue)
	{
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value : defaultValue;
	}
	public static TValue GetValueOrDefault<TKey, TValue>
		(this IDictionary<TKey, TValue> dictionary,
		 TKey key,
		 Func<TValue> defaultValueProvider)
	{
		TValue value;
		return dictionary.TryGetValue(key, out value) ? value
			: defaultValueProvider();
	}


	// Random method to compare the "r", "g", and "b" keys of two dictionarys
	public static bool CompareRGB(this IDictionary<string, float> dictionary, IDictionary<string, float> compareToDictionary)
	{
		// early exits
		if(dictionary == null || compareToDictionary == null)
			return false;

		if(dictionary["r"] == compareToDictionary["r"] || dictionary["g"] == compareToDictionary["g"] || dictionary["b"] == compareToDictionary["b"])
			return true;
		
		return false;
	}


	// Usage: ParseEnum<AudioBase.AudioType>("Music")
	public static T ParseEnum<T>( string value )
	{
		return (T) Enum.Parse( typeof( T ), value, true );
	}

}
