# 3DAmsterdam

3D Amsterdam is a platform ( available at 3d.amsterdam.nl ) where the city of Amsterdam can be experienced interactively in 3D.

The main goals are:
- providing information about the city;
- making communication and participation more accessible through visuals;
- viewing and sharing 3D models.

More and more information and data is embedded, allowing future features like running simulations, visualization of solar and wind studies and showing impact of spatial urban changes. It will improve public insight in decision making processes.

## Unity 2020.3 (LTS)
The project is using the latest LTS (long-term support) release of Unity: 2020.3.<br/>
We will stick to this version untill new engine feature updates are required for the sake of maximum stability.
## WebGL/Universal Render Pipeline
Currently the main target platform is a WebGL(2.0) application.<br/>
The project is set up to use the [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline), focussing on high performance for lower end machines. Please note that shaders/materials have specific requirements in order to work in this render pipeline.
## Code convention 
All the project code and comments should be written in English. Content text is written in Dutch.<br/>
For C#/.NET coding conventions please refer to the Microsoft conventions:<br/>
[https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)<br/>
For variable naming please refer to:<br/>
[https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)<br/>

## Project Setup 

The files structure of the project looks like this. 

```
3DAmsterdam
├── README.md
├── .gitattributes
├── .gitignore
├── docs
|   ├── gettingstarted.md    #Getting started with a new manucipality
|   ├── assetgeneration.md   #How to generate assets for the 3D layers
|   └── faq.md               #Frequently asked questions
├── 3DAmsterdam              #Main Unity project folder
|   └──Assets
|      ├──3DAmsterdam        #Amsterdam specific config, scenes and assets
|      ├──Netherlands3D      #Shared assets for all manucipalities where we encourage contribution
|      └──WebGLTemplates     #Special Unity folder containing our Netherlands3D fullscreen WebGL template
```

To set-up a project for a new municipality or city, please refer to the [getting started documentation](docs/GettingStarted.md)

After setting up your project you can start generating your own tile-based layer datasets using the [asset generation documentation](docs/GeneratingTileAssets.md)

## Unity Input System

The project uses the new Unity input system called Input Action Assets. The input mappings are defined in 
`\Assets\Netherlands3D\Input\3DNetherlands.inputactions`

This will then generate a c# class that will be used in 
`\Assets\Netherlands3D\Input\ActionHandler.cs`

## Tile System

The platform uses a tile based system consisting of 1km by 1km tiles. The handling of the tiles is initiated by the TileHandler which resides under the /Layers in the scene and needs to be configured with the active Layers that implement the Layer baseclass. 

We currently have 4 implemented Layers which are 

- Buildings
- Trees
- Sewerage
- Terrain

Each Layer script needs implement the HandleLayer method. This function receives a TileChange variable that provides the [RD](https://nl.wikipedia.org/wiki/Rijksdriehoeksco%C3%B6rdinaten)  X and Y coordinate and an action.

The available actions are

- Create (create the tile)
- Upgrade (the Level of detail increases)
- Downgrade (the Level of detail decreases)
- Remove (remove the tile)

## AssetBundles

To improve performance and reduce memory usage the platform uses prebuild assets that are downloaded from the webserver in runtime. 

To create assets the workflow is as follows

- Generate the assets from code using AssetDatabase.CreateAsset. 
  This will create the asset as text based YAML file
- Then build the asset for WebGL using BuildPipeline.BuildAssetBundles. 
  This will create binary files and needs to be copied to the webserver that will serve them.

## Copyright and License
```
Copyright (C) X Gemeente
              X Amsterdam
              X Economic Services Departments

Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License.
You may obtain a copy of the License at:

  https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing
permissions and limitations under the License.
```
## Third parties and licenses

DXF export library - [Lesser General Public Licence](https://github.com/Amsterdam/3DAmsterdam/blob/master/3DAmsterdam/Assets/Netherlands3D/Plugins/netDxf/README.md)<br/>
Sun Script - Credits to [Paul Hayes](https://gist.github.com/paulhayes)<br/>
3D BAG - [CC-BY licentie (TU Delft)](https://docs.3dbag.nl/en/copyright/)<br/>
BruTile (only used for its Extent struct) - [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)<br/>
<br/>
Simple SJON - [MIT License](https://github.com/simplejson/simplejson/blob/master/LICENSE.txt)<br/>
Mesh Simplifier - MIT License<br/>
Clipping Triangles - MIT License<br/>
scripts/extentions/ListExtensions.cs - MIT License<br/>
Scripts/RuntimeHandle derivative work - MIT License<br/>
quantized mesh tiles (no longer used) - MIT License<br/>
JSZip - MIT License
