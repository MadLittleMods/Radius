using UnityEngine;
using System.Collections;

public class ProceduralPlane : MonoBehaviour {

	public MeshFilter meshFilter;

	public int SegmentsX = 10;
	public int SegmentsZ = 10;

	public float Width = 10f;
	public float Height = 10f;

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
			Mesh mesh = GeneratePlane(this.SegmentsX, this.SegmentsZ, this.Width/this.SegmentsX, this.Height/this.SegmentsZ);
			this.meshFilter.mesh = mesh;
			
			/*
			Mesh mesh = this.meshFilter.mesh;
			mesh.Clear();

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			*/
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}



	
	
	public static Mesh GeneratePlane(int segmentsX, int segmentsZ, float segmentSizeX, float segmentSizeZ, Vector3 translate = new Vector3(), Quaternion rotation = new Quaternion())
	{
		Mesh mesh = new Mesh();
		mesh.Clear();

		if(segmentsX > 0 && segmentsZ > 0 && segmentSizeX > 0 && segmentSizeZ > 0)
		{
			// 3 points per triangle
			// 2 triangles per square face
			// many square faces
			Vector3[] meshVertices = new Vector3[3*2*segmentsX*segmentsZ];
			
			for(int segZ = 0; segZ < segmentsZ; segZ++) {
				for(int segX = 0; segX < segmentsX; segX++) {
					//rowMeshVertices[vert] = GenerateSquareFace(new Vector3((float)vert*segmentSize, 0f, 0f), segmentSize, Quaternion.Euler(0, 0, 90));
					
					Vector3[] squareFace = MeshUtils.GenerateRectFaceTris(segmentSizeX, segmentSizeZ, new Vector3(segX*segmentSizeX - ((segmentsX*segmentSizeX)/2) + (segmentSizeX/2), 0f, segZ*segmentSizeZ - ((segmentsZ*segmentSizeZ)/2) + (segmentSizeZ/2)) + translate, rotation);
					
					System.Array.Copy(
						squareFace, 
						0, 
						meshVertices, 
						(segZ*segmentsX*squareFace.Length) + segX*squareFace.Length, 
						squareFace.Length
					);
				}
			}
			
			// Set the verts we generated into the mesh
			mesh.vertices = meshVertices;
			
			
			// --------------------------------------------------------------
			
			
			// Set up the mesh UV array
			Vector2[] meshUVs;
			
			bool overlapUVs = true;
			if(overlapUVs)
			{
				meshUVs = MeshUtils.GenerateOverlappingUVArrayForTris(meshVertices.Length);
			}
			else
			{
				meshUVs = MeshUtils.GenerateNonOverlappingUVArrayForTris(meshVertices.Length, segmentsX, segmentsZ);
			}
			
			// Set the UVs we generated into the mesh
			mesh.uv = meshUVs;
			
			
			// --------------------------------------------------------------

			// Set up the mesh triangle array
			//
			// "3*" 3 entries that make up a triangle
			// "2*" 2 triangles make up the quad
			int[] meshTriangles = MeshUtils.GenerateTriArrayForTris(meshVertices.Length);
			
			// Set the triangles we generated into the mesh
			mesh.triangles = meshTriangles;



			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

		}

		return mesh;
	}


}
