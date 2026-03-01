using UnityEngine;

namespace loaforcsSoundAPI.Core.Util.Extensions;

public static class AudioSourceExtensions {
    public static AudioSourceAdditionalData GetAdditionalData(this AudioSource source) {
        return AudioSourceAdditionalData.GetOrCreate(source);
    }
}