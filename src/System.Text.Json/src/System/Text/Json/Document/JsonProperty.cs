// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Text.Json
{
    /// <summary>
    ///   Represents a single property for a JSON object.
    /// </summary>
    public readonly struct JsonProperty
    {
        /// <summary>
        ///   The value of this property.
        /// </summary>
        public JsonElement Value { get; }

        internal JsonProperty(JsonElement value)
        {
            Value = value;
        }

        /// <summary>
        ///   The name of this property.
        /// </summary>
        public string Name => Value.GetPropertyName();

        /// <summary>
        /// Compares the string text to the unescaped JSON token value in the source and returns true if they match.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>True if the JSON token value in the source matches the look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a JSON token that is not of type <see cref="JsonTokenType.PropertyName"/>).
        /// </exception>
        /// <remarks>
        /// If the look up text is invalid or incomplete UTF-16 text (i.e. unpaired surrogates), the method will return false
        /// since you cannot have invalid UTF-16 within the JSON payload.
        /// </remarks>
        /// <remarks>
        /// The comparison of the JSON token value in the source and the look up text is done by first unescaping the JSON value in source,
        /// if required. The look up text is matched as is, without any modifications to it.
        /// </remarks>
        public bool NameEquals(string text)
        {
            return NameEquals(text.AsSpan());
        }

        /// <summary>
        /// Compares the UTF-8 encoded text to the unescaped JSON token value in the source and returns true if they match.
        /// </summary>
        /// <param name="utf8Text">The UTF-8 encoded text to compare against.</param>
        /// <returns>True if the JSON token value in the source matches the UTF-8 encoded look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a JSON token that is not of type <see cref="JsonTokenType.PropertyName"/>).
        /// </exception>
        /// <remarks>
        /// If the look up text is invalid UTF-8 text, the method will return false since you cannot have 
        /// invalid UTF-8 within the JSON payload.
        /// </remarks>
        /// <remarks>
        /// The comparison of the JSON token value in the source and the look up text is done by first unescaping the JSON value in source,
        /// if required. The look up text is matched as is, without any modifications to it.
        /// </remarks>
        public bool NameEquals(ReadOnlySpan<byte> utf8Text)
        {
            return Value.TextEqualsHelper(utf8Text, isPropertyName: true);
        }

        /// <summary>
        /// Compares the string text to the unescaped JSON token value in the source and returns true if they match.
        /// </summary>
        /// <param name="text">The text to compare against.</param>
        /// <returns>True if the JSON token value in the source matches the look up text.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to find a text match on a JSON token that is not of type <see cref="JsonTokenType.PropertyName"/>).
        /// </exception>
        /// <remarks>
        /// If the look up text is invalid or incomplete UTF-16 text (i.e. unpaired surrogates), the method will return false
        /// since you cannot have invalid UTF-16 within the JSON payload.
        /// </remarks>
        /// <remarks>
        /// The comparison of the JSON token value in the source and the look up text is done by first unescaping the JSON value in source,
        /// if required. The look up text is matched as is, without any modifications to it.
        /// </remarks>
        public bool NameEquals(ReadOnlySpan<char> text)
        {
            return Value.TextEqualsHelper(text, isPropertyName: true);
        }

        /// <summary>
        ///   Provides a <see cref="string"/> representation of the property for
        ///   debugging purposes.
        /// </summary>
        /// <returns>
        ///   A string containing the un-interpreted value of the property, beginning
        ///   at the declaring open-quote and ending at the last character that is part of
        ///   the value.
        /// </returns>
        public override string ToString()
        {
            return Value.GetPropertyRawText();
        }
    }
}
