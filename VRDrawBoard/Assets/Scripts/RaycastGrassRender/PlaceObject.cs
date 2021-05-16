using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    private void Awake() {
        // get the input event controller instance
        inputEventController = InputEventController.instance;

        // load model prefab
        _coniferPrefab = LoadPrefabFromFile("Conifer_Desktop");
        
        // get the main camera
        _camera = Camera.main;
    }

    private void Update() {
        // In this case, our ray starts from the camera position
        // and shoots toward the camera's forward vector
        // Debug draw ray in scene mode
        Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 10.0f, Color.red);
    }

    private void OnEnable() {
        // clear all events and register events again
        inputEventController.objectPlacementEvent.ClearEvents();
        // inputEventController.objectPlacementEvent.onMouseLeftClick.AddListener(MouseLeftClickEvent);
    }


    /// <summary>
    /// A mouse left click event which can be registered
    /// (Raycast and place objects under ObjectPlacement mode)
    /// </summary>
    public void MouseLeftClickEvent() {
        Debug.Log("Mouse Left Click.");

        RaycastHit hit = new RaycastHit();
        if (GetRaycastHit(_camera.transform.position, _camera.transform.forward, ref hit)) {
            SpawnOnSurface(_coniferPrefab, hit.point, hit.normal);
        } else {
            Debug.Log("no raycast hit.");
        }

    }


    /// <summary>
    /// For the first collider a ray intersects with, get the raycast hit object
    /// </summary>
    /// <param name="i_raySourcePosition"></param>
    /// <param name="i_rayDirection"></param>
    /// <param name="o_raycastHit"></param>
    /// <returns>Returns if a raycast hit was detected or not</returns>
    bool GetRaycastHit(Vector3 i_raySourcePosition, Vector3 i_rayDirection, ref RaycastHit o_raycastHit) {
        RaycastHit raycastHit;
        if (Physics.Raycast(i_raySourcePosition, i_rayDirection, out raycastHit, 100.0f)) {
            o_raycastHit = raycastHit;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Instantiate a gameObject on a mesh surface, with the up vector aligned with surface normal.
    /// </summary>
    /// <param name="i_originalObject">The gameObject prefab to clone from</param>
    /// <param name="i_basePosition">GameObject root position</param>
    /// <param name="i_surfaceNormal">The normal we get on the mesh surface</param>
    void SpawnOnSurface(GameObject i_originalObject, Vector3 i_basePosition, Vector3 i_surfaceNormal) {
        Quaternion objectRotation = Quaternion.FromToRotation(Vector3.up, i_surfaceNormal);
        Instantiate(i_originalObject, i_basePosition, objectRotation);
    }

    /// <summary>
    /// Utility function to load gameObject from file.
    /// </summary>
    /// <param name="i_filename">file path (excluding "Resources")</param>
    /// <returns>Returns gameObject instance</returns>
    private GameObject LoadPrefabFromFile(string i_filename) {
        Debug.Log("Trying to load prefab from file (Resouces/" + i_filename + ")...");
        var loadedObject = Resources.Load<GameObject>(i_filename);
        if (loadedObject == null) {
            throw new FileNotFoundException("no file found - please check the configuration");
        }
        return loadedObject;
    }


    InputEventController inputEventController;
    private GameObject _coniferPrefab;
    private Camera _camera;

}
