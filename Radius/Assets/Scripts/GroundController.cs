using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GroundController : MonoBehaviour {
	
	public ProceduralPlane proceduralPlane;
	
	public float Width = 10f;
	public float Height = 10f;
	
	// Give us the texture so that we can scale proportianally the width according to the height variable below
	// We will grab it from the meshRenderer
	public Texture texture;
	public float textureToMeshZ = 2f; // Use this to contrain texture to a certain size
	
	
	float prevWidth = 10f;
	float prevHeight = 10f;
	float prevTextureToMeshZ = 2f;
	
	// Use this for initialization
	void Start () {
		this.prevWidth = this.Width;
		this.prevHeight = this.Height;
		this.prevTextureToMeshZ = this.textureToMeshZ;
		
		// Do calculations and Generate the mesh
		this.UpdatePlaneSize();
	}
	
	// Update is called once per frame
	void Update () {
		// If something has changed
		if(this.Width != this.prevWidth || this.Height != this.prevHeight || this.textureToMeshZ != this.prevTextureToMeshZ)
			this.UpdatePlaneSize();
		
		
		// Maintain previous state variables
		this.prevWidth = this.Width;
		this.prevHeight = this.Height;
		this.prevTextureToMeshZ = this.textureToMeshZ;
	}
	
	[ContextMenu("UpdatePlaneSize")]
	void UpdatePlaneSize()
	{
		//Debug.Log("updating ground plane");
		
		// We will pack as many height segments in collider height
		this.proceduralPlane.SegmentsZ = (int)Mathf.Floor((float)this.Height/this.textureToMeshZ);
		// Multiply amount of height segments by the texture-to-mesh height
		// This will not be the same as the collider height. 
		this.proceduralPlane.Height = this.proceduralPlane.SegmentsZ * this.textureToMeshZ;
		
		// Figure out texture-to-mesh width based on user set texture-to-mesh height
		float textureToMeshX = ((float)this.texture.width/this.texture.height)*this.textureToMeshZ;
		// Proportianally pack in the width segments
		this.proceduralPlane.SegmentsX = (int)Mathf.Floor((float)this.Width/textureToMeshX);
		// Multiply amount of width segments by the texture-to-mesh width
		this.proceduralPlane.Width = this.proceduralPlane.SegmentsX * textureToMeshX;
		
		// Generate mesh
		this.proceduralPlane.RecalculateMesh();
	}
}