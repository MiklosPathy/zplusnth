using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static Z_nthCommon.Zplusnthbase;

namespace Z_nthCommon
{
    public class ChannelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Channel chn = value as Channel;
            if (chn == null) return "Non Channel Input Object";
            return chn.State == ChannelState.Inactive ? "" : Math.Round(chn.Freq).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
