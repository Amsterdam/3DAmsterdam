using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

[CustomEditor(typeof(EnviroSkyMgr))]
public class EnviroSkyMgrEditor : Editor {

	GUIStyle boxStyle;
	GUIStyle wrapStyle;
    GUIStyle headerStyle;
    EnviroSkyMgr myTarget;

    private Color modifiedColor;
    private Color greenColor;

    void OnEnable()
	{
		myTarget = (EnviroSkyMgr)target;
        modifiedColor = Color.red;
        modifiedColor.a = 0.5f;

        greenColor = Color.green;
        greenColor.a = 0.5f;
    }
	
	public override void OnInspectorGUI ()
	{

		myTarget = (EnviroSkyMgr)target;

		if (boxStyle == null) {
			boxStyle = new GUIStyle (GUI.skin.box);
			boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
			boxStyle.fontStyle = FontStyle.Bold;
			boxStyle.alignment = TextAnchor.UpperLeft;
		}

		if (wrapStyle == null)
		{
			wrapStyle = new GUIStyle(GUI.skin.label);
			wrapStyle.fontStyle = FontStyle.Normal;
			wrapStyle.wordWrap = true;
			wrapStyle.alignment = TextAnchor.UpperLeft;
		}

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.wordWrap = true;
            headerStyle.alignment = TextAnchor.UpperLeft;
        }


        GUILayout.BeginVertical("Enviro - Sky Manager 2.1.0", boxStyle);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Welcome to the Enviro Sky Manager! Add Lite and Standard Enviro instances and switch between those. Add third party support components or choose your render-pipeline if you own Enviro Pro.", wrapStyle);
		GUILayout.EndVertical ();

        GUILayout.BeginVertical("", boxStyle);
    
        myTarget.showSetup = EditorGUILayout.BeginToggleGroup(" Setup", myTarget.showSetup);
    
        if (myTarget.showSetup)
        {
            //  
            GUILayout.BeginVertical("General", boxStyle);
            GUILayout.Space(20);
            myTarget.dontDestroy = EditorGUILayout.ToggleLeft("  Don't Destroy On Load", myTarget.dontDestroy);

            GUILayout.EndVertical();

#if ENVIRO_PRO
            GUILayout.BeginVertical("SRP Setup", boxStyle);
            GUILayout.Space(20);
            if (myTarget.currentRenderPipeline == EnviroSkyMgr.EnviroRenderPipeline.HDRP)
                GUILayout.Label("Current RenderPipeline: HDRP", headerStyle);
            else if (myTarget.currentRenderPipeline == EnviroSkyMgr.EnviroRenderPipeline.LWRP)
                GUILayout.Label("Current RenderPipeline: LWRP", headerStyle);
            else
                GUILayout.Label("Current RenderPipeline: Legacy", headerStyle);
            GUILayout.Space(10);
            if (myTarget.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.HDRP)
            if (GUILayout.Button("Activate HDRP Support"))
                {
                    myTarget.ActivateHDRP();
                    AddDefineSymbol("ENVIRO_HDRP");
                    RemoveDefineSymbol("ENVIRO_LWRP");
                }
            if (myTarget.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.LWRP)
                if (GUILayout.Button("Activate LWRP Support"))
                {
                     myTarget.ActivateLWRP();
                     AddDefineSymbol("ENVIRO_LWRP");
                     RemoveDefineSymbol("ENVIRO_HDRP");
                }
            if (myTarget.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.Legacy)
                if (GUILayout.Button("Activate Legacy Support"))
                {
                    myTarget.ActivateLegacyRP();
                    RemoveDefineSymbol("ENVIRO_LWRP");
                    RemoveDefineSymbol("ENVIRO_HDRP");
                }
            GUILayout.EndVertical();
#endif

#if ENVIRO_HD
            GUILayout.BeginVertical("Standard Version", boxStyle);
            GUILayout.Space(20);
            if (myTarget.enviroHDInstance == null)
            {
                if (GUILayout.Button("Create Standard Instance"))
                {
                    myTarget.CreateEnviroHDInstance();
                }
                if (GUILayout.Button("Create Standard VR Instance"))
                {
                    myTarget.CreateEnviroHDVRInstance();
                }
            }
            else
            {
                GUILayout.Label("Current Instance found!", headerStyle);
                GUILayout.Label("Delete " + myTarget.enviroHDInstance.gameObject.name + " if you want to add other prefab!");
            }
            GUILayout.EndVertical();
#endif
#if ENVIRO_LW
            GUILayout.BeginVertical("Lite Version", boxStyle);
            GUILayout.Space(20);
            if (myTarget.enviroLWInstance == null)
            {
                if (GUILayout.Button("Create Lite Instance"))
                {
                    myTarget.CreateEnviroLWInstance();
                }
                if (GUILayout.Button("Create Lite Mobile Instance"))
                {
                    myTarget.CreateEnviroLWMobileInstance();
                }
            }
            else
            {
                GUILayout.Label("Current Instance found!", headerStyle);
                GUILayout.Label("Delete " + myTarget.enviroLWInstance.gameObject.name + " if you want to add other prefab!");
            }
            GUILayout.EndVertical();
#endif
        }
            GUILayout.EndVertical();
       
        EditorGUILayout.EndToggleGroup();
      
        GUILayout.BeginVertical("", boxStyle);
        myTarget.showInstances = EditorGUILayout.BeginToggleGroup(" Instances", myTarget.showInstances);
        if (myTarget.showInstances)
        {
          //  GUILayout.Space(10);
#if ENVIRO_HD
            if (myTarget.enviroHDInstance != null)
            {
                if (myTarget.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.HD)
                    GUI.backgroundColor = modifiedColor;
                else
                {
                    if (myTarget.enviroHDInstance.Player == null || myTarget.enviroHDInstance.PlayerCamera == null)
                        GUI.backgroundColor = modifiedColor;
                    else
                        GUI.backgroundColor = greenColor;
                }

                GUILayout.BeginVertical(myTarget.enviroHDInstance.gameObject.name, boxStyle);
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                if (myTarget.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.HD)
                {
                    if (GUILayout.Button("Activate"))
                    {
                        myTarget.ActivateHDInstance();
                    }
                }
                else if (myTarget.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.HD)
                {
                    if (myTarget.enviroHDInstance.Player == null || myTarget.enviroHDInstance.PlayerCamera == null)
                    {
                        GUILayout.Label("Player and/or camera assignment is missing!");

                        if (GUILayout.Button("Auto Assign"))
                        {
                            myTarget.enviroHDInstance.AssignAndStart(Camera.main.gameObject, Camera.main);
                        }
                    }
                    else
                    {
                        if (Application.isPlaying)
                        {
                            if (!myTarget.enviroHDInstance.started)
                            {
                                if (GUILayout.Button("Play"))
                                {
                                    myTarget.enviroHDInstance.Play(myTarget.enviroHDInstance.GameTime.ProgressTime);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Stop"))
                                {
                                    myTarget.enviroHDInstance.Stop(false, true);
                                }

                            }
                        }

                        if (GUILayout.Button("Deactivate"))
                        {
                            myTarget.DeactivateHDInstance();
                        }
           
                    }
                }

                if (GUILayout.Button("Show"))
                {                  
                    Selection.activeObject = myTarget.enviroHDInstance;
                }

                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Delete Instance?", "Are you sure that you want to delete this instance?", "Delete", "Cancel"))
                        myTarget.DeleteHDInstance();
                }

                GUILayout.EndVertical();
            }
#endif

#if ENVIRO_LW

            if (myTarget.enviroLWInstance != null)
            {
                if (myTarget.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.LW)
                    GUI.backgroundColor = modifiedColor;
                else
                {
                    if (myTarget.enviroLWInstance.Player == null || myTarget.enviroLWInstance.PlayerCamera == null)
                        GUI.backgroundColor = modifiedColor;
                    else
                        GUI.backgroundColor = greenColor;
                }

                GUILayout.BeginVertical(myTarget.enviroLWInstance.gameObject.name, boxStyle);
                GUI.backgroundColor = Color.white;
                GUILayout.Space(20);
                if (myTarget.currentEnviroSkyVersion != EnviroSkyMgr.EnviroSkyVersion.LW)
                {
                    if (GUILayout.Button("Activate"))
                    {
                        myTarget.ActivateLWInstance();
                    }
                }
                else if (myTarget.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.LW)
                {
                    if (myTarget.enviroLWInstance.Player == null || myTarget.enviroLWInstance.PlayerCamera == null)
                    {
                        GUILayout.Label("Player and/or camera assignment is missing!");

                        if (GUILayout.Button("Auto Assign"))
                        {
                            if (Camera.main != null)
                                myTarget.enviroLWInstance.AssignAndStart(Camera.main.gameObject, Camera.main);
                        }
                    }
                    else
                    {
                        if (Application.isPlaying)
                        {
                            if (!myTarget.enviroLWInstance.started)
                            {
                                if (GUILayout.Button("Play"))
                                {
                                    myTarget.enviroLWInstance.Play(myTarget.enviroLWInstance.GameTime.ProgressTime);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("Stop"))
                                {
                                    myTarget.enviroLWInstance.Stop(false, true);
                                }
                            }
                        }
                        if (GUILayout.Button("Deactivate"))
                        {
                            myTarget.DeactivateLWInstance();
                        }

           
                    }
                }
                 
                if (GUILayout.Button("Show"))
                {
             
                    Selection.activeObject = myTarget.enviroLWInstance;
                }

                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Delete Instance?", "Are you sure that you want to delete this instance?", "Delete", "Cancel"))
                        myTarget.DeleteLWInstance();
                }

                GUILayout.EndVertical();
            }
#endif
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndToggleGroup();

        GUILayout.BeginVertical("", boxStyle);
        myTarget.showThirdParty = EditorGUILayout.BeginToggleGroup(" Third Party Support", myTarget.showThirdParty);
       
        if (myTarget.showThirdParty)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("", boxStyle);
            myTarget.showThirdPartyMisc = EditorGUILayout.BeginToggleGroup(" Miscellaneous", myTarget.showThirdPartyMisc);
           
       
            if (myTarget.showThirdPartyMisc)
            {
                //WAPI
                GUILayout.BeginVertical("World Manager API", boxStyle);
                GUILayout.Space(20);
#if WORLDAPI_PRESENT
                if (myTarget.gameObject.GetComponent<EnviroWorldAPIIntegration>() == null)
                {
                    if (GUILayout.Button("Add WAPI Support"))
                    {
                        myTarget.gameObject.AddComponent<EnviroWorldAPIIntegration>();
                    }
                }
                else
                {
                    if (GUILayout.Button("Remove WAPI Support"))
                    {
                        DestroyImmediate(myTarget.gameObject.GetComponent<EnviroWorldAPIIntegration>());
                    }

                }
#else
            EditorGUILayout.LabelField("World Manager API no found!", wrapStyle);
#endif
                GUILayout.EndVertical();

                //Vegetation Studio Pro
                GUILayout.BeginVertical("Vegetation Studio Pro", boxStyle);
                GUILayout.Space(20);
#if VEGETATION_STUDIO_PRO
                if (myTarget.gameObject.GetComponent<EnviroVegetationStudioPro>() == null)
                {
                    if (GUILayout.Button("Add Vegetation Studio Pro Support"))
                    {
                        myTarget.gameObject.AddComponent<EnviroVegetationStudioPro>();
                    }
                }
                else
                {
                    if (GUILayout.Button("Remove Vegetation Studio Pro Support"))
                    {
                        DestroyImmediate(myTarget.gameObject.GetComponent<EnviroVegetationStudioPro>());
                    }

                }
#else
                EditorGUILayout.LabelField("Vegetation Studio Pro not found in project!", wrapStyle);
#endif
                GUILayout.EndVertical();


                //PEGASUS
                GUILayout.BeginVertical("Pegasus", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_PEGASUS_SUPPORT
            EditorGUILayout.LabelField("Pegasus support is activated! Please use the new enviro trigger to drive enviro settings with Pegasus.");
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate Pegasus Support"))
            {
                RemoveDefineSymbol("ENVIRO_PEGASUS_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("Pegasus support not activated! Please activate if you have Pegasus in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate Pegasus Support"))
                {
                    AddDefineSymbol("ENVIRO_PEGASUS_SUPPORT");
                }
                if (GUILayout.Button("Deactivate Pegasus Support"))
                {
                    RemoveDefineSymbol("ENVIRO_PEGASUS_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////


                //FogVolume
                GUILayout.BeginVertical("FogVolume 3", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_FV3_SUPPORT

            if (myTarget.gameObject.GetComponent<EnviroFogVolumeIntegration>() == null)
            {
                if (GUILayout.Button("Add FogVolume Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroFogVolumeIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove FogVolume Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroFogVolumeIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate FogVolume Support"))
            {
                RemoveDefineSymbol("ENVIRO_FV3_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("FogVolume3 support not activated! Please activate if you have FogVolume3 package in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate FogVolume Support"))
                {
                    AddDefineSymbol("ENVIRO_FV3_SUPPORT");
                }
                if (GUILayout.Button("Deactivate FogVolume Support"))
                {
                    RemoveDefineSymbol("ENVIRO_FV3_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

            }
                GUILayout.EndVertical();
                EditorGUILayout.EndToggleGroup();


            GUILayout.BeginVertical("", boxStyle);
            myTarget.showThirdPartyShaders = EditorGUILayout.BeginToggleGroup(" Shaders", myTarget.showThirdPartyShaders);

            if (myTarget.showThirdPartyShaders)
            {

                //CTS
                GUILayout.BeginVertical("Complete Terrain Shader", boxStyle);
                GUILayout.Space(20);
#if CTS_PRESENT
            if(myTarget.gameObject.GetComponent<EnviroCTSIntegration>() == null)
            {     
                if (GUILayout.Button("Add CTS Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroCTSIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove WAPI Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroCTSIntegration>());
                }

            }
#else
                EditorGUILayout.LabelField("CTS not found in project!", wrapStyle);
#endif
                GUILayout.EndVertical();


                //MicroSplat
                GUILayout.BeginVertical("MicroSplat", boxStyle);
                GUILayout.Space(20);

#if ENVIRO_MICROSPLAT_SUPPORT

            if (myTarget.gameObject.GetComponent<EnviroMicroSplatIntegration>() == null)
            {
                if (GUILayout.Button("Add MicroSplat Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroMicroSplatIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove MicroSplat Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroMicroSplatIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate MicroSplat Support"))
            {
                RemoveDefineSymbol("ENVIRO_MICROSPLAT_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("MicroSplat support not activated! Please activate if you have Microsplat in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate MicroSplat Support"))
                {
                    AddDefineSymbol("ENVIRO_MICROSPLAT_SUPPORT");
                }
                if (GUILayout.Button("Deactivate MicroSplat Support"))
                {
                    RemoveDefineSymbol("ENVIRO_MICROSPLAT_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

                //MegaSplat
                GUILayout.BeginVertical("MegaSplat", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_MEGASPLAT_SUPPORT

            if (myTarget.gameObject.GetComponent<EnviroMegaSplatIntegration>() == null)
            {
                if (GUILayout.Button("Add MegaSplat Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroMegaSplatIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove MegaSplat Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroMegaSplatIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate MegaSplat Support"))
            {
                RemoveDefineSymbol("ENVIRO_MEGASPLAT_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("MegaSplat support not activated! Please activate if you have MegaSplat in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate MegaSplat Support"))
                {
                    AddDefineSymbol("ENVIRO_MEGASPLAT_SUPPORT");
                }
                if (GUILayout.Button("Deactivate MegaSplat Support"))
                {
                    RemoveDefineSymbol("ENVIRO_MEGASPLAT_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

                //RTP
                GUILayout.BeginVertical("Relief Terrain Shader", boxStyle);
                GUILayout.Space(20);

#if ENVIRO_RTP_SUPPORT
            if (myTarget.gameObject.GetComponent<EnviroRTPIntegration>() == null)
            {
                if (GUILayout.Button("Add RTP Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroRTPIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove RTP Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroRTPIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate RTP Support"))
            {
                RemoveDefineSymbol("ENVIRO_RTP_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("Relief Terrain Shader support not activated! Please activate if you have Relief Terrain Shader package in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate RTP Support"))
                {
                    AddDefineSymbol("ENVIRO_RTP_SUPPORT");
                }
                if (GUILayout.Button("Deactivate RTP Support"))
                {
                    RemoveDefineSymbol("ENVIRO_RTP_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

                //UBER
                GUILayout.BeginVertical("UBER Shaderframework", boxStyle);
                GUILayout.Space(20);

#if ENVIRO_UBER_SUPPORT
            if (myTarget.gameObject.GetComponent<EnviroRTPIntegration>() == null)
            {
                if (GUILayout.Button("Add UBER Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroRTPIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove UBER Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroRTPIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate UBER Support"))
            {
                RemoveDefineSymbol("ENVIRO_UBER_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("UBER Shader support not activated! Please activate if you have UBER Shader package in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate UBER Support"))
                {
                    AddDefineSymbol("ENVIRO_UBER_SUPPORT");
                }
                if (GUILayout.Button("Deactivate UBER Support"))
                {
                    RemoveDefineSymbol("ENVIRO_UBER_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

                //LUX
                GUILayout.BeginVertical("LUX Shaderframework", boxStyle);
                GUILayout.Space(20);

#if ENVIRO_LUX_SUPPORT
            if (myTarget.gameObject.GetComponent<EnviroLUXIntegration>() == null)
            {
                if (GUILayout.Button("Add LUX Support"))
                {
                    myTarget.gameObject.AddComponent<EnviroLUXIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove LUX Support"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroLUXIntegration>());
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Deactivate LUX Support"))
            {
                RemoveDefineSymbol("ENVIRO_LUX_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("LUX Shader support not activated! Please activate if you have LUX Shader package in your project.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate LUX Support"))
                {
                    AddDefineSymbol("ENVIRO_LUX_SUPPORT");
                }
                if (GUILayout.Button("Deactivate LUX Support"))
                {
                    RemoveDefineSymbol("ENVIRO_LUX_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

        
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndToggleGroup();


            GUILayout.BeginVertical("", boxStyle);
            myTarget.showThirdPartyNetwork = EditorGUILayout.BeginToggleGroup(" Networking", myTarget.showThirdPartyNetwork);

            if (myTarget.showThirdPartyNetwork)
            {

                //UNET
                GUILayout.BeginVertical("UNet Networking", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_UNET_SUPPORT
            EditorGUILayout.LabelField("UNET support is activated! Please also add the EnviroUNetPlayer component to your players!");

            if (myTarget.gameObject.GetComponent<EnviroUNetServer>() == null)
            {
                if (GUILayout.Button("Add UNet Integration Component"))
                {
                    myTarget.gameObject.AddComponent<EnviroUNetServer>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove UNet Integration Component"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroUNetServer>());
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Deactivate UNet Support"))
            {
                RemoveDefineSymbol("ENVIRO_UNET_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("UNet support not activated! Please activate if would like to use UNet with Enviro.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate UNet Support"))
                {
                    AddDefineSymbol("ENVIRO_UNET_SUPPORT");
                }
                if (GUILayout.Button("Deactivate UNet Support"))
                {
                    RemoveDefineSymbol("ENVIRO_UNET_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////



                //Mirror
                GUILayout.BeginVertical("Mirror Networking", boxStyle);
                GUILayout.Space(20);
#if ENVIRO_MIRROR_SUPPORT
            EditorGUILayout.LabelField("Mirror support is activated! Please also add the EnviroMirrorPlayer component to your players!");

            if (myTarget.gameObject.GetComponent<EnviroMirrorServer>() == null)
            {
                if (GUILayout.Button("Add Mirror Integration Component"))
                {
                    myTarget.gameObject.AddComponent<EnviroMirrorServer>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove Mirror Integration Component"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroMirrorServer>());
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Deactivate Mirror Support"))
            {
                RemoveDefineSymbol("ENVIRO_MIRROR_SUPPORT");
            }
#else
                EditorGUILayout.LabelField("Mirror support not activated! Please activate if would like to use UNet with Enviro.");
                GUILayout.Space(10);
                if (GUILayout.Button("Activate Mirror Support"))
                {
                    AddDefineSymbol("ENVIRO_MIRROR_SUPPORT");
                }
                if (GUILayout.Button("Deactivate Mirror Support"))
                {
                    RemoveDefineSymbol("ENVIRO_MIRROR_SUPPORT");
                }
#endif
                GUILayout.EndVertical();
                //////////

                //Photon
                GUILayout.BeginVertical("Photon Networking", boxStyle);
            GUILayout.Space(20);
#if ENVIRO_PHOTON_SUPPORT
            EditorGUILayout.LabelField("Photon PUN 2 support is activated!");

            if (myTarget.gameObject.GetComponent<EnviroPhotonIntegration>() == null)
            {
                if (GUILayout.Button("Add Photon Integration Component"))
                {
                    myTarget.gameObject.AddComponent<EnviroPhotonIntegration>();
                }
            }
            else
            {
                if (GUILayout.Button("Remove Photon Integration Component"))
                {
                    DestroyImmediate(myTarget.gameObject.GetComponent<EnviroPhotonIntegration>());
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Deactivate Photon Support"))
            {
                RemoveDefineSymbol("ENVIRO_PHOTON_SUPPORT");
            }
#else
            EditorGUILayout.LabelField("Photon support not activated! Please activate if you have Photon PUN 2 in your project.");
            GUILayout.Space(10);
            if (GUILayout.Button("Activate Photon Support"))
            {
                AddDefineSymbol("ENVIRO_PHOTON_SUPPORT");
            }
            if (GUILayout.Button("Deactivate Photon Support"))
            {
                RemoveDefineSymbol("ENVIRO_PHOTON_SUPPORT");
            }
#endif
            GUILayout.EndVertical();
                //////////




            }
            GUILayout.EndVertical();
            EditorGUILayout.EndToggleGroup();


        }
        // END THIRDPARTY
        GUILayout.EndVertical();
        EditorGUILayout.EndToggleGroup();

       }

    public void AddDefineSymbol(string symbol)
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.Add(symbol);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
    }

    public void RemoveDefineSymbol(string symbol)
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (symbols.Contains(symbol))
        {
            symbols = symbols.Replace(symbol + "; ", "");
            symbols = symbols.Replace(symbol, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
        }
    }
}
