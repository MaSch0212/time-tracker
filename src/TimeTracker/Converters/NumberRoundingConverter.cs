using MaSch.Core.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace TimeTracker.Converters
{
    public class NumberRoundingConverter : IValueConverter
    {
#if NETFRAMEWORK
        public RoundingMode RoundingMode { get; set; }
#else
        public MidpointRounding RoundingMode { get; set; }
#endif
        public int Decimals { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dValue = value.ConvertTo<double>();
#if NETFRAMEWORK
            var pow = Math.Pow(10, Decimals);
            return RoundingMode switch
            {
                RoundingMode.ToZero or RoundingMode.ToNegativeInfinity => Math.Floor(dValue * pow) / pow,
                RoundingMode.ToPositiveInfinity => Math.Ceiling(dValue * pow) / pow,
                _ => Math.Round(dValue, Decimals, (MidpointRounding)RoundingMode)
            };
#else
            return Math.Round(dValue, Decimals, RoundingMode);
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

#if NETFRAMEWORK
    public enum RoundingMode
    {
        //
        // Summary:
        //     Round to nearest mode: when a number is halfway between two others, it is rounded
        //     toward the nearest even number.
        ToEven = 0,
        //
        // Summary:
        //     Round to nearest mode: when a number is halfway between two others, it is rounded
        //     toward the nearest number that is away from zero.
        AwayFromZero = 1,
        //
        // Summary:
        //     Directed mode: the number is rounded toward zero, with the result closest to
        //     and no greater in magnitude than the infinitely precise result.
        ToZero = 2,
        //
        // Summary:
        //     Directed mode: the number is rounded down, with the result closest to and no
        //     greater than the infinitely precise result.
        ToNegativeInfinity = 3,
        //
        // Summary:
        //     Directed mode: the number is rounded up, with the result closest to and no less
        //     than the infinitely precise result.
        ToPositiveInfinity = 4
    }
#endif
}
