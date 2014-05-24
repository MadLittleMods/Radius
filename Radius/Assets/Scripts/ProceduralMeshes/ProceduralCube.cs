/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: ProceduralCube.cs, May 2014
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralCube : MonoBehaviour {

	public int BlockCountX = 1;
	public int BlockCountY = 1;
	public int BlockCountZ = 1;

	public float BlockSize = 2;
	
	public MeshFilter meshFilter;

	// Use this for initialization
	void Start () {
		this.RecalculateRingCubeMesh();
	}

	[ContextMenu("RecalculateCubeMesh")]
	public void RecalculateRingCubeMesh()
	{
		if(this.meshFilter)
		{
			//Debug.Log("Recalculating Plane Mesh");
			Mesh mesh = this.GenerateCube(this.BlockCountX, this.BlockCountY, this.BlockCountZ, this.BlockSize);
			this.meshFilter.mesh = mesh;
		}

	}

	
	// Update is called once per frame
	void Update () {
	
	}




	public static Vector3[] GenerateSquareSpline(int segmentsX, int segmentsZ, float segmentSize)
	{
		// +: origin
		// Z-axis
		// Λ
		// │ 4v         <3
		// │    |‾˙‾˙‾|
		// │    |  +  |
		// │    |_._._| 
		// │ 1>         ^2
		// │
		// └──────────────> X-axis


		// No overlap in verts for the extrude
		Vector3[] squareVerts = new Vector3[((segmentsX+2)*2) + ((segmentsZ-2)*2)];
		//Debug.Log(segmentsX + " : " + segmentsZ);
		//Debug.Log(squareVerts.Length);

		// Make first side x
		for(int vert = 0; vert < segmentsX+1; vert++)
		{
			squareVerts[vert] = new Vector3((vert*segmentSize) - ((segmentsX*segmentSize)/2), 0f, -((segmentsZ*segmentSize)/2));
		}

		// Make first side z
		for(int vert = 0; vert < segmentsZ-1; vert++)
		{
			squareVerts[vert+segmentsX+1] = new Vector3((segmentsX*segmentSize)/2, 0f, ((vert+1)*segmentSize) - ((segmentsZ*segmentSize)/2));
		}

		// Make second side x
		for(int vert = 0; vert < segmentsX+1; vert++)
		{
			squareVerts[vert+segmentsX+1+segmentsZ-1] = new Vector3(((segmentsX*segmentSize)/2) - (vert*segmentSize), 0f, (segmentsZ*segmentSize)/2);
		}

		// Make second side z
		for(int vert = 0; vert < segmentsZ-1; vert++)
		{
			squareVerts[vert+((segmentsX+1)*2)+segmentsZ-1] = new Vector3(-((segmentsX*segmentSize)/2), 0f, ((segmentsZ*segmentSize)/2) - ((vert+1)*segmentSize));

		}

		return squareVerts;

	}


	



	public static Mesh GenerateCube(int segmentsX, int segmentsY, int segmentsZ, float segmentSize)
	{
		//Mesh meshFilterMesh = this.meshFilter.mesh;

		// Generate Cube Loft
		Vector3[] spline = GenerateSquareSpline(segmentsX, segmentsZ, segmentSize);
		Mesh loftMesh = MeshUtils.GenerateLoftNurb(spline, segmentsY, segmentsY*segmentSize);
		//Debug.Log(loftMesh.vertices.Length);

		// Generate Bottom Cap
		Mesh bottomCapMesh = ProceduralPlane.GeneratePlane(segmentsX, segmentsZ, segmentSize, segmentSize, new Vector3(0, 0, 0), Quaternion.Euler(180, 0, 0));
		//Debug.Log(bottomCapMesh.vertices.Length);

		// Generate Top Cap
		Mesh topCapMesh = ProceduralPlane.GeneratePlane(segmentsX, segmentsZ, segmentSize, segmentSize, new Vector3(0, segmentsY*segmentSize, 0), Quaternion.Euler(0, 0, 0));
		//Debug.Log(topCapMesh.vertices.Length);

		// Combine the meshes
		//MeshUtils.CombineMeshes(this.meshFilter, MeshUtils.GenerateCombineMeshMatrixTransform(gameObject), loftMesh, bottomCapMesh, topCapMesh);

		return MeshUtils.CombineMeshes(MeshUtils.GenerateCombineMeshMatrixTransform(), loftMesh, bottomCapMesh, topCapMesh);
	}



	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 1f);
		

		/* * /
		Vector3[] sqaureVerts = this.GenerateSquare(this.BlockCountX, this.BlockCountZ, this.BlockSize);
		
		for(int i = 0; i < sqaureVerts.Length; i++)
		{
			if(i == 0)
				Gizmos.color = new Color(1f, 1f, 0f, 1f);
			else
				Gizmos.color = new Color((float)i/sqaureVerts.Length, 0, ((float)i/sqaureVerts.Length/2)+.2f, 1f);

			Gizmos.DrawLine(sqaureVerts[i] + transform.position, sqaureVerts[(i+1)%sqaureVerts.Length] + transform.position);
		}
		/* */



		/* * /
		Vector3[] squareFace2 = GenerateSquareFace(new Vector3(), this.BlockSize, Quaternion.Euler(0, 0, 0));
		for(int i = 0; i < squareFace2.Length; i++)
		{
			if(i == 0)
				Gizmos.color = new Color(1f, 1f, 0f, 1f);
			else
				Gizmos.color = new Color((float)i/squareFace2.Length, 0, ((float)i/squareFace2.Length/2)+.2f, 1f);
			
			Gizmos.DrawLine(squareFace2[i] + transform.position, squareFace2[(i+1)%squareFace2.Length] + transform.position);
		}
		/* */


	}
}
