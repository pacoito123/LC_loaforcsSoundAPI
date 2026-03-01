using HarmonyLib;
using loaforcsSoundAPI.Core.Util.Extensions;
using loaforcsSoundAPI.SoundPacks;
using loaforcsSoundAPI.SoundPacks.Data;
using UnityEngine;

namespace loaforcsSoundAPI.Core.Patches;

[HarmonyPatch(typeof(AudioSource))]
static class AudioSourcePatch {
	internal static bool bypassSpoofing;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AudioSource.Play), [])]
	[HarmonyPatch(nameof(AudioSource.Play), [typeof(ulong)])]
	[HarmonyPatch(nameof(AudioSource.Play), [typeof(double)])]
	static bool Play(AudioSource __instance) {
		AudioSourceAdditionalData data = AudioSourceAdditionalData.GetOrCreate(__instance);

		if(SoundReplacementHandler.TryReplaceAudio(__instance, data.OriginalClip, out SoundReplacementGroup _, out AudioClip replacement)) {
			if(replacement == null) return false;
			data.RealClip = replacement;
		}

		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AudioSource.PlayOneShot), [typeof(AudioClip), typeof(float)])]
	static bool PlayOneShot(AudioSource __instance, ref AudioClip clip) {
		if(SoundReplacementHandler.TryReplaceAudio(__instance, clip, out SoundReplacementGroup _, out AudioClip replacement, isOneShot: true)) {
			if(replacement == null) return false;
			clip = replacement;
		}

		return true;
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Setter)]
	[HarmonyPriority(Priority.Last)]
	[HarmonyPrefix]
	static void UpdateOriginalClip(AudioSource __instance, AudioClip value, bool __runOriginal) {
		if(!__runOriginal) return;

		AudioSourceAdditionalData data = __instance.GetAdditionalData();
		data.OriginalClip = value;
		Debuggers.AudioClipSpoofing?.Log($"({__instance.gameObject.name}) updating original clip to: {value.name}");
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Setter)]
	[HarmonyPrefix]
	static bool PreventClipRestartingWithSpoofed(AudioSource __instance, AudioClip value) {
		if(!PatchConfig.AudioClipSpoofing || bypassSpoofing) return true;

		/*
		 * Sometimes a game/mod (like REPO) will update AudioSource.clip frequently.
		 * In cases where SoundAPI will replace this clip it will cause the audio to consistently restart.
		 *
		 * This preforms the intended behaviour from the game/mod creator pov where the audio does not restart if they think they are setting it to the same thing
		 */
		AudioSourceAdditionalData data = __instance.GetAdditionalData();
		if(data.OriginalClip == value) Debuggers.AudioClipSpoofing?.Log("prevented clip from restarting");
		return data.OriginalClip != value;
	}

	[HarmonyPatch(nameof(AudioSource.clip), MethodType.Getter)]
	[HarmonyPostfix]
	static void SpoofAudioSourceClip(AudioSource __instance, ref AudioClip __result) {
		if(!PatchConfig.AudioClipSpoofing || bypassSpoofing) return;

		AudioSourceAdditionalData data = __instance.GetAdditionalData();
		__result = data.OriginalClip;
		Debuggers.AudioClipSpoofing?.Log($"({__instance.gameObject.name}) spoofing result to {((data.OriginalClip != null) ? data.OriginalClip.name : "null")}");
	}
}