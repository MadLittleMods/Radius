/*
 * Radius: Complete Unity Reference Project
 * 
 * Source: https://github.com/MadLittleMods/Radius
 * Author: Eric Eastwood, ericeastwood.com
 * 
 * File: CubeGridController.cs, May 2014
 */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class CubeGridController : MonoBehaviour {


	public int BlockCountX = 1;
	public int BlockCountY = 1;
	public int BlockCountZ = 1;
	
	public float BlockSize = 2;

	
	ProceduralCube proCube;
	
	BoxCollider boxCollider;

	// Use this for initialization
	void Start () {
		this.proCube = GetComponent<ProceduralCube>();

		this.boxCollider = GetComponent<BoxCollider>();

		this.UpdateColliderSize();
		this.UpdateProceduralCube();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void UpdateColliderSize()
	{
		if(!this.boxCollider)
			this.boxCollider = GetComponent<BoxCollider>();

		if(this.boxCollider)
		{
			this.boxCollider.center = new Vector3(0, (this.BlockCountY*this.BlockSize)/2, 0);

			this.boxCollider.size = new Vector3(this.BlockCountX*this.BlockSize, this.BlockCountY*this.BlockSize, this.BlockCountZ*this.BlockSize);
		}
	}

	void UpdateProceduralCube()
	{
		if(!this.proCube)
			this.proCube = GetComponent<ProceduralCube>();

		if(this.proCube)
		{
			this.proCube.BlockCountX = this.BlockCountX;
			this.proCube.BlockCountY = this.BlockCountY;
			this.proCube.BlockCountZ = this.BlockCountZ;
			this.proCube.BlockSize = this.BlockSize;

			this.proCube.RecalculateRingCubeMesh();
		}
	}

	[ContextMenu("UpdateCubeGrid")]
	void UpdateCubeGrid()
	{
		this.UpdateColliderSize();
		this.UpdateProceduralCube();
	}
}
