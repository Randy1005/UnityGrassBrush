using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GridMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private int xSize, zSize;

    private Vector3[] vertices;
    private Mesh mesh;

    private void GenerateMesh() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (zSize + 1)];
		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, 0.0f, z);
			}
		}
		mesh.vertices = vertices;

		int[] triangles = new int[xSize * zSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;

    }

    private void OnDrawGizmos() {
        if (vertices == null)
            return;

        Gizmos.color = Color.black;
        foreach (var vertex in vertices) {
            Gizmos.DrawSphere(vertex, 0.1f);
        }    
    }

    private void Awake() {
        GenerateMesh();
    }
    void Update()
    {
        
    }
}
