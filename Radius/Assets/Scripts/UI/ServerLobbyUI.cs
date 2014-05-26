/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ServerLobbyUI.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using Coherent.UI;
using Coherent.UI.Binding;
using System.Collections.Generic;


public class ServerLobbyUI : MonoBehaviour {

	
	private CoherentUIView m_View;
	private bool viewReady = false;

	[SerializeField]
	private GameManager gameManager;
	[SerializeField]
	private PlayerManager playerManager;
	[SerializeField]
	private NetworkManager networkManager;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		// Separate the UI into its own group 
		// so it doesn't get disabled when we change levels
		networkView.group = 1;
		
		this.m_View = GetComponent<CoherentUIView>();
		this.m_View.OnViewCreated += (view) => {this.viewReady = true;};
		this.m_View.OnViewDestroyed += () => {this.viewReady = false;};

		this.playerManager.OnPlayerJoined += this.PlayerJoinedEvent;
		this.playerManager.OnPlayerLeft += this.PlayerLeftEvent;
		this.playerManager.OnPlayerUpdated += this.PlayerUpdatedEvent;

		this.gameManager.OnGameStarted += (sender) => {
			if(this.viewReady)
				this.m_View.View.TriggerEvent("respondToStartGame");
		};

		this.gameManager.OnGameEnded += (sender) => {
			if(this.viewReady)
				this.m_View.View.TriggerEvent("respondToEndGame");
		};

		// Make the Coherent View receive input
		if(this.m_View)
			this.m_View.ReceivesInput = true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlayerJoinedEvent(MonoBehaviour sender, PlayerManager.PlayerActivityEventArgs e)
	{
		Debug.Log("Player Joined: " + e.PlayerData.Gamertag + " - " + e.PlayerData.guid);
		if(this.viewReady)
			this.m_View.View.TriggerEvent("lobbyUpdatePlayer", e.PlayerData);
	}
	public void PlayerLeftEvent(MonoBehaviour sender, PlayerManager.PlayerActivityEventArgs e)
	{
		Debug.Log("Player Left: " + e.PlayerData.Gamertag + " - " + e.PlayerData.guid);
		if(this.viewReady)
			this.m_View.View.TriggerEvent("lobbyRemovePlayer", e.PlayerData.guid);
	}
	public void PlayerUpdatedEvent(MonoBehaviour sender, PlayerManager.PlayerActivityEventArgs e)
	{
		Debug.Log("Player Updated: " + e.PlayerData.Gamertag + " - " + e.PlayerData.guid);
		if(this.viewReady)
			this.m_View.View.TriggerEvent("lobbyUpdatePlayer", e.PlayerData);
	}
	
	[Coherent.UI.CoherentMethod("GUIAddAllToPlayerList")]
	public void AddAllToPlayerList()
	{
		if(this.viewReady)
		{
			foreach(KeyValuePair<string, Player> playerEntry in this.playerManager.PlayerList)
			{
				this.m_View.View.TriggerEvent("lobbyUpdatePlayer", playerEntry.Value.GetPlayerData());
			}
		}
	}

	[ContextMenu("UpdateGUIPlayerList")]
	private void UpdateGUIPlayerList()
	{
		if(this.viewReady)
			this.m_View.View.TriggerEvent("updatePlayerList");
	}

	[Coherent.UI.CoherentMethod("GUIGetPartySize")]
	public Dictionary<string, int> GUIGetPartySize()
	{
		Dictionary<string, int> partyDictionary = new Dictionary<string, int>() {
			{"connectedPlayers", this.networkManager.ServerData.connectedPlayers },
			{"playerLimit", this.networkManager.ServerData.playerLimit },
		};

		return partyDictionary;

	}


	[Coherent.UI.CoherentMethod("GUIStartGame")]
	public void GUIStartGame()
	{
		this.gameManager.ChangeLevel(this.gameManager.SelectedLevel);
		this.gameManager.GameStatus = GameManager.GameState.started;
	}

	[Coherent.UI.CoherentMethod("GUIEndGame")]
	public void GUIEndGame()
	{
		this.gameManager.GameStatus = GameManager.GameState.notStarted;
	}
	

	[Coherent.UI.CoherentMethod("GUIProfileColorChange")]
	public void ProfileColorChange(string guid, ValueObject colorObj)
	{
		Debug.Log("Profile Color Change: rgb(" + (int)colorObj["r"] + ", " + (int)colorObj["g"] + ", " + (int)colorObj["b"] + ")");

		this.playerManager.GetPlayer(guid).PersonalColor = ColorData.Color255ToColor1((int)colorObj["r"], (int)colorObj["g"], (int)colorObj["b"], 1f);
	}

	[Coherent.UI.CoherentMethod("GUIProfileGamertagChange")]
	public void ProfileGamertagChange(string guid, string newGamertag)
	{
		Debug.Log("Profile Gamertag Change: " + newGamertag);
		
		this.playerManager.GetPlayer(guid).Gamertag = newGamertag;
	}

}
