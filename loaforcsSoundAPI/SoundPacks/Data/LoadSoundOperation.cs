using UnityEngine.Networking;

namespace loaforcsSoundAPI.SoundPacks.Data;

internal sealed class LoadSoundOperation(
	SoundInstance soundInstance,
	UnityWebRequestAsyncOperation webRequest
) {
	public readonly UnityWebRequest WebRequest = webRequest.webRequest;
	public bool IsReady => WebRequest.isDone;
	public bool IsDone { get; set; }
	public readonly SoundInstance Sound = soundInstance;
}