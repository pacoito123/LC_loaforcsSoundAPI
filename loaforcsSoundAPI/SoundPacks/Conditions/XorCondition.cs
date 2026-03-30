using System;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Checks if exactly one condition is true.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>xor</id>
/// </soundapi>
[SoundAPICondition("xor")]
public class XorCondition : LogicGateCondition {
    protected override string ValidateWarnMessage => "'xor' condition has no conditions and will always return false!";

    public override bool Evaluate(IContext context) {
        return Array.FindAll(Conditions, condition => condition is not InvalidCondition && condition.Evaluate(context)).Length == 1;
    }
}

/// <summary>
/// Checks if either all conditions are false, or all conditions are true.
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>xnor</id>
/// </soundapi>
[SoundAPICondition("xnor")]
public sealed class XnorCondition : LogicGateCondition {
    protected override string ValidateWarnMessage => "'xnor' condition has no conditions and will always return true!";

    public override bool Evaluate(IContext context) {
        return Array.FindAll(Conditions, condition => condition is not InvalidCondition && condition.Evaluate(context)).Length != 1;
    }
}