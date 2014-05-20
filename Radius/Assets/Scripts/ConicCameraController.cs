/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ConicCameraController.cs, May 2014
 */

using UnityEngine;
using System.Collections;

public class ConicCameraController : MonoBehaviour {
	
	public GameObject followVehicle;
	public float lerpSpeed = 4f;
	
	public float height = 50;
	public float radius = 30;
	
	public float minHeight = 20;
	
	private GameManager gameManager;
	
	// u = minHeight to height
	private float u = 0;
	// theta = 0 to 2pi
	private float angle = 0;
	
	private Vector3 targetPosition;
	
	// Use this for initialization
	void Start () {
		// Init the conic variables
		this.u = this.minHeight;
		this.angle = 0;
		// Place the camera at the initial position of the cone
		transform.position = this.GetConePoint(this.u, this.angle, this.GetFollowVehiclePosition());

		this.targetPosition = transform.position;
	}
	
	
	
	// Update is called once per frame
	void LateUpdate () {
		
		// Scroll back and forth to zoom in and out
		this.u = Mathf.Clamp(this.u + 10*-Input.GetAxis("Mouse ScrollWheel"), this.minHeight, this.height);
		
		// Hold middle button and drag side to side to orbit around
		if(Input.GetMouseButton(2))
		{
			this.angle = this.WrapAngleRad(this.angle + Input.GetAxis("Mouse X")/10);
		}
		

		// Find the new target position
		this.targetPosition = this.GetConePoint(this.u, this.angle, this.GetFollowVehiclePosition());//new Vector3(x, y, z);

		// Lerp to the target position
		this.transform.position = Vector3.Lerp(transform.position, this.targetPosition, this.lerpSpeed*Time.deltaTime);
		this.transform.LookAt(this.GetFollowVehiclePosition());
		
		
	}
	
	void OnDrawGizmos()
    {
		Gizmos.color = new Color(.6039f, .4509f, 1f, 1f);
		for(float angle = 0; angle <= 2*Mathf.PI; angle += Mathf.PI/20)
		{
			Gizmos.DrawLine(GetConePoint(this.minHeight, angle, this.GetFollowVehiclePosition()), GetConePoint(this.height, angle, this.GetFollowVehiclePosition()));
		}
    }
	
	
	
	Vector3 GetConePoint(float u, float angle, Vector3 offset)
	{
		Vector3 point;
		
		//((this.height - u)/this.height)
		point.x = (u/this.height) * this.radius * Mathf.Cos(angle) + offset.x;
		point.y = u + offset.y;
		point.z = (u/this.height) * this.radius * Mathf.Sin(angle) + offset.z;
		
		return point;
	}
	
			
	float WrapAngleRad(float rad)
	{
		if (rad > 2*Mathf.PI)
		{
			rad = rad % (2*Mathf.PI);
		}
		
		return rad;
	}

	Vector3 GetFollowVehiclePosition()
	{
		if(this.followVehicle)
			return this.followVehicle.transform.position;
		else
			return new Vector3();
	}
	
	
	
}
			 