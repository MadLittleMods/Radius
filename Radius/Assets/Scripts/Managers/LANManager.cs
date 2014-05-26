/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: LANManager.cs, May 2014
 */

using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public class LANManager : MonoBehaviour {


	public delegate void ServerListUpdatedEventHandler(ServerData[] serverList);
	public event ServerListUpdatedEventHandler OnServerListUpdated;

	delegate void RequestReceivedEventHandler(string message);
	event RequestReceivedEventHandler OnRequestReceived;

	delegate void ServerDataReceivedEventHandler(string message);
	event ServerDataReceivedEventHandler OnServerDataReceived;


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
	protected virtual void ThisRequestReceived(string message)
	{
		RequestReceivedEventHandler handler = OnRequestReceived;
		if(handler != null)
		{
			handler(message);
		}
	}


	
	// Use this to trigger the event
	protected virtual void ThisServerDataReceived(string message)
	{
		ServerDataReceivedEventHandler handler = OnServerDataReceived;
		if(handler != null)
		{
			handler(message);
		}
	}


	public int requestPort = 55795;
	public int dataPort = 55797;


	public int numberRequestPackets = 2;
	public float timeForAllRequestPackets = .8f;

	
	[SerializeField]
	private NetworkManager networkManager;
	[SerializeField]
	private MainThreadTap mainThreadTap;


	UdpClient udpRequestSender;
	UdpClient udpRequestReceiver;

	UdpClient udpDataSender;
	UdpClient udpDataReceiver;


	bool requestingServers = false;


	private Dictionary<string, ServerData> serverList = new Dictionary<string, ServerData>();
	public ReadOnlyDictionary<string, ServerData> ServerList
	{
		get {
			return new ReadOnlyDictionary<string, ServerData>(this.serverList);
		}
	}


	// Use this for initialization
	void Start () {

		this.OnRequestReceived += (message) => {
			Debug.Log("Someone is looking for LAN servers: " + message);
		};

		this.OnServerDataReceived += (message) => {
			Debug.Log("Server Data Received: " + message);
		};
	}
	
	int currentNumberSentRequestPackets = 0;
	float timeSinceLastRequest = 0f;
	// Update is called once per frame
	void Update () {

		this.HandleRequestingServers();

	}

	void OnApplicationQuit() {
		this.StopDiscovery();
		this.StopListening();
	}

	public void ClearServerList()
	{
		this.serverList.Clear();
	}


	void HandleRequestingServers()
	{
		if(this.requestingServers)
		{
			if(this.currentNumberSentRequestPackets < this.numberRequestPackets)
			{
				if(this.timeSinceLastRequest >= this.timeForAllRequestPackets/this.numberRequestPackets)
				{
					this.SendRequest();
					
					this.currentNumberSentRequestPackets++;
					this.timeSinceLastRequest = 0f;
				}
				else
					this.timeSinceLastRequest += Time.deltaTime;
			}
			else
			{
				// If we sent all of the packets, we are no longer requesting
				this.requestingServers = false;
				this.currentNumberSentRequestPackets = 0;
			}
		}
	}


	public void StartListening()
	{
		// Setup the listener of requests
		// Setup the sender of server data to send back to the requests

		// Set up the receiver for the requests
		// Listen for anyone looking for our server
		if(this.udpRequestReceiver == null)
		{
			this.udpRequestReceiver = new UdpClient();
			this.udpRequestReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			IPEndPoint requestGroupEP = new IPEndPoint(IPAddress.Any, this.requestPort);
			this.udpRequestReceiver.Client.Bind(requestGroupEP);
			this.udpRequestReceiver.BeginReceive(new AsyncCallback(AsyncRequestReceiveData), null);
		}

		// Set up the sender for server data
		// And then send them the details we hear a request
		if(this.udpDataSender == null)
		{
			this.udpDataSender = new UdpClient();
			IPEndPoint dataGroupEP = new IPEndPoint(IPAddress.Broadcast, this.dataPort);
			this.udpDataSender.Connect(dataGroupEP);
		}
	}
	public void StopListening()
	{
		if(this.udpRequestReceiver != null)
			this.udpRequestReceiver.Close();

		this.udpRequestReceiver = null;

		if(this.udpDataSender != null)
			this.udpDataSender.Close();

		this.udpDataSender = null;
	}

	public void StartDiscovery()
	{
		// Setup the sender of requests
		// Setup the listener of server data

		// Set up the sender for the requests
		// We will send out requests looking for servers on the network
		if(this.udpRequestSender == null)
		{
			this.udpRequestSender = new UdpClient();
			IPEndPoint requestGroupEP = new IPEndPoint(IPAddress.Broadcast, this.requestPort);
			this.udpRequestSender.Connect(requestGroupEP);
		}
		
		// Set up the receiver for server data
		// Then we will receive any responses and parse them
		if(this.udpDataReceiver == null)
		{
			this.udpDataReceiver = new UdpClient();
			this.udpDataReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			IPEndPoint dataGroupEP = new IPEndPoint(IPAddress.Any, this.dataPort);
			this.udpDataReceiver.Client.Bind(dataGroupEP);
			this.udpDataReceiver.BeginReceive(new AsyncCallback(AsyncServerDataReceiveData), null);
		}

		// Start the update loop
		this.requestingServers = true;
	}
	public void StopDiscovery()
	{
		if(this.udpRequestSender != null)
			this.udpRequestSender.Close();

		this.udpRequestSender = null;

		if(this.udpDataReceiver != null)
			this.udpDataReceiver.Close();

		this.udpDataReceiver = null;
	}

	void SendRequest()
	{
		try
		{
			string message = "requestingServers";

			if (message != "") 
			{
				if(this.udpRequestSender != null)
					this.udpRequestSender.Send(System.Text.Encoding.ASCII.GetBytes(message), message.Length);
				else
					Debug.LogWarning("UDP Server Data sender is null");
			}
		}
		catch (ObjectDisposedException e)
		{
			Debug.LogWarning("Trying to send data on already disposed UdpClient: " + e);
			return;
		}
	}

	
	void AsyncRequestReceiveData(IAsyncResult result)
	{
		IPEndPoint receiveIPGroup = new IPEndPoint(IPAddress.Any, this.requestPort);
		byte[] received;
		if (this.udpRequestReceiver != null) {
			received = this.udpRequestReceiver.EndReceive(result, ref receiveIPGroup);
		} else {
			return;
		}
		this.udpRequestReceiver.BeginReceive (new AsyncCallback(AsyncRequestReceiveData), null);
		string receivedString = System.Text.Encoding.ASCII.GetString(received);

		if(receivedString == "requestingServers")
		{
			this.mainThreadTap.QueueOnMainThread(() => {
				// Fire the event
				this.ThisRequestReceived(receivedString);

				this.SendServerData();
			});
		}
	}


	void SendServerData()
	{
		try
		{
			if(this.networkManager)
			{
				// guid + maxPlayers + serverName + comment(serverMap, serverGameType, serverDescription)
				// WWW.EscapeURL(Network.player.ipAddress)
				//string message = WWW.EscapeURL(Network.player.guid) + ":" + WWW.EscapeURL(Network.maxConnections.ToString()) + ":" + WWW.EscapeURL(this.networkManager.ServerName) + ":" + WWW.EscapeURL(this.networkManager.ServerComment);
				//string message = "adsf";
				string message = this.networkManager.ServerData.ToSepString();//this.networkManager.serverData;

				Debug.Log("Sending Server Data: " + message);

				if (message != "") 
				{
					if(this.udpDataSender != null)
						this.udpDataSender.Send(System.Text.Encoding.ASCII.GetBytes(message), message.Length);
					else
						Debug.LogWarning("UDP Server Data sender is null");
				}
			}
			else
				Debug.LogWarning("No NetworkManager hooked up to the LANManager");
		}
		catch (ObjectDisposedException e)
		{
			Debug.LogWarning("Trying to send data on already disposed UdpClient: " + e);
			return;
		}

	}

	void AsyncServerDataReceiveData(IAsyncResult result)
	{
		IPEndPoint receiveIPGroup = new IPEndPoint(IPAddress.Any, this.dataPort);
		byte[] received;
		if (this.udpDataReceiver != null) {
			received = this.udpDataReceiver.EndReceive(result, ref receiveIPGroup);
		} else {
			return;
		}
		this.udpDataReceiver.BeginReceive (new AsyncCallback(AsyncServerDataReceiveData), null);
		string receivedString = System.Text.Encoding.ASCII.GetString(received);

		this.mainThreadTap.QueueOnMainThread(() => {
			// Fire the event
			this.ThisServerDataReceived(receivedString);

			this.ParseServerData(receivedString);


		});
	}

	void ParseServerData(string receivedString)
	{
		ServerData receivedServerData = new ServerData(receivedString);

		bool entryUpdated = false;
		// Check if list is going to add it for the first time
		// This is just a quick check first before looking at equality
		entryUpdated = !this.serverList.ContainsKey(receivedServerData.guid);
		// If we haven't determined that the entry has been updated, we need to check for equality
		if(!entryUpdated)
		{
			// We know that the guid key exists, because we tested above "ContainsKey"
			entryUpdated = !this.serverList[receivedServerData.guid].Equals(receivedServerData);
		}

		// Put the received server data in the server list
		this.serverList[receivedServerData.guid] = receivedServerData;

		// If the entry was updated, fire the event
		if(entryUpdated)
			this.ThisServerListUpdated(new List<ServerData>(this.ServerList.Values).ToArray());
	}



	[ContextMenu("Print Server List")]
	void DebugPrintServerList()
	{
		Debug.Log(new List<ServerData>(this.ServerList.Values).ToDebugString());
	}


}
