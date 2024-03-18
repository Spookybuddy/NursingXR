using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// This class exists to facilitate common integer and floating point comparisons
    /// for the DDR system (which is limited to string values).
    /// 
    /// This uses string values and parsing with long and double. As such it should
    /// work for all primitive integer and floating point types.
    /// 
    /// Does not support ulong.
    /// </summary>
    public class NumericComparer
    {
        private ComparerNumericType defaultType;
        private NumericComparison comparison;

        public NumericComparer(ComparerNumericType defaultType, NumericComparison comparison)
        {
            this.defaultType = defaultType;
            this.comparison = comparison;
        }

        public bool Compare(string value1, string value2)
        {
            return Compare(ParseAsDefault(value1), ParseAsDefault(value2));
        }

        public bool Compare(long value1, string value2)
        {
            return Compare(value1, ParseAs(value2, ComparerNumericType.Integer));
        }

        public bool Compare(string value1, long value2)
        {
            return Compare(ParseAs(value1, ComparerNumericType.Integer), value2);
        }

        public bool Compare(double value1, string value2)
        {
            return Compare(value1, ParseAs(value2, ComparerNumericType.FloatingPoint));
        }

        public bool Compare(string value1, double value2)
        {
            return Compare(ParseAs(value1, ComparerNumericType.FloatingPoint), value2);
        }

        public bool Compare(IComparable v1, IComparable v2)
        {
            return Compare(v1, v2, comparison, defaultType);
        }

        public static bool Compare(IComparable v1, IComparable v2, NumericComparison comparison, ComparerNumericType defaultType)
        {
            Type t1 = null;
            Type t2 = null;

            // upgrade to long/double
            v1 = EnsureType(v1, ref t1, defaultType);
            v2 = EnsureType(v2, ref t2, defaultType);

            // if one is long and other is double, upgrade to double
            if (t1 != t2)
            {
                if (t1 != typeof(double))
                {
                    v1 = ConvertTo(v1 as IConvertible, ComparerNumericType.FloatingPoint);
                }
                if (t2 != typeof(double))
                {
                    v2 = ConvertTo(v2 as IConvertible, ComparerNumericType.FloatingPoint);
                }
            }

            // compare
            switch (comparison)
            {
                case NumericComparison.Equal:
                    return v1.CompareTo(v2) == 0;

                case NumericComparison.NotEqual:
                    return v1.CompareTo(v2) != 0;

                case NumericComparison.LessThan:
                    return v1.CompareTo(v2) < 0;

                case NumericComparison.GreaterThan:
                    return v1.CompareTo(v2) > 0;

                case NumericComparison.LessThanOrEqual:
                    return v1.CompareTo(v2) <= 0;

                case NumericComparison.GreaterThanOrEqual:
                    return v1.CompareTo(v2) >= 0;

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private IComparable ParseAsDefault(string value)
        {
            return ParseAs(value, defaultType);
        }

        private static IComparable ParseAs(string value, ComparerNumericType type)
        {
            switch (type)
            {
                case ComparerNumericType.FloatingPoint:
                    return double.Parse(value);

                case ComparerNumericType.Integer:
                    return long.Parse(value);

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        // convert all integer primitives to long, and all floating point primitives to double
        private static IComparable EnsureType(IComparable value, ref Type type, ComparerNumericType defaultType)
        {
            type = value.GetType();

            // do nothing to doubles and longs
            if (type == typeof(double) || type == typeof(long))
            {
                return value;
            }

            // parse strings with default type
            if (type == typeof(string))
            {
                return ParseAs(value as string, defaultType);
            }
            
            // convert smaller integer types to long
            if (type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(ushort) ||
                type == typeof(int) ||
                type == typeof(uint))
            {
                type = typeof(long);

                // for some reason a (long) cast doesn't work here? is it because value is IComparable?
                return ConvertTo(value as IConvertible, ComparerNumericType.Integer);
            }

            // convert smaller floating point types to double
            if (type == typeof(float))
            {
                type = typeof(double);
                return ConvertTo(value as IConvertible, ComparerNumericType.FloatingPoint);
            }

            throw new InvalidCastException($"Cannot cast {type} to double or long.");
        }

        private static IComparable ConvertTo(IConvertible value, ComparerNumericType numericType)
        {
            switch(numericType)
            {
                case ComparerNumericType.Integer:
                    return value.ToInt64(null);
                case ComparerNumericType.FloatingPoint:
                    return value.ToDouble(null);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public static bool ConditionsNullEmptyOrMet(List<NumericComparisonConfiguration> conditions, IComparable input)
        {
            return ConditionsNullEmptyOrMet(conditions, input, ComparerNumericType.FloatingPoint);
        }

        public static bool ConditionsNullEmptyOrMet(List<NumericComparisonConfiguration> conditions, IComparable input, ComparerNumericType numericType)
        {
            return conditions == null || conditions.Count == 0 ||
                conditions.TrueForAll((condition) => NumericComparer.Compare(input, condition.comparisonTarget, condition.comparison, numericType));
        }

        public static bool OneOrMoreConditionsMet(List<NumericComparisonConfiguration> conditions, IComparable input)
        {
            return OneOrMoreConditionsMet(conditions, input, ComparerNumericType.FloatingPoint);
        }

        public static bool OneOrMoreConditionsMet(List<NumericComparisonConfiguration> conditions, IComparable input, ComparerNumericType numericType)
        {
            return conditions != null && conditions.Exists((condition) => NumericComparer.Compare(input, condition.comparisonTarget, condition.comparison, numericType));
        }
    }

    [Serializable]
    public enum NumericComparison
    {
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual
    }

    [Serializable]
    public enum ComparerNumericType
    {
        Integer,
        FloatingPoint
    }

    [Serializable]
    public struct NumericComparisonConfiguration
    {
        public NumericComparison comparison;
        public float comparisonTarget;
    }
}
