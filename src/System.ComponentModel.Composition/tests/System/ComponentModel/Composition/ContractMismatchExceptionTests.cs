// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System.UnitTesting;
using Xunit;

namespace System.ComponentModel.Composition
{
    public class ContractMismatchExceptionTests
    {
        [Fact]
        public void Constructor1_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionContractMismatchException();

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [Fact]
        public void Constructor2_NullAsMessageArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionContractMismatchException((string)null);

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [Fact]
        public void Constructor3_NullAsMessageArgument_ShouldSetMessagePropertyToDefault()
        {
            var exception = new CompositionContractMismatchException((string)null, new Exception());

            ExceptionAssert.HasDefaultMessage(exception);
        }

        [Fact]
        public void Constructor2_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionContractMismatchException(e);

                Assert.Equal(e, exception.Message);
            }
        }

        [Fact]
        public void Constructor3_ValueAsMessageArgument_ShouldSetMessageProperty()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = new CompositionContractMismatchException(e, new Exception());

                Assert.Equal(e, exception.Message);
            }
        }

        [Fact]
        public void Constructor1_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionContractMismatchException();

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor2_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionContractMismatchException("Message");

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor3_NullAsInnerExceptionArgument_ShouldSetInnerExceptionPropertyToNull()
        {
            var exception = new CompositionContractMismatchException("Message", (Exception)null);

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor3_ValueAsInnerExceptionArgument_ShouldSetInnerExceptionProperty()
        {
            var expectations = Expectations.GetInnerExceptions();

            foreach (var e in expectations)
            {
                var exception = new CompositionContractMismatchException("Message", e);

                Assert.Same(e, exception.InnerException);
            }
        }

#if FEATURE_SERIALIZATION

        [Fact]
        public void Constructor4_NullAsInfoArgument_ShouldThrowArgumentNull()
        {
            var context = new StreamingContext();

            ExceptionAssert.ThrowsArgument<ArgumentNullException>("info", () =>
            {
                SerializationTestServices.Create<CompositionContractMismatchException>((SerializationInfo)null, context);
            });
        }

        [Fact]
        public void InnerException_CanBeSerialized()
        {
            var expectations = Expectations.GetInnerExceptionsWithNull();

            foreach (var e in expectations)
            {
                var exception = CreateContractMismatchException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                ExtendedAssert.IsInstanceOfSameType(exception.InnerException, result.InnerException);
            }
        }

        [Fact]
        public void Message_CanBeSerialized()
        {
            var expectations = Expectations.GetExceptionMessages();

            foreach (var e in expectations)
            {
                var exception = CreateContractMismatchException(e);

                var result = SerializationTestServices.RoundTrip(exception);

                Assert.Equal(exception.Message, result.Message);
            }
        }

#endif //FEATURE_SERIALIZATION

        private static CompositionContractMismatchException CreateContractMismatchException()
        {
            return CreateContractMismatchException((string)null, (Exception)null);
        }

        private static CompositionContractMismatchException CreateContractMismatchException(string message)
        {
            return CreateContractMismatchException(message, (Exception)null);
        }

        private static CompositionContractMismatchException CreateContractMismatchExceptionFromId(string id)
        {
            return CreateContractMismatchException((string)null, (Exception)null);
        }

        private static CompositionContractMismatchException CreateContractMismatchException(Exception innerException)
        {
            return CreateContractMismatchException((string)null, innerException);
        }

        private static CompositionContractMismatchException CreateContractMismatchException(string message, Exception innerException)
        {
            return new CompositionContractMismatchException(message, innerException);
        }
    }
}
