using System.Collections;
using System.Windows;
using System.Windows.Documents;

namespace FlowElements
{
    public class TemplatedTableRowGroup : TableRowGroup
    {
        #region ItemSource
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
            "ItemSource",
            typeof(IEnumerable),
            typeof(TemplatedTableRowGroup),
            new PropertyMetadata(new PropertyChangedCallback(onItemSourceChanged)));

        private static void onItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TemplatedTableRowGroup)d).Refresh();
        }

        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }
        #endregion

        #region ItemsTemplate
        public static readonly DependencyProperty ItemsTemplateProperty = DependencyProperty.Register(
            "ItemsTemplate",
            typeof(DataTemplate),
            typeof(TemplatedTableRowGroup),
            new PropertyMetadata(new PropertyChangedCallback(onItemsTemplateChanged)));

        private static void onItemsTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TemplatedTableRowGroup)d).Refresh();
        }

        public DataTemplate ItemsTemplate
        {
            get { return (DataTemplate)GetValue(ItemsTemplateProperty); }
            set { SetValue(ItemsTemplateProperty, value); }
        }
        #endregion

        void Refresh()
        {
            if (Rows == null) return;
            if (ItemSource == null) return;

            Rows.Clear();
            foreach (var obj in ItemSource)
            {
                var content = ItemsTemplate.LoadContent();
                if (content is TableRow)
                {
                    var row = (TableRow)content;
                    row.DataContext = obj;
                    Rows.Add(row);
                }
            }
        }
    }
}