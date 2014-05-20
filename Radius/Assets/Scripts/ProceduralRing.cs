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
	
	// Author: Eric (EricEastwood.com)
	//
	// Description:
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
		this.GenerateRing(this.numSides, this.radius, this.heightSegments, this.height);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	Vector3[] GenerateNGon(int numSides, float radius)
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
	
	void GenerateRing(int numSides, float radius, int heightSegments, float height)
	{
		if(this.meshFilter)
		{
			Mesh mesh = this.meshFilter.mesh;
			mesh.Clear();
			
			// Generate nGon verts
			// We use this same ring all along the height of the ring
			Vector3[] nGonVerts = this.GenerateNGon(numSides, radius); 
			
			// We do this so we don't have to share vertices
			Vector3[] doubleNGonVerts = new Vector3[2*nGonVerts.Length]; // For top and bottom height segments
			for(int vert = 0; vert < nGonVerts.Length; vert++)
			{
				doubleNGonVerts[vert*2] = nGonVerts[vert];
				doubleNGonVerts[(vert*2)+1] = nGonVerts[vert];
			}
			
			
			// Set up the mesh vert array
			// 2 double n-gon for top and bottom
			// For all other segments we need quadruple n-gon so
			Vector3[] meshVertices = new Vector3[(doubleNGonVerts.Length*2) + (2*doubleNGonVerts.Length*(heightSegments-1))];
			
			//Debug.Log("meshVert length: " + meshVertices.Length);
			
			// Add all vertices
			// Vertices added bottom to top
			int verticeIndex = 0;
			for(int hSeg = 0; hSeg <= heightSegments; hSeg++)
			{
				// Figure out how high we are in the ring
				float currentHeightLevel = hSeg*(height/heightSegments);
				
				
				Vector3[] currentLevelVerts = (Vector3[])doubleNGonVerts.Clone(); // Copy the nGon verts
				// Now we make them to the current height level
				for(int vert = 0; vert < currentLevelVerts.Length; vert++)
				{
					currentLevelVerts[vert] += new Vector3(0, currentHeightLevel, 0);
				}
				
				System.Array.Copy(
					currentLevelVerts, 
					0, 
					meshVertices, 
					verticeIndex, 
					currentLevelVerts.Length
					);
				verticeIndex += currentLevelVerts.Length;
				
				// If we are not at the first or last height level
				// We need quadruple verts so add another double
				if(hSeg != 0 && hSeg != heightSegments)
				{
					System.Array.Copy(
						currentLevelVerts, 
						0, 
						meshVertices, 
						verticeIndex,
						currentLevelVerts.Length
						);
					verticeIndex += currentLevelVerts.Length;
				}
			}
			
			// Set the verts we generated into the mesh
			mesh.vertices = meshVertices; 
			this.debugRingVertices = meshVertices;
			
			
			
			// Set up the mesh UV array
			Vector2[] meshUVs = new Vector2[meshVertices.Length];
			
			// Each ring plane is drawn over top of each other
			// So that the material doesn't have to tile
			verticeIndex = 0;
			for(int hSeg = 0; hSeg <= heightSegments; hSeg++)
			{
				// We start at index 0 but this is the end of the UV from the last side
				bool uStart = false;
				bool vStart = hSeg != heightSegments ? false : true;
				int numberVertsThisHeightLevel = (hSeg == 0 || hSeg == heightSegments ? doubleNGonVerts.Length : 2*doubleNGonVerts.Length);
				for(int vert = 0; vert < numberVertsThisHeightLevel; vert++)
				{
					meshUVs[verticeIndex + vert] = new Vector2(uStart ? 0f : 1f, vStart ? 1f : 0f);
					
					uStart = !uStart;
					if((vert+1)%doubleNGonVerts.Length == 0)
						vStart = !vStart;
				}
				
				verticeIndex += numberVertsThisHeightLevel;
			}
			
			// Set the UVs we generated into the mesh
			mesh.uv = meshUVs;
			
			
			
			// Set up the mesh triangle array
			//
			// "3*" 3 entries that make up a triangle
			// "2*" 2 triangles make up the quad
			int[] meshTriangles = new int[3*(2*numSides*heightSegments)];
			
			//Debug.Log("Tri length: " + meshTriangles.Length);
			//Debug.Log("vertice count: " + meshVertices.Length);
			
			int currTriangleIndice = 0;
			for(int hSeg = 1; hSeg <= heightSegments; hSeg++)
			{
				//Debug.Log(hSeg + ": " + this.GetVerticeStartFromHeightLevel(hSeg));
				
				// Start at index 1 and end at last+1 (that should rollover to index 0)
				for(int vert = 1; vert < nGonVerts.Length+1; vert++)
				{
					/*
					if(currTriangleIndice >= meshTriangles.Length)
						Debug.Log("meshTriangles array index out of bounds. tri-indice: " + currTriangleIndice  + " vert: "+ vert);
					*/
					
					// 1____2
					//  |  /
					//  | /
					// 3|/
					meshTriangles[currTriangleIndice] = this.GetVerticeStartFromHeightLevel(hSeg)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+(((vert*2)-1)%doubleNGonVerts.Length);
					meshTriangles[currTriangleIndice+1] = this.GetVerticeStartFromHeightLevel(hSeg)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
					meshTriangles[currTriangleIndice+2] = this.GetVerticeStartFromHeightLevel(hSeg-1)+(((vert*2)-1)%doubleNGonVerts.Length);
					
					
					//    /|2
					//   / |
					//  /  |
					// 1‾‾‾‾3
					meshTriangles[currTriangleIndice+3] = this.GetVerticeStartFromHeightLevel(hSeg-1)+(((vert*2)-1)%doubleNGonVerts.Length);
					meshTriangles[currTriangleIndice+4] = this.GetVerticeStartFromHeightLevel(hSeg)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
					meshTriangles[currTriangleIndice+5] = this.GetVerticeStartFromHeightLevel(hSeg-1)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
					
					
					/*
					for(int currTri = 0; currTri < 6; currTri++)
					{
						if(meshTriangles[currTriangleIndice+currTri] >= meshVertices.Length)
							Debug.Log("out of bounds indice: " + currTriangleIndice + " " + currTri + " " + meshTriangles[currTriangleIndice+currTri]);
					}
					*/
					
					currTriangleIndice += 6;
					
					
				}
				
			}
			//Debug.Log("currTri: " + currTriangleIndice);
			//Debug.Log("mesh tri: " + mesh.triangles.Length);
			
			// Set the triangles we generated into the mesh
			mesh.triangles = meshTriangles;
			
			
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
		}
		
	}
	
	int GetVerticeStartFromHeightLevel(int hSeg)
	{
		// hSeg goes from the bottom to the top
		// Range: 0 to this.heightSegements+1
		
		// If they pass in a negative height segment return a invalid index
		// If they pass in a height segment greater than the possible, return a invalid index
		if(hSeg < 0 || hSeg > this.heightSegments)
			return -1;
		
		
		int doubleNGonvertLength = this.numSides*2;
		
		// The first height segment starts at the 0 index
		if(hSeg == 0)
			return 0;
		
		// Start out with the first layer
		int verticestart = doubleNGonvertLength;
		
		if(hSeg > 1)
			verticestart += (hSeg-1)*2*doubleNGonvertLength; // Now add any additional layers
		
		return verticestart;
		
	}
	
	[ContextMenu("RecalculateRingMesh")]
	public void RecalculateRingMesh()
	{
		this.GenerateRing(this.numSides, this.radius, this.heightSegments, this.height);
	}
	
	
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 1);
		
		
		Vector3[] nGonVerts = this.GenerateNGon(this.numSides, this.radius);
		
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
