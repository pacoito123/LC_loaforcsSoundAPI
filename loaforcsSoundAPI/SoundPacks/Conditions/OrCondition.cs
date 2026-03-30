using System;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Checks if any conditions are true.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>or</id>
/// </soundapi>
[SoundAPICondition("or")]
public class OrCondition : LogicGateCondition {
	protected override string ValidateWarnMessage => "'or' condition has no conditions and will always return false!";

	public override bool Evaluate(IContext context) {
		return Array.FindIndex(Conditions, condition => condition is not InvalidCondition && condition.Evaluate(context)) != -1;
	}
}

/// <summary>
/// Checks if any conditions are false.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>nor</id>
/// </soundapi>
[SoundAPICondition("nor")]
public sealed class NorCondition : LogicGateCondition {
	protected override string ValidateWarnMessage => "'nor' condition has no conditions and will always return true!";

	public override bool Evaluate(IContext context) {
		return Array.FindIndex(Conditions, condition => condition is not InvalidCondition && condition.Evaluate(context)) == -1;
	}
}