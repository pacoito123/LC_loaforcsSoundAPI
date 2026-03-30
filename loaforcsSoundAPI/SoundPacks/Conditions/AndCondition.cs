using System;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Checks if all conditions are true.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>and</id>
/// </soundapi>
[SoundAPICondition("and")]
public class AndCondition : LogicGateCondition {
	protected override string ValidateWarnMessage => "'and' condition has no conditions and will always return true!";

	public override bool Evaluate(IContext context) {
		return Array.FindIndex(Conditions, condition => condition is InvalidCondition || !condition.Evaluate(context)) == -1;
	}
}

/// <summary>
/// Checks if all conditions are false.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>nand</id>
/// </soundapi>
[SoundAPICondition("nand")]
public sealed class NandCondition : LogicGateCondition {
	protected override string ValidateWarnMessage => "'nand' condition has no conditions and will always return false!";

	public override bool Evaluate(IContext context) {
		return Array.FindIndex(Conditions, condition => condition is InvalidCondition || !condition.Evaluate(context)) != -1;
	}
}