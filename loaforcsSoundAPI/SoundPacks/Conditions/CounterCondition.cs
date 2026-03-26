using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

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
	protected override RangeOperator<int> DefaultRange => new(int.MinValue, int.MaxValue);

	/// <summary>
	/// Resets after reaching this number. Inclusive.
	/// </summary>
	/// <value><see cref="int"/></value>
	/// <example>5</example>
	public int? ResetsAt { get; private set; }

	private int _count;

	public override bool Evaluate(IContext context) {
		LogDebug("counter", $"counting: {_count} -> {_count + 1}");
		_count++;
		bool result = EvaluateRangeOperator(_count);
		LogDebug("counter", $"is {_count} in range ({Value})? {result}");
		if(ResetsAt.HasValue && _count >= ResetsAt.Value) {
			_count = 0;
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