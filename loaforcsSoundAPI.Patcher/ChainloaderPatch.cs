using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace loaforcsSoundAPI.Patcher;

[HarmonyPatch(typeof(Chainloader))]
internal static class ChainloaderPatch {
    [HarmonyPatch(nameof(Chainloader.Initialize)), HarmonyPostfix]
    public static void ChainloaderInitializePost() {
        MethodInfo? start = typeof(Chainloader).GetMethod(nameof(Chainloader.Start), BindingFlags.Static | BindingFlags.Public);
        // MethodInfo? transpiler = typeof(ChainloaderPatch).GetMethod(nameof(ChainloaderStartTranspiler), BindingFlags.Static | BindingFlags.Public);
        MethodInfo? postfix = typeof(LoaforcsSoundAPIPatcher).GetMethod(nameof(LoaforcsSoundAPIPatcher.OnChainloaderFinish), BindingFlags.Static | BindingFlags.NonPublic);
        if(start == null || postfix == null) return;

        // _ = LoaforcsSoundAPIPatcher.Instance.Harmony.Patch(start, transpiler: new(transpiler));
        _ = LoaforcsSoundAPIPatcher.Instance.Harmony.Patch(start, postfix: new(postfix));
    }
}
