using IPA.Config.Data;
using IPA.Config.Stores;
using System;
using UnityEngine;

namespace EnhancedStreamChat.Converters
{
    /// <summary>
    /// https://github.com/bsmg/BeatSaber-IPA-Reloaded/blob/master/IPA.Loader/Config/Stores/Converters.cs#L495
    /// </summary>
    internal class ColorConverterWithAlpha : ValueConverter<Color>
    {
        /// <summary>
        /// Converts a <see cref="Value"/> that is a <see cref="Text"/> node to the corresponding <see cref="Color" /> object.
        /// </summary>
        /// <param name="value">the <see cref="Value"/> to convert</param>
        /// <param name="parent">the object which will own the created object</param>
        /// <returns>the deserialized Color object</returns>
        /// <exception cref="ArgumentException">if <paramref name="value"/> is not a <see cref="Text"/> node or couldn't be parsed into a Color object</exception>
        public override Color FromValue(Value value, object parent)
        {
            if (value is Text t) {
                if (ColorUtility.TryParseHtmlString(t.Value, out var color)) {
                    return color;
                }

                throw new ArgumentException("Value cannot be parsed into a Color.", nameof(value));
            }

            throw new ArgumentException("Value not a string", nameof(value));
        }

        /// <summary>
        /// Converts color of type <see cref="Color"/> to a <see cref="Value"/> node.
        /// </summary>
        /// <param name="obj">the object to serialize</param>
        /// <param name="parent">the object which owns <paramref name="obj"/></param>
        /// <returns>a <see cref="Text"/> node representing <paramref name="obj"/></returns>
        public override Value ToValue(Color obj, object parent)
        {
            return Value.Text($"#{ColorUtility.ToHtmlStringRGBA(obj)}");
        }
    }
}
