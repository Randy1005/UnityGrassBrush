using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // mesh component of the object we attached to
    Mesh mesh;

    // list of grass blade root positions
    List<Vector3> bladeRootPositions;

    // cache the mass center of each triangle
    List<Vector3> bladeRootPositionCenters;
    
    // density of grass blades
    [SerializeField, Range(0, 5)]
    private int density;

    public int Density {
        get { return density; }
        set { density = Mathf.Clamp(value, 0, 5); }
    }



    

    void Start()
    {
        // get mesh component
        mesh = GetComponent<MeshFilter>().mesh;
        Debug.Assert(mesh != null);

        // initialize grass blade positions
        bladeRootPositions = new List<Vector3>();
        bladeRootPositionCenters = new List<Vector3>();

        CalculateBladePositions(density);

        Debug.Log("mass centers: " + bladeRootPositionCenters.Count);
        Debug.Log("all blade positions: " + bladeRootPositions.Count);

        foreach (Vector3 pos in bladeRootPositionCenters) {
            Debug.Log(pos);
        }
        
    }

    void Update()
    {

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        foreach (var pos in bladeRootPositions) {
            Gizmos.DrawSphere(pos, 0.02f);
        }  

        // foreach (var pos in bladeRootPositionCenters) {
        //     Gizmos.DrawSphere(pos, 0.02f);
        // }  
    }

    void CalculateBladePositions(int i_density) {
        bladeRootPositions.Clear();

        // if this is our first time calculating triangle mass centers
        if (bladeRootPositionCenters.Count == 0) {
            // we only need 1 blade root position, which is the mass center of each triangle
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3) {
                // get all indices of vertices of the current triangle
                int triangleIndexA = mesh.triangles[i];
                int triangleIndexB = mesh.triangles[i + 1];
                int triangleIndexC = mesh.triangles[i + 2];

                // calculate mass center of the current triangle
                Vector3 midPointB_C = (mesh.vertices[triangleIndexB] + mesh.vertices[triangleIndexC]) / 2.0f;

                // mass center is on the 2/3 of vector[A -> midpointB_C]
                Vector3 massCenter = mesh.vertices[triangleIndexA] + (0.66f) * (midPointB_C - mesh.vertices[triangleIndexA]);

                // cach mass center of each triangle
                bladeRootPositionCenters.Add(massCenter);
            }
        }

        // if density is 0, we only need the triangle mass centers
        if (i_density == 0) {
            bladeRootPositions = bladeRootPositionCenters;
        }
        else { 
            foreach (Vector3 positions in bladeRootPositionCenters) {
                bladeRootPositions.Add(positions);
            }

            // if density is not 0
            // say density == N, it means from the mass center pointing towards N triangle vertices, we each need one blade inserted between
            for (int i = 0; i < mesh.triangles.Length - 2; i += 3) {
                // get all indices of vertices of the current triangle
                int triangleIndexA = mesh.triangles[i];
                int triangleIndexB = mesh.triangles[i + 1];
                int triangleIndexC = mesh.triangles[i + 2];

                // calculate vector[massCenter -> 3 triangle vertices]
                Vector3 massCenter = bladeRootPositionCenters[i / 3];
                Vector3 centerToA = mesh.vertices[triangleIndexA] - massCenter;
                Vector3 centerToB = mesh.vertices[triangleIndexB] - massCenter;
                Vector3 centerToC = mesh.vertices[triangleIndexC] - massCenter;

                // calculate offset vectors in 3 directions (1 point -> divide by 2 ... etc)
                Vector3 offsetVectorCenterToA = centerToA / (float)(i_density + 1);
                Vector3 offsetVectorCenterToB = centerToB / (float)(i_density + 1);
                Vector3 offsetVectorCenterToC = centerToC / (float)(i_density + 1);

                // calculate the positions where we should insert the blades
                for (int j = 0; j < i_density; j++) {
                    bladeRootPositions.Add(massCenter + (j + 1) * offsetVectorCenterToA);
                    bladeRootPositions.Add(massCenter + (j + 1) * offsetVectorCenterToB);
                    bladeRootPositions.Add(massCenter + (j + 1) * offsetVectorCenterToC);
                }

            }

        }
    }
}
