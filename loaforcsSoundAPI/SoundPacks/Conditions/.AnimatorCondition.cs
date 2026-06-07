using System;
using System.Collections.Generic;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class AnimatorCondition : Condition {
    /// <summary>
    /// Name of the <c>Animator</c> parameter to evaluate.
    /// </summary>
    public string Parameter { get; private set; }
    protected int _parameterID;

    /// <summary>
    /// Type of the <c>Animator</c> parameter to evaluate.
    /// </summary>
    public string ParameterType { get; private set; }
    protected AnimatorParamType _parameterType = AnimatorParamType.None;

    /// <summary>
    /// Value of the <c>Animator</c> parameter to evaluate, of any (valid) type.
    /// </summary>
    public string Value { get; internal set; }
    protected bool _value;

    /// <summary>
    /// Floating value range operator, used if parameter type is <c>AnimatorParamType.Float</c>.
    /// </summary>
    public RangeOperator<float> FloatRange {
        get => _floatRange;
        private set => _floatRange = value;
    }
    RangeOperator<float> _floatRange;

    /// <summary>
    /// Integer value range operator, used if parameter type is <c>AnimatorParamType.Integer</c>.
    /// </summary>
    public RangeOperator<int> IntRange {
        get => _intRange;
        private set => _intRange = value;
    }
    RangeOperator<int> _intRange;

    protected abstract string ValidateWarnMessage { get; }

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        if(!TryGetAnimator(out Animator animator, context)) return false;

        switch(_parameterType) {
            case AnimatorParamType.Bool or AnimatorParamType.Trigger:
                if(animator.GetBool(_parameterID) == _value) return true;
                break;
            case AnimatorParamType.Float:
                if(FloatRange.EvaluateRangeOperator(animator.GetFloat(_parameterID))) return true;
                break;
            case AnimatorParamType.Integer:
                if(IntRange.EvaluateRangeOperator(animator.GetInteger(_parameterID))) return true;
                break;
            case AnimatorParamType.None:
            default:
                break;
        }

        return false;
    }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        IValidatable.ValidationResult result = new(IValidatable.ResultType.FAIL, ValidateWarnMessage);

        _parameterID = Animator.StringToHash(Parameter);
        if(!Enum.TryParse(ParameterType, ignoreCase: true, out _parameterType)) return [result];
        switch(_parameterType) {
            case AnimatorParamType.Bool or AnimatorParamType.Trigger:
                if(!bool.TryParse(Value, out _value)) return [result];
                break;
            case AnimatorParamType.Float:
                if(!RangeOperator<float>.ValidateRangeOperator(Value, out _floatRange, out result, static (parameter, ref result) =>
                    string.IsNullOrEmpty(parameter) || float.TryParse(parameter, out result))) return [result];
                break;
            case AnimatorParamType.Integer:
                if(!RangeOperator<int>.ValidateRangeOperator(Value, out _intRange, out result, static (parameter, ref result) =>
                    string.IsNullOrEmpty(parameter) ||  int.TryParse(parameter, out result))) return [result];
                break;
            case AnimatorParamType.None:
            default:
                break;
        }

        return [];
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"><c>Animator</c> instance to evaluate with.</param>
	/// <param name="context">Any possible context.</param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator, IContext context);
}

public abstract class AnimatorCondition<TContext> : AnimatorCondition, IContextCondition<TContext> where TContext : struct, IContext {
    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

        return EvaluateWithContext(type);
    }

    /// <inheritdoc/>
    public virtual bool EvaluateWithContext(TContext context) {
        return base.Evaluate(context);
    }

    /// <inheritdoc/>
    public virtual bool EvaluateFallback(IContext context) {
        return false;
    }

    /// <inheritdoc/>
    protected override bool TryGetAnimator(out Animator animator, IContext context) {
        animator = null;
        return (context is TContext contextWithType) && TryGetAnimator(out animator, contextWithType);
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"><c>Animator</c> instance to evaluate with.</param>
    /// <param name="context">Context of type <typeparamref name="TContext"/> to check.</param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator, TContext context);
}

public enum AnimatorParamType : byte {
    None,
    Bool,
    Float,
    Integer,
    Trigger
}