using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

/// <summary>
/// Non-generic Condition.
/// </summary>
/// <seealso cref="Condition{ContextType}"/>
/// <seealso cref="IContext"/>
public abstract class Condition : IValidatable {
	[field: NonSerialized]
	public Conditional Parent { get; internal set; } = null!;

	/// <summary>
	/// Utility property to quickly access an instance of a condition's <see cref="SoundPack"/>
	/// </summary>
	protected SoundPack Pack => Parent.Pack;

	/// <summary>
	/// When a condition is explicitly set to 'constant' it will compute the value on load.
	/// The 
	/// todo: For the config condition the Constant value should be implied to be true
	/// </summary>
	public bool? Constant { get; private set; }

	/// <summary>
	///	Called upon the Condition being successfully registered, for any additional initialization.
	/// </summary>
	protected internal virtual void OnRegistered() { }

	/// <summary>
	/// Evaluate Condition
	/// </summary>
	/// <param name="context">Any possible context</param>
	/// <returns>If condition succeeds</returns>
	public abstract bool Evaluate(IContext context);

	/// <inheritdoc/>
	public virtual List<IValidatable.ValidationResult> Validate() {
		return [];
	}
}

sealed class InvalidCondition(string type) : Condition {
	public override bool Evaluate(IContext context) {
		return false;
	}

	public override List<IValidatable.ValidationResult> Validate() {
		if(string.IsNullOrEmpty(type)) {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, "Condition must have a type!")
			];
		} else {
			return [
				new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"'{type}' is not a valid condition type!")
			];
		}
	}
}

sealed class ConstantCondition : Condition {
	public static ConstantCondition TRUE = new(true);
	public static ConstantCondition FALSE = new(false);

	public bool Value { get; private set; }

	ConstantCondition(bool constant) {
		Value = constant;
	}

	public override bool Evaluate(IContext context) {
		return Value;
	}
}

/// <summary>
/// A generic version of Condition to simplify working with Contexts.
/// </summary>
/// <seealso cref="Condition"/>
/// <seealso cref="IContext"/>
/// <typeparam name="TContext">Type of context</typeparam>
public abstract class Condition<TContext> : Condition, IContextCondition<TContext> where TContext : struct, IContext {
	/// <inheritdoc/>
	public override bool Evaluate(IContext context) {
		if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

		return EvaluateWithContext(type);
	}

	/// <inheritdoc/>
	public abstract bool EvaluateWithContext(TContext context);

	/// <inheritdoc/>
	public virtual bool EvaluateFallback(IContext context) {
		return false;
	}
}