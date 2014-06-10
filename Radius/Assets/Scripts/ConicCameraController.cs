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

	public float panAroundSpeed = .1f;
	public float zoomScrollSpeed = 10f;
	
	public float height = 50f;
	public float radius = 30f;
	
	public float minHeight = 20f;

	// u = minHeight to height
	private float targetU = 0f;
	private float currentU = 0f;
	// theta = 0 to 2pi
	private float targetAngle = 0f;
	private float currentAngle = 0f;
	
	// Use this for initialization
	void Start () {
		// Init the conic variables
		this.targetU = this.minHeight;
		this.currentU = this.targetU;
		this.targetAngle = 0f;
		this.currentAngle = this.targetAngle;

		// Place the camera at the initial position of the cone
		transform.position = this.GetConePoint(this.targetU, this.targetAngle, this.GetFollowVehiclePosition());

	}
	
	
	
	// Update is called once per frame
	void LateUpdate () {
		// Scroll back and forth to zoom in and out
		this.targetU = Mathf.Clamp(this.targetU + this.zoomScrollSpeed*-Input.GetAxis("Mouse ScrollWheel"), this.minHeight, this.height);
		
		// Hold middle button and drag side to side to orbit around
		if(Input.GetMouseButton(2))
		{
			this.targetAngle = this.WrapAngleRad(this.targetAngle + this.panAroundSpeed*Input.GetAxis("Mouse X"));
		}
		

		this.currentU = Mathf.Lerp(this.currentU, this.targetU, this.lerpSpeed*Time.deltaTime);
		this.currentAngle = Mathf.Lerp(this.currentAngle, this.targetAngle, this.lerpSpeed*Time.deltaTime);

		// Find the new target position
		this.transform.position = this.GetConePoint(this.currentU, this.currentAngle, this.GetFollowVehiclePosition());//new Vector3(x, y, z);

		// Lerp to the target position
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
			 