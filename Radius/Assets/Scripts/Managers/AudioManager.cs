/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: AudioManager.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour {

	public class VolumeChangeEventArgs : EventArgs
	{
		public AudioBase.AudioType audioType;
		public float volume;
		
		public VolumeChangeEventArgs(AudioBase.AudioType audioType, float volume)
		{
			this.audioType = audioType;
			this.volume = volume;
		}
	}

	public delegate void VolumeChangedEventHandler(MonoBehaviour sender, VolumeChangeEventArgs e);
	public event VolumeChangedEventHandler OnVolumeChange;

	// Use this to trigger the event
	protected virtual void ThisVolumeChanged(MonoBehaviour sender, VolumeChangeEventArgs e)
	{
		VolumeChangedEventHandler handler = OnVolumeChange;
		if(handler != null)
		{
			handler(sender, e);
		}
	}

	// Store all of the different volumes
	private Dictionary<AudioBase.AudioType, float> masterVolume = new Dictionary<AudioBase.AudioType, float>();

	// Use this for initialization
	void Start () {
		foreach(AudioBase.AudioType type in (AudioBase.AudioType[]) Enum.GetValues(typeof(AudioBase.AudioType)))
		{
			this.SetMasterVolume(type, PlayerPrefs.GetFloat("MasterVolume_" + type, 1f));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public float GetMasterVolume(AudioBase.AudioType type) 
	{
		float volume = 1f;
		this.masterVolume.TryGetValue(type, out volume);

		return volume;
	}

	public void SetMasterVolume(AudioBase.AudioType type, float volume) 
	{
		this.masterVolume[type] = Mathf.Clamp(volume, 0, 1);
		Debug.Log("Changing Master Volume: " + type + " - " + this.masterVolume[type]);

		// Save it persistently
		PlayerPrefs.SetFloat("MasterVolume_" + type, this.masterVolume[type]);

		// Fire the event
		this.ThisVolumeChanged(this, new VolumeChangeEventArgs(type, volume));
	}


}
