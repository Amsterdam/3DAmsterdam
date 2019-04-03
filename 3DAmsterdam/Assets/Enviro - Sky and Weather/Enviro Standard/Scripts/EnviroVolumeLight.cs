//  Copyright(c) 2016, Michal Skalsky, (Modified for use in Enviro - Sky and Weather)
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

[AddComponentMenu("Enviro/Volume Light")]
[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class EnviroVolumeLight : MonoBehaviour 
{
	public event Action<EnviroSkyRendering, EnviroVolumeLight, CommandBuffer, Matrix4x4> CustomRenderEvent;

    private Light _light;
	public Material _material;
	public Shader volumeLightShader;
	public Shader volumeLightBlurShader;
    private CommandBuffer _commandBuffer;
    private CommandBuffer _cascadeShadowCommandBuffer;
	public RenderTexture temp;
	//[Header("Quality")]
	[Range(1, 64)]
    public int SampleCount = 8;
	//[Header("Light Settings")]
	public bool scaleWithTime = true;
    [Range(0.0f, 1.0f)]
    public float ScatteringCoef = 0.5f;
    [Range(0.0f, 0.1f)]
    public float ExtinctionCoef = 0.01f;
	[Range(0.0f, 0.999f)]
	public float Anistropy = 0.1f;

	[Header("3D Noise")]
    public bool Noise = false;
	[HideInInspector]public float NoiseScale = 0.015f;
	[HideInInspector]public float NoiseIntensity = 1.0f;
	[HideInInspector]public float NoiseIntensityOffset = 0.3f;
	[HideInInspector]public Vector2 NoiseVelocity = new Vector2(3.0f, 3.0f);

    public Light Light { get { return _light; } }
    public Material VolumetricMaterial { get { return _material; } }


    private bool _reversedZ = false;

    /// <summary>
    /// 
    /// </summary>
    void Start() 
    {

#if UNITY_5_5_OR_NEWER
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStation4 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan || SystemInfo.graphicsDeviceType == GraphicsDeviceType.XboxOne)
        {
            _reversedZ = true;
        }
#endif

#if UNITY_2017_3_OR_NEWER
        //
#else
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D9)
        {
            this.enabled = false;
            return;
        }
#endif



	}

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (EnviroSkyMgr.instance == null)
        {
            this.enabled = false;
            return;
        }
         
#if ENVIRO_LW
        if (EnviroSkyMgr.instance.currentEnviroSkyVersion == EnviroSkyMgr.EnviroSkyVersion.LW)
        {
            this.enabled = false;
            return;
        }
#endif

#if ENVIRO_PRO
        if (EnviroSkyMgr.instance.currentRenderPipeline != EnviroSkyMgr.EnviroRenderPipeline.Legacy)
        {
            this.enabled = false;
            return;
        }
#endif


        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Light Command Buffer";

        _cascadeShadowCommandBuffer = new CommandBuffer();
        _cascadeShadowCommandBuffer.name = "Dir Light Command Buffer";
        _cascadeShadowCommandBuffer.SetGlobalTexture("_CascadeShadowMapTexture", new UnityEngine.Rendering.RenderTargetIdentifier(UnityEngine.Rendering.BuiltinRenderTextureType.CurrentActive));

        _light = GetComponent<Light>();

        if (_light.type == LightType.Directional)
        {
            _light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
            _light.AddCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
        }
        else
        {
            _light.AddCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
        }

        if (volumeLightShader == null)
            volumeLightShader = Shader.Find("Enviro/VolumeLight");

        if (volumeLightShader == null)
            throw new Exception("Critical Error: \"Enviro/VolumeLight\" shader is missing.");

        if (_light.type != LightType.Directional)
        {
            _material = new Material(volumeLightShader);
        }

        if (GetComponent<Light>() != null && GetComponent<Light>().type != LightType.Directional)
            EnviroSkyRendering.PreRenderEvent += VolumetricLightRenderer_PreRenderEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (_light != null && _commandBuffer != null)
        {
            if (_light.type == LightType.Directional)
            {
                _light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
                _light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
            }
            else
            {
                _light.RemoveCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);
            }
        }

        if (GetComponent<Light>() != null && GetComponent<Light>().type != LightType.Directional)
            EnviroSkyRendering.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnDestroy()
    {        
        DestroyImmediate(_material);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="viewProj"></param>
	private void VolumetricLightRenderer_PreRenderEvent(EnviroSkyRendering renderer, Matrix4x4 viewProj, Matrix4x4 viewProjSP)
    {
        if (EnviroSky.instance == null)
            return;

        // light was destroyed without deregistring, deregister now
        if (_light == null || _light.gameObject == null)
        {
            EnviroSkyRendering.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
        }

        if (!_light.gameObject.activeInHierarchy || _light.enabled == false)
            return;

        _material.SetVector("_CameraForward", Camera.current.transform.forward);
        _material.SetInt("_SampleCount", SampleCount);
		_material.SetVector("_NoiseVelocity", new Vector4(EnviroSky.instance.volumeLightSettings.noiseVelocity.x, EnviroSky.instance.volumeLightSettings.noiseVelocity.y) *  EnviroSky.instance.volumeLightSettings.noiseScale);
		_material.SetVector("_NoiseData", new Vector4(EnviroSky.instance.volumeLightSettings.noiseScale, EnviroSky.instance.volumeLightSettings.noiseIntensity, EnviroSky.instance.volumeLightSettings.noiseIntensityOffset));
		_material.SetVector("_MieG", new Vector4(1 - (Anistropy * Anistropy), 1 + (Anistropy * Anistropy), 2 * Anistropy, 1.0f / (4.0f * Mathf.PI)));
		float scatter = ScatteringCoef;
		if (scaleWithTime)
			scatter = ScatteringCoef * (1 - EnviroSky.instance.GameTime.solarTime);
		_material.SetVector ("_VolumetricLight", new Vector4 (scatter, ExtinctionCoef, _light.range, 1.0f));// - SkyboxExtinctionCoef));
        _material.SetTexture("_CameraDepthTexture", renderer.GetVolumeLightDepthBuffer());      
        _material.SetFloat("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);            

        if(_light.type == LightType.Point)
        {
			SetupPointLight(renderer, viewProj, viewProjSP);
        }
        else if(_light.type == LightType.Spot)
        {
			SetupSpotLight(renderer, viewProj, viewProjSP);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="viewProj"></param>
	private void SetupPointLight(EnviroSkyRendering renderer, Matrix4x4 viewProj,Matrix4x4 viewProjSP)
    {
        _commandBuffer.Clear();

        int pass = 0;
        if (!IsCameraInPointLightBounds())
            pass = 2;

        _material.SetPass(pass);

		Mesh mesh = EnviroSkyRendering.GetPointLightMesh();
        
        float scale = _light.range * 2.0f;
        Matrix4x4 world = Matrix4x4.TRS(transform.position, _light.transform.rotation, new Vector3(scale, scale, scale));

        _material.SetMatrix("_WorldViewProj", viewProj * world);
		_material.SetMatrix("_WorldViewProj_SP", viewProjSP * world);

        if (Noise)
            _material.EnableKeyword("NOISE");
        else
            _material.DisableKeyword("NOISE");

        _material.SetVector("_LightPos", new Vector4(_light.transform.position.x, _light.transform.position.y, _light.transform.position.z, 1.0f / (_light.range * _light.range)));
        _material.SetColor("_LightColor", _light.color * _light.intensity);

        if (_light.cookie == null)
        {
            _material.EnableKeyword("POINT");
            _material.DisableKeyword("POINT_COOKIE");
        }
        else
        {
            Matrix4x4 view = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;
            _material.SetMatrix("_MyLightMatrix0", view);

            _material.EnableKeyword("POINT_COOKIE");
            _material.DisableKeyword("POINT");
            
            _material.SetTexture("_LightTexture0", _light.cookie);
        }

        bool forceShadowsOff = false;
        if ((_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).magnitude >= QualitySettings.shadowDistance)
            forceShadowsOff = true;

        if (_light.shadows != LightShadows.None && forceShadowsOff == false)
        {
			if (UnityEngine.XR.XRSettings.enabled) 
			{
				if (EnviroSky.instance.singlePassVR) 
				{
					_material.EnableKeyword ("SHADOWS_CUBE");
					_commandBuffer.SetGlobalTexture ("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
					_commandBuffer.SetRenderTarget (renderer.GetVolumeLightBuffer ());
					_commandBuffer.DrawMesh (mesh, world, _material, 0, pass);

					if (CustomRenderEvent != null)
						CustomRenderEvent (renderer, this, _commandBuffer, viewProj);
					
				} else {	   
					/* _commandBuffer.SetShadowSamplingMode (BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.CompareDepths);
						_commandBuffer.Blit (BuiltinRenderTextureType.CurrentActive,new RenderTargetIdentifier(temp));
						renderer._afterLightPass.Clear ();
						renderer._afterLightPass.SetGlobalTexture ("_ShadowMapTexture", temp);
						_material.SetTexture ("_ShadowMapTexture", temp);
						renderer._afterLightPass.SetRenderTarget (renderer.GetVolumeLightBuffer ());
						renderer._afterLightPass.DrawMesh (mesh, world, _material, 0, pass);

					if (CustomRenderEvent != null)
						CustomRenderEvent (renderer, this, _commandBuffer, viewProj);*/

					_material.DisableKeyword ("SHADOWS_CUBE");
					renderer.GlobalCommandBuffer.DrawMesh (mesh, world, _material, 0, pass);

					if (CustomRenderEvent != null)
						CustomRenderEvent (renderer, this, renderer.GlobalCommandBuffer, viewProj);
				}
			}
            else
            {
				_material.EnableKeyword ("SHADOWS_CUBE");
				_commandBuffer.SetGlobalTexture ("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
				_commandBuffer.SetRenderTarget (renderer.GetVolumeLightBuffer ());
				_commandBuffer.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, _commandBuffer, viewProj);
			}
        }
		else 
        {
            _material.DisableKeyword("SHADOWS_DEPTH");

			if (EnviroSky.instance.PlayerCamera.actualRenderingPath == RenderingPath.Forward) {
				//renderer.GlobalCommandBufferForward.Clear ();
				renderer.GlobalCommandBufferForward.SetRenderTarget (renderer.GetVolumeLightBuffer ());
				renderer.GlobalCommandBufferForward.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, renderer.GlobalCommandBufferForward, viewProj);
			} else {
				renderer.GlobalCommandBuffer.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, renderer.GlobalCommandBuffer, viewProj);
			}
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="viewProj"></param>
	private void SetupSpotLight(EnviroSkyRendering renderer, Matrix4x4 viewProj,Matrix4x4 viewProjSP)
    {
        _commandBuffer.Clear();

        int pass = 1;
        if (!IsCameraInSpotLightBounds())
        {
            pass = 3;     
        }

		Mesh mesh = EnviroSkyRendering.GetSpotLightMesh();
                
        float scale = _light.range;
        float angleScale = Mathf.Tan((_light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad) * _light.range;

        Matrix4x4 world = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(angleScale, angleScale, scale));

        Matrix4x4 view = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;

        Matrix4x4 clip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3(-0.5f, -0.5f, 1.0f));
        Matrix4x4 proj = Matrix4x4.Perspective(_light.spotAngle, 1, 0, 1);

        _material.SetMatrix("_MyLightMatrix0", clip * proj * view);

        _material.SetMatrix("_WorldViewProj", viewProj * world);
		_material.SetMatrix("_WorldViewProj_SP", viewProjSP * world);

        _material.SetVector("_LightPos", new Vector4(_light.transform.position.x, _light.transform.position.y, _light.transform.position.z, 1.0f / (_light.range * _light.range)));
        _material.SetVector("_LightColor", _light.color * _light.intensity);


        Vector3 apex = transform.position;
        Vector3 axis = transform.forward;
        // plane equation ax + by + cz + d = 0; precompute d here to lighten the shader
        Vector3 center = apex + axis * _light.range;
        float d = -Vector3.Dot(center, axis);

        // update material
        _material.SetFloat("_PlaneD", d);        
        _material.SetFloat("_CosAngle", Mathf.Cos((_light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad));

        _material.SetVector("_ConeApex", new Vector4(apex.x, apex.y, apex.z));
        _material.SetVector("_ConeAxis", new Vector4(axis.x, axis.y, axis.z));

        _material.EnableKeyword("SPOT");

        if (Noise)
            _material.EnableKeyword("NOISE");
        else
            _material.DisableKeyword("NOISE");

        if (_light.cookie == null)
        {
			_material.SetTexture("_LightTexture0", EnviroSkyRendering.GetDefaultSpotCookie());
        }
        else
        {
            _material.SetTexture("_LightTexture0", _light.cookie);
        }

        bool forceShadowsOff = false;
        if ((_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).magnitude >= QualitySettings.shadowDistance)
            forceShadowsOff = true;

        if (_light.shadows != LightShadows.None && forceShadowsOff == false)
        {
			if (UnityEngine.XR.XRSettings.enabled) {
				if (EnviroSky.instance.singlePassVR) {
					clip = Matrix4x4.TRS (new Vector3 (0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3 (0.5f, 0.5f, 0.5f));

					if (_reversedZ)
						proj = Matrix4x4.Perspective (_light.spotAngle, 1, _light.range, _light.shadowNearPlane);
					else
						proj = Matrix4x4.Perspective (_light.spotAngle, 1, _light.shadowNearPlane, _light.range);

					Matrix4x4 m = clip * proj;
					m [0, 2] *= -1;
					m [1, 2] *= -1;
					m [2, 2] *= -1;
					m [3, 2] *= -1;

					//view = _light.transform.worldToLocalMatrix;
					_material.SetMatrix ("_MyWorld2Shadow", m * view);
					_material.SetMatrix ("_WorldView", m * view);

					_material.EnableKeyword ("SHADOWS_DEPTH");
					_commandBuffer.SetGlobalTexture ("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
					_commandBuffer.SetRenderTarget (renderer.GetVolumeLightBuffer ());

					_commandBuffer.DrawMesh (mesh, world, _material, 0, pass);

					if (CustomRenderEvent != null)
						CustomRenderEvent (renderer, this, _commandBuffer, viewProj);   
				} else {
					_material.DisableKeyword ("SHADOWS_DEPTH");
					renderer.GlobalCommandBuffer.DrawMesh (mesh, world, _material, 0, pass);

					if (CustomRenderEvent != null)
						CustomRenderEvent (renderer, this, renderer.GlobalCommandBuffer, viewProj);
				}
			} else {
				clip = Matrix4x4.TRS (new Vector3 (0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3 (0.5f, 0.5f, 0.5f));

				if (_reversedZ)
					proj = Matrix4x4.Perspective (_light.spotAngle, 1, _light.range, _light.shadowNearPlane);
				else
					proj = Matrix4x4.Perspective (_light.spotAngle, 1, _light.shadowNearPlane, _light.range);

				Matrix4x4 m = clip * proj;
				m [0, 2] *= -1;
				m [1, 2] *= -1;
				m [2, 2] *= -1;
				m [3, 2] *= -1;

				//view = _light.transform.worldToLocalMatrix;
				_material.SetMatrix ("_MyWorld2Shadow", m * view);
				_material.SetMatrix ("_WorldView", m * view);

				_material.EnableKeyword ("SHADOWS_DEPTH");
				_commandBuffer.SetGlobalTexture ("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
				_commandBuffer.SetRenderTarget (renderer.GetVolumeLightBuffer ());

				_commandBuffer.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, _commandBuffer, viewProj);   

			}
        }
        else
        {
            _material.DisableKeyword("SHADOWS_DEPTH");
			if (EnviroSky.instance.PlayerCamera.actualRenderingPath == RenderingPath.Forward) {
				//renderer.GlobalCommandBufferForward.Clear ();
				renderer.GlobalCommandBufferForward.SetRenderTarget (renderer.GetVolumeLightBuffer ());
				renderer.GlobalCommandBufferForward.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, renderer.GlobalCommandBufferForward, viewProj);
			} else {
				renderer.GlobalCommandBuffer.DrawMesh (mesh, world, _material, 0, pass);

				if (CustomRenderEvent != null)
					CustomRenderEvent (renderer, this, renderer.GlobalCommandBuffer, viewProj);
			}
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="viewProj"></param>
	/*private void SetupDirectionalLight(EnviroVolumetricRenderer renderer, Matrix4x4 viewProj,Matrix4x4 viewProjSP)
    {
        _commandBuffer.Clear();

        int pass = 4;

        _material.SetPass(pass);
        
        if (Noise)
            _material.EnableKeyword("NOISE");
        else
            _material.DisableKeyword("NOISE");

        _material.SetVector("_LightDir", new Vector4(_light.transform.forward.x, _light.transform.forward.y, _light.transform.forward.z, 1.0f / (_light.range * _light.range)));
        _material.SetVector("_LightColor", _light.color * _light.intensity);
        _material.SetFloat("_MaxRayLength", MaxRayLength);

        if (_light.cookie == null)
        {
            _material.EnableKeyword("DIRECTIONAL");
            _material.DisableKeyword("DIRECTIONAL_COOKIE");
        }
        else
        {
            _material.EnableKeyword("DIRECTIONAL_COOKIE");
            _material.DisableKeyword("DIRECTIONAL");

            _material.SetTexture("_LightTexture0", _light.cookie);
        }

		//bottom left
		_frustumCorners[0] = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.farClipPlane));
		// top left
		_frustumCorners[2] = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.farClipPlane));
		// top right
		_frustumCorners[3] = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.farClipPlane));
		// bottom right
		_frustumCorners[1] = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.farClipPlane));


        _material.SetVectorArray("_FrustumCorners", _frustumCorners);
		//_material.SetVectorArray("_FrustumCornersRight", _frustumCornersRight);
		//_material.SetVectorArray("_FrustumCornersLeft", _frustumCornersLeft);

        Texture nullTexture = null;

        if (_light.shadows != LightShadows.None)
        {
            _material.EnableKeyword("SHADOWS_DEPTH");       
			//CustomGraphicsBlit (renderer.GetVolumeLightBuffer(), _material, pass);
			_commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, renderer.GetVolumeLightBuffer());

            if (CustomRenderEvent != null)
                CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
        }
        else
        {
            _material.DisableKeyword("SHADOWS_DEPTH");
			//CustomGraphicsBlit (renderer.GetVolumeLightBuffer(), _material, pass);
			renderer.GlobalCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, renderer.GetVolumeLightBuffer());

            if (CustomRenderEvent != null)
                CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
        }
    }
	*/

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool IsCameraInPointLightBounds()
    {
        float distanceSqr = (_light.transform.position - EnviroSky.instance.PlayerCamera.transform.position).sqrMagnitude;
        float extendedRange = _light.range + 1;
        if (distanceSqr < (extendedRange * extendedRange))
            return true;
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool IsCameraInSpotLightBounds()
    {
        // check range
        float distance = Vector3.Dot(_light.transform.forward, (Camera.current.transform.position - _light.transform.position));
        float extendedRange = _light.range + 1;
        if (distance > (extendedRange))
            return false;

        // check angle
        float cosAngle = Vector3.Dot(transform.forward, (Camera.current.transform.position - _light.transform.position).normalized);
        if((Mathf.Acos(cosAngle) * Mathf.Rad2Deg) > (_light.spotAngle + 3) * 0.5f)
            return false;

        return true;
    }
}
