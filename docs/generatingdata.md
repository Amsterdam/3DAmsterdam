# Generating data assets

We have set up the Unity Project and our own municipality folder as described in [getting started](gettingstarted.md).
The next step is to set up our data generation scenes that generate the external asset files for the tile-based layers used in our Netherlands3D application:

- Buildings
- Trees
- Terrain

Because our manucipality folder is created as a copy from the 3DAmsterdam folder there are already a few scenes in our <YourManucipality>/Scenes/DataGeneration/ folder. These scenes are made to import different source files and generate unity .asset files when pressing 'Play' in the unity editor.

We will alter these scenes so they generate the files for our new manucipality, starting with the Buildings:

## Buildings

Open up the <YourManucipality>/Scenes/DataGeneration/GenerateBuildingTilesFromOBJ.scene 

You will see that it contains a ApplicationConfiguration object. Make sure the script points to the same config file we created in [getting started](gettingstarted.md), and save the scene.

Next you see that there are 3 objects with a Bag3DObjImporter script on it for different LOD levels. 
These practically do the same thing but use different source files, so as a example we will start with just one LOD level. 

Remove the two that are disabled, and select the remaining active BAG3DObjImportBuildings2.2 object so we can start setting the parameters:

| Property name                             | Property value                                               |
| ----------------------------------------- | ------------------------------------------------------------ |
| Bag 3D Source Files Folder                | Local folder path containing .OBJ files downloaded from www.3dbag.nl, for example "C:\BagObjs\\"  containing all our building models |
| Optional Skip List Path                   | An optional path to a text file, containing a list of BAG id's that you would like to skip importing. Use a newline for every bag ID you would like to skip. |
| Filter                                    | Filter for files to parse in the source files folder. By default this is *.obj, but you can specify it even more if you would like. |
| Exclusively Generate Tiles With Substring | If you fill in something here, for example an RD coordinate seperated by _, then only the tile with that substring will be generated. If you leave it open all tiles within the config boundingbox range are generated. |
| Skip Import of Objects Outside RD Bounds  | Set to True if you want the importer to skip importing objects that have no vertices inside the configurated boundingbox range |
| Lod Level                                 | This string will be used as a suffix for the generated data files. This way you can distinguish between LOD levels in the generated asset files later on |
| Tile Size                                 | The width and height in units of the generated tiles. Set to 1000 meter by default, which is the value used by all layers in 3DAmsterdam |
| Tile Offset                               | The offset of the origin of the generated meshes. 3DAmsterdam assets have the origin in the centre of the 1000x1000 tiles, so this is set to 500,500 by default. |
| Remove Prefix                             | The 3dbag.nl source .obj files contain building objects named by building BAG ID prefixed with 'NL.IMBAG.Pand.' We remove the prefix to retrieve the BAG ID. If your source files do not have this prefix you can leave this blank. |
| Skip Existing Files                       | If you set this to true, any existing tile assets will not be regenerated. |
| Render In Viewport                        | Set this to true if you want to show all parsed building objects in the viewport. (Only recommended for testing purposes. It will slow the editor down a lot, with unique objects for all buildings ) |
| Parse Per Frame                           | Maximum amount of .obj lines to parse per frame so the editor has a frame to show some feedback while generating. |
| Default Material                          | Default material to apply to the generated meshes.           |
| Enable On Finish                          | Optional GameObject to enable when the importer has finished. You can use this to chain multiple importers with different LOD settings. |

Save your scene after applying your settings, and press the unity editor 'Play' button to start the scene, and start importing the .obj files. 

First, the script goes through all the .obj source files and creates GameObjects for every building named by their BAG ID in the scene. This can take a while and will eat up quite a bit of system memory. Look at the console to see what stage the importer is at.

After importing all the .obj files, the script will combine the imported building GameObjects that reside inside the same tile into a single mesh per-tile. These .mesh assets will start appearing in a folder named GeneratedTileAssets. The folder will be auto-generated at that stage.
Metadata with the object seperation are saved next to the mesh data files and have a '-data' suffix.

The unity editor is pretty choked up while generating the tiles but you can use a file explorer to inspect the contents of the /Assets/GeneratedTileAssets/ folder. There the mesh assets will appear one by one.

When the script is done, the generated asset files will need to be turned into AssetBundles.
At the top bar of the Unity editor ( next to File, Edit, etc ) select 'Netherlands 3D>Tools>Export .asset files to AssetBundles'.
This will create the AssetBundles in the folder '/BuildingAssetBundles/', next to the '/Assets/' folder of the Unity project.

Upload the assetbundle files to a unique folder on your webserver (for example https://example.nl/buildings/)

Now open up our project main scene and select the Buildings layer GameObject (Netherlands3D>Layers>Buildings) in the hierarchy. Change the Datasets length to 1 for now (we only generated one LOD level) on the AssetbundleMeshLayer script.
Change the remaining dataset description to a nice description, and change the 'Path' property so it reflect the path to the dataset on your webserver. Use '{x}' and '{y}' as the RD coordinate placeholders in the filename. 
In case of the of the example above, this path would be 'buildings/buildings\_{x}\_{y}.2.2' and our application config file would have https://example.nl/ as a webserver root path.

Press the unity editor 'Play' button to see your building tiles appear in runtime.
If nothing appears, double-check if you set the right paths in the application config file, and the Datasets properties of the Layers>Buildings AssetbundleMeshLayer script.

## Terrain



## Trees



