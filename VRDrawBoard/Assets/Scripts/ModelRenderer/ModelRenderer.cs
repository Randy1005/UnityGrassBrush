using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ModelRenderer : MonoBehaviour
{   
    // ground / terrain mesh to draw models on
    public MeshFilter terrainMeshFilter;

    // prefab model to draw
    public GameObject modelPrefab;

    private List<MeshFilter> m_modelMeshFilters;

    // density of the models (slider)
    [SerializeField, Range(1, 1023)]
    private int m_density;
    public int Density {
        get { return m_density; }
        set { m_density = value; }
    }

    // cache density from last frame
    private int m_lastFrameDensity;

    // positions of the models
    private List<Matrix4x4> m_modelInstanceMatrices;
    // private List<Vector3> m_modelPositions;


    // Start is called before the first frame update
    void Start() {
        m_modelMeshFilters = new List<MeshFilter>();
        m_modelInstanceMatrices = new List<Matrix4x4>();

        m_modelMeshFilters.Add(modelPrefab.GetComponent<MeshFilter>());
        
        // if a prefab is made up of multiple meshes, grab mesh filters from children
        if (m_modelMeshFilters.Any(item => item == null)) {
            m_modelMeshFilters.Clear();
            MeshFilter[] mfInChildren = modelPrefab.GetComponentsInChildren<MeshFilter>();

            for (int i = 0; i < mfInChildren.Length; i++) {
                m_modelMeshFilters.Add(mfInChildren[i]);
            }

        }

        // make sure we now have valid model mesh filters
        Debug.Assert(m_modelMeshFilters.Count != 0);


        Bounds meshBounds = terrainMeshFilter.GetComponent<Renderer>().bounds;
        Vector3 meshCenter = new Vector3(meshBounds.center.x, 0.0f, meshBounds.center.z);
        for (int i = 0; i < Density; i++) {
            Vector3 raycastStartPos = new Vector3(Random.Range(-meshBounds.size.x / 2.0f, meshBounds.size.x / 2.0f), 10000.0f, Random.Range(-meshBounds.size.z / 2.0f, meshBounds.size.z / 2.0f)) + meshCenter;
            RaycastHit hit;
            if (Physics.Raycast(raycastStartPos, Vector3.down, out hit, Mathf.Infinity)) {
                m_modelInstanceMatrices.Add(Matrix4x4.TRS(hit.point, Quaternion.identity, Vector3.one));
            }
        }

        // populate model positions
        // TEST: only one instance now
        // m_modelInstanceMatrices.Add(Matrix4x4.TRS(terrainMesh.bounds.center, Quaternion.identity, Vector3.one));


        Debug.Assert(m_modelInstanceMatrices.Count != 0);

    }

    // Update is called once per frame
    void Update() {
        DrawModels();
    }

    // update model instance transform matrices
    void UpdateModelInstanceMatrices() {

    }

    // draw mesh utility method for model instances
    void DrawModels() {
        foreach (MeshFilter mf in m_modelMeshFilters) {
            // for some reason, meshfilter.gameObject is always inactive
            // if (!mf.gameObject.activeInHierarchy) {
            //     print("not active");
            //     continue;
            // }

            int mfSubMeshCount = mf.sharedMesh.subMeshCount;
            for (int i = 0; i < mfSubMeshCount; i++) {
                Graphics.DrawMeshInstanced(mf.sharedMesh, i, mf.GetComponent<Renderer>().sharedMaterial, m_modelInstanceMatrices);
            }
        }
    }
}
