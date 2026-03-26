using System.Linq;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.Core;

/// <summary>
/// Contains additional data for a specific audio source.
/// </summary>
public class AudioSourceAdditionalData {
	internal AudioSourceAdditionalData(AudioSource source) {
		Source = source;
	}

	/// <summary>
	/// AudioSource that this AdditonalData is describing.
	/// </summary>
	public AudioSource Source { get; private set; }

	SoundReplacementGroup _replacedWith;

	/// <summary>
	/// AudioClip before replacement, this may differ from <see cref="RealClip"/>.
	/// </summary>
	public AudioClip OriginalClip { get; internal set; }

	/// <summary>
	/// AudioClip that will actually be played by Unity
	/// </summary>
	/// <remarks>
	/// This should be used almost everywhere internally in SoundAPI when updating the AudioClip on an AudioSource.
	/// </remarks>
	public AudioClip? RealClip {
		get {
			using(new SpoofBypassContext()) {
				string realClip = "null";
				if(Source.clip != null) realClip = Source.clip.name;

				string originalClip = "null";
				if(OriginalClip != null) originalClip = OriginalClip.name;
				Debuggers.AudioSourceAdditionalData?.Log($"({Source.name}) Getting real clip: {realClip} (original clip: {originalClip})");

				return Source.clip;
			}
		}
		set {
			using(new SpoofBypassContext()) {
				string originalClip = "null";
				if(OriginalClip != null) originalClip = OriginalClip.name;
				if(value != null)
					Debuggers.AudioSourceAdditionalData?.Log($"({Source.name}) Setting real clip: {value.name} (original clip: {originalClip})");

				Source.clip = value;
			}
		}
	}

	internal SoundReplacementGroup ReplacedWith {
		get => _replacedWith;
		set {
			_replacedWith = value;

			// todo: kind of icky just modifying the list raw
			if(RequiresUpdateFunction()) {
				if(SoundAPIAudioManager.liveAudioSourceData.Contains(this)) return; // dont add to list twice

				SoundAPIAudioManager.liveAudioSourceData.Add(this);
			} else if(SoundAPIAudioManager.liveAudioSourceData.Contains(this)) {
				SoundAPIAudioManager.liveAudioSourceData.Remove(this);
			}
		}
	}

	/// <summary>
	/// Should SoundAPI ignore replacing for this Audio Source?
	/// </summary>
	public bool DisableReplacing { get; private set; }

	/// <summary>
	/// Current Context, may be null.
	/// </summary>
	public IContext CurrentContext { get; set; }

	/// <summary>
	/// Is the current Audio Source part of the pool for OneShots?
	/// </summary>
	public bool IsPooled { get; internal set; }

	internal void Update() {
		if(!RequiresUpdateFunction() || !AudioSourceIsPlaying()) return;

		// Debuggers.UpdateEveryFrame?.Log($"success: updating every frame for {Source.name}");

		CurrentContext ??= new DefaultContext(Source);
		SoundInstance? sound = ReplacedWith?.Sounds?.FirstOrDefault(x => x.Evaluate(CurrentContext));

		if(sound == null) return;
		if(sound.Parent?.Volume.HasValue == true)
			Source.volume = sound.Parent.Volume.Value;

		if(sound.Clip == Source.clip) return;
		Debuggers.UpdateEveryFrame?.Log("new clip found, swapping!!");

		float currentTime = Source.time;
		if(currentTime >= sound.Clip.length) {
			Source.Stop(); // TODO: Condition to remember playback time.
			return;
		}
		Source.clip = sound.Clip;

		Source.Play();
		Source.time = currentTime;

		Debuggers.UpdateEveryFrame?.Log("new clip found, swapped");
	}

	bool RequiresUpdateFunction() {
		return ReplacedWith != null && ReplacedWith.UpdateEveryFrame && !IsPooled;
	}

	bool AudioSourceIsPlaying() {
		return Source != null && Source.enabled && Source.isPlaying;
	}

	public static AudioSourceAdditionalData GetOrCreate(AudioSource source) {
		if(SoundAPIAudioManager.audioSourceData.TryGetValue(source, out AudioSourceAdditionalData sourceData)) return sourceData;

		sourceData = new AudioSourceAdditionalData(source);
		sourceData.OriginalClip = sourceData.RealClip;
		SoundAPIAudioManager.audioSourceData[source] = sourceData;

		return sourceData;
	}
}