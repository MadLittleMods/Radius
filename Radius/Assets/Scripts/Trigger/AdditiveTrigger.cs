/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: AdditiveTrigger.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdditiveTrigger : ExposedTrigger {
	
	public ExposedTrigger[] oredCollidersTriggerState;

	Dictionary<ExposedTrigger, List<GameObject>> elementListForTriggerDictionary = new Dictionary<ExposedTrigger, List<GameObject>>();

	// Use this for initialization
	protected virtual void Start () {
		
		// Hook all exposed trigger events
		foreach(ExposedTrigger exposedTrigger in this.oredCollidersTriggerState)
		{
			exposedTrigger.OnThisTriggerEnter += this.ExposedTriggerEnter;
			exposedTrigger.OnThisTriggerExit += this.ExposedTriggerExit;
		}
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		//Debug.Log(elementListForTriggerDictionary.ToDebugStringWithList());
	}

	// Method hide these so that the event is called by the base class
	void OnTriggerEnter(Collider other) {
	}
	void OnTriggerExit(Collider other) {	
	}

	bool CheckIfElementInAdditiveTrigger(GameObject element)
	{
		bool elementInTrigger = false; // We start false because we use the OR operator
		foreach(KeyValuePair<ExposedTrigger, List<GameObject>> entry in elementListForTriggerDictionary)
		{
			elementInTrigger |= entry.Value.Contains(element);

			// All it takes is one trigger to be in an additive trigger
			if(elementInTrigger)
				break;
		}
		
		return elementInTrigger;
	}
	
	void ExposedTriggerEnter(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		GameObject element = e.gameObject;
		if(element != null)
		{
			
			// Add elements
			List<GameObject> elementList;
			if (!this.elementListForTriggerDictionary.TryGetValue(sender, out elementList))
				this.elementListForTriggerDictionary.Add(sender, elementList = new List<GameObject>());
			
			// Add it to the list only if it doesn't exist already
			// This is just a safety, as it should get removed before triggered again
			if(!elementList.Contains(element))
			{
				elementList.Add(element);
				
				// Since this is a additive trigger
				// Any enter triggers a enter.
				// But we want to make sure to only send one enter if already in this additive trigger
				if(!triggerInstanceList.Contains(element))
				{
					triggerInstanceList.Add(element);
					ThisTriggerEnter(this, new ExposedTrigger.TriggerActivityEventArgs(element));
				}
			}
		}
	}
	
	void ExposedTriggerExit(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		GameObject element = e.gameObject;
		if(element != null)
		{
			
			// Remove the element
			List<GameObject> elementList;
			if (!this.elementListForTriggerDictionary.TryGetValue(sender, out elementList))
				this.elementListForTriggerDictionary.Add(sender, elementList = new List<GameObject>());
			
			elementList.Remove(element);
			
			// Check if we are still in any of the triggers
			if(!this.CheckIfElementInAdditiveTrigger(element))
			{
				triggerInstanceList.Remove(element);
				ThisTriggerExit(this, new ExposedTrigger.TriggerActivityEventArgs(element));
			}
		}
	}
}
