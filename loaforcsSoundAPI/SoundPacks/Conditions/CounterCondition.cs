using System.Collections.Generic;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Increments a counter by one every time this condition is evaluated.
///
/// For the following example, it will trigger once every 5 times.
/// Be careful when using with `and`, `nand`, `or` or `nor` as these have performance optimizations that may skip increasing the counter in some cases.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>counter</id>
/// </soundapi>
[SoundAPICondition("counter")]
public class CounterCondition : RangeCondition<int> {
	private static readonly Dictionary<AudioSource, int> localCounters = [];

	protected override RangeOperator<int> DefaultRange => new(int.MinValue, int.MaxValue);

	/// <summary>
	/// Resets after reaching this number. Inclusive.
	/// </summary>
	/// <value><see cref="int"/></value>
	/// <example>5</example>
	public int? ResetsAt { get; private set; }

	public bool? IsLocal { get; internal set; }

	private int _count;

	/// <inheritdoc/>
	protected internal override void OnRegistered() {
		SceneManager.sceneUnloaded += ClearDestroyed;
	}

	private void ClearDestroyed(Scene scene) {
		int destroyedSources = 0;
		foreach(AudioSource key in localCounters.Keys) {
			if(key == null && localCounters.Remove(key!)) {
				destroyedSources++;
			}
		}
		if(destroyedSources > 0) {
			LogDebug("counter", $"Removed {destroyedSources} destroyed AudioSources.");
		}
	}

	public override bool Evaluate(IContext context) {
		if(IsLocal.GetValueOrDefault() && context.Source != null) {
			_ = localCounters.TryGetValue(context.Source, out int count);
			bool result = IncreaseCounter(ref count);
			localCounters[context.Source] = count;
			return result;
		}
		return IncreaseCounter(ref _count);
	}

	private bool IncreaseCounter(ref int count) {
		LogDebug("counter", $"counting: {count} -> {count + 1}, local: {IsLocal.GetValueOrDefault()}");
		count++;
		bool result = EvaluateRangeOperator(count);
		LogDebug("counter", $"is {count} in range ({Value})? {result}");
		if(ResetsAt.HasValue && count >= ResetsAt.Value) {
			count = 0;
			LogDebug("counter", $"reset count to 0.");
		}
		return result;
	}

	protected override bool TryParseValue(string parameter, ref int value) {
		return string.IsNullOrEmpty(parameter) || int.TryParse(parameter, out value);
	}

	private static void LogDebug(string name, object message) {
		Debuggers.ConditionsInfo?.Log($"({name}) {message}");
	}
}