using System;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace Sample
{
    public class ReportPaginator : DocumentPaginator
    {
        Size pageSize;

        /// <summary>
        /// Reference to a original flowdoc paginator
        /// </summary>
        readonly DocumentPaginator paginator;

        readonly PageDefinition pageDef;

        /// <summary>
        /// Real total page count number
        /// </summary>
        int pageCount;

        /// <summary>
        /// Minimal space between page header/footer and page content
        /// </summary>
        private const int minimalOffset = 25;

        /// <summary>
        /// Helper method to create page header o footer from flow document template
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="pageDef"></param>
        /// <returns></returns>
        public static XpsDocument CreateXpsDocument(FlowDocument fd, PageDefinition pageDef)
        {
            const string pack = "pack://report.xps";

            //var ms = new MemoryStream();
            //Package pkg = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            //PackageStore.RemovePackage(new Uri(pack));
            //PackageStore.AddPackage(new Uri(pack), pkg);
            //var doc = new XpsDocument(pkg, CompressionOption.SuperFast, pack);
            //var rsm = new XpsSerializationManager(new XpsPackagingPolicy(doc), false);
            //DocumentPaginator paginator = ((IDocumentPaginatorSource)fd).DocumentPaginator;

            //var rp = new ReportPaginator(paginator, new Size(96/2.54*21, 96/2.54*28.7), pageDef);//PrintHelper.GetPageSize()
            //rsm.SaveAsXaml(rp);

            //return doc;



            var ms = new MemoryStream();
            var package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            var doc = new XpsDocument(package, CompressionOption.SuperFast, pack);
            PackageStore.AddPackage(new Uri(pack), package);

            DocumentPaginator paginator = ((IDocumentPaginatorSource)fd).DocumentPaginator;

            XpsDocument.CreateXpsDocumentWriter(doc).Write(paginator);
            //ReplacePngsWithJpegs(package);

            //var fixedDoc = doc.GetFixedDocumentSequence();
            //fixedDoc.DocumentPaginator.PageSize = GetPaperSize(reportPaperSize);

            return doc;

        }

        public static void ReplacePngsWithJpegs(Package package)
        {
            // We're modifying the enumerable as we iterate, so take a snapshot with ToList()
            foreach (var part in package.GetParts().ToList())
            {
                if (part.ContentType == "image/png")
                {
                    using (var jpegStream = new MemoryStream())
                    using (var image = System.Drawing.Image.FromStream(part.GetStream()))
                    {
                        image.Save(jpegStream, ImageFormat.Jpeg);
                        jpegStream.Seek(0, SeekOrigin.Begin);
                        // Cannot access Uri after part is removed, so store it
                        var uri = part.Uri;
                        package.DeletePart(uri);
                        var jpegPart = package.CreatePart(uri, "image/jpeg");
                        jpegStream.CopyTo(jpegPart.GetStream());
                    }
                }
            }
        }

        public ReportPaginator(DocumentPaginator paginator, Size pageSize, PageDefinition pd)
        {
            this.pageSize = pageSize;
            this.paginator = paginator;
            pageDef = pd;
            paginator.PageSize = new Size(pageSize.Width - pd.Margin.Width * 2, pageSize.Height - 2 * minimalOffset - pd.HeaderHeight - pd.FooterHeight - pd.Margin.Height * 2);
        }

        ContainerVisual getPartVisual(string template, int pageNo)
        {
            template = template.Replace("@PageNumber", (pageNo + 1).ToString());
            template = template.Replace("@PageCount", pageCount.ToString());
            var ph = ReportEngine.createReportPart<Section>(template);//, null);

            var tmpDoc = new FlowDocument
                             {
                                 ColumnWidth = double.PositiveInfinity,
                                 PageWidth = paginator.PageSize.Width
                             };

            tmpDoc.Blocks.Add(ph);
            DocumentPage dp = ((IDocumentPaginatorSource)tmpDoc).DocumentPaginator.GetPage(0);
            return (ContainerVisual)dp.Visual;

        }

        /// <summary>
        /// This is the most importan method , modifies the original 
        /// </summary>
        public override DocumentPage GetPage(int pageNumber)
        {
            DocumentPage page = paginator.GetPage(pageNumber);

            if (pageNumber == 0)
            {
                paginator.ComputePageCount();
                pageCount = paginator.PageCount;
            }

            var newpage = new ContainerVisual();
            if (pageDef.HeaderTemplate != null)
            {
                ContainerVisual v = getPartVisual(pageDef.HeaderTemplate, pageNumber);
                v.Offset = new Vector(pageDef.Margin.Width, pageDef.Margin.Height);
                newpage.Children.Add(v);
            }

            var smallerPage = new ContainerVisual();
            smallerPage.Children.Add(page.Visual);
            smallerPage.Offset = new Vector(pageDef.Margin.Width, pageDef.HeaderHeight + pageDef.Margin.Height + minimalOffset);
            newpage.Children.Add(smallerPage);

            if (pageDef.FooterTemplate != null)
            {
                ContainerVisual footer = getPartVisual(pageDef.FooterTemplate, pageNumber);
                footer.Offset = new Vector(pageDef.Margin.Width, pageSize.Height - pageDef.FooterHeight - pageDef.Margin.Height - minimalOffset);
                newpage.Children.Add(footer);
            }

            var dp = new DocumentPage(newpage, new Size(pageSize.Width, pageSize.Height), page.BleedBox, page.ContentBox);

            return dp;

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

    public class ReportDefinition
    {
        public string HeaderTemplate { get; set; }

        PageDefinition page;

        public PageDefinition Page
        {
            get
            {
                if (page == null)
                    page = new PageDefinition();

                return page;
            }
            set { page = value; }
        }

        public string FooterTemplate { get; set; }

        public string TableDefinition { get; set; }
    }

    public class PageDefinition
    {
        public Size Margin { get; set; }
        public string HeaderTemplate { get; set; }
        public string FooterTemplate { get; set; }

        double headerHeight;

        public double HeaderHeight
        {
            get
            {
                if (headerHeight < 0)
                    return 0;
                return headerHeight;
            }
            set { headerHeight = value; }
        }

        double footerHeight;

        public double FooterHeight
        {
            get
            {
                if (footerHeight >= 0)
                {
                    return footerHeight;
                }
                return 0;
            }
            set { footerHeight = value; }
        }
    }

    public class ReportEngine
    {
        static ParserContext xamlContext;
        /// <summary>
        /// Helps in namespace mapping during Xaml loading for each template
        /// </summary>
        public static ParserContext XamlContext
        {
            get
            {
                if (xamlContext == null)
                {
                    xamlContext = new ParserContext();
                    xamlContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                }
                return xamlContext;
            }
        }

        /// <summary>
        /// Creates report part
        /// </summary>
        internal static T createReportPart<T>(string template) where T : TextElement 
        {
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(template));
            var templatedItem = (T)XamlReader.Load(ms, XamlContext);
            return templatedItem;
        }

        public XpsDocument CreateReport(ReportDefinition rd)
        {
            const int pageWidth = 700;
            var fd = new FlowDocument {ColumnWidth = (pageWidth - 100)};
           
            var tableStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rd.TableDefinition));
            var table = (Table)XamlReader.Load(tableStream, XamlContext);

            fd.Blocks.Add(table);
            fd.Blocks.Add(createReportPart<Section>(rd.FooterTemplate));
            return ReportPaginator.CreateXpsDocument(fd, rd.Page);

        }
    }
}