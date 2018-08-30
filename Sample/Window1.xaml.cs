using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using FlowElements;

namespace Sample
{
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
        }

        const string PackUriName = "pack://temp.xps";
        readonly Uri PackUri = new Uri(PackUriName);

        XpsDocument doc;

        Package package;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (doc != null) doc.Close();
            if (package != null)
            {
                PackageStore.RemovePackage(PackUri);
                package.Flush();
                package.Close();
            }

            var ms = new MemoryStream();
            package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            doc = new XpsDocument(package, CompressionOption.SuperFast, PackUriName);
            PackageStore.AddPackage(PackUri, package);

            var pg = new SampleReport { Report = { DataContext = new SampleVM() } };
            var report = pg.report;
            //var report = pg.Report;

            report.HeaderHeight = int.Parse(hh.Text);
            report.FooterHeight = int.Parse(fh.Text);

            report.PagePadding = new Thickness(int.Parse(paddingLeft.Text), int.Parse(paddingTop.Text), int.Parse(paddingRight.Text), int.Parse(paddingBottom.Text));

            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(doc);
            xpsWriter.Write(new XpsReportPaginator(report, new Size(96 / 2.54 * 21, 96 / 2.54 * 28.7)));

            dv.Document = doc.GetFixedDocumentSequence();
        }
    }
}
