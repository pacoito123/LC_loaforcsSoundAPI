using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class RangeCondition<T> : Condition where T : struct, IComparable<T> {
    /// <summary>
    /// Range of values to check against (as a string to be parsed).
    /// </summary>
    /// <value>ValueRange</value>
    public string Value { get; private set; } = null;

    /// <summary>
    /// Range of values to check against (as a range operator).
    /// </summary>
    /// <value>RangeOperator</value>
    public RangeOperator<T> Range {
        get => _range;
        private set => _range = value;
    }
    RangeOperator<T> _range;

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
        return ValidateRangeOperator(condition, out _range, out result);
    }

    /// <summary>
    /// Validates a range operator's formatting.
    /// </summary>
    /// <param name="condition">Value to attempt to parse into a range operator.</param>
    /// <param name="range">Validated range operator given as an out parameter.</param>
    /// <param name="result">Unsuccessful validations, if any were found.</param>
    /// <returns>Whether the validation succeeded or not.</returns>
    protected bool ValidateRangeOperator(string condition, out RangeOperator<T> range, out IValidatable.ValidationResult result) {
        return RangeOperator<T>.ValidateRangeOperator(condition, out range, out result, TryParseValue, DefaultRange);
    }

    /// <summary>
    /// Attempt to parse a string parameter for the range operator to use for this Condition.
    /// </summary>
    /// <param name="parameter">Individual parameter in the range operator.</param>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c> parsed from the parameter.</param>
    /// <returns>Whether a value of type <c><typeparamref name="T"/></c> was successfully parsed or not.</returns>
    protected abstract bool TryParseValue(string parameter, ref T value);
}

public abstract class RangeCondition<T, TContext> : RangeCondition<T>, IContextCondition<TContext> where T : struct, IComparable<T> where TContext : struct, IContext {
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