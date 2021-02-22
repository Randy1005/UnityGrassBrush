using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGrassRenderer : MonoBehaviour
{
    
    InputEventController inputEventController;
    Camera camera;

    public Mesh mesh;
    public Material material;
    private List<Matrix4x4> matrices;
    public int meshNumber;


    private void Awake()
    {
        inputEventController = InputEventController.instance;
        camera = Camera.main;

        matrices = new List<Matrix4x4>(meshNumber);
    }

    private void OnEnable()
    {
        // clear all events and register events again
        inputEventController.objectPlacementEvent.ClearEvents();
        inputEventController.objectPlacementEvent.onMouseLeftClick.AddListener(MouseLeftClickEvent);
    }


    private void Update()
    {

        if (matrices != null)
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices);
    }


    /// <summary>
    /// For the first collider a ray intersects with, get the raycast hit object
    /// </summary>
    /// <param name="i_raySourcePosition"></param>
    /// <param name="i_rayDirection"></param>
    /// <param name="o_raycastHit"></param>
    /// <returns>Returns if a raycast hit was detected or not</returns>
    bool GetRaycastHit(Vector3 i_raySourcePosition, Vector3 i_rayDirection, ref RaycastHit o_raycastHit)
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(i_raySourcePosition, i_rayDirection, out raycastHit, 100.0f))
        {
            o_raycastHit = raycastHit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// A mouse left click event which can be registered
    /// (Raycast and place objects under ObjectPlacement mode)
    /// </summary>
    public void MouseLeftClickEvent()
    {
        Debug.Log("Left Mouse clicked.");
        RaycastHit hit = new RaycastHit();
        if (GetRaycastHit(camera.transform.position, camera.transform.forward, ref hit))
        {
            matrices.Add(Matrix4x4.TRS(hit.point, Quaternion.Euler(0f, 0f, 0f), Vector3.one));
        }
        else
        {
            Debug.Log("no raycast hit.");
        }
    }



}
