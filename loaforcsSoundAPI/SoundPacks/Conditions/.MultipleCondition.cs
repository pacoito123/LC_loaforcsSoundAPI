using System.Collections.Generic;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class MultipleCondition<T> : Condition {
    /// <summary>
    /// Multiple <c>string</c> matches to parse and cache, to later evaluate.
    /// </summary>
    public List<string> Value { get; private set; }

    /// <summary>
    /// Unique values of type <c><typeparamref name="T"/></c> parsed from the defined <c>string</c> matches.
    /// </summary>
    public HashSet<T> _cachedValues { get; } = [];

    protected abstract string ValidateWarnMessage { get; }

    /// <inheritdoc/>
    public override void OnRegistered() {
        SceneManager.sceneLoaded -= PopulateValues;
        SceneManager.sceneLoaded += PopulateValues; // Populate cached values upon loading a scene.
    }

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        return TryObtainValue(out T value, context) && _cachedValues.Contains(value); // Check if obtained value is valid for this Condition.
    }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        if(Value.Count > 0) return [];

        return [new IValidatable.ValidationResult(IValidatable.ResultType.FAIL, ValidateWarnMessage)];
    }

    void PopulateValues(Scene scene, LoadSceneMode mode) => PopulateValues();

    /// <summary>
    /// Populate cached values of type <c><typeparamref name="T"/></c> for this <c>Condition</c>.
    /// </summary>
    protected virtual void PopulateValues() {
        foreach(string match in Value)
            if(TryCacheValue(out T value, match))
                _cachedValues.Add(value);
        if(_cachedValues.Count == 0 && Debuggers.SoundReplacementLoader != null)
            Pack.Logger.LogWarning($"[Warning-SoundReplacementLoader] No matches found for the following values:\n - {string.Join("\n - ", Value)}");
        OnValuesPopulated();
    }

    /// <summary>
    /// Called after cached values of type <c><typeparamref name="T"/></c> finish being populated.
    /// </summary>
    protected virtual void OnValuesPopulated() => SceneManager.sceneLoaded -= PopulateValues;

    /// <summary>
    /// Try to find and cache a value of type <c><typeparamref name="T"/></c> from a given match <c>string</c>.
    /// </summary>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c> corresponding to the match <c>string</c>.</param>
    /// <param name="match">Match <c>string</c> to attempt to obtain a value from.</param>
    /// <returns>Whether a value was successfully found using the given match <c>string</c> or not.</returns>
    protected abstract bool TryCacheValue(out T value, string match);

    /// <summary>
    /// Try to obtain a value of type <c><typeparamref name="T"/></c> to evaluate for this <c>Condition</c>.
    /// </summary>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c> being evaluated.</param>
    /// <param name="context">Any possible context.</param>
    /// <returns>Whether a value to evaluate was obtained or not.</returns>
    protected abstract bool TryObtainValue(out T value, IContext context);
}

public abstract class MultipleCondition<T, TContext> : MultipleCondition<T>, IContextCondition<TContext> where TContext : struct, IContext {
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
    protected override bool TryObtainValue(out T value, IContext context) {
        value = default;
        return (context is TContext contextWithType) && TryObtainValueWithContext(out value, contextWithType);
    }

    /// <summary>
    /// Try to obtain a value of type <c><typeparamref name="T"/></c> to evaluate for this <c>Condition</c>.
    /// </summary>
    /// <param name="value">Value of type <c><typeparamref name="T"/></c> being evaluated.</param>
    /// <param name="context">Context of type <c><typeparamref name="TContext"/></c>.</param>
    /// <returns>Whether a value to evaluate was obtained or not.</returns>
    protected abstract bool TryObtainValueWithContext(out T value, TContext context);
}