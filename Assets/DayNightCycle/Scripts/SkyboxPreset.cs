using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skybox;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SunPreset
{
    [SerializeField] public float SunIntensity;
    [SerializeField] public Color SunColor;
}

[System.Serializable]
public class FogPreset
{
    [SerializeField] public float FogStart, FogEnd;
    [SerializeField] public Color FogColor;
}

[System.Serializable]
public class SidedSkyboxPreset
{
    [SerializeField] public Texture FrontTex, BackTex, LeftTex, RightTex, UpTex, DownTex;
}

[System.Serializable]
[CreateAssetMenu(fileName = "SkyboxPreset", menuName = "Preset/SkyboxPreset")]
public class SkyboxPreset : ScriptableObject
{
    [SerializeField] public SidedSkyboxPreset SidedSkyboxPreset = null;
    [SerializeField] public FogPreset FogPreset = null;
    [SerializeField] public SunPreset SunPreset = null;

    [Range(0f, 8f)] [SerializeField] public float LightningIntensityMultiplier = 1.0f;
    [Range(0f, 1f)] [SerializeField] public float ReflectionsIntensityMultiplier = 1.0f;

    public void LoadCurrentSettings()
    {
        try
        {
            this.LightningIntensityMultiplier = RenderSettings.ambientIntensity;
            this.ReflectionsIntensityMultiplier = RenderSettings.reflectionIntensity;

            if (RenderSettings.fogMode == FogMode.Linear)
            {
                FogPreset fogPreset = new FogPreset();
                fogPreset.FogStart = RenderSettings.fogStartDistance;
                fogPreset.FogEnd = RenderSettings.fogEndDistance;
                fogPreset.FogColor = RenderSettings.fogColor;
                this.FogPreset = fogPreset;
            }

            GameObject directionalLightGo = GameObject.Find("Directional Light");
            if (directionalLightGo != null)
            {
                Light directionalLight = directionalLightGo.GetComponent<Light>();
                if (directionalLight != null && !directionalLight.useColorTemperature)
                {
                    SunPreset sunPreset = new SunPreset();
                    sunPreset.SunIntensity = directionalLight.intensity;
                    sunPreset.SunColor = directionalLight.color;
                    this.SunPreset = sunPreset;
                }
            }

            Material mat = RenderSettings.skybox;
            SidedSkyboxPreset sidedSkyboxPreset = new SidedSkyboxPreset();
            sidedSkyboxPreset.FrontTex = mat.GetTexture("_FrontTex");
            sidedSkyboxPreset.BackTex = mat.GetTexture("_BackTex");
            sidedSkyboxPreset.LeftTex = mat.GetTexture("_LeftTex");
            sidedSkyboxPreset.RightTex = mat.GetTexture("_RightTex");
            sidedSkyboxPreset.UpTex = mat.GetTexture("_UpTex");
            sidedSkyboxPreset.DownTex = mat.GetTexture("_DownTex");
            this.SidedSkyboxPreset = sidedSkyboxPreset;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}