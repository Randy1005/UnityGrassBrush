using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Material))]
public class TestScript : MonoBehaviour
{
    // mesh component of the object we attached to
    Mesh mesh;

    // list of grass blade root positions
    List<Vector3> bladeRootPositions;

    // cache the mass center of each triangle
    List<Vector3> bladeRootPositionCenters;
    
    // density of grass blades
    [SerializeField, Range(2, 15)]
    [Tooltip("density of grass blades (density increases starting from mass center of each triangle)")]
    private float density;
    public float Density {
        get { return density; }
        set { density = value; }
    }

    // cache density on last frame update
    float previousDensity;

    // mesh instance transform matrices
    List<Matrix4x4> meshInstanceMatrices;

    private Renderer rend;

    // material to use on mesh
    public Material grassMaterial;

    void Start()
    {
        // get mesh component
        mesh = GetComponent<MeshFilter>().mesh;
        Debug.Assert(mesh != null);

        // initialize grass blade positions
        bladeRootPositions = new List<Vector3>();
        bladeRootPositionCenters = new List<Vector3>();

        // calculate root positions of the grass blades
        // CalculateBladePositions(density);

        // cache density for this frame update
        previousDensity = density;

        // initialize mesh instance transform matrices
        meshInstanceMatrices = new List<Matrix4x4>();

        // initialize grass density through editing shader
        grassMaterial.SetFloat("_GrassBlades", 2.0f);
        
        // store mesh instance transfrom matrices
        meshInstanceMatrices.Add(Matrix4x4.TRS(mesh.bounds.center, Quaternion.identity, Vector3.one));
    }

    void Update()
    {
        // update density if user changed during runtime
        if (density != previousDensity) {
            // CalculateBladePositions(density);

            // set shader data from material
            grassMaterial.SetFloat("_GrassBlades", density);

        }

        // draw mesh instances
        if (meshInstanceMatrices != null && meshInstanceMatrices.Count != 0) {
            Graphics.DrawMeshInstanced(mesh, 0, grassMaterial, meshInstanceMatrices);
        }

       previousDensity = density;
     
    }

    private void OnDrawGizmos() {
        // Gizmos.color = Color.red;

        // foreach (var pos in bladeRootPositions) {
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
            foreach (Vector3 positions in bladeRootPositionCenters.ToArray()) {
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
