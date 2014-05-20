/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: CharacterDriverLabel.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CharacterDriver))]
public class CharacterDriverLabel : Editor {
	
	GUISkin editorSkin;
	
	GameObject targetGameObject; // The GameObject that this editor uses
	CharacterDriver scriptOfOurType; // The script we add the custom editor for

	float groundSpeedCache = 0f;

	void OnEnable()
	{
		this.editorSkin = (GUISkin)(Resources.LoadAssetAtPath("Assets/Editor/EditorGUISkin.guiskin", typeof(GUISkin)));
		
		this.scriptOfOurType = (CharacterDriver)target;    
		this.targetGameObject = (GameObject)this.scriptOfOurType.gameObject;
	}
	
	void OnSceneGUI ()
	{
		// Get gizmo string jump height
		string jumpHeight = "Jump: " + (this.scriptOfOurType.debugJumpYMax - this.scriptOfOurType.debugJumpYStart);
		// Get gizmo string speed
		this.groundSpeedCache = Mathf.Lerp(this.groundSpeedCache, this.scriptOfOurType.debugGroundSpeed, .05f);
		string groundSpeed = "Ground Speed: " + this.groundSpeedCache.ToString("f2");

		Handles.Label(this.targetGameObject.transform.position, jumpHeight + "\n" + groundSpeed, editorSkin.GetStyle("Label"));
	}
}
