using JboxTransfer.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JboxTransfer.Converters
{
    public class SyncTaskStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (int)value;
            switch(state)
            {
                case 0:
                    return "待传输";
                    break;
                case 1:
                    return "传输中/排队中";
                    break;
                case 2:
                    return "已停止（失败）";
                    break;
                case 3:
                    return "已完成";
                    break;
                case 4:
                    return "已取消";
                    break;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
