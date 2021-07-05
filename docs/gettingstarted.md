# Getting started

After opening the Unity project in the latest Unity 2020.3 LTS we want to create the root folder for our new municipality/city similar to the Assets/3DAmsterdam/ folder. 

In this example we will refer to our new folder as '3DMyManucipality' so we create this new folder in the Assets folder:

Assets/3DMyManucipality/

## Application config file

Now we want to add a config file that will contain our application settings. 

Right click our new 3DMyManucipality folder and select Create>ScriptableObjects>ConfigurationFile. Give the config file a logical name like 'Config3DMyManucipality'.

Now use the inspector to set the properties of our new config file:

| **Bounding Box coordinates**            |                                                              |
| --------------------------------------- | ------------------------------------------------------------ |
| Relative Center RD                      | The RD x and y coordinate that will be the center ( 0,0,0 ) of our main Unity scene |
| Bottom Left RD                          | The bottom-left coordinate of our bounding-box in RD coordinates |
| Top Right RD                            | The top-right coordinate of our bounding-box in RD coordinates |
| Zero Ground Level Y                     | The NAP value that corresponds to the 0 Y value of our Unity scene |
| **Minimap Tiled Web Map**               |                                                              |
| Enable minimap                          | Enable/Disable the map on the bottom-right of the application. <br />If you do not have a tile map service that you can use you can disable it here. |
| Minimap Service URL                     | The url of your tile map service, using {zoom},{x} and {y} as placeholders for their corresponding values. |
| Minimap Tile Numbering Type             | Some map services use a different way of numbering the tiles, choose the one used by your service |
| Minimap Bottom left RD_X                | Bottom-left X coordinate of the bounding box as a RD coordinate |
| Minimap Bottom left RD_Y                | Bottom-left Y coordinate of the bounding box as a RD coordinate |
| Minimap Zoom 0RD Size                   | The size (width and height) in RD units, at zoom level 0     |
| **Tile layers external assets paths**   |                                                              |
| Webserver root path                     | The url to the path where the external assets folders and files are stored |
| Buildings Meta Data Path                | Specific url for the path where the buildings metadata files are stored |
| Sharing Base URL                        | The url for for the service that stores the shared scenes.   |
| Sharing Scene Subdirectory              | The suffix of the url above that the scene json data is posted to |
| Sharing View URL                        | The url used in combination with the unique shared scene ID's to view the scene |
| **External URLs**                       |                                                              |
| Location suggestion URL                 | The service url used for giving suggestions to a user while typing in the search field |
| Lookup URL                              | The service url returning the data for a search result when it is clicked by a user |
| **Sewerage API URLS**                   |                                                              |
| Sewerage API Type                       | Choose PDok by default. Amsterdam uses a specific api that is different to the PDOK service. |
| Sewer Pipes Wfs URL                     | The WFS url to retrieve the sewerpipe features by boundingbox. Use the suggested PDok url by default. |
| Sewer Manholes Wfs URL                  | The WFS url to retrieve the manholes features by boundingbox. Use the suggested PDok url by default. |
| **Bag API URLS**                        |                                                              |
| Bag Api Type                            | The type of BAG service Api used. Choose Kadaster by default. |
| Building URL                            | The service URL for retrieving building data by BAG id       |
| Number Indicator URL                    | The service URL for retrieving addresses tied to a BAG id    |
| Number Indicator Instance URL           | The service URL for retrieving address specific data using a BAG id |
| Monument URL                            | The service URL for retrieving monuments tied to a BAG id (Not used in the application atm.) |
| More Building Info URL                  | The URL used as a hyperlink when more information for a building is requested, using {bagid} as a placeholder for the BAG id |
| More Address Info URL                   | The URL used as a hyperlink when more information for an address is requested, using {bagid} as a placeholder for the BAG id |
| Bag Id Request Service Bounding Box URL | The mapserver API used to retrieve the BAG id's found within a bounding box used for selections. A boundingbox is appended to the url in runtime. |
| Bag Id Request Service Polygon Url      | The mapserver API used to retrieve the BAG id's found within a polygon used for selections, using a hardcoded mapserver Intersect filter. |
|                                         |                                                              |
|                                         |                                                              |
|                                         |                                                              |
|                                         |                                                              |
|                                         |                                                              |
|                                         |                                                              |
|                                         |                                                              |



Next we want to create our unity scenes. The fastest way is to copy the Scenes folder from 3DAmsterdam, so we can change make changes to those scenes.