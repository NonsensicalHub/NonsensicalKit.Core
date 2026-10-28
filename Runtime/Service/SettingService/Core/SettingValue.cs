using System;
using System.Globalization;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 强类型设置值，统一以字符串持久化。
    /// </summary>
    public struct SettingValue : IEquatable<SettingValue>
    {
        public string Raw;

        public SettingValue(string raw)
        {
            Raw = raw ?? string.Empty;
        }

        public static SettingValue FromBool(bool v)
        {
            return new SettingValue(v ? "true" : "false");
        }

        public static SettingValue FromInt(int v)
        {
            return new SettingValue(v.ToString(CultureInfo.InvariantCulture));
        }

        public static SettingValue FromFloat(float v)
        {
            return new SettingValue(v.ToString(CultureInfo.InvariantCulture));
        }

        public static SettingValue FromString(string v)
        {
            return new SettingValue(v ?? string.Empty);
        }

        public bool AsBool()
        {
            if (string.Equals(Raw, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            bool result;
            return bool.TryParse(Raw, out result) && result;
        }

        public int AsInt()
        {
            int result;
            if (int.TryParse(Raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            float f;
            if (float.TryParse(Raw, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
            {
                return (int)f;
            }

            return 0;
        }

        public float AsFloat()
        {
            float result;
            if (float.TryParse(Raw, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            return 0f;
        }

        public string AsString()
        {
            return Raw ?? string.Empty;
        }

        public bool Equals(SettingValue other)
        {
            return string.Equals(Raw, other.Raw, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is SettingValue && Equals((SettingValue)obj);
        }

        public override int GetHashCode()
        {
            return Raw != null ? Raw.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Raw ?? string.Empty;
        }

        public static bool operator ==(SettingValue a, SettingValue b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SettingValue a, SettingValue b)
        {
            return !a.Equals(b);
        }
    }
}
