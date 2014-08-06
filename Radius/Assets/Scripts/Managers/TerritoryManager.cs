/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: TerritoryManager.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerritoryManager : MonoBehaviour {


	public delegate void ATerritoryEnteredEventHandler(MonoBehaviour sender, TerritoryController.TerritoryActivityEventArgs e);
	public event ATerritoryEnteredEventHandler OnATerritoryEntered = delegate { };
	
	public delegate void ATerritoryExitedEventHandler(MonoBehaviour sender, TerritoryController.TerritoryActivityEventArgs e);
	public event ATerritoryExitedEventHandler OnATerritoryExited = delegate { };

	public delegate void ATerritoryUpdatedEventHandler(MonoBehaviour sender, TerritoryController.TerritoryData tData);
	public event ATerritoryUpdatedEventHandler OnATerritoryUpdated = delegate { };



	[SerializeField]
	private GameManager gameManager;
	
	List<TerritoryController> territoryList = new List<TerritoryController>();


	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		networkView.group = 1;

		// Everytime we change levels we need to regather all of the territories
		this.gameManager.OnLevelChanged += (sender, e) => {
			// 1: empty level
			if(e.level != 1)
				this.GatherTerritories(); 
		};
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void GatherTerritories()
	{
		// Updates the territory list for this level
		Debug.Log("Collecting Territories");

		this.territoryList.Clear();
		TerritoryController[] territories = GameObject.FindObjectsOfType(typeof(TerritoryController)) as TerritoryController[];
		Debug.Log("Collected: " + territories.Length + " territories");
		foreach (TerritoryController territory in territories) {
			this.territoryList.Add(territory);

			// Binding this territory
			territory.OnTerritoryEntered += (sender, e) => {
				// Fire this scripts event
				//Debug.Log("ThisATerritoryEntered");
				this.OnATerritoryEntered(sender, e);
			};
			territory.OnTerritoryExited += (sender, e) => {
				// Fire this scripts event
				//Debug.Log("ThisATerritoryExited");
				this.OnATerritoryExited(sender, e);
			};
			territory.OnTerritoryUpdated += (sender, tData) => {
				// Fire this scripts event
				//Debug.Log("ThisATerritoryUpdated");
				this.OnATerritoryUpdated(sender, tData);
			};
		}
	}
}
