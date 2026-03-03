using UnityEngine;

namespace loaforcsSoundAPI.Core.Util.Extensions;

public static class AudioSourceExtensions {
    public static void CopyTo(this AudioSource original, AudioSource copy) {
        copy.volume = original.volume;
        copy.pitch = original.pitch;
        // copy.time = original.time;
        // copy.timeSamples = original.timeSamples;
        // copy.clip = original.clip;
        copy.outputAudioMixerGroup = original.outputAudioMixerGroup;
        copy.loop = false;
        copy.ignoreListenerVolume = original.ignoreListenerVolume;
        // copy.playOnAwake = original.playOnAwake;
        copy.ignoreListenerPause = original.ignoreListenerPause;
        copy.velocityUpdateMode = original.velocityUpdateMode;
        copy.panStereo = original.panStereo;
        copy.spatialBlend = original.spatialBlend;
        copy.spatialize = false;
        copy.reverbZoneMix = original.reverbZoneMix;
        copy.bypassEffects = original.bypassEffects;
        copy.bypassListenerEffects = original.bypassListenerEffects;
        copy.dopplerLevel = original.dopplerLevel;
        copy.spread = original.spread;
        copy.priority = original.priority;
        copy.mute = original.mute;
        copy.minDistance = original.minDistance;
        copy.maxDistance = original.maxDistance;
        copy.rolloffMode = original.rolloffMode;
        copy.bypassReverbZones = original.bypassReverbZones;

        copy.SetCustomCurve(AudioSourceCurveType.CustomRolloff, original.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
        copy.SetCustomCurve(AudioSourceCurveType.SpatialBlend, original.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
        copy.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, original.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
        copy.SetCustomCurve(AudioSourceCurveType.Spread, original.GetCustomCurve(AudioSourceCurveType.Spread));
    }

    public static AudioSourceAdditionalData GetAdditionalData(this AudioSource source) {
        return AudioSourceAdditionalData.GetOrCreate(source);
    }
}