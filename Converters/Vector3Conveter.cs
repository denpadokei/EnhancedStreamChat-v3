using IPA.Config.Data;
using IPA.Config.Stores;
using System;
using UnityEngine;

namespace EnhancedStreamChat.Converters
{
    public class Vector3Conveter : ValueConverter<Vector3>
    {
        /// <summary>
        /// Converts a config value map into a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="value">The config value.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public override Vector3 FromValue(Value value, object parent)
        {
            if (value is Map m) {
                m.TryGetValue("x", out var x);
                m.TryGetValue("y", out var y);
                m.TryGetValue("z", out var z);
                var vec = Vector3.zero;
                if (x is FloatingPoint pointX) {
                    vec.x = (float)pointX.Value;
                }
                if (y is FloatingPoint pointY) {
                    vec.y = (float)pointY.Value;
                }
                if (z is FloatingPoint pointZ) {
                    vec.z = (float)pointZ.Value;
                }
                return vec;
            }
            throw new ArgumentException("Value cannot be parsed into a Vector", nameof(value));
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> into a config value map.
        /// </summary>
        /// <param name="obj">The vector to convert.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public override Value ToValue(Vector3 obj, object parent)
        {
            var map = Value.Map();
            map.Add("x", Value.Float((decimal)obj.x));
            map.Add("y", Value.Float((decimal)obj.y));
            map.Add("z", Value.Float((decimal)obj.z));
            return map;
        }
    }
}
