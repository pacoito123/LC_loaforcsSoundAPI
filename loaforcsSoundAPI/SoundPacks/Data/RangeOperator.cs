using System;
using loaforcsSoundAPI.Core.Data;

namespace loaforcsSoundAPI.SoundPacks.Data;

public struct RangeOperator<T>(T min, T max) where T : struct, IComparable<T> {
    public T min = min;
    public T max = (min.CompareTo(max) < 0) ? max : min;

    public RangeOperator(T target) : this(target, target) { }

    public readonly bool EvaluateRangeOperator(T value) {
        return min.CompareTo(value) <= 0 && max.CompareTo(value) >= 0;
    }

    public readonly override string ToString() {
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
        result = null;
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