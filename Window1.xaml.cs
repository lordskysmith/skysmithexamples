using Microsoft.Win32;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        public void CreatePDF()
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = Txtbox_ESN.Text + " As Shipped Photos";

            int fileCount = 1;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "As Shipped Photos|*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
//                ProgressBarWindow window = new ProgressBarWindow();
//                window.Show();

                PdfPage coverpage = document.AddPage();
                coverpage.Orientation = PageOrientation.Landscape;

                XGraphics gfx = XGraphics.FromPdfPage(coverpage);

                XFont font = new XFont("GE Inspira Sans", 100, XFontStyle.Bold);

                gfx.DrawString(Txtbox_ESN.Text, font, XBrushes.Black, new XRect(0, 0, coverpage.Width, coverpage.Height), XStringFormats.Center);

                foreach (string file in openFileDialog.FileNames)
                {
                    var bitmap = new BitmapImage();

                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        bitmap.BeginInit();
                        bitmap.DecodePixelWidth = bitmap.DecodePixelWidth / 100;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                    }

                    if (bitmap.PixelWidth > 2048)
                    {
                        var scaleFactor = 2048d / bitmap.PixelWidth;
                    }

                    var resizedBitmap = new TransformedBitmap(bitmap, new ScaleTransform(scaleFactor, scaleFactor));

                    var encoder = new JpegBitmapEncoder();
                    encoder.QualityLevel = 80;
                    encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));

                    using (var stream = new FileStream(file + ".tmp.jpg", FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    XImage img = XImage.FromFile(file + ".tmp.jpg");

                    PdfPage page = document.AddPage();
                    page.Width = img.PointWidth;
                    page.Height = img.PointHeight;

                    XGraphics xgr = XGraphics.FromPdfPage(document.Pages[fileCount++]);

                    double leftOffset = (xgr.PageSize.Width / 2.0) - (img.PointWidth / 2.0);
                    double topOffset = (xgr.PageSize.Height / 2.0) - (img.PointHeight / 2.0);
                    xgr.DrawImage(img, leftOffset, topOffset);

                    File.Delete(file + ".tmp.jpg");
                }

                document.Save(Txtbox_ESN.Text + ".pdf");

                Process.Start(Txtbox_ESN.Text + ".pdf");

                this.Close();
//                window.Close();
            }

        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(Txtbox_ESN.Text))
            {
//                Properties.Settings.Default.setESN = Txtbox_ESN.Text;

                CreatePDF();
            }
            else if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrEmpty(Txtbox_ESN.Text))
                {
                    Txtbox_ESN.Clear();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void Btn_CLB_Click(object sender, RoutedEventArgs e)
        {
            Txtbox_ESN.Clear();
        }

        private void Btn_Generate_PDF_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Txtbox_ESN.Text))
            {
//                Properties.Settings.Default.setESN = Txtbox_ESN.Text;

                CreatePDF();
            }
        }
    }
}
