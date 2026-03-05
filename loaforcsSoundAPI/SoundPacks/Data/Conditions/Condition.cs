using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core;
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
	/// 	The range of values to check.
	/// </summary>
	public RangeOperator<double> Range {
		get => _range;
		private set => _range = value;
	}
	private RangeOperator<double> _range;

	protected static readonly RangeOperator<double> infinityRange = new(double.NegativeInfinity, double.PositiveInfinity);

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

	/// <inheritdoc cref="EvaluateRangeOperator(double)"/>
	protected bool EvaluateRangeOperator(int value) {
		return EvaluateRangeOperator((double)value);
	}

	/// <inheritdoc cref="EvaluateRangeOperator(double)"/>
	protected bool EvaluateRangeOperator(int value, RangeOperator<double> condition) {
		return EvaluateRangeOperator((double)value, condition);
	}

	/// <inheritdoc cref="EvaluateRangeOperator(double)"/>
	protected bool EvaluateRangeOperator(float value) {
		return EvaluateRangeOperator((double)value);
	}

	/// <inheritdoc cref="EvaluateRangeOperator(double)"/>
	protected bool EvaluateRangeOperator(float value, RangeOperator<double> condition) {
		return EvaluateRangeOperator((double)value, condition);
	}

	/// <inheritdoc cref="EvaluateRangeOperator(double,RangeOperator&lt;double&gt;)"/>
	protected bool EvaluateRangeOperator(double value) {
		return EvaluateRangeOperator(value, Range);
	}

	/// <summary>
	/// Evaluates a range operator.
	/// </summary>
	/// <param name="value">The current value.</param>
	/// <param name="condition">The range of values to check.</param>
	/// <returns></returns>
	protected bool EvaluateRangeOperator(double value, RangeOperator<double> condition) {
		return condition.EvaluateRangeOperator(value);
	}

	protected bool ValidateRangeOperator(string condition, out IValidatable.ValidationResult result) {
		return ValidateRangeOperator(condition, out _range, out result);
	}

	protected static bool ValidateRangeOperator(string condition, out RangeOperator<double> range, out IValidatable.ValidationResult result) {
		range = infinityRange;
		result = null!;
		if(string.IsNullOrEmpty(condition)) {
			result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator can not be missing or empty!");
			return false;
		}

		string[] parts = condition.Split("..", StringSplitOptions.None);

		switch(parts.Length) {
			case 1:
				// Case when there's only one number in the condition
				if(double.TryParse(parts[0], out double target))
					range = new(target, target);
				else
					result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[0]}' as a number!");
				break;
			case 2:
				// Case when there's a range specified
				double lowerBound, upperBound;

				if(string.IsNullOrEmpty(parts[0])) {
					lowerBound = double.NegativeInfinity;
				} else {
					if(!double.TryParse(parts[0], out lowerBound)) {
						// Invalid input
						result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[0]}' as a number!");
					}
				}

				if(string.IsNullOrEmpty(parts[1])) {
					upperBound = double.PositiveInfinity;
				} else {
					if(!double.TryParse(parts[1], out upperBound)) {
						// Invalid input
						result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[1]}' as a number!");
					}
				}

				if(result == null)
					range = new(lowerBound, upperBound);
				break;
			case > 2:
				result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator: '{condition}' uses .. more than once!");
				break;
			default:
				break;
		}

		return result == null;
	}

	protected static void LogDebug(string name, object message) {
		Debuggers.ConditionsInfo?.Log($"({name}) {message}");
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
public abstract class Condition<TContext> : Condition where TContext : IContext {

	/// <summary>
	/// Evaluate Condition. If the context type of the parameter does not match this condition, it will evaluate using the fallback.
	/// </summary>
	/// <param name="context">Any possible context</param>
	/// <returns>If condition succeeds</returns>
	public override bool Evaluate(IContext context) {
		if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

		return EvaluateWithContext(type);
	}

	/// <summary>
	/// Context type matches
	/// </summary>
	/// <param name="context">At least the correct context type, but could be any inherited class</param>
	/// <returns>If condition succeeds</returns>
	protected abstract bool EvaluateWithContext(TContext context);

	/// <summary>
	/// Context type did not match
	/// </summary>
	/// <param name="context">Unknown context type</param>
	/// <returns>If condition succeeds</returns>
	protected virtual bool EvaluateFallback(IContext context) {
		return false;
	}
}