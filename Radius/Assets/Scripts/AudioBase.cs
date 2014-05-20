/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: AudioBase.cs, May 2014
 */

using UnityEngine;
using System.Collections;

public class AudioBase : MonoBehaviour {

	public enum AudioType {
		SoundEffect,
		Music
	}

	public AudioType audioType;
	public AudioClip soundEffect;
	
	public bool playOnAwake = false;
	public bool loop = false;

	[Range(0, 1)]
	public float volume = 1f;
	public float pitch = 1f;


	private AudioManager audioManager;

	private AudioSource audioSource;

	// Use this for initialization
	void Start () {
		// Grab the Player Manager
		this.audioManager = GameObject.FindGameObjectsWithTag("Manager")[0].GetComponent<AudioManager>();

		// Listen for a master volume change and adjust it
		this.audioManager.OnVolumeChange += (sender, e) => {
			if(e.audioType == this.audioType)
				this.audioSource.volume = e.volume * this.volume;
		};

		GameObject go = new GameObject ("Audio: " +  this.soundEffect.name);
		go.transform.position = gameObject.transform.position;
		go.transform.parent = gameObject.transform;

		this.audioSource = go.AddComponent<AudioSource>();
		this.audioSource.clip = this.soundEffect;
		this.audioSource.volume = this.audioManager.GetMasterVolume(this.audioType) * this.volume;
		this.audioSource.pitch = this.pitch;
		this.audioSource.loop = this.loop;
		if(this.playOnAwake)
			this.Play();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Play() {
		this.audioSource.Play();
	}
	public void Stop() {
		this.audioSource.Stop();
	}
	public void Pause() {
		this.audioSource.Pause();
	}

	public void PlayOneShot() {
		// Play the sound effect
		this.audioSource.PlayOneShot(soundEffect, this.audioManager.GetMasterVolume(this.audioType) * volume);
	}
}
