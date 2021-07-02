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
