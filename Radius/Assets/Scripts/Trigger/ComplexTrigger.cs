/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ComplexTrigger.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComplexTrigger : ExposedTrigger {

	public ExposedTrigger[] andedCollidersTriggerState;
	
	Dictionary<ExposedTrigger, List<GameObject>> elementListForTriggerDictionary = new Dictionary<ExposedTrigger, List<GameObject>>();

	// Use this for initialization
	protected virtual void Start () {
		
		// Hook all exposed trigger events
		foreach(ExposedTrigger exposedTrigger in this.andedCollidersTriggerState)
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


	
	bool CheckIfElementInComplexTrigger(GameObject element)
	{
		bool elementInTrigger = true; // We start true because we use the AND operator
		// If the dictionary actually has all keys for each anded trigger for this complex collider
		if(this.elementListForTriggerDictionary.Count == andedCollidersTriggerState.Length)
		{
			// Give the object a chance and check if it is in each trigger
			foreach(KeyValuePair<ExposedTrigger, List<GameObject>> entry in this.elementListForTriggerDictionary)
			{
				elementInTrigger &= entry.Value.Contains(element);
			}
		}
		// Otherwise there is no way that it is in each because there aren't enough dictionary keys
		else
			elementInTrigger = false;

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

				// Check if we entered all of the triggers
				if(this.CheckIfElementInComplexTrigger(element))
				{
					if(!triggerInstanceList.Contains(element))
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

			// If the element exited a trigger
			// this means they are not in all of the triggers
			// so they exited the complex trigger
			// But they could of already exited a trigger so we don't want to double exit
			bool wasRemoved = triggerInstanceList.Remove(element);
			if(wasRemoved)
			{
				ThisTriggerExit(this, new ExposedTrigger.TriggerActivityEventArgs(element));
			}
		}
	}
}
