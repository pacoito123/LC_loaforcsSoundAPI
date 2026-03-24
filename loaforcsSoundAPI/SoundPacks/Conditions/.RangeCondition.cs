using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

[Serializable]
public struct RangeOperator<T>(T min, T max) where T : struct, IComparable<T> {
    public T min = min;
    public T max = (min.CompareTo(max) < 0) ? max : min;

    public RangeOperator(T target) : this(target, target) { }

    public readonly bool EvaluateRangeOperator(T value) {
        return min.CompareTo(value) <= 0 && max.CompareTo(value) >= 0;
    }

    public override readonly string ToString() {
        return $"{min}..{max}";
    }

    /// <summary>
    /// Validates a range operator's formatting.
    /// </summary>
    /// <param name="condition">Value to attempt to parse into a range operator.</param>
    /// <param name="range">Valid range operator to use for this Condition.</param>
    /// <param name="result">Unsuccessful validations, if any were found.</param>
    /// <param name="tryParse">TODO.</param>
    /// <param name="defaultRange">TODO.</param>
    /// <returns>Whether the validation succeeded or not.</returns>
    public static bool ValidateRangeOperator(string condition, out RangeOperator<T> range, out IValidatable.ValidationResult result, ParseAction tryParse, RangeOperator<T> defaultRange = default) {
        range = defaultRange;
        result = null!;
        if(string.IsNullOrEmpty(condition)) {
            result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator can not be missing or empty!");
            return false;
        }

        string[] parts = condition.Split("..", StringSplitOptions.None);

        switch(parts.Length) {
            case 1:
                // Case when there's only one number in the condition.
                T target = range.min;
                if(!tryParse(parts[0], ref target)) {
                    // Invalid input.
                    result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[0]}' as a '{typeof(T).FullName}'!");
                    break;
                }
                range = new(target);
                break;
            case 2:
                // Case when there's a range specified.
                T lowerBound = range.min;
                if(!tryParse(parts[0], ref lowerBound)) {
                    // Invalid input.
                    result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[0]}' as a '{typeof(T).FullName}'!");
                    break;
                }

                T upperBound = range.max;
                if(!tryParse(parts[1], ref upperBound)) {
                    // Invalid input.
                    result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Failed to parse: '{parts[1]}' as a '{typeof(T).FullName}'!");
                    break;
                }

                range = new(lowerBound, upperBound);
                break;
            case > 2:
                result = new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, $"Range operator '{condition}' uses '..' more than once!");
                break;
            default:
                break;
        }

        return result == null;
    }

    public delegate bool ParseAction(string value, ref T result);
}

public abstract class RangeCondition<T> : Condition where T : struct, IComparable<T> {
    /// <summary>
    /// Range of values to check against (as a string to be parsed).
    /// </summary>
    /// <value>ValueRange</value>
    [CanBeNull]
    public string Value { get; private set; } = null!;

    /// <summary>
    /// Range of values to check against (as a range operator).
    /// </summary>
    /// <value>RangeOperator</value>
    public RangeOperator<T> Range {
        get => _range;
        private set => _range = value;
    }
    private RangeOperator<T> _range;

    /// <summary>
    /// Default range of values for this Condition, if a lower or upper bound is not specified.
    /// </summary>
    protected abstract RangeOperator<T> DefaultRange { get; }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        return !ValidateRangeOperator(Value, out IValidatable.ValidationResult result) ? [result] : [];
    }

    /// <inheritdoc cref="EvaluateRangeOperator(T,RangeOperator&lt;T&gt;)"/>
    protected bool EvaluateRangeOperator(T value) {
        return EvaluateRangeOperator(value, Range);
    }

    /// <summary>
    /// Evaluates a range operator.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="condition">The range of values to check.</param>
    /// <returns>Whether the evaluation succeeded or not.</returns>
    protected bool EvaluateRangeOperator(T value, RangeOperator<T> condition) {
        return condition.EvaluateRangeOperator(value);
    }

    /// <summary>
    /// Validates a range operator's formatting.
    /// </summary>
    /// <param name="condition">Value to attempt to parse into a range operator.</param>
    /// <param name="result">Unsuccessful validations, if any were found.</param>
    /// <returns>Whether the validation succeeded or not.</returns>
    protected bool ValidateRangeOperator(string condition, out IValidatable.ValidationResult result) {
        return RangeOperator<T>.ValidateRangeOperator(condition, out _range, out result, TryParseValue, DefaultRange);
    }

    /// <summary>
    /// Attempt to parse a string parameter for the range operator to use for this Condition.
    /// </summary>
    /// <param name="parameter">Individual parameter in the range operator.</param>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c> parsed from the parameter.</param>
    /// <returns>Whether a value of type <c><typeparamref name="T"/></c> was successfully parsed or not.</returns>
    protected abstract bool TryParseValue(string parameter, ref T value);
}

public abstract class RangeCondition<T, TContext> : RangeCondition<T>, IContextCondition<TContext> where T : struct, IComparable<T> where TContext : IContext {
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