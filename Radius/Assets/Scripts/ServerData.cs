/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ServerData.cs, May 2014
 */

using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class ServerData
{
	public enum ServerStatusCode
	{
		Closed,
		Open
	}

	public class ServerStatus
	{
		// This is a wrapper class to make it easier using the enum


		public ServerStatusCode statusCode = ServerStatusCode.Closed;

		// Constructor:
		// Creates a ServerStatus given a ServerStatusEnum
		// ex. 
		//		ServerStatus myStatus = ServerStatusEnum.Open;
		public static implicit operator ServerStatus(ServerStatusCode group)
		{ 
			return new ServerStatus(group);
		}

		// Constructor:
		// Creates a ServerStatus given a string
		// ex. 
		//		ServerStatus myStatus = "Open";
		public static implicit operator ServerStatus(string str)
		{ 
			return new ServerStatus(str);
		}



		// Creates a ServerStatusEnum given a ServerStatus
		// ex.
		//		ServerStatusEnum myEnumStatus = new ServerStatus
		public static implicit operator ServerStatusCode(ServerStatus s)
		{
			return s.statusCode;
		}

		// Creates a String given a ServerStatus
		// ex.
		//		string asdf = new ServerStatus
		public static implicit operator string(ServerStatus s)
		{
			return s.statusCode.ToString();
		}



		public ServerStatus()
		{
		}

		public ServerStatus(ServerStatusCode group)
		{
			this.statusCode = group;
		}

		public ServerStatus(string str)
		{
			try {
				this.statusCode = (ServerStatusCode)Enum.Parse(typeof(ServerStatusCode), str, true);
			} catch(ArgumentException e) {
				Debug.LogWarning(e);
			}
		}

		public string AsString
		{
			get {
				return this.statusCode.ToString();
			}
		}

		public override string ToString()
		{
			return this.statusCode.ToString();
		}
	}

	public string ip;
	public int port;
	// We use IP and Port if False
	// We use guid NAT punchthrough if true
	public bool useNat = false;
	public string guid = "";
	public string serverName = "";
	public string description = "";
	
	public ServerStatus status = ServerStatusCode.Closed;

	public bool isLan = false;
	public bool pwProtected = false;

	public int connectedPlayers = 0;
	public int playerLimit = 1;
	
	public string map = "";
	public string gameType = "";


	public ServerData()
	{

	}

	public ServerData(HostData hostData)
	{
		this.ParseServerDataSeparatedString(hostData.comment);

		/* * /
		// We could also overwrite it with this, by why...
		this.guid = hostData.guid;
		this.serverName = WWW.UnEscapeURL(hostData.gameName);
		
		this.pwProtected = hostData.passwordProtected;

		this.connectedPlayers = hostData.connectedPlayers;
		this.playerLimit = hostData.playerLimit;
		/* */
		
	}

	public ServerData(string separatedString)
	{
		this.ParseServerDataSeparatedString(separatedString);
	}

	public ServerData(string guid, string serverName, string description)
	{
		this.guid = guid;
		this.serverName = serverName;
		this.description = description;
	}

	public HostData GetHostData()
	{
		HostData hostData = new HostData();
		hostData.comment = this.ToSepString();

		hostData.ip = new string[1] { this.ip };
		hostData.port = this.port;
		hostData.useNat = this.useNat;
		hostData.guid = this.guid;
		hostData.gameName = this.serverName;
		hostData.passwordProtected = this.pwProtected;
		hostData.connectedPlayers = this.connectedPlayers;
		hostData.playerLimit = this.playerLimit;

		return hostData;
	}



	bool ParseServerDataSeparatedString(string separatedString)
	{
		bool corrupt = false;
		
		// 1:guid + 2:serverName + 3:serverDescription + 4:status + 5:isLan + 6:pwProtected + 7:connectedPlayers + 8:playerLimit +  + + 9:serverMap + 10:serverGameType 
		Dictionary<string, string> chunks = UtilityMethods.ParseSeparatedStrToDict(separatedString);

		this.ip = chunks.GetValueOrDefault("ip", "");
		this.port = 0;
		corrupt &= Int32.TryParse(chunks.GetValueOrDefault("port", ""), out this.port);
		this.useNat = chunks.GetValueOrDefault("useNat", "false").ToLower() == "true";
		this.guid = chunks.GetValueOrDefault("guid", "");

		this.serverName = chunks.GetValueOrDefault("serverName", "");
		this.description = chunks.GetValueOrDefault("description", "");

		this.status = chunks.GetValueOrDefault("status", "Closed");
		this.isLan = chunks.GetValueOrDefault("isLan", "false").ToLower() == "true";
		this.pwProtected = chunks.GetValueOrDefault("pwProtected", "false").ToLower() == "true";
		
		this.connectedPlayers = 0;
		corrupt &= Int32.TryParse(chunks.GetValueOrDefault("connectedPlayers", "0"), out this.connectedPlayers);
		
		this.playerLimit = 0;
		corrupt &= Int32.TryParse(chunks.GetValueOrDefault("playerLimit", "0"), out this.playerLimit);
		
		this.map = chunks.GetValueOrDefault("map", "");
		this.gameType = chunks.GetValueOrDefault("gameType", "");


		return corrupt;
	}


	public string ToSepString()
	{
		// 1:guid + 2:serverName + 3:serverDescription + 4:status + 5:isLan + 6:pwProtected + 7:connectedPlayers + 8:playerLimit +  + + 9:serverMap + 10:serverGameType 
		var lanValues = new Dictionary<string, string>() {
			{"ip", this.ip},
			{"port", this.port.ToString()},
			{"useNat", this.useNat.ToString()},
			{"guid", this.guid},

			{"serverName", this.serverName},
			{"description", this.description},

			{"status", this.status.ToString()},
			{"isLan", this.isLan.ToString()},
			{"pwProtected", this.pwProtected.ToString()},

			{"connectedPlayers", this.connectedPlayers.ToString()},
			{"playerLimit", this.playerLimit.ToString()},

			{"map", this.map},
			{"gameType", this.gameType}
		};

		return UtilityMethods.GenerateSeparatedString(lanValues);
	}

	public override bool Equals(object obj)
	{
		ServerData sData = obj as ServerData;
		return sData != null ? this.Equals(sData) : false;
	}
	public bool Equals(ServerData sData)
	{
		// Tried to put these in order of quickest to fail if differnt
		return sData != null
			&& this.status == sData.status 
			&& this.guid == sData.guid 
			&& this.ip == sData.ip
			&& this.port == sData.port
			&& this.connectedPlayers == sData.connectedPlayers
			&& this.useNat == sData.useNat
			&& this.pwProtected == sData.pwProtected
			&& this.serverName == sData.serverName
			&& this.description == sData.description
			&& this.playerLimit == sData.playerLimit
			&& this.map == sData.map
			&& this.gameType == sData.gameType
			&& this.isLan == sData.isLan;
	}

	public override int GetHashCode()
	{
		return Int32.Parse(this.guid);
	}

	// Nicely print the ServerData as a single string
	public string ToDebugString()
	{
		string debugString = "{";

		debugString += this.status.ToString() + ", ";
		debugString += (this.useNat ? this.guid : this.ip+":"+this.port) + ", ";
		debugString += "name="+this.serverName;

		debugString += "}";

		return debugString;
	}



}