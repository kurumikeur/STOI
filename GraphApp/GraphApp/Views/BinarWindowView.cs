using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphApp.Models;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;

namespace GraphApp.Views
{
    public class BinarWindowView : BaseView
    {
        private byte[] converted_bytes;
        private BitmapSource _input_image = BitmapSource.Create(1, 1, 1, 1, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        private BitmapSource _result_image = BitmapSource.Create(1, 1, 1, 1, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        private BitmapSource _result_convertedimage = BitmapSource.Create(1, 1, 1, 1, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        private IMGView _img = new IMGView();
        private BinarOperations _binaroperation = BinarOperations.GetBinarOperationsList()[0];
        private double _k = -0.2;
        private int _area = 10;
        private int _x;
        private int _y;

        public byte[] ConvertedBytes
        {
            get { return converted_bytes; }
        }
        public BitmapSource result_image
        {
            get
            {
                return _result_image;
            }
            set
            {
                _result_image = value;
                OnPropertyChanged(nameof(result_image));
            }
        }
        public BitmapSource result_convertedimage
        {
            get
            {
                return _result_convertedimage;
            }
            set
            {
                _result_convertedimage = value;
                OnPropertyChanged(nameof(result_convertedimage));
            }
        }
        public BitmapSource input_image
        {
            get
            {
                return _input_image;
            }
            set
            {
                _input_image = value;
                OnPropertyChanged(nameof(input_image));
            }
        }
        public IMGView Img
        {
            get
            {
                return _img;
            }
            set
            {
                _img = value;
            }
        }
        public BinarOperations BinarOperation
        {
            get { return _binaroperation; }
            set { _binaroperation = value; OnPropertyChanged(nameof(BinarOperation)); CalculateLayers(); }
        }

        public double K
        {
            get { return _k; }
            set { _k = value; OnPropertyChanged(nameof(K)); CalculateLayers(); }

        }
        public int Area
        {
            get { return _area; }
            set { 
                if (value > 0)
                {
                    _area = value;
                }
                else
                    _area = 1;
                OnPropertyChanged(nameof(Area));
                CalculateLayers();
            }
        }
        public int X
        {
            get { return _x; }
            set
            {
                if (value < 0)
                    _x = 0;
                else if (value > 255)
                    _x = 255;
                else
                    _x = value;
            }
        }
        public int Y
        {
            get { return _y; }
            set
            {
                if (value < 0)
                    _y = 0;
                else if (value > 255)
                    _y = 255;
                else
                    _y = value;
            }
        }
        private void CalculateConvertedImg()
        {
            this.converted_bytes = new byte[Img.Height * Img.Width];
            int img_size = Img.Width * Img.Height * 4;
            Parallel.For(0, img_size / 4, index =>
            {
                int pix = index * 4;
                double b = Img.ImgBytes[pix];
                double g = Img.ImgBytes[pix + 1];
                double r = Img.ImgBytes[pix + 2];
                this.converted_bytes[index] = (byte)(int)Math.Round(r * 0.2125 + g * 0.7154 + b * 0.0721);
            });
            

            result_convertedimage = BitmapSource.Create(Img.Width, Img.Height, 96, 96, PixelFormats.Gray8, null, this.converted_bytes, Img.Width);
        }
        private void CalculateResultImg()
        {
            int img_size = Img.Width * Img.Height * 4;
            byte[] buffer_bytes = BinarOperation._operation(this);

            result_image = BitmapSource.Create(Img.Width, Img.Height, 96, 96, PixelFormats.Gray8, null, buffer_bytes, Img.Width);
        }
        public List<double> CalculateHistogramBytes()
        {
            List<double> buffer = new List<double>(new double[256]);
            int img_size = Img.Width * Img.Height;

            for (int index = 0; index < img_size; index++)
                buffer[converted_bytes[index]] += 1;

            Parallel.For(0, 256, index =>
            {
                buffer[index] = (buffer[index] / img_size);

            });

            return buffer;

        }
        public void CalculateLayers()
        {
            if (Img.ImgData == null)
            {
                return;
            }
            CalculateConvertedImg();
            CalculateResultImg();
            input_image = Img.ImgData;
        }
        public void AddIMGView(IMGView _imgv)
        {
            Img = _imgv;
        }
        public void DeleteImg(IMGView Img)
        {
            if (Img != null)
            {
                Img.ImgData = null;
                Img.ImgBytes = null;
            }
        }
        public void OpenFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All files (*.*) |*.jpg;*.png;*.jpeg;*.gif| JPG Files (*.jpg) | JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|*.jpg|GIF Files (*.gif)|*.gif";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                FileInfo F = new FileInfo(filename);
                try
                {
                    AddIMGView(new IMGView { ImgData = new BitmapImage(new Uri(filename)), Name = F.Name });
                    CalculateLayers();
                }
                catch (Exception ex)
                {

                }
            }
        }
        public void SaveFile()
        {
            if (result_image == null)
                return;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "PNG Image|*.png";
            dlg.Title = "Save an Image File";
            dlg.ShowDialog();
            var Image = result_image;
            if (dlg.FileName != "")
            {
                using (System.IO.FileStream fs = (System.IO.FileStream)dlg.OpenFile())
                {

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(Image));
                    encoder.Save(fs);
                    fs.Close();
                }
            }
        }

        public BinarWindowView()
        {
        }

    }
}
