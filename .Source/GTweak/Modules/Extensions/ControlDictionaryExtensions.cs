using System;
using System.Collections.Generic;

namespace GTweak.Modules.Extensions
{
    internal static class ControlDictionaryExtensions
    {
        private static readonly char[] Digits = "0123456789".ToCharArray();

        internal static bool TryGetAction<TAction>(this Dictionary<Enum, TAction> dict, string controlName, out TAction action)
        {
            action = default;
            if (string.IsNullOrEmpty(controlName))
            {
                return false;
            }

            int digitIndex = controlName.IndexOfAny(Digits);

            if (digitIndex >= 0 && int.TryParse(controlName.Substring(digitIndex), out int index))
            {
                string prefix = controlName.Substring(0, digitIndex);

                string expectedSuffix = prefix switch
                {
                    "TglButton" => "Toggle",
                    "Checkbox" => "Checkbox",
                    "Slider" => "Slider",
                    "ColorPicker" => "Color",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(expectedSuffix))
                {
                    return false;
                }

                foreach (var kvp in dict)
                {
                    if (Convert.ToInt32(kvp.Key) == index && kvp.Key.GetType().Name.EndsWith(expectedSuffix))
                    {
                        action = kvp.Value;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}