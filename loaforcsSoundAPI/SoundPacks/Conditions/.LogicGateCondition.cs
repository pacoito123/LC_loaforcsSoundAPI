using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class LogicGateCondition : Condition {
	public Condition[]? Conditions { get; protected set; }

	protected abstract string ValidateWarnMessage { get; }

	protected internal override void OnRegistered() {
		for(int i = 0; i < Conditions?.Length; i++) {
			Condition? condition = Conditions[i];
			if(condition != null) {
				condition.Parent = Parent;
				condition.OnRegistered();
			}
		}
	}

	public override List<IValidatable.ValidationResult> Validate() {
		if(Conditions == null || Conditions.Length == 0)
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.WARN, ValidateWarnMessage)
			];

		return [];
	}
}