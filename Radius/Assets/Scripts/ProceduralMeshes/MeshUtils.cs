using UnityEngine;
using System.Collections;

public class MeshUtils : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static Mesh CombineMeshes(Matrix4x4 matrixTransform, params Mesh[] meshs) 
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
			combine[meshIndex].transform = matrixTransform;
		}

		/*
		if(mFilter)
		{
			mFilter.mesh.CombineMeshes(combine);
			
			mFilter.mesh.RecalculateBounds();
			mFilter.mesh.RecalculateNormals();
		}
		*/

		Mesh combinedMesh = new Mesh();
		combinedMesh.CombineMeshes(combine);

		combinedMesh.RecalculateBounds();
		combinedMesh.RecalculateNormals();

		return combinedMesh;
	}

	// Use to generate combine mesh matrix transform.
	// Good for usage with `MeshUtil.CombineMeshes()`
	// Usage:
	//		MeshUtil.CombineMeshes(MeshUtil.GenerateCombineMeshMatrixTransform(gameObject), loftMesh, bottomCapMesh, topCapMesh);
	public static Matrix4x4 GenerateCombineMeshMatrixTransform()
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
	}



	// ------------------------------------------------------------------
	// ------------------------------------------------------------------

	public static Vector3[] GenerateRectFaceTris(float sizeX = 1f, float sizeZ = 1f, Vector3 translate = new Vector3(), Quaternion rotation = new Quaternion()) 
	{
		Vector3[] meshVertices = new Vector3[6];
		
		// 1____2
		//  |  /
		//  | /
		// 3|/
		meshVertices[0] = rotation * (new Vector3(-(sizeX/2), 0f, (sizeZ/2))) + translate;
		meshVertices[1] = rotation * (new Vector3((sizeX/2), 0f, (sizeZ/2))) + translate;
		meshVertices[2] = rotation * (new Vector3(-(sizeX/2), 0f, -(sizeZ/2))) + translate;
		
		
		//    /|2
		//   / |
		//  /  |
		// 1‾‾‾‾3
		meshVertices[3] = rotation * (new Vector3(-(sizeX/2), 0f, -(sizeZ/2))) + translate;
		meshVertices[4] = rotation * (new Vector3((sizeX/2), 0f, (sizeZ/2))) + translate;
		meshVertices[5] = rotation * (new Vector3((sizeX/2), 0f, -(sizeZ/2))) + translate;
		
		return meshVertices;
	}

	public static Vector2[] GenerateOverlappingUVArrayForTris(int vertLength)
	{
		if(vertLength%6 != 0)
			Debug.LogError("GenerateOverlappingUVArray(int vertLength): Please pass in a multiple of 6. Can only generate UVs for groups of rect tri faces.");
		
		Vector2[] meshUVs = new Vector2[vertLength];

		// In case we don't want to go to the edges
		float xMax = 1f;
		float yMax = 1f;

		// Each ring plane is drawn over top of each other
		// So that the material doesn't have to tile
		for(int i = 0; i < meshUVs.Length; i+=6)
		{
			// (0, 1) 1____2 (1, 1)
			//         |  /
			//         | /
			// (0, 0) 3|/
			meshUVs[i] = new Vector2(0, yMax);
			meshUVs[i+1] = new Vector2(xMax, yMax);
			meshUVs[i+2] = new Vector2(0, 0);
			
			//           /|2 (1, 1)
			//          / |
			//         /  |
			// (0, 0) 1‾‾‾‾3 (1, 0)
			meshUVs[i+3] = new Vector2(0, 0);
			meshUVs[i+4] = new Vector2(xMax, yMax);
			meshUVs[i+5] = new Vector2(xMax, 0);
		}
		
		return meshUVs;
	}

	public static Vector2[] GenerateNonOverlappingUVArrayForTris(int vertLength, int numRectX, int numRectY)
	{
		// TODO: finish non-overlapping uvs

		if(vertLength%6 != 0)
			Debug.LogError("GenerateOverlappingUVArray(int vertLength): Please pass in a multiple of 6. Can only generate UVs for groups of rect tri faces.");
		
		Vector2[] meshUVs = new Vector2[vertLength];
		
		// In case we don't want to go to the edges
		float xMax = 1f;
		float yMax = 1f;

		// (0, 1) _______ (1, 1)
		//        |     |
		//        |     |
		//        |     | 
		// (0, 0) ‾‾‾‾‾‾‾ (1, 0)

		// Each ring plane is drawn over top of each other
		// So that the material doesn't have to tile
		for(int i = 0; i < meshUVs.Length; i+=6)
		{
			//float xOffset = (i%(6*numRectX);
			//Debug.Log(xOffset);

			// (0, 1) 1____2 (1, 1)
			//         |  /
			//         | /
			// (0, 0) 3|/
			meshUVs[i] = new Vector2(0, yMax);
			meshUVs[i+1] = new Vector2(xMax, yMax);
			meshUVs[i+2] = new Vector2(0, 0);
			
			//           /|2 (1, 1)
			//          / |
			//         /  |
			// (0, 0) 1‾‾‾‾3 (1, 0)
			meshUVs[i+3] = new Vector2(0, 0);
			meshUVs[i+4] = new Vector2(xMax, yMax);
			meshUVs[i+5] = new Vector2(xMax, 0);
		}
		
		return meshUVs;
	}
	
	
	// This creates the int[] for a bunch of verts already ordered as tri's
	public static int[] GenerateTriArrayForTris(int vertLength)
	{
		int[] meshTriangles = new int[vertLength];
		
		for(int vertIndex = 0; vertIndex < meshTriangles.Length; vertIndex++) {
			meshTriangles[vertIndex] = vertIndex;
		}
		
		return meshTriangles;
	}




	// ------------------------------------------------------------------
	// ------------------------------------------------------------------

	public static Mesh GenerateLoftNurb(Vector3[] spline, int heightSegments, float height)
	{
		Mesh mesh = new Mesh();
		mesh.Clear();

		if(spline.Length > 0 && heightSegments > 0 && height > 0)
		{
			// Set up the mesh vert array
			Vector3[] meshVertices = new Vector3[6*spline.Length*heightSegments];

			int verticeIndex = 0;
			for(int hSeg = 0; hSeg < heightSegments; hSeg++)
			{
				float posY = hSeg*(height/heightSegments) + ((height/heightSegments)/2);

				// Now we make them to the current height level
				for(int vert = 0; vert < spline.Length; vert++)
				{
					float sizeX = Vector3.Distance(spline[vert], spline[(vert+1)%spline.Length]);

					// Find midpoint in the x
					Vector3 posXZ = Vector3.Lerp(spline[vert], spline[(vert+1)%spline.Length], 0.5f);

					Vector3 angleVector = spline[(vert+1)%spline.Length] - spline[vert];
					float angle = -1f*Mathf.Rad2Deg*Mathf.Atan2(angleVector.z, angleVector.x); 

					Vector3[] rectFaceTris = GenerateRectFaceTris(sizeX, height/heightSegments, posXZ + new Vector3(0, posY, 0), Quaternion.Euler(0, angle, 0) * Quaternion.Euler(-90, 0, 0));
				
					System.Array.Copy(
						rectFaceTris, 
						0, 
						meshVertices, 
						verticeIndex, 
						rectFaceTris.Length
					);
					verticeIndex += rectFaceTris.Length;
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
				meshUVs = GenerateOverlappingUVArrayForTris(meshVertices.Length);
			}
			else
			{
				meshUVs = GenerateNonOverlappingUVArrayForTris(meshVertices.Length, spline.Length/6, heightSegments);
			}

			// Set the UVs we generated into the mesh
			mesh.uv = meshUVs;



			// --------------------------------------------------------------

			// Set up the mesh triangle array
			//
			// "3*" 3 entries that make up a triangle
			// "2*" 2 triangles make up the quad
			int[] meshTriangles = GenerateTriArrayForTris(meshVertices.Length);

			// Set the triangles we generated into the mesh
			mesh.triangles = meshTriangles;


		}

		
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		
		return mesh;
	}



}
