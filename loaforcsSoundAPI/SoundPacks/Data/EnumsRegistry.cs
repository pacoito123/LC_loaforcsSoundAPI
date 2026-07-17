using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.SoundPacks.Data;

public class EnumsRegistry<T> : Registry<T, HashSet<T>> where T : struct, Enum {
    public EnumsRegistry() : base() => SceneManager.sceneLoaded += PopulateRegistry;

    /// <inheritdoc/>
    public override void OnRegistryPopulated() => SceneManager.sceneLoaded -= PopulateRegistry;

    /// <inheritdoc/>
    public override bool TryParse(out T value, JToken token) {
        value = default;

        string match = token.ToString();
        if(string.IsNullOrEmpty(match)) return false;

        return Enum.TryParse(match, ignoreCase: true, out value);
    }

    void PopulateRegistry(Scene scene, LoadSceneMode loadSceneMode) => PopulateRegistry();
}