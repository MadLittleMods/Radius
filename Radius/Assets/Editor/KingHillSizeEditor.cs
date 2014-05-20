/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: KingHillSizeEditor.cs, May 2014
 */

using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(KingHillController))]
public class KingHillSizeEditor : Editor {

	KingHillController m_Instance;
	PropertyField[] m_fields;
	
	
	public void OnEnable()
	{
		m_Instance = target as KingHillController;
		m_fields = ExposeProperties.GetProperties( m_Instance );
	}
	
	public override void OnInspectorGUI () {
		
		if ( m_Instance == null )
			return;
		
		this.DrawDefaultInspector();
		
		ExposeProperties.Expose( m_fields );
		
	}
}
