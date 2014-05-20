/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: CharacterDriver.cs, May 2014
 */

using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class CharacterDriver : MonoBehaviour {

	// Optional
	public Animator animator;

	public float maxSurgeSpeed = 5; // Forward, backward
	public float maxSwaySpeed = 5; // Side to side
	public float jumpHeight = 4;

	// Optional
	public AudioBase jumpSoundEffect;

	public Vector3 movementVelocity {get; private set;} 


	public bool forceNetworkViewToBeMine = true;
	public bool forceGameManagerStatus = true;

	CharacterController characterController;
	NetworkView netView;

	Vector3 auxiliaryVelocity = new Vector3();


	GameManager gameManager;


	[HideInInspector]
	public float debugJumpYStart;
	[HideInInspector]
	public float debugJumpYMax;

	[HideInInspector]
	public Vector3 debugPrevPosition;
	[HideInInspector]
	public float debugGroundSpeed;


	// Use this for initialization
	void Start () {
		this.characterController = GetComponent<CharacterController>();
		this.netView = GetComponent<NetworkView>();

		// Grab the Player Manager
		GameObject[] managers = GameObject.FindGameObjectsWithTag("Manager");
		if(managers.Length > 0)
			this.gameManager = managers[0].GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {

		// Only control the player if the networkView belongs to you
		if(!this.forceNetworkViewToBeMine || (this.netView && this.netView.isMine))
		{
			// If the game has started
			if(!this.forceGameManagerStatus || (this.gameManager && this.gameManager.GameStatus == GameManager.GameState.started))
				this.MoveUpdate(Time.deltaTime);
		}
		else
		{
			enabled = false; // Disable this script if we can't control this player
		}
	}

	void MoveUpdate(float deltaTime)
	{
		// Adjust mecanim parameters
		if(animator)
		{
			animator.SetFloat("SurgeSpeed", Input.GetAxis("Vertical"));
			animator.SetFloat("SwaySpeed", Input.GetAxis("Horizontal"));
		}

		// We use this for a gizmo
		this.debugGroundSpeed = (Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - Vector3.Scale(this.debugPrevPosition, new Vector3(1, 0, 1))).magnitude / deltaTime;
		this.debugPrevPosition = transform.position;


		// Add the gravity
		if(characterController.isGrounded)
			this.auxiliaryVelocity = Physics.gravity * deltaTime;
		else
			this.auxiliaryVelocity += Physics.gravity * deltaTime;

		
		// Jumping
		if(characterController.isGrounded && Input.GetButtonDown("Jump")) 
		{
			// Play the jumping sound effect
			if(this.jumpSoundEffect)
				this.jumpSoundEffect.PlayOneShot();

			// Jump the player
			this.auxiliaryVelocity.y += this.CalculateJumpVerticalSpeed(this.jumpHeight);

			// Debugging jump height
			// Get the height the jump started at
			this.debugJumpYStart = transform.position.y;
			this.debugJumpYMax = transform.position.y; // Reset this
		}

		// Debugging jump height
		// GEt the new max so we can compare to where we started
		if(transform.position.y > this.debugJumpYMax)
			this.debugJumpYMax = transform.position.y;
		


		// Movement Surge and Sway
		// Elliptical player/character movement. See diagram for more info:
		// 		http://i.imgur.com/am2OYj1.png
		float angle =  Mathf.Atan2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
		float surgeSpeed = Mathf.Abs(Input.GetAxis("Vertical")) * this.maxSurgeSpeed * Mathf.Sin(angle); // Forward and Backward
		float swaySpeed = Mathf.Abs(Input.GetAxis("Horizontal")) * this.maxSwaySpeed * Mathf.Cos(angle); // Left and Right



		// Get the camera directions without the Y component
		Vector3 cameraForwardNoY = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 cameraRightNoY = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

		this.movementVelocity = (cameraForwardNoY*surgeSpeed + cameraRightNoY*swaySpeed) * deltaTime ;
		this.movementVelocity += this.auxiliaryVelocity * deltaTime;

		this.characterController.Move(this.movementVelocity);
	}


	float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		// See for formula: http://math.stackexchange.com/a/222585/60008
		return Mathf.Sqrt(2f * targetJumpHeight * Mathf.Abs(Physics.gravity.y));
	}
}
