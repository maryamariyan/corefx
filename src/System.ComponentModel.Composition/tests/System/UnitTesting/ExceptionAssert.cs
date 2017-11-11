// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Runtime.Serialization;
using Xunit;

namespace System.UnitTesting
{
    public static class ExceptionAssert
    {
        // NOTE: To catch state corrupting exceptions, it by design that 
        // the ThrowsXXX methods retry by default. To prevent this in a 
        // test, simply use one of the overloads that takes a RetryMode.
        
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
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception of type <typeparam name="TInner"/>.
        /// </summary>
        public static T Throws<T, TInner>(Action action)
            where T : Exception
            where TInner : Exception
        {
            return Throws<T, TInner>(RetryMode.Retry, action);
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception of type <typeparam name="TInner"/>, and indicating 
        ///     whether to retry.
        /// </summary>
        public static T Throws<T, TInner>(RetryMode retry, Action action)
            where T : Exception
            where TInner : Exception
        {
            return Throws<T, TInner>(retry, action, (Action<T, int>)null);
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception of type <typeparam name="TInner"/>, indicating 
        ///     whether to retry and running the specified validator.
        /// </summary>
        public static T Throws<T, TInner>(RetryMode retry, Action action, Action<T, int> validator)
            where T : Exception
            where TInner : Exception
        {
            var exception = Throws<T>(retry, action, (actual, retryCount) =>
            {
                AssertIsExactInstanceOfInner(typeof(TInner), actual, retryCount);

                if (validator != null)
                {
                    validator(actual, retryCount);
                }
            });

            return exception;
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception.
        /// </summary>
        public static T Throws<T>(Exception innerException, Action action)
            where T : Exception
        {
            return Throws<T>(innerException, RetryMode.Retry, action, (Action<T, int>)null);
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception, and indicating whether to retry.
        /// </summary>
        public static T Throws<T>(Exception innerException, RetryMode retry, Action action)
            where T : Exception
        {
            return Throws<T>(innerException, RetryMode.Retry, action, (Action<T, int>)null);
        }

        /// <summary>
        ///     Verifies that the specified action throws an exception of type <typeparam name="T"/>,
        ///     with the specified inner exception, indicating whether to retry and running the 
        ///     specified validator.
        /// </summary>
        public static T Throws<T>(Exception innerException, RetryMode retry, Action action, Action<T, int> validator)
            where T : Exception
        {
            T exception = Throws<T>(retry, action, (actual, retryCount) =>
            {
                AssertSameInner(innerException, actual, retryCount);

                if (validator != null)
                {
                    validator(actual, retryCount);
                }
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
        ///     indicating whether to retry.
        /// </summary>
        public static T Throws<T>(RetryMode retry, Action action)
            where T : Exception
        {
            return Throws<T>(retry, action, (Action<T, int>)null);
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

        /// <summary>
        ///     Verifies that the specified action throws the specified exception.
        /// </summary>
        public static void Throws(Exception expected, Action action)
        {
            Throws(expected, RetryMode.Retry, action);
        }

        /// <summary>
        ///     Verifies that the specified action throws the specified exception,
        ///     indicating whether to retry.
        /// </summary>
        public static void Throws(Exception expected, RetryMode retry, Action action)
        {
            Throws(expected, retry, action, (Action<Exception, int>)null);
        }

        /// <summary>
        ///     Verifies that the specified action throws the specified exception,
        ///     indicating whether to retry and running the specified validator.
        /// </summary>
        public static void Throws(Exception expected, RetryMode retry, Action action, Action<Exception, int> validator)
        {
            Run(retry, action, (actual, retryCount) =>
            {
                AssertSame(expected, actual, retryCount);

                if (validator != null)
                {
                    validator(actual, retryCount);
                }
            });
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

            Assert.Equal(objectName, actual.ObjectName); // "Retry Count {0}: Expected {1}.ObjectName to be '{2}', however, '{3}' is.", retryCount, actual.GetType().Name, objectName, actual.ObjectName);
        }

        private static void AssertSame(Exception expected, Exception actual, int retryCount)
        {
            Assert.Same(expected, actual);// "Retry Count {0}: Expected '{1}' to be thrown, however, '{2}' was thrown.", retryCount, expected, actual);
        }

        private static void AssertSameInner(Exception innerException, Exception actual, int retryCount)
        {
            Assert.Same(innerException, actual.InnerException); // "Retry Count {0}: Expected '{1}' to be the inner exception, however, '{2}' is.", retryCount, innerException, actual.InnerException);
        }

        private static void AssertIsExactInstanceOf(Type expectedType, Exception actual, int retryCount)
        {
            if (actual == null)
                Assert.False(true);// "Retry Count {0}: Expected '{1}' to be thrown", retryCount, expectedType);

            Type actualType = actual.GetType();

            Assert.Same(expectedType, actualType); // "Retry Count {0}: Expected '{1}' to be thrown, however, '{2}' was thrown.", retryCount, expectedType, actualType);
        }

        private static void AssertIsExactInstanceOfInner(Type expectedType, Exception actual, int retryCount)
        {
            if (actual.InnerException == null)
                Assert.False(true);// "Retry Count {0}: Expected '{1}' be the inner exception, however, it is null.", retryCount, expectedType);

            Type actualType = actual.InnerException.GetType();

            Assert.Same(expectedType, actualType); // "Retry Count {0}: Expected '{1}' to be the inner exception, however, '{2}' is.", retryCount, expectedType, actualType);
        }
    }
}
