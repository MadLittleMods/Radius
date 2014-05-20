/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: SurgeMove.cs, May 2014
 */

using UnityEngine;
using System.Collections;

public class SurgeMove : MonoBehaviour {

	public float speed = 2f;
	public float length = 1f;

	Vector3 initPosition;

	// Use this for initialization
	void Start () {
		this.initPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = this.initPosition + new Vector3(Mathf.PingPong(Time.time * speed, length), 0, 0);
	}
}
