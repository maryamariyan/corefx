// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.UnitTesting
{
    public static class ExceptionAssert
    {
        /// <summary>
        ///     Verifies that the specified action throws an ObjectDisposedException.
        /// </summary>
        public static ObjectDisposedException ThrowsDisposed(object instance, Action action)            
        {
            var exception = Throws<ObjectDisposedException>(RetryMode.Retry, action, (actual, retryCount) =>
            {
                AssertObjectDisposed(instance, actual, retryCount);
            });

            return exception;
        }

        /// <summary>
        ///     Verifies that the specified action throws an ArgumentException of type <typeparam name="T"/>.
        /// </summary>
        public static T ThrowsArgument<T>(string parameterName, Action action)
            where T : ArgumentException
        {
            var exception = Throws<T>(RetryMode.Retry, action, (actual, retryCount) =>
            {
                AssertSameParameterName(parameterName, actual, retryCount);
            });

            return exception;
        }
        
        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>.
        /// </summary>
        public static T Throws<T>(Action action)
            where T : Exception
        {
            return Throws<T>(RetryMode.Retry, action, (Action<T, int>)null);
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>, 
        ///     indicating whether to retry and running the specified validator.
        /// </summary>
        public static T Throws<T>(RetryMode retry, Action action, Action<T, int> validator)
            where T : Exception
        {
            var exception = (T)Run(retry, action, (actual, retryCount) =>
            {
                AssertIsExactInstanceOf(typeof(T), actual, retryCount);

                if (validator != null)
                {
                    validator((T)actual, retryCount);
                }
            });

            return exception;
        }

        private static Exception Run(RetryMode retry, Action action, Action<Exception, int> validator)
        {
            Exception exception = null;

            for (int i = -1; i < (int)retry; i++)
            {
                exception = Run(action);

                validator(exception, i + 2);
            }

            return exception;
        }

        private static Exception Run(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static void AssertObjectDisposed(object instance, ObjectDisposedException actual, int retryCount)
        {
            string objectName = instance.GetType().FullName;

            Assert.Equal(objectName, actual.ObjectName);
        }

        private static void AssertSameParameterName(string parameterName, ArgumentException actual, int retryCount)
        {
            Assert.Contains(actual.Message, parameterName);
        }

        private static void AssertIsExactInstanceOf(Type expectedType, Exception actual, int retryCount)
        {
            if (actual == null)
                Assert.False(true);

            Type actualType = actual.GetType();

            Assert.Same(expectedType, actualType);
        }
    }
}
