/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: NetworkManager.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public class NetworkManager : MonoBehaviour {

	public enum ServerCreationEventCode
	{
		MSRegistrationFailed, // We tried to register on the master server but it didn't like what we gave it
		MSRegistrationFailedNoInternet, // Can't register on the master server without internet
		ServerInitFailedNoPorts, // We tried all `serverPorts`, but none are available
		RegistrationSucceeded
	}

	public class ServerCreationEvent
	{
		public ServerCreationEventCode eventCode;

		// Constructor:
		// Creates a ServerCreationEvent given a ServerCreationEventCode
		// ex. 
		//		ServerCreationEvent myEvent = ServerCreationEventCode.RegistrationSucceeded;
		public static implicit operator ServerCreationEvent(ServerCreationEventCode eCode)
		{ 
			return new ServerCreationEvent(eCode);
		}

		// Constructor:
		// Creates a ServerCreationEvent given a MasterServerEvent
		// ex. 
		//		ServerCreationEvent e = MasterServerEvent.RegistrationSucceeded
		public static implicit operator ServerCreationEvent(MasterServerEvent mse)
		{ 
			return new ServerCreationEvent(mse);
		}


		// Creates a ServerCreationEvent given a MasterServerEvent
		// ex.
		//		ServerCreationEventCode eCode = new ServerCreationEvent
		public static implicit operator ServerCreationEventCode(ServerCreationEvent sce)
		{
			return sce.eventCode;
		}


		public ServerCreationEvent(ServerCreationEventCode eCode)
		{
			this.eventCode = eCode;
		}

		public ServerCreationEvent(MasterServerEvent mse)
		{
			if(mse == MasterServerEvent.RegistrationSucceeded)
				this.eventCode = ServerCreationEventCode.RegistrationSucceeded;
			else if(mse == MasterServerEvent.RegistrationFailedGameName || mse == MasterServerEvent.RegistrationFailedGameType || mse == MasterServerEvent.RegistrationFailedNoServer)
				this.eventCode = ServerCreationEventCode.MSRegistrationFailed;
		}
	}
	
	public class NetworkConnectionErrorResponse
	{
		public string message = "";

		// We use "Yet" on the following variables because a errors response can mean
		// different things at certain stages of the whole connecting process.
		// We first see this in the `Network.Connect()` function but we don't really know
		// if we really connected until we get Network.OnFailedToConnect()` or `Network.OnConnectedToServer()`
		public bool noErrorsYet = false; // Always assume error unless otherwise specified
		public bool ableToConnectYet = false;
	}

	public delegate void ServerListUpdatedEventHandler(ServerData[] serverList);
	public event ServerListUpdatedEventHandler OnServerListUpdated;

	public delegate void RegisterServerAttemptEventHandler(ServerCreationEvent sce);
	public event RegisterServerAttemptEventHandler OnServerRegisterAttempt;

	public delegate void AsyncConnectInfoEventHandler(NetworkConnectionErrorResponse response);
	public event AsyncConnectInfoEventHandler OnAsyncConnectInfoEvent;

	public delegate void DisconnectedFromServerEventHandler(NetworkDisconnection info);
	public event DisconnectedFromServerEventHandler OnDisconnected;

	// Use this to trigger the event
	protected virtual void ThisServerListUpdated(ServerData[] serverList)
	{
		ServerListUpdatedEventHandler handler = OnServerListUpdated;
		if(handler != null)
		{
			handler(serverList);
		}
	}
	
	// Use this to trigger the event
	protected virtual void ThisRegisterServerAttempt(ServerCreationEvent sce)
	{
		RegisterServerAttemptEventHandler handler = OnServerRegisterAttempt;
		if(handler != null)
		{
			handler(sce);
		}
	}

	// Use this to trigger the event
	protected virtual void ThisAsyncConnectInfo(NetworkConnectionErrorResponse response)
	{
		AsyncConnectInfoEventHandler handler = OnAsyncConnectInfoEvent;
		if(handler != null)
		{
			handler(response);
		}
	}

	// Use this to trigger the event
	protected virtual void ThisDisconnected(NetworkDisconnection info)
	{
		DisconnectedFromServerEventHandler handler = OnDisconnected;
		if(handler != null)
		{
			handler(info);
		}
	}

	[SerializeField]
	private PlayerManager playerManager;
	[SerializeField]
	private GameManager gameManager;
	[SerializeField]
	private LANManager lanManager;
	[SerializeField]
	private InternetTapQueue internetTapQueue;


	public float refreshTimeOut = 5f; // Seconds until we stop "looking" for servers

	[SerializeField]
	private string uniqueGameName;

	[SerializeField]
	private int serverPort = 55789;

	// UDP Ports that are game could use: https://svn.nmap.org/nmap/nmap-services
	// Feel free to add more ports
	// This is basically the number of servers that can be run on one machine
	[SerializeField]
	private int[] serverPorts = new int[] { 
		55789, 
		55784, 
		55769, 
		55767, 
		55766, 
		55763, 
		55760, 
		55752, 
		55746, 
		55745 
	};

	private bool refreshing = false;
	private float timeSinceRefreshRequest = 0;

	private Dictionary<string, ServerData> serverList = new Dictionary<string, ServerData>();
	public ReadOnlyDictionary<string, ServerData> ServerList
	{
		get {
			return new ReadOnlyDictionary<string, ServerData>(this.serverList);
		}
	}


	public ReadOnlyDictionary<string, ServerData> CombinedServerList
	{
		get {
			if(this.lanManager)
				if(this.lanManager.ServerList != null && this.lanManager.ServerList.Count > 0)
					return new ReadOnlyDictionary<string, ServerData>(this.ServerList.Concat(this.lanManager.ServerList).GroupBy(d => d.Key)
						.ToDictionary (d => d.Key, d => d.First().Value));

			return new ReadOnlyDictionary<string, ServerData>(this.ServerList);
		}
	}

	public ServerData[] CombinedServerListArray
	{
		get {
			/* * /
			var combinedServerList = new List<ServerData>(this.serverDataList.Values);
			if(this.lanManager)
				if(this.lanManager.serverList != null)
					combinedServerList.AddRange(new List<ServerData>(this.lanManager.serverList.Values));
			return combinedServerList.ToArray();
			/* */
			return new List<ServerData>(this.CombinedServerList.Values).ToArray();
		}
	}

	// Since we can't trust Network.isServer and Network.isClient
	// We need to manage our own variable..
	private bool _isConnected = false;
	public bool IsConnected
	{
		get {
			return this._isConnected;
		}
		private set {
			this._isConnected = value;
		}
	}


	// See: http://answers.unity3d.com/questions/660868/random-masterservereventregistrationsucceeded-on-p.html
	private bool _serverRegistered;
	public bool ServerRegistered
	{
		get {
			return this._serverRegistered;
		}
		private set {
			this._serverRegistered = value;
		}
	}

	// Keep track of the details on the server we are hosting or the one we joined
	private ServerData _serverData = new ServerData();
	public ServerData ServerData
	{
		get {
			this._serverData.connectedPlayers = 1 + Network.connections.Length; // Add 1 for ourselves
			return this._serverData;
		}
		private set {
			this._serverData = value;
		}
	}








	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		networkView.group = 1;

		this.lanManager.OnServerListUpdated += (serverList) => {
			// Fire the overrall network server list updated
			this.ThisServerListUpdated(this.CombinedServerListArray);
		};
	}
	
	// Update is called once per frame
	void Update () {
		this.HandleRefreshing();
	}

	void HandleRefreshing()
	{
		// If we are refreshing
		if(this.refreshing)
		{
			// Keep adding the time
			this.timeSinceRefreshRequest += Time.deltaTime;
			// If we go over a timeout
			if(this.timeSinceRefreshRequest > this.refreshTimeOut)
			{
				Debug.Log("Refreshing timed out");
				
				// No longer refresh, we tried
				this.refreshing = false;
				// Reset the timeSince just because
				this.timeSinceRefreshRequest = 0;

				// Stop listening for LAN servers
				this.lanManager.StopDiscovery();
			}
		}
	}

	public void StartServer(string serverName, bool isLan, string serverPassword, int serverMaxPlayers, string serverDescription, string serverMap, string serverGameType)
	{
		// Do some clean up in case there was a server going before
		Network.Disconnect();
		this.ServerRegistered = false;

		if(!isLan)
		{
			this.internetTapQueue.QueueIfOnline(() => {
				// (true) Internet access
				this.StartServer(true, serverName, isLan, serverPassword, serverMaxPlayers, serverDescription, serverMap, serverGameType);
			}, () => {
				// TODO: Tell our host config dialog that you can't do that
				Debug.Log("Can't Register the server online: No access to the internet");
				this.ThisRegisterServerAttempt(ServerCreationEventCode.MSRegistrationFailedNoInternet);
			});
		
		}
		else
		{
			this.StartServer(false, serverName, isLan, serverPassword, serverMaxPlayers, serverDescription, serverMap, serverGameType);
		}
	}

	void StartServer(bool isInternetAvailable, string serverName, bool isLan, string serverPassword, int serverMaxPlayers, string serverDescription, string serverMap, string serverGameType)
	{
		// `isInternetAvailable`: 
		//		Only useful if you are trying to make an online game
		//		Whether we can use nat punchthrough 
		//		Whether we can register on the master server


		serverMaxPlayers = Mathf.Clamp(serverMaxPlayers, 1, 32);
		
		// We will use this later on when the game starts
		this.gameManager.SelectedLevel = serverMap;
		
		Network.InitializeSecurity();
		Network.incomingPassword = serverPassword;
		Network.maxConnections = serverMaxPlayers;
		bool useNat = !isLan && !Network.HavePublicAddress() && isInternetAvailable;
		
		NetworkConnectionError? error = null;
		int portIndex = 0;
		int port = this.serverPorts[portIndex];
		while((error == null || error != NetworkConnectionError.NoError) && portIndex < this.serverPorts.Length)
		{
			port = this.serverPorts[portIndex];
			
			error = Network.InitializeServer(serverMaxPlayers-1, port, useNat);
			
			this.serverPort = port;
			
			
			portIndex++;
		}
		
		
		
		// Only register the server if we had no problem setting it up
		if(error == NetworkConnectionError.NoError)
		{
			Debug.Log("Running Server on Port: " + this.serverPort);
			
			ServerData sData = new ServerData();
			sData.ip = Network.player.ipAddress;
			sData.port = this.serverPort;
			sData.useNat = useNat;
			sData.guid = Network.player.guid;
			
			sData.serverName = serverName;
			sData.description = serverDescription;
			
			sData.status = ServerData.ServerStatusCode.Open;
			sData.isLan = isLan;
			
			sData.pwProtected = serverPassword != "";
			sData.connectedPlayers = 1 + Network.connections.Length; // Add 1 for ourselves
			sData.playerLimit = serverMaxPlayers;
			
			sData.map = serverMap;
			sData.gameType = serverGameType;
			
			this.ServerData = sData;
			
			
			if(!isLan)
			{
				if(isInternetAvailable)
					MasterServer.RegisterHost(this.uniqueGameName, WWW.EscapeURL(serverName), this.ServerData.ToSepString());
				else
				{
					// You probably won't get here because you check for internet higher in the stack
					// TODO: Tell our host config dialog that you can't do that
					Debug.Log("Can't Register the server online: No access to the internet");
					this.ThisRegisterServerAttempt(ServerCreationEventCode.MSRegistrationFailedNoInternet);
				}
			}
			else
			{
				// We need to inform the others on LAN about our game
				this.lanManager.StartListening();
				
				// We have essentially registered the server locally
				this.ThisRegisterServerAttempt(ServerCreationEventCode.RegistrationSucceeded); // Fire Event
			}
		}
	}


	public void RefreshHostList()
	{
		/* */
		this.internetTapQueue.QueueIfOnline(() => {
			// Get servers from the masterserver (online)
			Debug.Log("MasterServer.ClearHostList()");
			MasterServer.ClearHostList();
			Debug.Log("MasterServer.RequestHostList");
			MasterServer.RequestHostList(this.uniqueGameName);
		});
		/* */

		this.serverList.Clear();
		this.lanManager.ClearServerList();
		// We cleared the lists so fire the event
		this.ThisServerListUpdated(this.CombinedServerListArray); // Fire Event

		// Start finding servers that are LAN
		this.lanManager.StartDiscovery();


		// Start the update loop
		this.refreshing = true;
		this.timeSinceRefreshRequest = 0;
	}



	public NetworkConnectionErrorResponse Connect(string guid, string pw)
	{
		Debug.Log("Trying to Connect");
		// Not all errors are immediate as Network.Connect is async
		// Also check for errors in OnFailedToConnect()
		NetworkConnectionError connectError;

		this.ServerData = this.CombinedServerList[guid];

		// Use nat punchthrough
		if(this.ServerData.useNat)
			connectError = Network.Connect(guid, pw);
		// Or just use IP, port
		else
			connectError = Network.Connect(this.ServerData.ip, this.ServerData.port, pw);


		// TODO: stuff with the message
		return this.GetResponseFromNetworkConnectionError(connectError);

	}

	public void DisconnectCleanUp(bool internetAccess = false)
	{	
		// Hide the server (change the fields things like "closed"
		this.HideServerFromMasterServer(internetAccess);
		
		// Clear the ServerData
		this.ServerData = new ServerData();

		// Then disconnect because we need to have the server running to `MasterServer.RegisterHost` when we hide
		Network.Disconnect();

		// No longer registered
		this.ServerRegistered = false;

	}

	public void HideServerFromMasterServer(bool internetAccess = false)
	{
		// This basically does UnregisterHost() since `MasterServer.UnregisterHost();` doesn't work
		if(Network.isServer && !this.ServerData.isLan)
		{
			if(internetAccess)
			{
				ServerData closedServerData = new ServerData();
				closedServerData.status = ServerData.ServerStatusCode.Closed;
				closedServerData.serverName = "nojoin";
				closedServerData.description = "closed";
				closedServerData.playerLimit = -1;
				closedServerData.map = "nomap";
				closedServerData.gameType = "nogametype";
				MasterServer.RegisterHost(this.uniqueGameName, closedServerData.serverName, closedServerData.ToSepString());
				
				// Then unregister the host, but this doesn't work...
				MasterServer.UnregisterHost();
			}
		}

	}

	public void Disconnect()
	{
		// Manual function to disconnect

		// Stop telling everyone on LAN about the server
		this.lanManager.StopListening();

		this.internetTapQueue.QueueIfOnline(() => {
			// Hide the server (change the fields things like "closed"
			this.DisconnectCleanUp(true);
			Debug.Log("Disconnected Server: " + Network.isServer);
		}, () => {
			// Even if we don't have internet we still need to disconnect
			this.DisconnectCleanUp(false);
			Debug.Log("Disconnected Server: " + Network.isServer);
		});

	}


	
	
	void OnServerInitialized()
	{
		// Called on the server whenever a Network.InitializeServer was invoked and has completed.
		Debug.Log("Server Initialized");

		
		// We are now a host
		this.IsConnected = true;

		// Add yourself to the player list
		this.playerManager.GenerateSelf();
	}

	void OnConnectedToServer()
	{
		// Called on the client when you have successfully connected to a server.
		Debug.Log("You have Connected");

		// We are connected
		this.IsConnected = true;

		// Add yourself to the player list
		this.playerManager.GenerateSelf();

		// handle errors
		NetworkConnectionErrorResponse response = new NetworkConnectionErrorResponse();
		response.noErrorsYet = true;
		response.ableToConnectYet = true;
		this.ThisAsyncConnectInfo(response);

		// Put the player in the game if the game is already going
		if(this.gameManager.GameStatus == GameManager.GameState.started)
			this.playerManager.SpawnPlayer("self");
	}

	void OnPlayerConnected(NetworkPlayer player) 
	{
		// Called on the SERVER whenever a new player has successfully connected.
		// We add players in OnConnectedToServer()
	}

	void OnPlayerDisconnected(NetworkPlayer netPlayer)
	{
		// Called on the SERVER whenever a player is disconnected from the server.
		Debug.Log("Clean up after player " + netPlayer);

		// Delete the player when they leave
		// and clean up
		if(Network.isServer)
			networkView.RPC("RPC_Player_RemovePlayer", RPCMode.AllBuffered, netPlayer.guid);
		
		Network.RemoveRPCs(netPlayer);
		Network.DestroyPlayerObjects(netPlayer);
	}

	[RPC]
	public void RPC_Player_RemovePlayer(string guid)
	{
		this.playerManager.RemovePlayer(guid);
	}


	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		// Called on client during disconnection from server, 
		// but also on the server when the connection has disconnected.

		// We are not connected anymore
		this.IsConnected = false;

		this.PlayerDisconnectCleanUp();

		// Call the event
		// This will probably trigger a dialog in the UI
		this.ThisDisconnected(info);
	}

	public void PlayerDisconnectCleanUp()
	{
		// Change the gamestatus to reflect
		this.gameManager.GameStatus = GameManager.GameState.notStarted;
		
		// Remove the players
		this.playerManager.RemoveAllPlayers();
		// Switch to the empty scene
		this.gameManager.ChangeLevel("empty_scene");
	}

	void OnFailedToConnect(NetworkConnectionError connectError)
	{
		// We are not connected anymore
		this.IsConnected = false;

		this.ThisAsyncConnectInfo(this.GetResponseFromNetworkConnectionError(connectError));
	}

	void OnMasterServerEvent(MasterServerEvent mse)
	{
		// Only send the event if it pertains to registering a server
		if(mse != MasterServerEvent.HostListReceived)
		{
			Debug.Log("Registered Server: " + mse);

			// See; http://answers.unity3d.com/questions/660868/random-masterservereventregistrationsucceeded-on-p.html
			// Only fire the event once
			if(!this.ServerRegistered)
				this.ThisRegisterServerAttempt(mse); // Fire Event

			this.ServerRegistered = true;
		}

		if(mse == MasterServerEvent.HostListReceived)
		{
			Debug.Log("Refresh HostList received");

			// No longer refresh
			this.refreshing = false;
			// Reset the timeSince just because we're nice
			this.timeSinceRefreshRequest = 0;


			// Add the servers received from the master server to the list
			foreach(HostData hostData in MasterServer.PollHostList())
			{
				this.serverList[hostData.guid] = new ServerData(hostData);
			}
			//this.serverDataList.Values.ToArray(); // uses linq
			//var asdf = new List<ServerData>(this.serverDataList.Values).ToArray();

			// Fire Event
			this.ThisServerListUpdated(this.CombinedServerListArray);
		}
	}

	[ContextMenu ("Analyze networkView ID")]
	void AnalyzeNetworkViews() {
		NetworkView[] netViews = FindObjectsOfType(typeof(NetworkView)) as NetworkView[];

		Debug.Log("-------------------------");
		foreach(NetworkView netView in netViews)
		{
			Debug.Log(netView + " - id: " + netView.viewID + " - guid: " + netView.owner.guid);
		}
		Debug.Log("-------------------------");
	}


	public NetworkConnectionErrorResponse GetResponseFromNetworkConnectionError(NetworkConnectionError connectError)
	{
		NetworkConnectionErrorResponse response = new NetworkConnectionErrorResponse();
		string message = "";

		if(connectError == NetworkConnectionError.NoError) {
			message = "Connected to Server";
			response.noErrorsYet = true;
		}
		else if(connectError == NetworkConnectionError.RSAPublicKeyMismatch) {
			message = "RSA public key did not match what the system we connected to is using.";
		}
		else if(connectError == NetworkConnectionError.InvalidPassword) {
			message = "Invalid Password. Could not connect to server";
		}
		else if(connectError == NetworkConnectionError.ConnectionFailed) {
			message = "Could not connect to server";
		}
		else if(connectError == NetworkConnectionError.TooManyConnectedPlayers) {
			message = "Server Full. Could not connect to server";
		}
		else if(connectError == NetworkConnectionError.ConnectionBanned) {
			message = "You are banned from the system we attempted to connect to (likely temporarily).";
		}
		else if(connectError == NetworkConnectionError.AlreadyConnectedToServer) {
			message = "You are already connected to this particular server (can happen after fast disconnect/reconnect).";
		}
		else if(connectError == NetworkConnectionError.AlreadyConnectedToAnotherServer) {
			message = "Already Connected to a Server";
		}
		else if(connectError == NetworkConnectionError.CreateSocketOrThreadFailure) {
			message = "Internal error while attempting to initialize network interface. Socket possibly already in use.";
		}
		else if(connectError == NetworkConnectionError.IncorrectParameters) {
			message = "Incorrect parameters given to Connect function";
		}
		else if(connectError == NetworkConnectionError.EmptyConnectTarget) {
			message = "No host target given in Connect";
		}
		else if(connectError == NetworkConnectionError.InternalDirectConnectFailed) {
			message = "You could not connect internally to same network NAT enabled server.";
		}
		else if(connectError == NetworkConnectionError.NATTargetNotConnected) {
			message = "The NAT target you are trying to connect to is not connected to the facilitator server.";
		}
		else if(connectError == NetworkConnectionError.NATTargetConnectionLost) {
			message = "Connection lost while attempting to connect to NAT target.";
		}
		else if(connectError == NetworkConnectionError.NATPunchthroughFailed) {
			message = "NAT punchthrough attempt has failed. Could not connect to server";
		}

		response.message = message;

		return response;
	}



	[ContextMenu("Print Server List")]
	void DebugPrintServerList()
	{
		Debug.Log(new List<ServerData>(this.ServerList.Values).ToDebugString());
	}

	[ContextMenu("Print Combined Server List")]
	void DebugPrintCombinedServerList()
	{
		Debug.Log(new List<ServerData>(this.CombinedServerList.Values).ToDebugString());
	}
}
