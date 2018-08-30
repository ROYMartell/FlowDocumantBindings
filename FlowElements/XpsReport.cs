using System.Windows;
using System.Windows.Documents;

namespace FlowElements
{

    public interface IReport
    {
        XpsReport report { get; }
    }

    public interface IReportView<T> : IReport
    {
        T Presenter { get; set; }
    }

    public class XpsReport : FlowDocument
    {
        public static readonly DependencyProperty FirstPageHeaderProperty = DependencyProperty.Register(
           "FirstPageHeader", typeof(DataTemplate), typeof(XpsReport));

        public DataTemplate FirstPageHeader
        {
            get { return (DataTemplate)GetValue(FirstPageHeaderProperty); }
            set { SetValue(FirstPageHeaderProperty, value); }
        }

        public static readonly DependencyProperty PageHeaderProperty = DependencyProperty.Register(
            "PageHeader", typeof(DataTemplate), typeof(XpsReport));

        public DataTemplate PageHeader
        {
            get { return (DataTemplate)GetValue(PageHeaderProperty); }
            set { SetValue(PageHeaderProperty, value); }
        }

        public static readonly DependencyProperty PageFooterProperty = DependencyProperty.Register(
            "PageFooter", typeof(DataTemplate), typeof(XpsReport));

        public DataTemplate PageFooter
        {
            get { return (DataTemplate)GetValue(PageFooterProperty); }
            set { SetValue(PageFooterProperty, value); }
        }

        public static readonly DependencyProperty LastPageFooterProperty = DependencyProperty.Register(
            "LastPageFooter", typeof(DataTemplate), typeof(XpsReport));

        public DataTemplate LastPageFooter
        {
            get { return (DataTemplate)GetValue(LastPageFooterProperty); }
            set { SetValue(LastPageFooterProperty, value); }
        }

        public double FirstHeaderHeight { get; set; }
        public double HeaderHeight { get; set; }
        public double FooterHeight { get; set; }
        public double LastFooterHeight { get; set; }

        public XpsReport()
        {
            //Это дело лучше в классе отчета указывать т.к. при объявлении 
            //здесь реагирует не та как должно, да и в верхней границе косяк есть, 
            //PagePadding = new Thickness(37, HeaderHeight, 37, FooterHeight);
        }
    }
}