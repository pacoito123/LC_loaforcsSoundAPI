/* using HarmonyLib;
using UnityEngine;

namespace loaforcsSoundAPI.Core.Patches;

[HarmonyPatch(typeof(Application))]
static class ApplicationPatch {
    [HarmonyPrefix, HarmonyPatch(nameof(Application.Quit), [])]
    internal static void ApplicationQuitPrefix() {
        foreach(AudioClip clip in SoundAPIAudioManager.loadedClips) {
            Object.Destroy(clip);
        }
    }
} */