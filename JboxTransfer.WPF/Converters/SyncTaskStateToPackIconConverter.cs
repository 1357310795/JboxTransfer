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
    public class SyncTaskStateToPackIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (SyncTaskState)value;
            switch(state)
            {
                case SyncTaskState.Wait:
                    return PackIconKind.Play;
                    break;
                case SyncTaskState.Running:
                    return PackIconKind.Pause;
                    break;
                case SyncTaskState.Error:
                    return PackIconKind.Play;
                    break;
                case SyncTaskState.Complete:
                    return PackIconKind.Refresh;
                    break;
                case SyncTaskState.Pause:
                    return PackIconKind.Play;
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
