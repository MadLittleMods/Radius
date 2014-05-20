/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: DebugExposeTriggerState.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugExposeTriggerState : MonoBehaviour {

	public ExposedTrigger[] triggerList;

	// Use this for initialization
	void Start () {
		// Hook all exposed trigger events
		foreach(ExposedTrigger exposedTrigger in this.triggerList)
		{
			exposedTrigger.OnThisTriggerEnter += this.ExposedTriggerEnter;
			exposedTrigger.OnThisTriggerExit += this.ExposedTriggerExit;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void ExposedTriggerEnter(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		Debug.Log("Enter ~~ Sender: " + sender + " Object: " + e.gameObject);
	}

	void ExposedTriggerExit(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		Debug.Log("Exit ~~ Sender: " + sender + " Object: " + e.gameObject);
	}
}
