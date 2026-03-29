using System.Collections.Generic;
using JetBrains.Annotations;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

/// <summary>
/// Checks if the provided config option matches the provided value
/// </summary>
/// <soundapi>
///		<type>condition</type>
///		<id>config</id>
/// </soundapi>
[SoundAPICondition("config")]
public sealed class ConfigCondition : Condition {
	/// <summary>
	/// Config name
	/// </summary>
	/// <value><see cref="string"/></value>
	/// <example>Replacements:replace_spider_sounds</example>
	[CanBeNull]
	public string Config { get; private set; } = null!;

	/// <summary>
	/// Value to check against.
	/// </summary>
	/// <value>matches config</value>
	/// <example>true</example>
	/// <default>defaults to `true` if bool, defaults to empty if string</default>
	[CanBeNull]
	public object Value { get; private set; } = null!;

    public bool PreventLoading => _preventLoading ?? (Validate().Count > 0);
    private bool? _preventLoading;

	/// <inheritdoc/>
	public override bool Evaluate(IContext context) => _preventLoading == false;

	/// <inheritdoc/>
	public override List<IValidatable.ValidationResult> Validate() {
		if(!Pack.TryGetConfigValue(Config, out object data))
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Config '{Config}' does not exist on SoundPack '{Pack.Name}'")
			];

		if(Value != null && data.GetType() != Value.GetType())
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Config '{Config}' has a type of: '{data.GetType()}' but the Value type is '{Value.GetType()}'!")
			];

		_preventLoading = data switch {
			bool booleanData => booleanData != (Value == null || (bool)Value),
			string stringData => stringData != ((Value != null) ? (string)Value : string.Empty),
			_ => false,
		};

		return [];
	}
}