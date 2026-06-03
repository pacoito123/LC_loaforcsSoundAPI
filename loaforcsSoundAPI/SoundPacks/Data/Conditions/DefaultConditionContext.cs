using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

struct DefaultConditionContext(AudioSource source) : IContext {
	internal static readonly DefaultConditionContext DEFAULT = new DefaultConditionContext(null);

	public readonly AudioSource Source => source;
}