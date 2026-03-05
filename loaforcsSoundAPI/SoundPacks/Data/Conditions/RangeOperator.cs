using System;

namespace loaforcsSoundAPI.SoundPacks.Data.Conditions;

[Serializable]
public struct RangeOperator<T>(T? min, T? max) where T : IComparable<T> {
    public T? min = min;
    public T? max = (max != null && min?.CompareTo(max) < 0) ? max : min;

    public RangeOperator(T? target) : this(target, target) { }

    public readonly bool EvaluateRangeOperator(T value) {
        return (min == null || value.CompareTo(min) >= 0) && (max == null || value.CompareTo(max) <= 0);
    }

    public override readonly string ToString() {
        return (max != null && min?.CompareTo(max) < 0) ? $"{min}..{max}" : $"{min}";
    }
}