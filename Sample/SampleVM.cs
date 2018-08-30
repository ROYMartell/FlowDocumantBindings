using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sample
{
    public class TableData
    {
        public string Name { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class SampleVM: INotifyPropertyChanged
    {
        public String NumDoc { get { return "№ пример хитрой печати от сегодняшнего числа сего года"; } }

        public List<TableData> Items { get; private set; }

        public String TotalText { get { return "До фига денег"; } }

        public string Source { get; set; }

        private int pageCount;
        public int PageCount
        {
            get { return pageCount; }
            set
            {
                pageCount = value;
                OnPropertyChanged("PageCount");
            }
        }

        private int pageNumber;
        public int PageNumber
        {
            get { return pageNumber; }
            set
            {
                pageNumber = value;
                OnPropertyChanged("PageNumber");
            }
        }

        public SampleVM()
        {
            //Items = new List<TableData>();
            //for (var i = 0; i < 100; i++)
            //{
            //    Items.Add(new TableData
            //                {
            //                    Name =
            //                        string.Format(
            //                        "Это большое наименование может быть товара {0}, а может быть и нет, но для примера надо. Число={1}",
            //                        i, i),
            //                    Qty = i % 10 + 1,
            //                    Price = i
            //                });
            //}

            Source = "sample.jpg";
        }

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
