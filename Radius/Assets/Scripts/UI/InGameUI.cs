/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: InGameUI.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using Coherent.UI;
using Coherent.UI.Binding;

public class InGameUI : MonoBehaviour {

	
	private CoherentUIView m_View;
	private bool viewReady = false;

	[SerializeField]
	private PlayerManager playerManager;
	[SerializeField]
	private TerritoryManager territoryManager;
	[SerializeField]
	private ScoreManager scoreManager;
	[SerializeField]
	private GameManager gameManager;

	private MonoBehaviour currentTerritory;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		// Separate the UI into its own group 
		// so it doesn't get disabled when we change levels
		networkView.group = 1; 
		
		this.m_View = GetComponent<CoherentUIView>();
		this.m_View.OnViewCreated += (view) => {this.viewReady = true;};
		this.m_View.OnViewDestroyed += () => {this.viewReady = false;};

		// Make the Coherent View receive input
		if(this.m_View)
			this.m_View.ReceivesInput = true;

		// If someone enters a territory
		this.territoryManager.OnATerritoryEntered += (sender, e) => {
			// And it is us...
			// Show the progress
			if(e.player.guid == this.playerManager.GetPlayer("self").guid)
			{
				this.ShowTerritoryProgress(e.TerritoryData);
				this.currentTerritory = sender;
			}

		};

		// If someone exits a territory
		this.territoryManager.OnATerritoryExited += (sender, e) => {
			// And it is us...
			// Hide the progress
			if(Object.ReferenceEquals(this.currentTerritory, sender))
			{
				if(e.player.guid == this.playerManager.GetPlayer("self").guid)
				{
					this.HideTerritoryProgress();
					this.currentTerritory = null;
				}
			}

		};

		// If a territory is updated
		this.territoryManager.OnATerritoryUpdated += (sender, tData) => {
			// If the territory we are in is the same as the one updated
			if(Object.ReferenceEquals(this.currentTerritory, sender))
				if(this.viewReady)
					this.m_View.View.TriggerEvent("territoryUpdated", tData);
		};

		// If the score is updated
		this.scoreManager.OnScoreUpdated += (sender, e) => {
			this.UpdateScoreBox(e.ScoreData);
		};

		this.playerManager.OnPlayerUpdated += this.PlayerUpdatedEvent;


		// Stop the time once the game ends
		this.gameManager.OnGameEnded += (sender) => {
			this.StopGameTimeBox();
		};
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void ShowTerritoryProgress(TerritoryController.TerritoryData tData)
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("showTerritoryProgress", tData);
	}

	void HideTerritoryProgress()
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("hideTerritoryProgress");
	}


	void UpdateScoreBox(ScoreManager.ScoreData sData)
	{
		//Debug.Log("Updating Score Box");
		if(this.viewReady)
			this.m_View.View.TriggerEvent("updateScoreBox", sData);
	}

	public void PlayerUpdatedEvent(MonoBehaviour sender, PlayerManager.PlayerActivityEventArgs e)
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("hudUpdatePlayer", e.PlayerData);
	}



	void UpdateGameTimeBox(float time)
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("updateGameTimeBox", time);
	}



	void StopGameTimeBox()
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("stopCountDownGameTime");
	}


	
	

	

}
