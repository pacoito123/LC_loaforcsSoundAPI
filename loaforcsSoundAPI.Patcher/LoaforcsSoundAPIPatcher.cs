using System;
using System.Collections.Generic;
using HarmonyLib;
using Mono.Cecil;

namespace loaforcsSoundAPI.Patcher;

public class LoaforcsSoundAPIPatcher {
    public static IEnumerable<string> TargetDLLs { get; } = [];

    public static LoaforcsSoundAPIPatcher Instance { get; private set; } = null!;

    public Harmony Harmony { get; private set; } = null!;

    public Action? ChainloaderFinish;

    public static void Initialize() {
        Instance = new();
    }

    public static void Finish() {
        Instance.Harmony = new("loaforcsSoundAPI.Patcher");
        Instance.Harmony.PatchAll(typeof(ChainloaderPatch));
    }

    internal static void OnChainloaderFinish() {
        Instance.ChainloaderFinish?.Invoke();
        Instance.ChainloaderFinish = null;
    }

    public static void Patch(AssemblyDefinition _) { }
}