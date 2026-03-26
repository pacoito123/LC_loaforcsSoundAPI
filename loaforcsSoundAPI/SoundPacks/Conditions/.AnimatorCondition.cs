using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class AnimatorCondition : Condition {
    public enum AnimatorParamType : byte {
        None,
        Bool,
        Float,
        Integer,
        Trigger
    }

    [CanBeNull]
    public string Parameter { get; private set; } = null!;
    protected internal int _parameterID;

    [CanBeNull]
    public string ParameterType { get; private set; } = null!;
    protected internal AnimatorParamType _parameterType = AnimatorParamType.None;

    [CanBeNull]
    public string Value { get; internal set; } = null!;
    protected internal bool _value;

    public RangeOperator<float> FloatRange {
        get => _floatRange;
        private set => _floatRange = value;
    }
    private RangeOperator<float> _floatRange;

    public RangeOperator<int> IntRange {
        get => _intRange;
        private set => _intRange = value;
    }
    private RangeOperator<int> _intRange;

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        return TryGetAnimator(out Animator animator) && _parameterType switch {
            AnimatorParamType.Bool or AnimatorParamType.Trigger => animator.GetBool(_parameterID) == _value,
            AnimatorParamType.Float => FloatRange.EvaluateRangeOperator(animator.GetFloat(_parameterID)),
            AnimatorParamType.Integer => IntRange.EvaluateRangeOperator(animator.GetInteger(_parameterID)),
            AnimatorParamType.None or _ => false,
        };
    }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        if(!string.IsNullOrEmpty(Parameter)) {
            _parameterID = Animator.StringToHash(Parameter);

            SoundAPIConditionAttribute? attribute = GetType()?.GetCustomAttribute<SoundAPIConditionAttribute>();
            IValidatable.ValidationResult result = new(IValidatable.ResultType.FAIL,
                $"ParameterType field for one \"{attribute?.ID}\" condition in SoundPack '{Pack.Name}' is empty, missing or invalid!");

            if(string.IsNullOrEmpty(ParameterType) || !Enum.TryParse(ParameterType, ignoreCase: true, out _parameterType)) return [result];
            if(!string.IsNullOrEmpty(Value)) {
                switch(_parameterType) {
                    case AnimatorParamType.Bool:
                    case AnimatorParamType.Trigger:
                        if(bool.TryParse(Value, out _value)) return [result];
                        break;
                    case AnimatorParamType.Float:
                        if(!RangeOperator<float>.ValidateRangeOperator(Value, out _floatRange, out result, static (parameter, ref result) =>
                            string.IsNullOrEmpty(parameter) || float.TryParse(parameter, out result))) return [result];
                        break;
                    case AnimatorParamType.Integer:
                        if(!RangeOperator<int>.ValidateRangeOperator(Value, out _intRange, out result, static (parameter, ref result) =>
                            string.IsNullOrEmpty(parameter) || int.TryParse(parameter, out result))) return [result];
                        break;
                    case AnimatorParamType.None:
                    default:
                        break;
                }
            }
        }

        return [];
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"></param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator);
}

public abstract class AnimatorCondition<TContext> : AnimatorCondition, IContextCondition<TContext> where TContext : struct, IContext {
    protected TContext? _currentContext;

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        if(context is not TContext type) return EvaluateFallback(context); // mismatching context, use fallback

        return EvaluateWithContext(type);
    }

    /// <inheritdoc/>
    public virtual bool EvaluateWithContext(TContext context) {
        _currentContext ??= context;

        return base.Evaluate(context);
    }

    /// <inheritdoc/>
    public bool EvaluateFallback(IContext context) {
        return false;
    }

    /// <inheritdoc/>
    protected override bool TryGetAnimator(out Animator animator) {
        animator = null!;
        return _currentContext.HasValue && TryGetAnimator(out animator, _currentContext.Value);
    }

    /// <summary>
    ///     Attempt to obtain the specific <c>Animator</c> instance this <c>Condition</c> should use to evaluate.
    /// </summary>
    /// <param name="animator"><c>Animator</c> instance to use.</param>
    /// <param name="context">Context of type <typeparamref name="TContext"/> to check.</param>
    /// <returns>Whether an <c>Animator</c> was successfully obtained or not.</returns>
    protected abstract bool TryGetAnimator(out Animator animator, TContext context);
}