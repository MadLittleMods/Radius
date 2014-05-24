/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: KingHillController.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class KingHillController : MonoBehaviour {
	
	ProceduralRing proRing;

	TerritoryController territoryController;

	public float scorePerTick = 5f;
	public float timePerTick = 1f;
	float tickTime;


	public float radius = 2f;
	float prevRadius;
	
	public float height = 1f;
	float prevHeight;
	
	
	// Give us the texture so that we can scale proportianally the width according to the height variable below
	// We will grab it from the meshRenderer
	public Texture texture;
	public float textureToMeshHeight = .25f; // Use this to contrain texture to a certain size
	
	
	// Variables used for capturing
	public CapsuleCollider capsuleCollider;
	
	public GameObject andBoxColliderObject; // We use the capsule collider and this to determine if in the "cylinder"
	BoxCollider andBoxCollider;

	public NetworkView netView;
	
	private PlayerManager playerManager;
	private ScoreManager scoreManager;
	private GameManager gameManger;


	// Use this for initialization
	void Start () {

		this.proRing = GetComponent<ProceduralRing>();
		this.territoryController = GetComponent<TerritoryController>();

		if(Network.isServer)
		{
			this.territoryController.OnTerritoryCaptured += this.HillCapturedEvent;
			this.territoryController.OnTerritoryLost += this.HillLostEvent;
		}

		this.prevRadius = this.radius;
		this.prevHeight = this.height;
		
		// Initialize ring color
		if(gameObject.renderer)
			gameObject.renderer.material.color = Player.TeamToColor(Player.Team.None);
		
		this.andBoxCollider = this.andBoxColliderObject.GetComponent<BoxCollider>();

		
		// Initilize ring size
		this.UpdateHillSize();


		GameObject[] managerObjects = GameObject.FindGameObjectsWithTag("Manager");
		if(managerObjects.Length > 0)
		{
			GameObject managerObject = managerObjects[0];

			// Grab the Player Manager
			this.playerManager = managerObject.GetComponent<PlayerManager>();
			this.scoreManager = managerObject.GetComponent<ScoreManager>();
			this.gameManger = managerObject.GetComponent<GameManager>();

			// Need to update the hill color if the player changes color
			this.playerManager.OnPlayerUpdated += this.PlayerUpdatedEvent;

		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Network.isServer)
		{
			if(this.gameManger.GameStatus == GameManager.GameState.started)
			{
				this.tickTime += Time.deltaTime;

				// Check for a hill size change
				this.CheckForHillSizeChange();

				// Tick the score up
				if(this.territoryController.LockedCaptureState && this.tickTime > this.timePerTick)
				{
					//Debug.Log("Territory updating score");
					this.scoreManager.UpdateScore("score", this.territoryController.LockedCaptureState.guid, this.scorePerTick);
					// Reset the counter
					this.tickTime = 0f;
				}
			}
		}
	}
	
	void CheckForHillSizeChange()
	{
		// Check to see whether it is changed
		if(this.prevRadius != this.radius || this.prevHeight != this.height)
		{
			if(this.radius < 0)
				this.radius = 0;
			if(this.height < 0)
				this.height = 0;

			if(this.netView)
				this.netView.RPC("RPC_KingHillController_UpdateHillSize", RPCMode.AllBuffered, this.radius, this.height);
			else
				this.UpdateHillSize();
		}
		
		this.prevRadius = this.radius;
		this.prevHeight = this.height;
	}
	
	[RPC]
	void RPC_KingHillController_UpdateHillSize(float radius, float height)
	{
		this.radius = radius;
		this.height = height;

		this.UpdateHillSize();
	}
	
	void UpdateHillSize()
	{
		this.UpdateColliderBounds();
		
		//Debug.Log("Texture Size: " + this.texture.width + " ~ " + this.texture.height);
		
		// We will pack as many height segments in collider height
		this.proRing.heightSegments = (int)Mathf.Floor((float)this.height/this.textureToMeshHeight);
		// Multiply ammount of height segments by the texture-to-mesh height
		// This will not be the same as the collider height. 
		this.proRing.height = this.proRing.heightSegments*this.textureToMeshHeight;
		
		float textureToMeshWidth = ((float)texture.width/texture.height)*this.textureToMeshHeight;
		this.proRing.numSides = (int)Mathf.Floor((2*Mathf.PI*this.radius)/textureToMeshWidth);
		this.proRing.radius = (this.proRing.numSides*textureToMeshWidth)/(2*Mathf.PI);
		
		//if (Application.isPlaying)
		this.proRing.RecalculateMesh();
	}
	
	void UpdateColliderBounds()
	{
		this.capsuleCollider.radius = this.radius;
		this.capsuleCollider.height = this.height + (2*this.radius);
		
		this.capsuleCollider.center = new Vector3(this.capsuleCollider.center.x, this.height/2, this.capsuleCollider.center.z);
		
		
		this.andBoxCollider.size = new Vector3(2*this.radius, this.height, 2*this.radius);
		this.andBoxCollider.center = new Vector3(0, this.height/2, 0);
	}



	void HillCapturedEvent(MonoBehaviour sender, TerritoryController.TerritoryActivityEventArgs e)
	{
		// Called when the hill is captured
		if(Network.isServer)
			this.netView.RPC("RPC_KingHillController_HillCaptured", RPCMode.AllBuffered, e.player.networkView.owner.guid);
		else
			this.HillCaptured(e.player);
	}

	[RPC]
	void RPC_KingHillController_HillCaptured(string guid)
	{
		//Debug.Log(this.playerManager.PlayerList.ToDebugString());
		Debug.Log(this.playerManager.GetPlayer(guid).ThisTeamToColor() + " " + this.playerManager.GetPlayer(guid).PersonalColor);
		this.HillCaptured(this.playerManager.GetPlayer(guid));
	}

	void HillCaptured(Player player)
	{
		// TODO: Play sound
		Debug.Log(player.PlayerTeam + "Captured the hill");

		if(gameObject.renderer)
			gameObject.renderer.material.color = player.ThisTeamToColor();
	}



	void HillLostEvent(MonoBehaviour sender, TerritoryController.TerritoryActivityEventArgs e)
	{
		// Called when the hill is lost

		if(Network.isServer)
			this.netView.RPC("RPC_KingHillController_HillLost", RPCMode.AllBuffered, e.player.networkView.owner.guid);
		else
			this.HillLost(e.player);
	}

	[RPC]
	void RPC_KingHillController_HillLost(string guid)
	{
		this.HillLost(this.playerManager.GetPlayer(guid));
	}

	void HillLost(Player player)
	{
		// TODO: Play sound
		Debug.Log(player.PlayerTeam + "Lost the hill");

		if(gameObject.renderer)
			gameObject.renderer.material.color = Player.TeamToColor(Player.Team.None);
	}





	public void PlayerUpdatedEvent(MonoBehaviour sender, PlayerManager.PlayerActivityEventArgs e)
	{
		// Change the color when the player is updated
		if(this.territoryController)
			if(this.territoryController.LockedCaptureState)
				if(this.territoryController.LockedCaptureState.guid == e.PlayerData.guid)
					if(gameObject && gameObject.renderer)
						gameObject.renderer.material.color = ColorData.ToColor(e.PlayerData.TeamColor);
	}

}
