/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ProceduralCylinderLabel.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ProceduralRing))]
public class ProceduralCylinderLabel : Editor {

	GUISkin editorSkin;

	GameObject targetGameObject; // The GameObject that this editor uses
	ProceduralRing scriptOfOurType; // The script we add the custom editor for

	void OnEnable()
	{
		this.editorSkin = (GUISkin)(Resources.LoadAssetAtPath("Assets/Editor/EditorGUISkin.guiskin", typeof(GUISkin)));

		this.scriptOfOurType = (ProceduralRing)target;    
		this.targetGameObject = (GameObject)this.scriptOfOurType.gameObject;
	}

	void OnSceneGUI ()
	{
		Handles.Label(this.targetGameObject.transform.position, "Ring Object", editorSkin.GetStyle("Label"));

		/*
		// Label points
		for(int i = 0; i < this.scriptOfOurType.debugRingVertices.Length; i++)
		{
			Handles.Label(this.targetGameObject.transform.position + this.scriptOfOurType.debugRingVertices[i], i.ToString(), editorSkin.GetStyle("Label"));
		}
		*/
	}
}
