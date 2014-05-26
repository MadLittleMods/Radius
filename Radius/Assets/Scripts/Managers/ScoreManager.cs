/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ScoreManager.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScoreManager : MonoBehaviour {

	public enum ScoreSetType {
		delta, absolute
	}

	public struct ScoreData
	{
		public string guid;
		public string scoreType;
		public string playerGuid;
		public float value;
	}

	public class ScoreActivityEventArgs : EventArgs
	{
		public ScoreData ScoreData;
		
		public ScoreActivityEventArgs(ScoreData sData)
		{
			this.ScoreData = sData;
		}
	}

	public delegate void ScoreUpdatedEventHandler(MonoBehaviour sender, ScoreActivityEventArgs e);
	public event ScoreUpdatedEventHandler OnScoreUpdated;

	// Use this to trigger the event
	protected virtual void ThisScoreUpdated(MonoBehaviour sender, ScoreActivityEventArgs e)
	{
		ScoreUpdatedEventHandler handler = OnScoreUpdated;
		if(handler != null)
		{
			handler(sender, e);
		}
	}

	[SerializeField]
	private PlayerManager playerManager;
	[SerializeField]
	private GameManager gameManager;

	// <scoreType, <guid, value>>
	private Dictionary<string, Dictionary<string, float>> scoreList = new Dictionary<string, Dictionary<string, float>>();

	// Use this for initialization
	void Start () {
		// Reset the score when the game starts
		this.gameManager.OnGameStarted += (sender) => {
			this.ResetScore();
		};
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ResetScore() {
		// Reset the dictionary
		this.scoreList = new Dictionary<string, Dictionary<string, float>>();
		/*
		foreach(KeyValuePair<string, Player> entry in this.playerManager.PlayerList)
		{
			// Reset scores
			this.UpdateScore("score", entry.Value.guid, 0, ScoreSetType.absolute);
			//this.scoreList["score"][player.guid] = 0;
			// Reset capture count
			this.UpdateScore("captures", entry.Value.guid, 0, ScoreSetType.absolute);
			//this.scoreList["captures"][player.guid] = 0;
		}
		*/
	}

	public float UpdateScore(string scoreType, string playerGuid, float value, ScoreSetType setType = ScoreSetType.delta) {
		// Returns the new assigned value

		// Create the dictionary key if it doesn't exist
		if(!this.scoreList.ContainsKey(scoreType))
			this.scoreList[scoreType] = new Dictionary<string, float>();

		float newValue = -1;
		if(setType == ScoreSetType.delta)
		{
			float beforeValue = 0;
			this.scoreList[scoreType].TryGetValue(playerGuid, out beforeValue);

			newValue = beforeValue + value;
		}
		else//(setType == ScoreSetType.absolute)
		{
			newValue = value;
		}

		this.scoreList[scoreType][playerGuid] = newValue;

		// Fire the event
		//Debug.Log("Firing Score Updated Event");
		ScoreData scoreData = new ScoreData();
		scoreData.scoreType = scoreType;
		scoreData.guid = playerGuid;
		scoreData.value = newValue;
		this.ThisScoreUpdated(this, new ScoreActivityEventArgs(scoreData));

		// Tell the others the score
		if(Network.isServer)
			networkView.RPC("RPCUpdateScore", RPCMode.OthersBuffered, scoreType, playerGuid, newValue);

		return newValue;
	}

	[RPC]
	public void RPCUpdateScore(string scoreType, string playerGuid, float value)
	{
		this.UpdateScore(scoreType, playerGuid, value, ScoreSetType.absolute);
	}
}
