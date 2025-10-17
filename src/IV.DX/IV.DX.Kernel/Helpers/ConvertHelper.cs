using System.Globalization;

namespace IV.DX.Kernel.Helpers
{
    internal static class ConvertHelper
    {
        private static readonly string[] _allowedDateTimeFormats = { "d", "D", "g", "G", "MMM d yyyy h:mmtt" };

        public static string ParseString(object value)
        {
            if (value.GetType() == typeof(Guid))
                return value.ToString();

            return ((IConvertible)value).ToString();
        }

        public static string ParseString(object value, string defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return value.ToString();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ParseString(string value, string defaultValue)
        {
            return (String.IsNullOrEmpty(value) ? defaultValue : value);
        }

        public static decimal ParseCurrency(object value)
        {
            if (value is decimal)
            {
                return (decimal)value;
            }
            else if (value is string)
            {
                return ParseCurrency(System.Convert.ToString(value));
            }
            else
            {
                IsValidConversion(value, typeof(decimal));

                return ((IConvertible)value).ToDecimal(null);
            }
        }

        public static decimal ParseCurrency(string value)
        {
            return Decimal.Parse(value, NumberStyles.Currency);
        }

        public static decimal ParseCurrency(string value, decimal defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseCurrency(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ParseCurrency(object value, decimal defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseCurrency(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ParseDecimal(object value)
        {
            if (value is decimal)
            {
                return (decimal)value;
            }
            else if (value is string)
            {
                return ParseCurrency(System.Convert.ToString(value));
            }
            else
            {
                IsValidConversion(value, typeof(decimal));

                return ((IConvertible)value).ToDecimal(null);
            }
        }

        public static decimal ParseDecimal(string value)
        {
            return Decimal.Parse(value, NumberStyles.Number);
        }

        public static decimal ParseDecimal(string value, decimal defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseDecimal(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ParseDecimal(object value, decimal defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseDecimal(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseInt(object value)
        {
            if (value is int)
            {
                return (int)value;
            }
            else if (value is string)
            {
                return ParseInt(System.Convert.ToString(value));
            }
            else
            {
                IsValidConversion(value, typeof(int));

                return ((IConvertible)value).ToInt32(null);
            }
        }

        public static int ParseInt(string value)
        {
            return Int32.Parse(value, NumberStyles.Integer);
        }

        public static int ParseInt(string value, int defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseInt(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseInt(object value, sbyte defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseSByte(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseSByte(object value)
        {
            if (value is sbyte)
            {
                return (sbyte)value;
            }
            else if (value is string)
            {
                return ParseSByte(System.Convert.ToString(value));
            }
            else
            {
                IsValidConversion(value, typeof(sbyte));

                return ((IConvertible)value).ToSByte(null);
            }
        }

        public static int ParseSByte(string value)
        {
            var trimedValue = value.Trim().ToLower();

            if (trimedValue == "false")
                return 0;

            if (trimedValue == "true")
                return 1;

            return SByte.Parse(trimedValue, NumberStyles.Integer);
        }

        public static int ParseSByte(string value, sbyte defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseSByte(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseSByte(object value, sbyte defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseInt(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static double ParseDouble(object value)
        {
            if (value is double)
            {
                return (double)value;
            }
            else if (value is string)
            {
                return ParseDouble(System.Convert.ToString(value));
            }
            else
            {
                IsValidConversion(value, typeof(double));

                return ((IConvertible)value).ToDouble(null);
            }
        }

        public static double ParseDouble(string value)
        {
            return Double.Parse(value, NumberStyles.Float);
        }

        public static double ParseDouble(string value, double defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseDouble(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static double ParseDouble(object value, double defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseDouble(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ParseBool(object value)
        {
            if (value is bool)
            {
                return (bool)value;
            }
            else
            {
                return System.Convert.ToBoolean(value);
            }
        }

        public static bool ParseBool(string value)
        {
            return System.Convert.ToBoolean(value);
        }

        public static bool ParseBool(string value, bool defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseBool(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ParseBool(object value, bool defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseBool(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Guid ParseGuid(object value)
        {
            if (value is Guid)
            {
                return (Guid)value;
            }
            else
            {
                return ParseGuid(Convert.ToString(value));
            }
        }

        public static Guid ParseGuid(string value)
        {
            return new Guid(value);
        }

        public static Guid ParseGuid(string value, Guid defaultValue)
        {
            if (String.IsNullOrEmpty(value) || value.Length < 32)
            {
                return defaultValue;
            }

            try
            {
                return ParseGuid(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Guid ParseGuid(object value, Guid defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseGuid(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool IsGuid(object value)
        {
            if (value is Guid)
            {
                return true;
            }
            else
            {
                return IsGuid(Convert.ToString(value));
            }
        }

        public static bool IsGuid(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 32)
                return false;

            try
            {
                new Guid(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime ParseDateTime(object value)
        {
            if (value is DateTime)
            {
                return (DateTime)value;
            }

            else if (value is string)
            {
                return ParseDateTime(System.Convert.ToString(value));
            }
            else
            {
                return (DateTime)System.Convert.ChangeType(value, typeof(DateTime));
            }
        }

        public static TimeSpan ParseTimeSpan(object value)
        {
            if (value is TimeSpan)
            {
                return (TimeSpan)value;
            }

            else if (value is string)
            {
                return ParseTimeSpan(System.Convert.ToString(value));
            }
            else
            {
                return (TimeSpan)System.Convert.ChangeType(value, typeof(TimeSpan));
            }
        }

        public static DateTime ParseDateTime(string value)
        {
            return DateTime.ParseExact(value, _allowedDateTimeFormats, null, DateTimeStyles.AllowWhiteSpaces);
        }

        public static TimeSpan ParseTimeSpan(string value)
        {
            return TimeSpan.Parse(value);
        }

        public static DateTime ParseDateTime(string value, DateTime defaultValue)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return ParseDateTime(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime ParseDateTime(object value, DateTime defaultValue)
        {
            if (value == null || value is DBNull)
            {
                return defaultValue;
            }

            try
            {
                return ParseDateTime(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string TruncateString(string value, int length)
        {
            value = ConvertToNull(value);
            return (value != null && value.Length > length) ? value.Substring(0, length) : value;
        }

        public static string ConvertToNull(string value)
        {
            return (String.IsNullOrEmpty(value)) ? null : value;
        }

        private static bool IsValidConversion(object value, Type conversionType)
        {
            IConvertible convertible = value as IConvertible;
            if (convertible == null)
            {
                Type sourceType = value.GetType();
                if (sourceType != conversionType)
                {
                    throw new InvalidCastException(
                        String.Format("Unable to convert {0} to {1}", sourceType, conversionType));
                }
            }

            return true;
        }
    }
}
