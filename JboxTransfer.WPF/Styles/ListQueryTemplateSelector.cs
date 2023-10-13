using JboxTransfer.ViewModels;
using JboxTransfer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JboxTransfer.Styles
{
    public class ListQueryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate QueryWait { get; set; }
        public DataTemplate QueryCompleted { get; set; }
        public DataTemplate QuerySql { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            SyncTaskQueryViewModel vm = item as SyncTaskQueryViewModel;
            switch (vm.queryType)
            {
                case QueryType.QueryWait:
                    return QueryWait;
                    break;
                case QueryType.QueryCompleted:
                    return QueryCompleted;
                    break;
                case QueryType.QuerySql:
                    return QuerySql;
                    break;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
