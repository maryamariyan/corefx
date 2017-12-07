// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit;

namespace System.UnitTesting
{
    public static class AssertExtensions
    {/// <summary>
     /// Tests whether the specified string contains the specified substring
     /// and throws an exception if the substring does not occur within the
     /// test string.
     /// </summary>
     /// <param name="value">
     /// The string that is expected to contain <paramref name="substring"/>.
     /// </param>
     /// <param name="substring">
     /// The string expected to occur within <paramref name="value"/>.
     /// </param>
     /// <exception cref="AssertFailedException">
     /// Thrown if <paramref name="substring"/> is not found in
     /// <paramref name="value"/>.
     /// </exception>
        public static void Contains(string value, string substring)
        {
            Contains(value, substring, string.Empty, null);
        }

        /// <summary>
        /// Tests whether the specified string contains the specified substring
        /// and throws an exception if the substring does not occur within the
        /// test string.
        /// </summary>
        /// <param name="value">
        /// The string that is expected to contain <paramref name="substring"/>.
        /// </param>
        /// <param name="substring">
        /// The string expected to occur within <paramref name="value"/>.
        /// </param>
        /// <param name="message">
        /// The message to include in the exception when <paramref name="substring"/>
        /// is not in <paramref name="value"/>. The message is shown in
        /// test results.
        /// </param>
        /// <exception cref="AssertFailedException">
        /// Thrown if <paramref name="substring"/> is not found in
        /// <paramref name="value"/>.
        /// </exception>
        public static void Contains(string value, string substring, string message)
        {
            Contains(value, substring, message, null);
        }

        /// <summary>
        /// Tests whether the specified string contains the specified substring
        /// and throws an exception if the substring does not occur within the
        /// test string.
        /// </summary>
        /// <param name="value">
        /// The string that is expected to contain <paramref name="substring"/>.
        /// </param>
        /// <param name="substring">
        /// The string expected to occur within <paramref name="value"/>.
        /// </param>
        /// <param name="message">
        /// The message to include in the exception when <paramref name="substring"/>
        /// is not in <paramref name="value"/>. The message is shown in
        /// test results.
        /// </param>
        /// <param name="parameters">
        /// An array of parameters to use when formatting <paramref name="message"/>.
        /// </param>
        /// <exception cref="AssertFailedException">
        /// Thrown if <paramref name="substring"/> is not found in
        /// <paramref name="value"/>.
        /// </exception>
        public static void Contains(string value, string substring, string message, params object[] parameters)
        {
            Assert.NotNull(value);
            Assert.NotNull(substring);
            if (0 > value.IndexOf(substring, StringComparison.Ordinal))
            {
                Assert.False(true);
            }
        }
    }

    public static class EqualityExtensions
    {
        public static void IsTrueForAll<T>(IEnumerable<T> source, Predicate<T> predicate)
        {
            IsTrueForAll(source, predicate, "IsTrueForAll Failed");
        }

        public static void IsTrueForAll<T>(IEnumerable<T> source, Predicate<T> predicate, string message)
        {
            Assert.NotNull(source);

            foreach (T t in source)
            {
                Assert.True(predicate(t), message);
            }
        }

        private static MethodInfo GetExtensionMethod(Type extendedType)
        {
            if (extendedType.IsGenericType)
            {
                var x = typeof(EqualityExtensions).GetMethods()
                    ?.Where(m =>
                        m.Name == "IsEqual" &&
                        m.GetParameters().Length == 2 &&
                        m.IsGenericMethodDefinition);

                MethodInfo method = typeof(EqualityExtensions).GetMethods()
                    ?.SingleOrDefault(m =>
                        m.Name == "IsEqual" &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType.Name == extendedType.Name &&
                        m.IsGenericMethodDefinition);

                // If extension method found, make it generic and return
                if (method != null)
                    return method.MakeGenericMethod(extendedType.GenericTypeArguments[0]);
            }

            return typeof(EqualityExtensions).GetMethod("IsEqual", new[] { extendedType, extendedType });
        }

        public static bool CheckEquals(object objA, object objB)
        {
            if (objA == null && objB == null)
                return true;

            if (objA != null && objB != null)
            {
                object equalityResult = null;
                Type objType = objA.GetType();

                // Check if custom equality extension method is available
                MethodInfo customEqualityCheck = GetExtensionMethod(objType);
                if (customEqualityCheck != null)
                {
                    equalityResult = customEqualityCheck.Invoke(objA, new object[] { objA, objB });
                }
                else
                {
                    // Check if object.Equals(object) is overridden and if not check if there is a more concrete equality check implementation
                    bool equalsNotOverridden = objType.GetMethod("Equals", new Type[] { typeof(object) }).DeclaringType == typeof(object);
                    if (equalsNotOverridden)
                    {
                        // If type doesn't override Equals(object) method then check if there is a more concrete implementation
                        // e.g. if type implements IEquatable<T>.
                        MethodInfo equalsMethod = objType.GetMethod("Equals", new Type[] { objType });
                        if (equalsMethod.DeclaringType != typeof(object))
                        {
                            equalityResult = equalsMethod.Invoke(objA, new object[] { objB });
                        }
                    }
                }

                if (equalityResult != null)
                {
                    return (bool)equalityResult;
                }
            }

            if (objA is IEnumerable objAEnumerable && objB is IEnumerable objBEnumerable)
            {
                return CheckSequenceEquals(objAEnumerable, objBEnumerable);
            }

            return objA.Equals(objB);
        }

        public static bool CheckSequenceEquals(this IEnumerable @this, IEnumerable other)
        {
            if (@this == null || other == null)
                return @this == other;

            if (@this.GetType() != other.GetType())
                return false;

            IEnumerator eA = null;
            IEnumerator eB = null;

            try
            {
                eA = (@this as IEnumerable).GetEnumerator();
                eB = (@this as IEnumerable).GetEnumerator();
                while (true)
                {
                    bool moved = eA.MoveNext();
                    if (moved != eB.MoveNext())
                        return false;
                    if (!moved)
                        return true;
                    if (eA.Current == null && eB.Current == null)
                        return true;
                    if (!CheckEquals(eA.Current, eB.Current))
                        return true;
                }
            }
            finally
            {
                (eA as IDisposable)?.Dispose();
                (eB as IDisposable)?.Dispose();
            }
        }
    }

    public static class EnumerableAssert
    {
        public static void AreEqual<T>(IEnumerable<T> actual, params T[] expected)
        {
            AreEqual<T>((IEnumerable<T>)expected, (IEnumerable<T>)actual);
        }

        public static void AreEqual<T>(IEnumerable expected, IList<T> actual)
        {
            foreach (object value in expected)
            {
                bool removed = actual.Remove((T)value);

                Assert.True(removed);
            }

            Assert.Equal(0, actual.Count);
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            // First, test the IEnumerable implementation
            Assert.Equal(expected.Count(), actual.Count());
            AreEqual((IEnumerable)expected, actual.ToList());

            // Second, test the IEnumerable<T> implementation
            Assert.Equal(expected.Count(), actual.Count());

            List<T> actualList = actual.ToList();

            foreach (T value in expected)
            {
                bool removed = actualList.Remove(value);

                Assert.True(removed);
            }

            Assert.Equal(0, actualList.Count);
        }
        
        public static void AreEqual<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (KeyValuePair<TKey, TValue> kvp in expected)
            {
                TValue firstValue = kvp.Value;
                TValue secondValue = default(TValue);
                if (!actual.TryGetValue(kvp.Key, out secondValue))
                {
                    Assert.False(true);
                }

                if ((firstValue is IDictionary<TKey, TValue>) && (secondValue is IDictionary<TKey, TValue>))
                {
                    AreEqual((IDictionary<TKey, TValue>)firstValue, (IDictionary<TKey, TValue>)secondValue);
                    continue;
                }

                Assert.Equal(kvp.Value, secondValue);
            }
        }
    }
}
