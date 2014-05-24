/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ProceduralRing.cs, May 2014
 */

using UnityEngine;
using System.Collections;

public class ProceduralRing : MonoBehaviour {

	// Generates a 3D ring
	// Basically a cylinder without caps(top, bottom)
	//
	// Good for King of the Hill boundaries (think Halo)
	
	public int numSides = 8;
	public float radius = 2f;
	
	public int heightSegments = 1; // How many planes in between top and bottom
	public float height = 8f;
	
	public MeshFilter meshFilter;
	
	[HideInInspector]
	public Vector3[] debugRingVertices; // Used for Draw Gizmos and Handles
	
	
	// Use this for initialization
	void Start () {
		this.RecalculateMesh();
	}

	[ContextMenu("RecalculateMesh")]
	public void RecalculateMesh()
	{
		if(this.meshFilter)
		{
			//Debug.Log("Recalculating Plane Mesh");
			Mesh mesh = this.GenerateRing(this.numSides, this.radius, this.heightSegments, this.height);
			this.meshFilter.mesh = mesh;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public static Vector3[] GenerateNGonSpline(int numSides, float radius)
	{
		// Returns an array of vertices that make a ngon.
		
		Vector3[] nGonVerts = new Vector3[numSides];
		for(int vert = 0; vert < numSides; vert++)
		{
			float r = vert*((2f*Mathf.PI)/numSides);
			
			nGonVerts[vert] = new Vector3(radius*Mathf.Cos(r), 0f, radius*Mathf.Sin(r));
		}
		
		return nGonVerts;
	}





	public static Mesh GenerateRing(int numSides, float radius, int heightSegments, float height)
	{
		Vector3[] spline = GenerateNGonSpline(numSides, radius);
		Mesh mesh = MeshUtils.GenerateLoftNurb(spline, heightSegments, height);

		return mesh;
	}




	
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 1);
		
		
		Vector3[] nGonVerts = this.GenerateNGonSpline(this.numSides, this.radius);
		
		for(int i = 0; i < nGonVerts.Length; i++)
		{
			Gizmos.DrawLine(nGonVerts[i] + transform.position, nGonVerts[(i+1)%nGonVerts.Length] + transform.position);
		}
		
		for(int i = 0; i < nGonVerts.Length; i++)
		{
			Gizmos.DrawLine(nGonVerts[i] + transform.position + new Vector3(0, this.height, 0), nGonVerts[(i+1)%nGonVerts.Length] + transform.position + new Vector3(0, this.height, 0));
		}
		
		
		
		/*
		if(this.debugRingVertices != null)
		{
			for(int i = 0; i < this.debugRingVertices.Length-1; i++)
			{
				Gizmos.DrawLine(this.debugRingVertices[i] + transform.position, this.debugRingVertices[i+1] + transform.position);
			}
		}
		*/
		
		
	}
}
