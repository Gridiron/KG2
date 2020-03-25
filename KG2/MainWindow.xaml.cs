using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using DmitryBrant.ImageFormats;
using System.Diagnostics;

namespace KG2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly List<ImageInfo> images = new List<ImageInfo>();
        public MainWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = images;
        }

        private static short GetCompressionType(Image image)
        {
            var compressionTagIndex = Array.IndexOf(image.PropertyIdList, 0x103);
            var compressionTag = image.PropertyItems[compressionTagIndex];
            return BitConverter.ToInt16(compressionTag.Value, 0);
        }

        readonly Dictionary<long, string> CompressType = new Dictionary<long, string>()
        {
            {1, "Uncompressed" } ,
            {2, "CCITT modified Huffman RLE"},
            {32773, "PackBits"},
            {3, "CCITT3"},
            {4, "CCITT4"},
            {5, "LZW"},
            {6, "JPEG_old"},
            {7, "JPEG_new"},
            {32946, "DeflatePKZIP"},
            {8, "DeflateAdobe"},
            {9, "JBIG_85"},
            {10, "JBIG_43"},
            {11, "JPEG"},
            {12, "JPEG"},
            {32766, "RLE_NeXT"},
            {32809, "RLE_ThunderScan"},
            {32895, "RasterPadding"},
            {32896, "RLE_LW"},
            {32897, "RLE_HC"},
            {32947, "RLE_BL"},
            {34661, "JBIG"},
            {34713, "Nikon_NEF"},
            {34712,"JPEG2000"}
        };

        private void ViewInfo(string[] files)
        {
            foreach (string s in files)
            {
                var str = "";
                var fileInfo = new FileInfo(s);
                Image image;

                if (fileInfo.Extension.ToLower() == ".pcx")
                {
                    image = PcxReader.Load(s);
                }
                else
                {
                    image = Image.FromFile(s);
                }

                var res = string.Empty;
                if (fileInfo.Extension.ToLower() == ".tif")
                {
                    var temp_res = GetCompressionType(image);
                    var final_res = temp_res < 0 ? 3 : temp_res;
                    res = CompressType[final_res];
                }
                else if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    res = "Baseline";
                }
                else if (fileInfo.Extension.ToLower() == ".png")
                {
                    res = "Deflate";
                }
                else if (fileInfo.Extension.ToLower() == ".png")
                {
                    res = "None";
                }

                var size = image.Height * image.Width;

                string pixelFormat = Image.GetPixelFormatSize(image.PixelFormat).ToString();
                images.Add(new ImageInfo
                    {
                        FileName = fileInfo.Name,
                        Size = size,
                        Dpi = Convert.ToInt32(image.HorizontalResolution),
                        ColorDepth = pixelFormat,
                        Comression = res
                    }
                );
                image.Dispose();
            }
        }

        private void ViewInfoParallel(string s)
        {
            var str = "";
            var fileInfo = new FileInfo(s);
            Image image;

            if (fileInfo.Extension.ToLower() == ".pcx")
            {
                image = PcxReader.Load(s);
            }
            else
            {
                image = Image.FromFile(s);
            }

            var res = string.Empty;
            if (fileInfo.Extension.ToLower() == ".tif")
            {
                var tempRes = GetCompressionType(image);
                var finalRes = tempRes < 0 ? 3 : tempRes;
                res = CompressType[finalRes];
            }
            else if (fileInfo.Extension.ToLower() == ".jpg")
            {
                res = "Baseline";
            }
            else if (fileInfo.Extension.ToLower() == ".png")
            {
                res = "Deflate";
            }
            else if (fileInfo.Extension.ToLower() == ".png")
            {
                res = "None";
            }

            var size = image.Height * image.Width;

            string pixelFormat = Image.GetPixelFormatSize(image.PixelFormat).ToString();
            images.Add(new ImageInfo
                { 
                    FileName=fileInfo.Name,
                    Size=size,
                    Dpi= Convert.ToInt32(image.HorizontalResolution),
                    ColorDepth=pixelFormat,
                    Comression=res
                }
            );
            image.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var op = new FolderBrowserDialog();
            var dirName = string.Empty;
            var sw = new Stopwatch();

            images.Clear();

            if (op.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dirName = op.SelectedPath;
                sw.Start();
            }

            if (Directory.Exists(dirName))
            {
                var files = Directory.GetFiles(dirName);
                Parallel.ForEach(files, ViewInfoParallel);
            }

            dataGrid.Items.Refresh();

            sw.Stop();

            
        }
    }
}
