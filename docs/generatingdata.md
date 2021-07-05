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
These practically do the same thing, but use different source files, so as a example we will start with just one LOD level. 

Remove the two that are disabled, and select the remaining active BAG3DObjImportBuildings2.2 object so we can start setting the parameters:

## Terrain



## Trees



