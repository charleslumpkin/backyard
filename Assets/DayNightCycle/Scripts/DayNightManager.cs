using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Skybox
{
    public class DayNightManager : MonoBehaviour
    {
        private static DayNightManager _instance = null;
        public static DayNightManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<DayNightManager>();

                return _instance;
            }
        }
        [SerializeField] private SkyboxPreset[] skyboxPreset;
        private Dictionary<string, SkyboxPreset> preloadSkyboxPresets = new Dictionary<string, SkyboxPreset>();
        private SkyboxPreset? currentPreset = null, prevPreset = null;
        [SerializeField] private Light directionalLight;
        [SerializeField] private Gradient ambientColor;
        //[SerializeField] private Gradient fogColor;
        [SerializeField] private Gradient directionalColor;
        [SerializeField, Range(0, 24)] private float timeOfDay;
        [SerializeField, Range(0.001f, 3)] public float timeSpeed = 1;
        public Text timeText;
        public Text dayText;
        private SkyType type;
        private int day = 1;
        private bool once = true;
        private Material skyboxMaterial;
        private float currentSkyboxRotation;
        private Coroutine blendEnviromentCoroutine;

        private void Awake()
        {
            skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;

            currentSkyboxRotation = skyboxMaterial.GetFloat("_Rotation");

            foreach (SkyboxPreset preset in skyboxPreset)
                preloadSkyboxPresets.Add(preset.name, preset);

            SkyboxPreset currentPreset = ScriptableObject.CreateInstance<SkyboxPreset>();
            currentPreset.LoadCurrentSettings();
            this.currentPreset = currentPreset;
        }

        private void Update()
        {
            currentSkyboxRotation += Time.deltaTime * timeSpeed / 2f;

            if (currentSkyboxRotation > 360f)
                currentSkyboxRotation -= 360f;

            skyboxMaterial.SetFloat("_Rotation", currentSkyboxRotation);
            timeOfDay += Time.deltaTime * timeSpeed * 0.1f;
            if (timeOfDay < 0.1f && once == false)
            {
                once = true;
                day++;
                dayText.text = "DAY " + day.ToString();
            }
            if (timeOfDay > 1)
            {
                once = false;
            }
            timeOfDay %= 24;
            UpdateLighting(timeOfDay / 24f);
        }
        private void UpdateLighting(float timePercent)
        {
            timeText.text = string.Format("{0:00}:{1:00}", Mathf.Floor(timeOfDay), Mathf.Floor((timeOfDay - Mathf.Floor(timeOfDay)) * 60f));
            if (timeOfDay > 6 && timeOfDay < 14)
            {
                BlendSkybox(SkyType.Day.ToString());
                type = SkyType.Day;
            }
            else if (timeOfDay > 14 && timeOfDay < 18)
            {
                BlendSkybox(SkyType.Mid.ToString());
                type = SkyType.Mid;
            }
            else if (timeOfDay > 18 && timeOfDay < 24)
            {
                BlendSkybox(SkyType.Night.ToString());
                type = SkyType.Night;
            }
            RenderSettings.ambientLight = ambientColor.Evaluate(timePercent);
            //RenderSettings.fogColor = fogColor.Evaluate(timePercent);
            if (directionalLight != null)
            {
                directionalLight.color = directionalColor.Evaluate(timePercent);
                directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, -170, 0));
            }
        }

        private void BlendSkybox(string str)
        {
            if (type.ToString() != str)
                BlendSkybox(str, 20.0f / timeSpeed);
        }
        public bool TryInvertEnviromentPreset(float duration)
        {
            if (prevPreset == null || currentPreset == null)
            {
                Debug.LogWarning("No Preset Loaded!");
                return false;
            }
            BlendEnviroment(prevPreset, duration);
            return true;
        }

        public void BlendSkybox(string key, float duration)
        {
            BlendEnviroment(preloadSkyboxPresets[key], duration);
        }

        public void BlendEnviroment(SkyboxPreset preset, float duration)
        {
            if (blendEnviromentCoroutine != null)
                StopCoroutine(blendEnviromentCoroutine);
            blendEnviromentCoroutine = StartCoroutine(CoBlendEnviroment(preset, duration));
        }

        private IEnumerator CoBlendEnviroment(SkyboxPreset preset, float duration)
        {
            prevPreset = currentPreset;
            currentPreset = preset;

            SkyboxPreset currentState = ScriptableObject.CreateInstance<SkyboxPreset>();
            currentState.LightningIntensityMultiplier = RenderSettings.ambientIntensity;
            currentState.ReflectionsIntensityMultiplier = RenderSettings.reflectionIntensity;
            float currentBlendValue = skyboxMaterial.GetFloat("_Blend");
            Color currentFogColor = RenderSettings.fogColor;
            float currentFogStart = RenderSettings.fogStartDistance;
            float currentFogEnd = RenderSettings.fogEndDistance;

            SetSkyboxTextures(preset.SidedSkyboxPreset);

            float process = 0f;
            while (process < 1f)
            {
                process += Time.deltaTime / duration;

                RenderSettings.ambientIntensity = Mathf.Lerp(currentState.LightningIntensityMultiplier, preset.LightningIntensityMultiplier, process);
                RenderSettings.reflectionIntensity = Mathf.Lerp(currentState.ReflectionsIntensityMultiplier, preset.ReflectionsIntensityMultiplier, process);

                RenderSettings.fogColor = Color.Lerp(currentFogColor, preset.FogPreset.FogColor, process);
                RenderSettings.fogStartDistance = Mathf.Lerp(currentFogStart, preset.FogPreset.FogStart, process);
                RenderSettings.fogEndDistance = Mathf.Lerp(currentFogEnd, preset.FogPreset.FogEnd, process);

                skyboxMaterial.SetFloat("_Blend", Mathf.Lerp(currentBlendValue, 1.0f, process));

                yield return null;
            }

            LoadBlendedTextures(preset.SidedSkyboxPreset);
            skyboxMaterial.SetFloat("_Blend", 0f);
        }

        private void SetSkyboxTextures(SidedSkyboxPreset sidedSkyboxPreset)
        {
            skyboxMaterial.SetTexture("_FrontTex2", sidedSkyboxPreset.FrontTex);
            skyboxMaterial.SetTexture("_BackTex2", sidedSkyboxPreset.BackTex);
            skyboxMaterial.SetTexture("_LeftTex2", sidedSkyboxPreset.LeftTex);
            skyboxMaterial.SetTexture("_RightTex2", sidedSkyboxPreset.RightTex);
            skyboxMaterial.SetTexture("_UpTex2", sidedSkyboxPreset.UpTex);
            skyboxMaterial.SetTexture("_DownTex2", sidedSkyboxPreset.DownTex);
        }

        private void LoadBlendedTextures(SidedSkyboxPreset sidedSkyboxPreset)
        {
            skyboxMaterial.SetTexture("_FrontTex", sidedSkyboxPreset.FrontTex);
            skyboxMaterial.SetTexture("_BackTex", sidedSkyboxPreset.BackTex);
            skyboxMaterial.SetTexture("_LeftTex", sidedSkyboxPreset.LeftTex);
            skyboxMaterial.SetTexture("_RightTex", sidedSkyboxPreset.RightTex);
            skyboxMaterial.SetTexture("_UpTex", sidedSkyboxPreset.UpTex);
            skyboxMaterial.SetTexture("_DownTex", sidedSkyboxPreset.DownTex);
        }
    }
}

public enum SkyType
{
    Day,
    Mid,
    Night,
}