using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Data.Converters;

namespace Parameter.Converters
{
	public class PasswordMaskConverter : IMultiValueConverter
	{
		public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
		{
			var value = values.Count > 0 ? values[0] as string : null;
			var hidden = values.Count > 1 && values[1] is bool b && b;

			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return hidden
				? new string('•', value.Length)
				: value;
		}
	}
}
