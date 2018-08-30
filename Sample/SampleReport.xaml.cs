using System.Windows;
using FlowElements;

namespace Sample
{
    /// <summary>
    /// Interaction logic for SampleReport.xaml
    /// </summary>
    public partial class SampleReport : IReportView<SampleVM>
    {
        public SampleReport()
        {
            InitializeComponent();
        }

        public XpsReport report
        {
            get
            {
                Report.HeaderHeight = 5;
                Report.FooterHeight = 5;
                Report.PagePadding = new Thickness(50, 10, 50, 10);
                return Report;
            }
        }

        public SampleVM Presenter
        {
            get { return (SampleVM)DataContext; }
            set { DataContext = value; }
        }
    }
}
