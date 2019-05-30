

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Xunit;

namespace System.Text.Json.Tests
{
    public static class JsonElementValueTests
    {
        [Fact]
        public static void ValueEquals_EmptyJsonString_True()
        {
            const string jsonString = "\"\"";
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();

            Assert.True(jElement.ValueEquals(""));
            Assert.True(jElement.ValueEquals("".AsSpan()));
            Assert.True(jElement.ValueEquals(default(ReadOnlySpan<char>)));
            Assert.True(jElement.ValueEquals(default(ReadOnlySpan<byte>)));
        }

        [Fact]
        public static void ValueEquals_TestTextEqualsLargeMatch_True()
        {
            var jsonChars = new char[320];  // Some value larger than 256 (stack threshold)
            jsonChars.AsSpan().Fill('a');

            Span<char> lookupChars = new char[jsonChars.Length];
            jsonChars.CopyTo(lookupChars);

            string jsonString = "\"" + new string(jsonChars) + "\"";
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();

            Assert.True(jElement.ValueEquals(new string(jsonChars)));
            Assert.True(jElement.ValueEquals(jsonChars));
            Assert.True(jElement.ValueEquals(lookupChars));
        }

        [Theory]
        [InlineData("\"conne\\u0063tionId\"", "connectionId")]
        [InlineData("\"connectionId\"", "connectionId")]
        [InlineData("\"123\"", "123")]
        [InlineData("\"My name is \\\"Ahson\\\"\"", "My name is \"Ahson\"")]
        public static void ValueEquals_JsonTokenStringType_True(string jsonString, string expected)
        {
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();
            Assert.True(jElement.ValueEquals(expected));
            Assert.True(jElement.ValueEquals(expected.AsSpan()));
            byte[] expectedGetBytes = Encoding.UTF8.GetBytes(expected);
            Assert.True(jElement.ValueEquals(expectedGetBytes));
        }

        [Theory]
        [InlineData("\"hello\"", new char[1] { (char)0xDC01 })]    // low surrogate - invalid
        [InlineData("\"hello\"", new char[1] { (char)0xD801 })]    // high surrogate - missing pair
        public static void InvalidUTF16Search(string jsonString, char[] lookup)
        {
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();
            Assert.False(jElement.ValueEquals(lookup));
        }

        [Theory]
        [InlineData("\"connectionId\"", "\"conne\\u0063tionId\"")]
        [InlineData("\"conne\\u0063tionId\"", "bonnectionId")] // intentionally changing the expected starting character
        public static void ValueEquals_JsonTokenStringType_False(string jsonString, string otherText)
        {
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();
            Assert.False(jElement.ValueEquals(otherText));
            Assert.False(jElement.ValueEquals(otherText.AsSpan()));
            byte[] expectedGetBytes = Encoding.UTF8.GetBytes(otherText);
            Assert.False(jElement.ValueEquals(expectedGetBytes));
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"\" : \"\"}")]
        [InlineData("{\"sample\" : \"\"}")]
        [InlineData("{\"sample\" : null}")]
        [InlineData("{\"sample\" : \"sample-value\"}")]
        [InlineData("{\"connectionId\" : \"123\"}")]
        public static void ValueEquals_JsonTokenNotString_Success(string jsonString)
        {
            const string errorMessage = "The requested operation requires an element of type 'String', but the target element has type 'Object'.";
            JsonElement jElement = (JsonElement)JsonDocument.Parse(jsonString).RootElement.Clone();
            AssertExtensions.Throws<InvalidOperationException>(() => jElement.ValueEquals("throws-anyway"), errorMessage);
            AssertExtensions.Throws<InvalidOperationException>(() => jElement.ValueEquals("throws-anyway".AsSpan()), errorMessage);
            byte[] expectedGetBytes = Encoding.UTF8.GetBytes("throws-anyway");
            AssertExtensions.Throws<InvalidOperationException>(() => jElement.ValueEquals(expectedGetBytes), errorMessage);
        }
    }
}