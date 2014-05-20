/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: TerritoryController.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TerritoryController : MonoBehaviour {

	public struct TerritoryData
	{
		public float Progress;
		public Dictionary<string, float> Color;
		public Dictionary<string, float> LockedColor;
		public Dictionary<string, float> TempColor;
	}

	public class TerritoryActivityEventArgs : EventArgs
	{
		public Player player;
		public TerritoryData TerritoryData;
		
		public TerritoryActivityEventArgs(Player player, TerritoryData tData)
		{
			this.player = player;
			this.TerritoryData = tData;
		}
	}

	public delegate void TerritoryCapturedEventHandler(MonoBehaviour sender, TerritoryActivityEventArgs e);
	public event TerritoryCapturedEventHandler OnTerritoryCaptured;

	public delegate void TerritoryLostEventHandler(MonoBehaviour sender, TerritoryActivityEventArgs e);
	public event TerritoryLostEventHandler OnTerritoryLost;

	public delegate void TerritoryUpdatedEventHandler(MonoBehaviour sender, TerritoryData tData);
	public event TerritoryUpdatedEventHandler OnTerritoryUpdated;

	public delegate void TerritoryEnteredEventHandler(MonoBehaviour sender, TerritoryActivityEventArgs e);
	public event TerritoryEnteredEventHandler OnTerritoryEntered;

	public delegate void TerritoryExitedEventHandler(MonoBehaviour sender, TerritoryActivityEventArgs e);
	public event TerritoryExitedEventHandler OnTerritoryExited;


	// Use this to trigger the event
	protected virtual void ThisTerritoryCaptured(MonoBehaviour sender, TerritoryActivityEventArgs e)
	{
		TerritoryCapturedEventHandler handler = OnTerritoryCaptured;
		if(handler != null)
		{
			handler(sender, e);
		}
	}

	// Use this to trigger the event
	protected virtual void ThisTerritoryLost(MonoBehaviour sender, TerritoryActivityEventArgs e)
	{
		TerritoryLostEventHandler handler = OnTerritoryLost;
		if(handler != null)
		{
			handler(sender, e);
		}
	}

	// Use this to trigger the event
	private float lastUpdateEventTime = 0f;
	private TerritoryData lastUpdatedTerritoryData;
	protected virtual void ThisTerritoryUpdated(MonoBehaviour sender, TerritoryData tData)
	{
		TerritoryUpdatedEventHandler handler = OnTerritoryUpdated;
		if(handler != null)
		{
			// Only fire this event every 10nth of a second
			if(Time.realtimeSinceStartup - this.lastUpdateEventTime > .1f)
			{
				// If something has actually updated and not set to the same value as before
				if(this.lastUpdatedTerritoryData.Progress != tData.Progress || !tData.Color.CompareRGB(this.lastUpdatedTerritoryData.Color))
					handler(sender, tData);
				
				this.lastUpdateEventTime = Time.realtimeSinceStartup;
				this.lastUpdatedTerritoryData = tData;
			}
		}
	}

	// Use this to trigger the event
	protected virtual void ThisTerritoryEntered(MonoBehaviour sender, TerritoryActivityEventArgs e)
	{
		TerritoryEnteredEventHandler handler = OnTerritoryEntered;
		if(handler != null)
		{
			handler(sender, e);
		}
	}
	// Use this to trigger the event
	protected virtual void ThisTerritoryExited(MonoBehaviour sender, TerritoryActivityEventArgs e)
	{
		TerritoryExitedEventHandler handler = OnTerritoryExited;
		if(handler != null)
		{
			handler(sender, e);
		}
	}


	public ExposedTrigger territoryTrigger; // The territory

	public float _timeToCapture = 3f;
	public float TimeToCapture
	{
		get {
			return this._timeToCapture;
		}
		set {
			if(value > 0)
				this._captureProgess = value;
		}
	}



	private float _captureProgess = 0f; // Range 0f to 1f
	public float CaptureProgress
	{
		get {
			return this._captureProgess;
		}
		private set {
			this._captureProgess = Mathf.Clamp(value, 0f, 1f);
		}
	}

	private Player tempCaptureState = null; // The current Locked in state of this point
	private bool territoryContested = false;


	private Player _lockedCaptureState = null; // The current Locked in state of this point
	public Player LockedCaptureState
	{
		get {
			return this._lockedCaptureState;
		}
		private set {
			this._lockedCaptureState = value;
		}
	}
	
	// Used for keeping track of individuals capturing the territory
	Dictionary<Player.Team, List<Player>> playersCapturing = new Dictionary<Player.Team, List<Player>>();

	Player playerCapturing = null; // Used in the update loop so we don't have to mess with the dictionary

	// Use this for initialization
	void Start () {
		this.territoryTrigger.OnThisTriggerEnter += this.TerritoryEnter;
		this.territoryTrigger.OnThisTriggerExit += this.TerritoryExit;

	}
	
	// Update is called once per frame
	void Update () {
		this.HandleCapturing();
	}

	void HandleCapturing() {
		// If there is a player capturing 
		if(this.playerCapturing)
		{
			if(this.tempCaptureState == this.playerCapturing)
			{
				// Capture point
				this.CaptureProgress += Time.deltaTime/this.TimeToCapture;
				
				// Only lock the capture state once we reach the progress of 1
				if(this.LockedCaptureState != this.tempCaptureState && this.CaptureProgress >= 1f)
				{
					// Captured point
					this.LockedCaptureState = this.playerCapturing;
					// Fire the event
					TerritoryData tDataLocked = new TerritoryData();
					tDataLocked.Progress = this.CaptureProgress;
					tDataLocked.Color = new ColorData(this.LockedCaptureState.ThisTeamToColor()).ToDictionary();
					tDataLocked.LockedColor = new ColorData(this.LockedCaptureState.ThisTeamToColor()).ToDictionary();
					tDataLocked.TempColor = new ColorData(this.LockedCaptureState.ThisTeamToColor()).ToDictionary();
					this.ThisTerritoryCaptured(this, new TerritoryActivityEventArgs(this.LockedCaptureState, tDataLocked));
					
				}
			}
			else
			{
				// Uncapture the point 
				// we trying to steal this territory
				this.CaptureProgress -= Time.deltaTime/this.TimeToCapture;
				
				// Only unlock the capture state once we reach the progress of 0
				if(this.CaptureProgress <= 0f)
				{
					Debug.Log("captureProgress <= 0 ~~ " + (this.LockedCaptureState ? this.LockedCaptureState.PersonalColor.ToString() : "") + " : " + (this.tempCaptureState ? this.tempCaptureState.PersonalColor.ToString() : ""));
					if(this.LockedCaptureState)
					{
						// Make sure to only unlock the point once
						if(this.LockedCaptureState != this.playerCapturing)
						{
							// Fire the event
							Debug.Log("ThisTerritoryLost");
							TerritoryData tDataUnlocked = new TerritoryData();
							tDataUnlocked.Progress = this.CaptureProgress;
							tDataUnlocked.Color = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
							tDataUnlocked.LockedColor = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
							tDataUnlocked.TempColor = new ColorData(this.playerCapturing.ThisTeamToColor()).ToDictionary();
							this.ThisTerritoryLost(this, new TerritoryActivityEventArgs(this.LockedCaptureState, tDataUnlocked));
						}
					}
					
					// Uncapture point
					this.LockedCaptureState = null;
					
					// Now that we uncaptured the point set the temp capture state to the capturing player
					this.tempCaptureState = this.playerCapturing;
				}
			}
			
			
			// Fire Event
			TerritoryData tData = new TerritoryData();
			tData.Progress = this.CaptureProgress;
			tData.Color = new ColorData(this.LockedCaptureState ? this.LockedCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
			tData.LockedColor = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
			tData.TempColor = new ColorData(this.playerCapturing.ThisTeamToColor()).ToDictionary();
			this.ThisTerritoryUpdated(this, tData);
			
		}
		else
		{
			// If the hill isn't contested then auto-progress
			if(!this.territoryContested)
			{
				// If someone is not capturing and it is captured by some team, auto progress up
				if(this.LockedCaptureState && this.LockedCaptureState.PlayerTeam != Player.Team.None)
				{
					this.CaptureProgress += Time.deltaTime/this.TimeToCapture;
					
					// Fire Event
					TerritoryData tData = new TerritoryData();
					tData.Progress = this.CaptureProgress;
					tData.Color = new ColorData(this.LockedCaptureState.ThisTeamToColor()).ToDictionary();
					tData.LockedColor = new ColorData(this.LockedCaptureState.ThisTeamToColor()).ToDictionary();
					tData.TempColor = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
					this.ThisTerritoryUpdated(this, tData);
				}
				// Otherwise we should auto progress down as the team trying didn't stay long enough
				else
				{
					this.CaptureProgress -= Time.deltaTime/this.TimeToCapture;
					
					// just clean up in case anything happened
					if(this.CaptureProgress <= 0f)
					{
						// Uncapture point
						this.LockedCaptureState = null;
						
						// Now that we uncaptured the point set the temp capture state to the capturing player
						this.tempCaptureState = this.playerCapturing;
					}
					
					
					// Fire Event
					TerritoryData tData = new TerritoryData();
					tData.Progress = this.CaptureProgress;
					tData.Color = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
					tData.LockedColor = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
					tData.TempColor = new ColorData(Player.TeamToColor(Player.Team.None)).ToDictionary();
					this.ThisTerritoryUpdated(this, tData);
				}
				
				
			}
		}
		
		
		
		//Debug.Log("Player: " + (this.playerCapturing ? this.playerCapturing.PlayerTeam.ToString() : "None") + (this.playerCapturing ? this.playerCapturing.PersonalColor.ToString() : "") + " - Contested?: " + this.territoryContested +" - Progress: " + this.CaptureProgress + " - TempState: " + (this.tempCaptureState ? this.tempCaptureState.PlayerTeam.ToString() : "None") + (this.tempCaptureState ? this.tempCaptureState.PersonalColor.ToString() : "") + " - LockedState: " + (this.LockedCaptureState ? this.LockedCaptureState.PlayerTeam.ToString() : "None") + (this.LockedCaptureState ? this.LockedCaptureState.PersonalColor.ToString() : ""));
	}

	void CheckIfPlayerIsCapturing()
	{
		if(this.playersCapturing.Count > 0)
		{
			int numTeamsInTerritory = 0;
			int numIndividualInTerritory = 0;
			Player firstPlayerOfTeamCapturing = null;
			foreach(KeyValuePair<Player.Team, List<Player>> entry in this.playersCapturing)
			{
				if(entry.Value.Count > 0)
				{
					numTeamsInTerritory++;
					firstPlayerOfTeamCapturing = entry.Value[0];
				}

				if(entry.Key == Player.Team.Individual)
				{
					numIndividualInTerritory = entry.Value.Count;
				}
			}


			this.territoryContested  = false; // Default this to false
			if(numTeamsInTerritory == 1 && numIndividualInTerritory == 1)
			{
				Debug.Log("Someone Capturing");
				this.playerCapturing = firstPlayerOfTeamCapturing;
			}
			else if(numTeamsInTerritory == 1 && numIndividualInTerritory == 0)
			{
				Debug.Log("Someone Capturing");
				this.playerCapturing = firstPlayerOfTeamCapturing;
			}
			else if(numTeamsInTerritory > 1 || numIndividualInTerritory > 1)
			{
				this.territoryContested = true; // Only true if it happens
				this.playerCapturing = null;
			}
	        else
			{
				this.playerCapturing = null;
			}
		}
		else
			this.playerCapturing = null;
	}

	void TerritoryEnter(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		Player player = e.gameObject.GetComponent<Player>();
		if(player != null)
		{
			// Add elements
			List<Player> playerList;
			if (!this.playersCapturing.TryGetValue(player.PlayerTeam, out playerList))
				this.playersCapturing.Add(player.PlayerTeam, playerList = new List<Player>());

			playerList.Add(player);
			this.CheckIfPlayerIsCapturing(); // Something changed so maybe someone is capturing
		}

		// Fire our own event from this script
		TerritoryData tData = new TerritoryData();
		tData.Progress = this.CaptureProgress;
		tData.Color = new ColorData(this.LockedCaptureState ? this.LockedCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		tData.LockedColor = new ColorData(this.LockedCaptureState ? this.LockedCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		tData.TempColor = new ColorData(this.tempCaptureState ? this.tempCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		this.ThisTerritoryEntered(this, new TerritoryActivityEventArgs(player, tData));
	}

	void TerritoryExit(ExposedTrigger sender, ExposedTrigger.TriggerActivityEventArgs e)
	{
		Player player = e.gameObject.GetComponent<Player>();
		if(player != null)
		{
			// Remove the element
			List<Player> playerList;
			if (!this.playersCapturing.TryGetValue(player.PlayerTeam, out playerList))
				this.playersCapturing.Add(player.PlayerTeam, playerList = new List<Player>());

			playerList.Remove(player);
			this.CheckIfPlayerIsCapturing(); // Something changed so maybe someone is capturing
		}
		
		// Fire our own event from this script
		TerritoryData tData = new TerritoryData();
		tData.Progress = this.CaptureProgress;
		tData.Color = new ColorData(this.LockedCaptureState ? this.LockedCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		tData.LockedColor = new ColorData(this.LockedCaptureState ? this.LockedCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		tData.TempColor = new ColorData(this.tempCaptureState ? this.tempCaptureState.ThisTeamToColor() : Player.TeamToColor(Player.Team.None)).ToDictionary();
		this.ThisTerritoryExited(this, new TerritoryActivityEventArgs(player, tData));
	}

}
