# UnityGrassBrush

## Guide
- Open up Unity scene "MeshTestScene", or open a new scene
- Create a new terrain gameObject, plane gameObject, etc. or anything that has a mesh attached to it
![Imgur](https://imgur.com/zrKVDgj.png)
- Attach the script component "Assets/Scripts/ModelRenderer/ModelRenderer.cs" to the gameObject
![Imgur](https://imgur.com/MSLaaGl.png)
- In the "Terrain Mesh Filter" field, drag the mesh filter component of the gameObject itself here; in the "Model Prefab" field, select a desired model prefab. 
    + You can select an example prefab model from "Assets/StandardAssets/Environment/SpeedTree" folder
- The "Density" field controls the number of model instances we wish to render [1 - 1023]
    + The maximum is 1023 model instances for now, which is the maximum we can render with Graphics.DrawMeshInstanced
- Click the "Play" button

## Notes
- Updating fields will not take effect during runtime for now, will implement this in the future