using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using loaforcsSoundAPI.Core;
using loaforcsSoundAPI.Core.Data;
using loaforcsSoundAPI.SoundPacks.Data.Conditions;
using UnityEngine.SceneManagement;

namespace loaforcsSoundAPI.SoundPacks.Conditions;

public abstract class MultipleCondition<T> : Condition {
    [CanBeNull]
    public string Value { get; private set; } = null!;

    [field: NonSerialized]
    public T[]? Values { get; private set; }

    [field: NonSerialized]
    public char Separator { get; protected set; } = ',';

    protected internal override void OnRegistered() {
        if(string.IsNullOrEmpty(Value)) return;
        SceneManager.sceneLoaded += PopulateValues;
    }

    /// <inheritdoc/>
    public override bool Evaluate(IContext context) {
        return Values?.Length > 0 && Array.FindIndex(Values, value => CheckValue(value, context)) != -1;
    }

    /// <inheritdoc/>
    public override List<IValidatable.ValidationResult> Validate() {
        if(!string.IsNullOrEmpty(Value)) return [];

        SoundAPIConditionAttribute[] attributes = [.. GetType().GetCustomAttributes<SoundAPIConditionAttribute>()];
        string str = string.Empty;

        if(attributes.Length != 1)
            for(int i = 0; i < attributes.Length; i++) {
                if(i > 0)
                    str += (i != attributes.Length - 1) ? "\", \"" : "\", or \"";
                str += attributes[i].ID;
            }
        else
            str += attributes[0].ID;

        return [new(IValidatable.ResultType.FAIL,
            $"Value field for one \"{str}\" condition in SoundPack '{Pack.Name}' is empty or missing!")];
    }

    private void PopulateValues(Scene scene, LoadSceneMode mode) {
        PopulateValues();
    }

    protected virtual void PopulateValues() {
        string[] matches = Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        HashSet<T> foundValues = new(matches.Length);

        for(int i = 0; i < matches.Length; i++) {
            string match = matches[i].Trim();
            if(TryGetValue(out T value, match)) _ = foundValues.Add(value);
        }

        Values = [.. foundValues];
        OnValuesPopulated();

        if(Values.Length == 0 && Debuggers.SoundReplacementLoader != null) {
            SoundAPIConditionAttribute[] attributes = [.. GetType().GetCustomAttributes<SoundAPIConditionAttribute>()];
            string str = string.Empty;

            if(attributes.Length != 1)
                for(int i = 0; i < attributes.Length; i++) {
                    if(i > 0)
                        str += (i != attributes.Length - 1) ? "\", \"" : "\", or \"";
                    str += attributes[i].ID;
                }
            else
                str += attributes[0].ID;

            Pack.Logger.LogWarning($"[Debug-SoundReplacementLoader] Value field '{Value}' for one \"{str}\" condition in SoundPack '{Pack.Name}' returned no successful matches!");
        }
    }

    protected virtual void OnValuesPopulated() {
        SceneManager.sceneLoaded -= PopulateValues;
    }

    /// <summary>
    ///     Attempt to obtain a value of type <typeparamref name="T"/> from a given match <c>string</c>.
    /// </summary>
    /// <param name="value">Value of type <typeparamref name="T"/> obtained from the match <c>string</c>.</param>
    /// <param name="match">Match <c>string</c> to attempt to obtain a value from.</param>
    /// <returns>Whether a value was successfully found using the given match <c>string</c> or not.</returns>
    protected abstract bool TryGetValue(out T value, string match);

    /// <summary>
    ///     Check if the given value of type <typeparamref name="T"/> is inside the list of accepted values for this <c>Condition</c>.
    /// </summary>
    /// <param name="value">Value of type <typeparamref name="T"/> to check.</param>
    /// <param name="context"></param>
    /// <returns>Whether the given value is present in the list of accepted values or not.</returns>
    protected abstract bool CheckValue(T value, IContext context);
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
    protected override bool CheckValue(T value, IContext context) {
        return (context is TContext contextWithType) && CheckValueWithContext(value, contextWithType);
    }

    /// <summary>
    ///     Check if the given value of type <typeparamref name="T"/> is inside the list of accepted values for this <c>Condition</c>.
    /// </summary>
    /// <param name="value">Value of type <typeparamref name="T"/> to check.</param>
    /// <param name="context">Context of type <typeparamref name="TContext"/> to check.</param>
    /// <returns>Whether the given value is present in the list of accepted values or not.</returns>
    protected abstract bool CheckValueWithContext(T value, TContext context);
}