using com.github.zehsteam.ImmersiveEntrance.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;

public class WeatherAudioManager : MonoBehaviour
{
    public static WeatherAudioManager Instance { get; private set; }

    private readonly Dictionary<WeatherEffect, List<AudioSource>> _weatherAudioSources = [];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        UpdateWeatherAudio();
    }

    private void UpdateWeatherAudio()
    {
        if (TimeOfDay.Instance == null)
            return;

        foreach (var effect in TimeOfDay.Instance.effects)
        {
            UpdateWeatherAudioSources(effect);
        }
    }

    private void UpdateWeatherAudioSources(WeatherEffect weatherEffect)
    {
        if (weatherEffect == null)
            return;

        bool isInsideInterior = PlayerUtils.IsLocalPlayerCameraInsideInterior();

        foreach (var audioSource in GetAudioSourcesForWeather(weatherEffect))
        {
            audioSource.mute = isInsideInterior;
        }
    }

    private List<AudioSource> GetAudioSourcesForWeather(WeatherEffect weatherEffect)
    {
        if (weatherEffect == null)
            return [];

        if (_weatherAudioSources.TryGetValue(weatherEffect, out List<AudioSource> result))
        {
            return result;
        }

        GameObject effectObject = weatherEffect.effectObject;
        if (effectObject == null) return [];

        List<AudioSource> audioSources = [.. effectObject.GetComponentsInChildren<AudioSource>(includeInactive: true)];
        _weatherAudioSources[weatherEffect] = audioSources;
        return audioSources;
    }
}
