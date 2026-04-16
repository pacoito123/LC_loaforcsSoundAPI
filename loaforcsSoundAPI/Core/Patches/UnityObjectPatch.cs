using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using loaforcsSoundAPI.SoundPacks;
using UnityEngine;

namespace loaforcsSoundAPI.Core.Patches;

internal static class UnityObjectPatch {
	private static readonly List<AudioSource> sourcesInLastObject = [];

	private static void InstantiatePatch(UnityEngine.Object __result) {
		if(__result is not Component component) return;
		component.gameObject.GetComponentsInChildren(includeInactive: true, sourcesInLastObject);
		for(int i = 0; i < sourcesInLastObject.Count; i++)
			SoundReplacementHandler.CheckAudioSource(sourcesInLastObject[i]);
	}

	internal static void Init(Harmony harmony) {
		HarmonyMethod postfixPatch = new(typeof(UnityObjectPatch).GetMethod(nameof(InstantiatePatch), BindingFlags.Static | BindingFlags.NonPublic));
		foreach(MethodInfo method in typeof(UnityEngine.Object).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
			if((method.MethodImplementationFlags & MethodImplAttributes.InternalCall) == 0 || method.ReturnType != typeof(UnityEngine.Object)) continue;

			if(method.Name.Contains("Instantiate", StringComparison.Ordinal) || method.Name.Contains("Clone", StringComparison.Ordinal)) {
				Debuggers.AudioSourceAdditionalData?.Log($"patching {method}");
				_ = harmony.Patch(method, postfix: postfixPatch);
			}
		}
	}
}