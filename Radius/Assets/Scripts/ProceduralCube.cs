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
		RecalculateRingCubeMesh();
	}

	[ContextMenu("RecalculateCubeMesh")]
	public void RecalculateRingCubeMesh()
	{
		this.GenerateCube(this.BlockCountX, this.BlockCountY, this.BlockCountZ, this.BlockSize);
	}

	
	// Update is called once per frame
	void Update () {
	
	}




	Vector3[] GenerateSquareSpline(int segmentsX, int segmentsZ, float segmentSize)
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

	Mesh GenerateRing(Vector3[] spline, int heightSegments, float height)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();
		
		// Generate Square verts
		// We use this same ring all along the height of the ring
		Vector3[] nGonVerts = spline;
		
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
		int[] meshTriangles = new int[3*(2*nGonVerts.Length*heightSegments)];
		
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
				meshTriangles[currTriangleIndice] = this.GetVerticeStartFromHeightLevel(hSeg, heightSegments, spline.Length)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+(((vert*2)-1)%doubleNGonVerts.Length);
				meshTriangles[currTriangleIndice+1] = this.GetVerticeStartFromHeightLevel(hSeg, heightSegments, spline.Length)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
				meshTriangles[currTriangleIndice+2] = this.GetVerticeStartFromHeightLevel(hSeg-1, heightSegments, spline.Length)+(((vert*2)-1)%doubleNGonVerts.Length);
				
				
				//    /|2
				//   / |
				//  /  |
				// 1‾‾‾‾3
				meshTriangles[currTriangleIndice+3] = this.GetVerticeStartFromHeightLevel(hSeg-1, heightSegments, spline.Length)+(((vert*2)-1)%doubleNGonVerts.Length);
				meshTriangles[currTriangleIndice+4] = this.GetVerticeStartFromHeightLevel(hSeg, heightSegments, spline.Length)+(hSeg != heightSegments ? doubleNGonVerts.Length : 0)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
				meshTriangles[currTriangleIndice+5] = this.GetVerticeStartFromHeightLevel(hSeg-1, heightSegments, spline.Length)+((((vert*2)-1)+1)%doubleNGonVerts.Length);
				
				
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


		return mesh;
	}

	int GetVerticeStartFromHeightLevel(int hSeg, int hSegMax, int splineLength)
	{
		// hSeg goes from the bottom to the top
		// Range: 0 to this.heightSegements+1
		
		// If they pass in a negative height segment return a invalid index
		// If they pass in a height segment greater than the possible, return a invalid index
		if(hSeg < 0 || hSeg > hSegMax)
			return -1;
		
		
		int doubleNGonvertLength = splineLength*2;
		
		// The first height segment starts at the 0 index
		if(hSeg == 0)
			return 0;
		
		// Start out with the first layer
		int verticestart = doubleNGonvertLength;
		
		if(hSeg > 1)
			verticestart += (hSeg-1)*2*doubleNGonvertLength; // Now add any additional layers
		
		return verticestart;
		
	}




	Vector3 RotateX(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.x, point.y*Mathf.Cos(radRotation) - point.z*Mathf.Sin(radRotation), point.y*Mathf.Sin(radRotation) + point.z*Mathf.Cos(radRotation));
	}
	Vector3 RotateY(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.z*Mathf.Sin(radRotation) + point.x*Mathf.Cos(radRotation), point.y, point.z*Mathf.Cos(radRotation) - point.x*Mathf.Sin(radRotation));
	}
	Vector3 RotateZ(Vector3 point, float rotation) {
		float radRotation = rotation * Mathf.Deg2Rad;
		return new Vector3(point.x*Mathf.Cos(radRotation) - point.y*Mathf.Sin(radRotation), point.x*Mathf.Sin(radRotation) + point.y*Mathf.Cos(radRotation), point.z);
	}
	
	Vector3 RotateXYZ(Vector3 point, Quaternion rotation)
	{
		Vector3 eulerRotation = rotation.eulerAngles;
		
		Vector3 adjustedPoint = point;
		
		if(eulerRotation.x != 0)
			adjustedPoint = this.RotateX(point, eulerRotation.x);
		
		if(eulerRotation.y != 0)
			adjustedPoint = this.RotateY(point, eulerRotation.y);
		
		if(eulerRotation.z != 0)
			adjustedPoint = this.RotateZ(point, eulerRotation.z);
		
		return adjustedPoint;
	}
	
	Vector3[] GenerateSquareFace(Vector3 translate = new Vector3(), float scale = 1f, Quaternion rotation = new Quaternion()) 
	{
		Vector3[] meshVertices = new Vector3[6];
		
		// 1____2
		//  |  /
		//  | /
		// 3|/
		meshVertices[0] = RotateXYZ(new Vector3(-.5f, 0f, .5f)*scale + translate, rotation) ;
		meshVertices[1] = RotateXYZ(new Vector3(.5f, 0f, .5f)*scale + translate, rotation);
		meshVertices[2] = RotateXYZ(new Vector3(-.5f, 0f, -.5f)*scale + translate, rotation);
		
		
		//    /|2
		//   / |
		//  /  |
		// 1‾‾‾‾3
		meshVertices[3] = RotateXYZ(new Vector3(-.5f, 0f, -.5f)*scale + translate, rotation);
		meshVertices[4] = RotateXYZ(new Vector3(.5f, 0f, .5f)*scale + translate, rotation);
		meshVertices[5] = RotateXYZ(new Vector3(.5f, 0f, -.5f)*scale + translate, rotation);
		
		return meshVertices;
	}


	Mesh GenerateSquareCap(int segmentsX, int segmentsZ, float segmentSize, Vector3 translate = new Vector3(), Quaternion rotation = new Quaternion())
	{
		Mesh mesh = new Mesh();
		mesh.Clear();

		// 3 points per triangle
		// 2 triangles per square face
		// many square faces
		Vector3[] meshVertices = new Vector3[3*2*segmentsX*segmentsZ];

		for(int segZ = 0; segZ < segmentsZ; segZ++) {
			for(int segX = 0; segX < segmentsX; segX++) {
				//rowMeshVertices[vert] = GenerateSquareFace(new Vector3((float)vert*segmentSize, 0f, 0f), segmentSize, Quaternion.Euler(0, 0, 90));

				Vector3[] squareFace = this.GenerateSquareFace(new Vector3(segX*segmentSize - ((segmentsX*segmentSize)/2) + (segmentSize/2), 0f, segZ*segmentSize - ((segmentsZ*segmentSize)/2) + (segmentSize/2)) + translate, segmentSize, rotation);

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
		Vector2[] meshUVs = new Vector2[meshVertices.Length];

		// Each ring plane is drawn over top of each other
		// So that the material doesn't have to tile
		for(int i = 0; i < meshUVs.Length; i+=6)
		{
			// 1____2
			//  |  /
			//  | /
			// 3|/
			meshUVs[i] = new Vector2(0, 0);
			meshUVs[i+1] = new Vector2(1, 0);
			meshUVs[i+2] = new Vector2(0, 1);

			//    /|2
			//   / |
			//  /  |
			// 1‾‾‾‾3
			meshUVs[i+3] = new Vector2(0, 1);
			meshUVs[i+4] = new Vector2(1, 0);
			meshUVs[i+5] = new Vector2(1, 1);
		}
		
		// Set the UVs we generated into the mesh
		mesh.uv = meshUVs;


		// --------------------------------------------------------------


		// Set up the mesh triangle array
		//
		// "3*" 3 entries that make up a triangle
		// "2*" 2 triangles make up the quad
		int[] meshTriangles = new int[meshVertices.Length];

		for(int vert = 0; vert < meshTriangles.Length; vert++) {
			meshTriangles[vert] = vert;
		}

		// Set the triangles we generated into the mesh
		mesh.triangles = meshTriangles;

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}


	void CombineMeshes(MeshFilter mFilter, params Mesh[] meshs) 
	{
		/* * /
		List<Vector3> finalVerts = new List<Vector3>();
		List<Vector2> finalUVs = new List<Vector2>();
		List<int> finalTriangles = new List<int>();

		for(int meshIndex = 0; meshIndex < meshs.Length; meshIndex++)
		{
			//Debug.Log ("combining");
			//Debug.Log(meshs[meshIndex].vertices.Length);
			finalVerts.AddRange(meshs[meshIndex].vertices);
			finalUVs.AddRange(meshs[meshIndex].uv);
			finalTriangles.AddRange(meshs[meshIndex].triangles);
		}

		Mesh finalMesh = new Mesh();
		finalMesh.vertices = finalVerts.ToArray();
		finalMesh.uv = finalUVs.ToArray();
		finalMesh.triangles = finalTriangles.ToArray();

		Debug.Log("Final Vertice Count: " + finalMesh.vertices.Length);
		Debug.Log("Final UV Count: " + finalMesh.uv.Length);
		Debug.Log("Final Triangle Count: " + finalMesh.triangles.Length);
		/* */

		CombineInstance[] combine = new CombineInstance[meshs.Length];
		for(int meshIndex = 0; meshIndex < meshs.Length; meshIndex++) 
		{
			combine[meshIndex].mesh = meshs[meshIndex];
			combine[meshIndex].transform = Matrix4x4.TRS(gameObject.transform.InverseTransformPoint(gameObject.transform.position), Quaternion.identity, Vector3.one);
		}

		if(mFilter)
		{
			mFilter.mesh.CombineMeshes(combine);

			mFilter.mesh.RecalculateBounds();
			mFilter.mesh.RecalculateNormals();
		}
	}


	void GenerateCube(int segmentsX, int segmentsY, int segmentsZ, float segmentSize)
	{
		//Mesh meshFilterMesh = this.meshFilter.mesh;

		// Generate Cube Loft
		Vector3[] spline = this.GenerateSquareSpline(segmentsX, segmentsZ, segmentSize);
		Mesh loftMesh = this.GenerateRing(spline, segmentsY, segmentsY*segmentSize);
		//Debug.Log(loftMesh.vertices.Length);

		// Generate Bottom Cap
		Mesh bottomCapMesh = this.GenerateSquareCap(segmentsX, segmentsZ, segmentSize, new Vector3(0, 0, 0), Quaternion.Euler(180, 0, 0));
		//Debug.Log(bottomCapMesh.vertices.Length);

		// Generate Top Cap
		Mesh topCapMesh = this.GenerateSquareCap(segmentsX, segmentsZ, segmentSize, new Vector3(0, segmentsY*segmentSize, 0), Quaternion.Euler(0, 0, 0));
		//Debug.Log(topCapMesh.vertices.Length);

		// Combine the meshes
		this.CombineMeshes(this.meshFilter, loftMesh, bottomCapMesh, topCapMesh);

		/* * /
		this.meshFilter.mesh = mesh;
		this.meshFilter.mesh.RecalculateBounds();
		this.meshFilter.mesh.RecalculateNormals();
		/* */

		/* * /
		meshFilterMesh.vertices = mesh.vertices;
		meshFilterMesh.uv = mesh.uv;
		meshFilterMesh.triangles = mesh.triangles;

		Debug.Log("Combined Mesh Vertice Count: " + meshFilterMesh.vertices.Length);
		Debug.Log("Combined Mesh UV Count: " + meshFilterMesh.uv.Length);
		Debug.Log("Combined Mesh Triangle Count: " + meshFilterMesh.triangles.Length);

		meshFilterMesh.RecalculateBounds();
		meshFilterMesh.RecalculateNormals();
		/* */
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
