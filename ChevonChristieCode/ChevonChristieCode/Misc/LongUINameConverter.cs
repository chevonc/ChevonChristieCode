using System;
using System.Windows.Data;

namespace ChevonChristieCode.Misc
{
   public class LongUINameConverter : IValueConverter
   {

      /// <summary>
      /// Modifies the source data before passing it to the target for display in the UI.
      /// </summary>
      /// <param name="value">The source data being passed to the target.</param>
      /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
      /// <param name="parameter">An the max length to display before replacing remainder of letters with "..."</param>
      /// <param name="culture">The culture of the conversion.</param>
      /// <returns>
      /// The value to be passed to the target dependency property.
      /// </returns>
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         int x;
         int.TryParse(parameter as string, out x);
         string val  = value as string;

         if (string.IsNullOrEmpty(val))
            return val;

         if (val.Length > x)
            return (val).Substring(0, x) + "...";
         
         return val;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}
