using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Material))]
public class TestScript : MonoBehaviour
{
    // mesh component of the plane object we attached to
    Mesh planeMesh;

    // prefab of the model we wish to use for gpu instancing (trees, flowers, etc.)
    public GameObject prefabModel;

    // number of model instances
    public int numModelInstances;

    // model mesh instances transform matrices
    List<Matrix4x4> modelInstanceMatrices;

    Mesh modelMesh;
    MeshFilter[] modelMeshFilters;
    Renderer[] modelRenderers;
    Material modelMaterial;

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

    // height map of terrain
    [SerializeField, Range(0, 5)]
    [Tooltip("height of noise sampled terrain")]
    private float terrainHeight;
    public float TerrainHeight {
        get { return terrainHeight; }
        set { terrainHeight = value; }
    }

    // cache density on last frame update
    float previousDensity;

    // cache sampled terrain height on last frame update
    float previousTerrainHeight;

    // plane mesh instance transform matrices
    List<Matrix4x4> planeMeshInstanceMatrices;

    // material to use on mesh
    public Material grassMaterial;

    void Start()
    {
        // get plane mesh component
        planeMesh = GetComponent<MeshFilter>().mesh;
        Debug.Assert(planeMesh != null);
        
        // cache density, terrain height for this frame update
        previousDensity = density;

        // initialize mesh instance transform matrices
        planeMeshInstanceMatrices = new List<Matrix4x4>();
        modelInstanceMatrices = new List<Matrix4x4>();

        // initialize grass density through editing shader
        grassMaterial.SetFloat("_GrassBlades", 2.0f);
        
        // store mesh instance transfrom matrices
        planeMeshInstanceMatrices.Add(Matrix4x4.TRS(planeMesh.bounds.center, Quaternion.identity, Vector3.one));


        Debug.Assert(prefabModel != null);

        // get model mesh filter
        MeshFilter modelMeshFilter = prefabModel.GetComponent<MeshFilter>();
        if (modelMeshFilter) {
            modelMesh = modelMeshFilter.sharedMesh;
            modelMaterial = prefabModel.GetComponent<Renderer>().sharedMaterial;
        }

        
        // if a prefab is made up of multiple meshes
        if (modelMesh == null)
            modelMeshFilters = prefabModel.GetComponentsInChildren<MeshFilter>();
        
        if (modelMaterial == null) {
            modelRenderers = prefabModel.GetComponentsInChildren<Renderer>();
        }



        // add model mesh instance transfrom matrices
        // TODO: randomize positions within plane mesh bounds
        modelInstanceMatrices.Add(Matrix4x4.TRS(planeMesh.bounds.center, Quaternion.identity, Vector3.one * 0.5f));

        
    }

    void Update()
    {
        // update density if user changed during runtime
        if (density != previousDensity) {
            // set shader data from material
            grassMaterial.SetFloat("_GrassBlades", density);
        }

        // update noise sampled terrain height if user changed during runtime
        if (terrainHeight != previousTerrainHeight) {
            // set terrain height data from material
            grassMaterial.SetFloat("_TerrainHeight", terrainHeight);
        }

        // draw plane mesh instances
        if (planeMeshInstanceMatrices != null && planeMeshInstanceMatrices.Count != 0) {
            Graphics.DrawMeshInstanced(planeMesh, 0, grassMaterial, planeMeshInstanceMatrices);
        }


        if (modelInstanceMatrices != null && modelInstanceMatrices.Count != 0) {
            if (modelMesh)
                Graphics.DrawMeshInstanced(modelMesh, 0, modelMaterial, modelInstanceMatrices);
            else {
                for (int i = 0; i < modelMeshFilters.Length; i++) {
                    for (int j = 0; j < modelMeshFilters[i].sharedMesh.subMeshCount; j++) {
                        for (int k = 0; k < modelRenderers[i].sharedMaterials.Length; k++) {
                            if (modelRenderers[i].sharedMaterials[k] != null)
                                Graphics.DrawMeshInstanced(modelMeshFilters[i].sharedMesh, j, modelRenderers[i].sharedMaterials[k], modelInstanceMatrices);
                        }
                    }
                }
            }
        }

       previousDensity = density;
       previousTerrainHeight = terrainHeight;
    }



    // unused, was planning to generate grass vertices with this
    void CalculateBladePositions(int i_density) {
        bladeRootPositions.Clear();

        // if this is our first time calculating triangle mass centers
        if (bladeRootPositionCenters.Count == 0) {
            // we only need 1 blade root position, which is the mass center of each triangle
            for (int i = 0; i < planeMesh.triangles.Length - 2; i += 3) {
                // get all indices of vertices of the current triangle
                int triangleIndexA = planeMesh.triangles[i];
                int triangleIndexB = planeMesh.triangles[i + 1];
                int triangleIndexC = planeMesh.triangles[i + 2];

                // calculate mass center of the current triangle
                Vector3 midPointB_C = (planeMesh.vertices[triangleIndexB] + planeMesh.vertices[triangleIndexC]) / 2.0f;

                // mass center is on the 2/3 of vector[A -> midpointB_C]
                Vector3 massCenter = planeMesh.vertices[triangleIndexA] + (0.66f) * (midPointB_C - planeMesh.vertices[triangleIndexA]);

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
            for (int i = 0; i < planeMesh.triangles.Length - 2; i += 3) {
                // get all indices of vertices of the current triangle
                int triangleIndexA = planeMesh.triangles[i];
                int triangleIndexB = planeMesh.triangles[i + 1];
                int triangleIndexC = planeMesh.triangles[i + 2];

                // calculate vector[massCenter -> 3 triangle vertices]
                Vector3 massCenter = bladeRootPositionCenters[i / 3];
                Vector3 centerToA = planeMesh.vertices[triangleIndexA] - massCenter;
                Vector3 centerToB = planeMesh.vertices[triangleIndexB] - massCenter;
                Vector3 centerToC = planeMesh.vertices[triangleIndexC] - massCenter;

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
