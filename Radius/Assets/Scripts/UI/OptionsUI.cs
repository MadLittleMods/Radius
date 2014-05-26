/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: OptionsUI.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using Coherent.UI;
using Coherent.UI.Binding;

public class OptionsUI : MonoBehaviour {

	
	private CoherentUIView m_View;
	private bool viewReady = false;

	[SerializeField]
	private AudioManager audioManager;

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
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	[Coherent.UI.CoherentMethod("GUISetMasterVolume")]
	public void GUISetMasterVolume(string audioTypeString, float volume)
	{
		//Debug.Log("Master Volume set from gui");

		if(audioTypeString.ToLower() == "music")
			this.audioManager.SetMasterVolume(AudioBase.AudioType.Music, volume);
		else if(audioTypeString.ToLower() == "soundeffect")
			this.audioManager.SetMasterVolume(AudioBase.AudioType.SoundEffect, volume);
	}

	[Coherent.UI.CoherentMethod("GetMasterVolume")]
	public float GetMasterVolume(string audioTypeString)
	{
		// Return the master volume for that type
		return this.audioManager.GetMasterVolume(UtilityMethods.ParseEnum<AudioBase.AudioType>(audioTypeString));
	}
}
