using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Size = System.Windows.Size;

namespace FlowElements
{
    public class XpsReportPaginator : DocumentPaginator
    {
        Size pageSize;

        readonly DocumentPaginator paginator;

        readonly XpsReport reportDef;

        int pageCount;

        public XpsReportPaginator(XpsReport reportDef, Size pageSize)
        {
            this.pageSize = pageSize;
            paginator = ((IDocumentPaginatorSource)reportDef).DocumentPaginator;
            this.reportDef = reportDef;
            paginator.PageSize = new Size(
                pageSize.Width, 
                pageSize.Height - reportDef.HeaderHeight - reportDef.FooterHeight);

            // Вот эта волшебная строка дергает установку биндингов. http://blog.fohjin.com/2008/07/saving-wpf-xaml-flowdocument-to-xps.html
            reportDef.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new DispatcherOperationCallback(delegate { return null; }), null);
        }

        /// <summary>
        /// This is the most important method , modifies the original 
        /// </summary>
        public override DocumentPage GetPage(int pageNumber)
        {
            var newpage = new ContainerVisual();
            if (pageNumber == 0 && reportDef.FirstPageHeader != null)
            {
                var v = GetVisualFromTemplate(reportDef.FirstPageHeader);
                v.Offset = new Vector(0, 0);
                newpage.Children.Add(v);
            }
            else if (pageNumber == 0 && reportDef.FirstPageHeader == null && reportDef.PageHeader != null)
            {
                var v = GetVisualFromTemplate(reportDef.PageHeader);
                v.Offset = new Vector(0, 0);
                newpage.Children.Add(v);
            }
            if (pageNumber > 0 && reportDef.PageHeader != null)
            {
                var v = GetVisualFromTemplate(reportDef.PageHeader);
                v.Offset = new Vector(0, 0);
                newpage.Children.Add(v);
            }

            var smallerPage = new ContainerVisual();
            var page = paginator.GetPage(pageNumber);
            smallerPage.Children.Add(page.Visual);

            if (pageNumber == 0 && reportDef.FirstPageHeader != null)
                smallerPage.Offset = new Vector(0, reportDef.FirstHeaderHeight);
            else
                smallerPage.Offset = new Vector(0, reportDef.HeaderHeight);
            newpage.Children.Add(smallerPage);

            if (pageNumber == pageCount - 1 && reportDef.LastPageFooter != null)
            {
                var v = GetVisualFromTemplate(reportDef.LastPageFooter);
                v.Offset = new Vector(0, reportDef.HeaderHeight + paginator.PageSize.Height - reportDef.LastFooterHeight - reportDef.PagePadding.Top);
                newpage.Children.Add(v);
            }
            else if (reportDef.PageFooter != null)
            {
                var v = GetVisualFromTemplate(reportDef.PageFooter);
                v.Offset = new Vector(0, reportDef.HeaderHeight + paginator.PageSize.Height - reportDef.FooterHeight - reportDef.PagePadding.Top);
                newpage.Children.Add(v);
            }

            var dp = new DocumentPage(newpage, new Size(pageSize.Width, pageSize.Height), page.BleedBox, page.ContentBox);
            return dp;
        }

        private ContainerVisual GetVisualFromTemplate(FrameworkTemplate template)
        {
            var tmpDoc = new FlowDocument
            {
                ColumnWidth = reportDef.ColumnWidth,
                PageWidth = paginator.PageSize.Width,
                PagePadding = reportDef.PagePadding,
                DataContext = reportDef.DataContext
            };
            var content = template.LoadContent();
            if (content is Fragment)
                content = ((Fragment)content).Content;

            var header = content as Section;
            if (header != null)
                tmpDoc.Blocks.Add(header);

            // Вот эта волшебная строка дергает установку биндингов. http://blog.fohjin.com/2008/07/saving-wpf-xaml-flowdocument-to-xps.html
            tmpDoc.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new DispatcherOperationCallback(delegate { return null; }), null);

            var tmpPage = ((IDocumentPaginatorSource)tmpDoc).DocumentPaginator.GetPage(0);
            return (ContainerVisual)tmpPage.Visual;
        }

        #region DefaultOverrides
        public override bool IsPageCountValid
        {
            get { return paginator.IsPageCountValid; }
        }

        public override int PageCount
        {
            get { return paginator.PageCount; }
        }

        public override Size PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }

        public override IDocumentPaginatorSource Source
        {
            get { return paginator.Source; }
        }
        #endregion

    }

    [ContentProperty("Content")]
    public class Fragment : FrameworkElement
    {
        private static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(FrameworkContentElement), typeof(Fragment));

        public FrameworkContentElement Content
        {
            get
            {
                return (FrameworkContentElement)GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }
    }
}