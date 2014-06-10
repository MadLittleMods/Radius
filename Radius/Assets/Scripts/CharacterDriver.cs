/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: CharacterDriver.cs, June 2014
 */

using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class CharacterDriver : MonoBehaviour {

	public class CharacterState
	{
		public Vector3 position = new Vector3();
		
		public Vector3 velocity = new Vector3();
		
		public CharacterState()
		{
			
		}
		
		public CharacterState(CharacterState s)
		{
			this.position = s.position;
			this.velocity = s.velocity;
		}
		
		public CharacterState(Vector3 position,Vector3 velocity)
		{
			this.position = position;
			
			this.velocity = velocity;
		}
		
		public static CharacterState Lerp(CharacterState from, CharacterState to, float t)
		{
			return new CharacterState(Vector3.Lerp(from.position, to.position, t), Vector3.Lerp(from.velocity, to.velocity, t));
		}

		public static implicit operator string(CharacterState s)
		{
			return s.ToString();
		}


		public string ToString()
		{
			return "p: " + this.position + " v: " + this.velocity;
		}
	}


	// Optional
	public Animator animator;

	public float maxSurgeSpeed = 5f; // Forward, backward
	public float maxSwaySpeed = 5f; // Side to side
	public float jumpHeight = 4f;

	// Optional
	public AudioBase jumpSoundEffect;

	public bool forceNetworkViewToBeMine = true;
	public bool forceGameManagerStatus = true;

	CharacterController characterController;
	NetworkView netView;



	GameManager gameManager;


	[HideInInspector]
	public float debugJumpYStart;
	[HideInInspector]
	public float debugJumpYMax;

	[HideInInspector]
	public Vector3 debugPrevPosition;
	[HideInInspector]
	public float debugGroundSpeed;


	// We use these to smooth between values in certain framerate situations in the `Update()` loop
	CharacterState currentState = new CharacterState();
	CharacterState previousState = new CharacterState();

	// Use this for initialization
	void Start () {
		this.characterController = GetComponent<CharacterController>();
		this.netView = GetComponent<NetworkView>();

		// Grab the Player Manager
		GameObject[] managers = GameObject.FindGameObjectsWithTag("Manager");
		if(managers.Length > 0)
			this.gameManager = managers[0].GetComponent<GameManager>();


		this.ResetCharacterDriver();
	}

	// Use this to reset any built up forces
	[ContextMenu("Reset Character State")]
	void ResetCharacterDriver()
	{
		// Set the transition state
		this.currentState = new CharacterState(transform.position, Vector3.zero);
		this.previousState = this.currentState;
	}

	float t = 0f;
	float dt = 0.01f;
	float currentTime = 0f;
	float accumulator = 0f;

	bool isFirstPhysicsFrame = true;

	// Update is called once per frame
	void Update () 
	{
		// Only control the player if the networkView belongs to you
		if(!this.forceNetworkViewToBeMine || (this.netView && this.netView.isMine))
		{
			// If the game has started
			if(!this.forceGameManagerStatus || (this.gameManager && this.gameManager.GameStatus == GameManager.GameState.started))
			{
				// Fixed deltaTime rendering at any speed with smoothing
				// Technique: http://gafferongames.com/game-physics/fix-your-timestep/
				float frameTime = Time.time - currentTime;
				this.currentTime = Time.time;
				
				this.accumulator += frameTime;
				
				while (this.accumulator >= this.dt)
				{
					this.previousState = this.currentState;
					this.currentState = this.MoveUpdate(this.currentState, this.dt);

					//integrate(state, this.t, this.dt);
					Vector3 movementDelta = currentState.position - transform.position;
					this.characterController.Move(movementDelta);
					this.currentState = new CharacterState(transform.position, this.currentState.velocity);

					accumulator -= this.dt;
					this.t += this.dt;
				}

			}
		}
		else
		{
			enabled = false; // Disable this script if we can't control this player
		}

		// Reset it
		this.isFirstPhysicsFrame = true;
	}

	CharacterState MoveUpdate(CharacterState state, float deltaTime)
	{
		CharacterState currentState = new CharacterState(state);

		// Adjust mecanim parameters
		if(animator)
		{
			animator.SetFloat("SurgeSpeed", Input.GetAxis("Vertical"));
			animator.SetFloat("SwaySpeed", Input.GetAxis("Horizontal"));
		}

		// We use this for a gizmo
		this.debugGroundSpeed = (Vector3.Scale(currentState.position, new Vector3(1, 0, 1)) - Vector3.Scale(this.debugPrevPosition, new Vector3(1, 0, 1))).magnitude / deltaTime;
		this.debugPrevPosition = currentState.position;


		// Add the gravity
		if(this.characterController.isGrounded)
		{
			// Remove the gravity in the direction we hit (ground)
			currentState.velocity -= Vector3.Project(currentState.velocity, Physics.gravity.normalized);
		}

		currentState.velocity += Physics.gravity * deltaTime;

		
		// Jumping
		if(this.characterController.isGrounded && Input.GetButtonDown("Jump") && this.isFirstPhysicsFrame) 
		{
			// Play the jumping sound effect
			if(this.jumpSoundEffect)
				this.jumpSoundEffect.PlayOneShot();

			// Jump the player
			currentState.velocity += -1f * Physics.gravity.normalized * this.CalculateJumpVerticalSpeed(this.jumpHeight);

			// Debugging jump height
			// Get the height the jump started at
			this.debugJumpYStart = currentState.position.y;
			this.debugJumpYMax = currentState.position.y; // Reset this
		}

		// Debugging jump height
		// GEt the new max so we can compare to where we started
		if(currentState.position.y > this.debugJumpYMax)
			this.debugJumpYMax = currentState.position.y;
		


		// Movement Surge and Sway
		// Elliptical player/character movement. See diagram for more info:
		// 		http://i.imgur.com/am2OYj1.png
		float angle =  Mathf.Atan2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
		float surgeSpeed = Mathf.Abs(Input.GetAxis("Vertical")) * this.maxSurgeSpeed * Mathf.Sin(angle); // Forward and Backward
		float swaySpeed = Mathf.Abs(Input.GetAxis("Horizontal")) * this.maxSwaySpeed * Mathf.Cos(angle); // Left and Right



		// Get the camera directions without the Y component
		Vector3 cameraForwardNoY = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 cameraRightNoY = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

		Vector3 movementDelta = (cameraForwardNoY*surgeSpeed + cameraRightNoY*swaySpeed) * deltaTime;
		movementDelta += currentState.velocity * deltaTime;

		currentState.position += movementDelta;

		// Set this so we don't get confused later on
		this.isFirstPhysicsFrame = false;

		return currentState;
	}


	float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		// See for formula: http://math.stackexchange.com/a/222585/60008
		return Mathf.Sqrt(2f * targetJumpHeight * Physics.gravity.magnitude);
	}
}
