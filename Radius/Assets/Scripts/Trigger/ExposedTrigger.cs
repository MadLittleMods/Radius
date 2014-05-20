/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ExposedTrigger.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ExposedTrigger : MonoBehaviour {

	public class TriggerActivityEventArgs : EventArgs
	{
		public GameObject gameObject;

		public TriggerActivityEventArgs(GameObject go)
		{
			this.gameObject = go;
		}
	}

	public delegate void ThisTriggerEnterEventHandler(ExposedTrigger sender, TriggerActivityEventArgs e);
	public event ThisTriggerEnterEventHandler OnThisTriggerEnter;

	public delegate void ThisTriggerExitEventHandler(ExposedTrigger sender, TriggerActivityEventArgs e);
	public event ThisTriggerExitEventHandler OnThisTriggerExit;

	// Use this to trigger the event
	protected virtual void ThisTriggerEnter(ExposedTrigger sender, TriggerActivityEventArgs e)
	{
		ThisTriggerEnterEventHandler handler = OnThisTriggerEnter;
		if(handler != null)
		{
			handler(sender, e);
		}
	}

	// Use this to trigger the event
	protected virtual void ThisTriggerExit(ExposedTrigger sender, TriggerActivityEventArgs e)
	{
		ThisTriggerExitEventHandler handler = OnThisTriggerExit;
		if(handler != null)
		{
			handler(sender, e);
		}
	}


	[HideInInspector]
	public List<GameObject> triggerInstanceList = new List<GameObject>(); // Objects in the trigger

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
		triggerInstanceList.Add(other.gameObject);
		this.ThisTriggerEnter(this, new TriggerActivityEventArgs(other.gameObject));
	}
	
	void OnTriggerExit(Collider other) {
		triggerInstanceList.Remove(other.gameObject);
		this.ThisTriggerExit(this, new TriggerActivityEventArgs(other.gameObject));
	}
}
