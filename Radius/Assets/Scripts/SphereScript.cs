/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: SphereScript.cs, May 2014
 */

using UnityEngine;
using System.Collections;

public class SphereScript : MonoBehaviour {

	// A basic networking testing script
	// When the object is entered, it will turn red
	// Just apply to a mesh

	Renderer myRenderer;
	NetworkView myNetworkView;

	// Use this for initialization
	void Start () {
		this.myRenderer = GetComponent<Renderer>();
		this.myNetworkView = GetComponent<NetworkView>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void OnTriggerEnter()
	{
		//Debug.Log("In sphere");
		this.myNetworkView.RPC("SetColor", RPCMode.AllBuffered, new Vector3(1, 0, 0));
	}

	[RPC]
	void SetColor(Vector3 color)
	{
		if(this.myRenderer)
			this.myRenderer.material.color = new Color(color.x, color.y, color.z, 1);
	}

}
