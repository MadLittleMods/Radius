/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ServerBrowserUI.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using Coherent.UI;
using Coherent.UI.Binding;
using System.Collections.Generic;


public class ServerBrowserUI : MonoBehaviour {


	[SerializeField]
	private NetworkManager netMan;
	[SerializeField]
	private PlayerManager playerManager;
	[SerializeField]
	private GameManager gameManager;

	
	private CoherentUIView m_View;
	private bool viewReady = false;

	private bool lastIsServer;
	private bool lastIsClient;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		networkView.group = 1;

		this.m_View = GetComponent<CoherentUIView>();
		m_View.OnViewCreated += (view) => {this.viewReady = true;};
		m_View.OnViewDestroyed += () => {this.viewReady = false;};

		// Make the Coherent View receive input
		if(this.m_View)
			this.m_View.ReceivesInput = true;

		if(this.netMan)
		{
			this.netMan.OnServerListUpdated += UpdateGUIServerList; // Bind to event
			this.netMan.OnServerRegisterAttempt += RespondToServerRegisterAttempt; // Bind to event
			this.netMan.OnAsyncConnectInfoEvent += RespondToAsyncConnectInfoEvent;
			this.netMan.OnDisconnected += RespondToDisconnect;
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Check if we are connected and add//subtract the connect and disconnect button
		if(Network.isServer != this.lastIsServer || Network.isClient != this.lastIsClient)
		{
			bool connected = Network.isServer || Network.isClient;

			if(this.viewReady)
				this.m_View.View.TriggerEvent("updateConnectedStatus", connected);

			this.lastIsServer = Network.isServer;
			this.lastIsClient = Network.isClient;
		}
	}

	public void NewServerBrowserMessage(string message)
	{
		// Adds a small message in the bottom left corner of the server browser
		// to better communicate what is happening
		Debug.Log(message);
		if(this.viewReady)
			this.m_View.View.TriggerEvent("newServerBrowserMessage", message);
	}

	public void UpdateGUIServerList(ServerData[] serverList)
	{
		// Update the server browser with the new-found servers
		if(this.viewReady)
			this.m_View.View.TriggerEvent("updateServerList", serverList);
		Debug.Log("Updated serverlist UI: " + serverList.Length);
		this.NewServerBrowserMessage("Refreshed Server List");
	}

	public void RespondToServerRegisterAttempt(NetworkManager.ServerCreationEvent sce)
	{
		bool pass = false;
		string message = "";

		if(sce == NetworkManager.ServerCreationEventCode.RegistrationSucceeded)
		{
			pass = true;
			message = "Server Registered Successfully";
			this.NewServerBrowserMessage("Started Server");
		}
		else
		{
			pass = false;
			message = "Error Starting Server";

			// If they tried to register a server online without having internet
			if(sce == NetworkManager.ServerCreationEventCode.MSRegistrationFailedNoInternet)
			{
				message = "Can't register server without Internet. Try a LAN game";

				if(this.viewReady)
					this.m_View.View.TriggerEvent("respondToNoInternetServerRegisterAttempt");
			}

			this.NewServerBrowserMessage(message);
			// Disconnect from the server we created, as it wasn't registered
			this.netMan.Disconnect();
		}

		if(this.viewReady)
			this.m_View.View.TriggerEvent("respondToServerRegisterAttempt", pass, message);
	}


	public void RespondToAsyncConnectInfoEvent(NetworkManager.NetworkConnectionErrorResponse response)
	{
		// This function responds to `OnConnectedToServer()` and `OnFailedToConnect()`
		// Which definitively say if we connected and are called async after `Connect()`

		// Add the message
		this.NewServerBrowserMessage(response.message);

		// If we are able to connect, close the menus
		if(this.viewReady)
			if(response.ableToConnectYet)
				this.m_View.View.TriggerEvent("respondToServerConnectSuccess"); // close menus
	}

	public void RespondToDisconnect(NetworkDisconnection info)
	{
		if(this.viewReady)
			m_View.View.TriggerEvent("respondToServerDisconnect");

		string message = "";

		if(info == NetworkDisconnection.LostConnection)
			message = "The connection to the server has been lost";
		else if(info == NetworkDisconnection.Disconnected)
			message = "The connection to the server has been closed";

		this.NewServerBrowserMessage(message);
	}


	[Coherent.UI.CoherentMethod("GUIStartServer")]
	public void GUIStartServer(ValueObject serverObj)
	{
		Debug.Log("Made it to Unity");

		/*
		foreach(var item in serverObj.Values)
		{
			Debug.Log((string)item);
		}
		*/

		try {
			int maxPlayers = 8;
			int.TryParse((string)serverObj["max_players"], out maxPlayers);

			this.netMan.StartServer(
				(string)serverObj["server_name"], 
				(string)serverObj["lan"] == "true" ? true : false, 
				(string)serverObj["server_pw"], 
				maxPlayers, 
				(string)serverObj["server_description"], 
				(string)serverObj["server_map"], 
				(string)serverObj["server_gametype"]
			);

			// Set the game time limit
			if(this.gameManager != null)
			{
				float timeLimit = -1; // Infinite
				float.TryParse((string)serverObj.GetValueOrDefault("time_limit", new Value("-1")), out timeLimit);
				this.gameManager.GameTimeLimit = timeLimit;
			}

		} catch(InvalidValueCastException e) {
			Debug.LogWarning("Cast exception while trying to start a server: " + e);
		}
	}

	[Coherent.UI.CoherentMethod("GUIRefreshServerList")]
	public void GUIRefreshServerList()
	{
		this.netMan.RefreshHostList();
		this.NewServerBrowserMessage("Refreshing Server List...");
	}

	[Coherent.UI.CoherentMethod("GUIConnect")]
	public void GUIConnect(string guid, string pw)
	{
		NetworkManager.NetworkConnectionErrorResponse response = this.netMan.Connect(guid, pw);

		string message = "";

		// Only show error if some error is returned
		// The response message could return "Yay, You connected" but we
		// only want error at this point
		if(!response.noErrorsYet)
			message = response.message;

		// Only add a message if there is a message
		if(message != "")
			this.NewServerBrowserMessage(message);
	}

	[Coherent.UI.CoherentMethod("GUIDisconnect")]
	public void GUIDisconnect()
	{
		this.netMan.Disconnect();
	}




	[ContextMenu ("Test trigger")]
	void DoSomething () {
		if(this.viewReady)
			this.m_View.View.TriggerEvent("testTrigger");
	}


	

}
