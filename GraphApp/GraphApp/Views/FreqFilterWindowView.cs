using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphApp.Models;
using System.Numerics;
using System.IO;
using System.Windows.Media.TextFormatting;
using System.Windows.Media.Media3D;
using GraphApp.Models.Operations;

namespace GraphApp.Views
{
    public class FreqFilterWindowView : BaseView
    {
        private BitmapSource _result_image;
        private BitmapSource _result_fourier_image;
        private BitmapSource _result_filter_image;
        private Complex[] _fourier_array = new Complex[0];
        private Complex[] _filter_array = new Complex[0];
        private byte[] _result_bytes;
        private byte[] _result_fourier_bytes;
        private byte[] _result_filter_bytes;
        private IMGView _img;
        private string _status = "";
        private string _input = null;
        private List<List<double>> _values = new List<List<double>>();
        private int inner_r = 0;
        private int outer_r = 0;
        private int offset_X = 0;
        private int offset_Y = 0;
        private FreqFilterOperations _freqOperation = FreqFilterOperations.GetFreqFilterOperationsList()[0];


        public int Inner_r
        {
            get { return inner_r; }
            set { inner_r = value < 0 ? 0 : value; OnPropertyChanged(nameof(Inner_r)); }
        }
        public int Outer_r
        {
            get { return outer_r; }
            set { outer_r = value < 0 ? 0 : value;
                if (outer_r < inner_r)
                    outer_r = inner_r + 1;
                    OnPropertyChanged(nameof(Outer_r)); 
            }
        }
        public int Offset_X
        {
            get { return offset_X; }
            set { offset_X = value < 0 ? 0 : value; OnPropertyChanged(nameof(Offset_X)); }
        }
        public int Offset_Y
        {
            get { return offset_Y; }
            set { offset_Y = value < 0 ? 0 : value; OnPropertyChanged(nameof(Offset_Y)); }
        }
        public BitmapSource result_image
        {
            get { return _result_image; }
            set { _result_image = value; OnPropertyChanged(nameof(result_image)); }
        }
        public BitmapSource result_fourier_image
        {
            get { return _result_fourier_image; }
            set { _result_fourier_image = value; OnPropertyChanged(nameof(result_fourier_image)); }
        }
        public BitmapSource result_filter_image
        {
            get { return _result_filter_image; }
            set { _result_filter_image = value; OnPropertyChanged(nameof(result_filter_image)); }
        }
        public IMGView Img
        {
            get { return _img; }
            set { _img = value; OnPropertyChanged(nameof(Img)); }
        }
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public string Input
        {
            get { return _input; }
            set { _input = value; OnPropertyChanged(nameof(Input)); }
        }
        public List<List<double>> Values
        {
            get { return _values; }
            set { _values = value; OnPropertyChanged(nameof(Values)); }
        }

        public FreqFilterOperations FreqOperation
        {
            get { return  _freqOperation; }
            set { _freqOperation = value; OnPropertyChanged(nameof(FreqOperation)); }
        }


        private void CalculateFourier()
        {
            int[] buffer_ints = new int[Img.Height * Img.Width * 4];
            byte[] buffer_bytes = new byte[Img.Height * Img.Width * 4];
            int[] b_channel_ints = new int[Img.Height * Img.Width];
            int[] g_channel_ints = new int[Img.Height * Img.Width];
            int[] r_channel_ints = new int[Img.Height * Img.Width];
            int width = Img.Width;
            int height = Img.Height;
            for (int i = 0; i < buffer_bytes.Length / 4; i++)
            {
                b_channel_ints[i] = (int)Img.ImgBytes[i * 4 + 0] * (int)Math.Pow(-1, i / width + i % width);
                g_channel_ints[i] = (int)Img.ImgBytes[i * 4 + 1] * (int)Math.Pow(-1, i / width + i % width);
                r_channel_ints[i] = (int)Img.ImgBytes[i * 4 + 2] * (int)Math.Pow(-1, i / width + i % width);
                buffer_bytes[i * 4 + 3] = 255;
            }
            _fourier_array = new Complex[Img.Height * Img.Width * 4];
            int N = height;
            int M = width;
            Complex[] buff_array = new Complex[Img.Width * Img.Height * 4];


            Parallel.For(0, N, w =>
            {
                for (int n = 0; n < M; n++)
                { 
                    double _fiM = -2.0 * Math.PI * n / M;
                    for (int m = 0; m < M; m++)
                    {
                        double fiM = _fiM * m;
                        buff_array[w * width * 4 + n * 4 + 0] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * b_channel_ints[w * width + m]);
                        buff_array[w * width * 4 + n * 4 + 1] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * g_channel_ints[w * width + m]);
                        buff_array[w * width * 4 + n * 4 + 2] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * r_channel_ints[w * width + m]);
                    }
                    buff_array[w * width * 4 + n * 4 + 0] = 1.0 / M * buff_array[w * width * 4 + n * 4 + 0];
                    buff_array[w * width * 4 + n * 4 + 1] = 1.0 / M * buff_array[w * width * 4 + n * 4 + 1];
                    buff_array[w * width * 4 + n * 4 + 2] = 1.0 / M * buff_array[w * width * 4 + n * 4 + 2];
                }

            });
            Parallel.For(0, M, w =>
            {
                for (int n = 0; n < N; n++)
                {
                    double _fiM = -2.0 * Math.PI * n / N;
                    for (int m = 0; m < N; m++)
                    {
                        double fiM = _fiM * m;
                        _fourier_array[n * width * 4 + w * 4 + 0] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 0]);
                        _fourier_array[n * width * 4 + w * 4 + 1] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 1]);
                        _fourier_array[n * width * 4 + w * 4 + 2] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 2]);
                    }
                    _fourier_array[n * width * 4 + w * 4 + 0] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 0];
                    _fourier_array[n * width * 4 + w * 4 + 1] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 1];
                    _fourier_array[n * width * 4 + w * 4 + 2] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 2];
                }

            });

            double max_log_b = 0;
            double max_log_g = 0;
            double max_log_r = 0;
            for (int i = 0; i < height * width * 4; i += 4)
            {
                if (max_log_b < Math.Log(_fourier_array[i].Magnitude + 1))
                    max_log_b = Math.Log(_fourier_array[i].Magnitude + 1);
                if (max_log_g < Math.Log(_fourier_array[i + 1].Magnitude + 1))
                    max_log_g = Math.Log(_fourier_array[i + 1].Magnitude + 1);
                if (max_log_r < Math.Log(_fourier_array[i + 2].Magnitude + 1))
                    max_log_r = Math.Log(_fourier_array[i + 2].Magnitude + 1);

            }
            for (int i = 0; i < height * width * 4; i += 4)
            {
                buffer_bytes[i] = (byte)(int)(Math.Ceiling(Math.Log(_fourier_array[i].Magnitude + 1) * (255.0 / max_log_b)));
                buffer_bytes[i + 1] = (byte)(int)(Math.Ceiling(Math.Log(_fourier_array[i + 1].Magnitude + 1) * (255.0 / max_log_g)));
                buffer_bytes[i + 2] = (byte)(int)(Math.Ceiling(Math.Log(_fourier_array[i + 2].Magnitude + 1) * (255.0 / max_log_r)));
                buffer_bytes[i + 3] = 255;

            }
            result_fourier_image = BitmapSource.Create(Img.Width, Img.Height, 48, 48, PixelFormats.Bgra32, null, buffer_bytes, Img.Width * 4);
            CalculateFreqFiltr(max_log_b, max_log_g, max_log_r);
        }
        private void CalculateFreqFiltr(double max_log_b, double max_log_g, double max_log_r)
        {
            int width = Img.Width;
            int height = Img.Height;
            byte[] buffer_bytes = new byte[Img.Height * Img.Width * 4];
            _filter_array = FreqOperation._operation(_fourier_array, inner_r, outer_r, width * 4, offset_X, offset_Y);

            for (int i = 0; i < height * width * 4; i += 4)
            {
                if (max_log_b < Math.Log(_filter_array[i].Magnitude + 1))
                    max_log_b = Math.Log(_filter_array[i].Magnitude + 1);
                if (max_log_g < Math.Log(_filter_array[i + 1].Magnitude + 1))
                    max_log_g = Math.Log(_filter_array[i + 1].Magnitude + 1);
                if (max_log_r < Math.Log(_filter_array[i + 2].Magnitude + 1))
                    max_log_r = Math.Log(_filter_array[i + 2].Magnitude + 1);

            }
            for (int i = 0; i < height * width * 4; i += 4)
            {
                buffer_bytes[i] = (byte)(int)(Math.Ceiling(Math.Log(_filter_array[i].Magnitude + 1) * (255.0 / max_log_b)));
                buffer_bytes[i + 1] = (byte)(int)(Math.Ceiling(Math.Log(_filter_array[i + 1].Magnitude + 1) * (255.0 / max_log_g)));
                buffer_bytes[i + 2] = (byte)(int)(Math.Ceiling(Math.Log(_filter_array[i + 2].Magnitude + 1) * (255.0 / max_log_r)));
                buffer_bytes[i + 3] = 255;

            }
            result_filter_image = BitmapSource.Create(Img.Width, Img.Height, 48, 48, PixelFormats.Bgra32, null, buffer_bytes, Img.Width * 4);
        }
        private void CalculateResultImage()
        {
            int[] buffer_ints = new int[Img.Height * Img.Width * 4];
            byte[] buffer_bytes = new byte[Img.Height * Img.Width * 4];
            int width = Img.Width;
            int height = Img.Height;            
            int N = height;
            int M = width;
            Complex[] buff_array = new Complex[Img.Width * Img.Height * 4];
            Complex[] result_array = new Complex[Img.Width * Img.Height * 4];

            Parallel.For(0, N, w =>
            {
                for (int n = 0; n < M; n++)
                {
                    double _fiM = 2.0 * Math.PI * n / M;
                    for (int m = 0; m < M; m++)
                    {
                        double fiM = _fiM * m;
                        buff_array[w * width * 4 + n * 4 + 0] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * _filter_array[w * width * 4 + m * 4]);
                        buff_array[w * width * 4 + n * 4 + 1] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * _filter_array[w * width * 4 + m * 4+ 1]);
                        buff_array[w * width * 4 + n * 4 + 2] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * _filter_array[w * width * 4 + m * 4+ 2]);
                    }
                }
            });
            Parallel.For(0, M, w =>
            {
                for (int n = 0; n < N; n++)
                {
                    double _fiM = 2.0 * Math.PI * n / N;
                    for (int m = 0; m < N; m++)
                    {
                        double fiM = _fiM * m;
                        result_array[n * width * 4 + w * 4 + 0] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 0]);
                        result_array[n * width * 4 + w * 4 + 1] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 1]);
                        result_array[n * width * 4 + w * 4 + 2] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width * 4 + w * 4 + 2]);
                    }
                }

            });

            double max_log_b = 0;
            double max_log_g = 0;
            double max_log_r = 0;
            for (int i = 0; i < height * width * 4; i += 4)
            {
                if (max_log_b < Math.Log(result_array[i].Magnitude + 1))
                    max_log_b = Math.Log(result_array[i].Magnitude + 1);
                if (max_log_g < Math.Log(result_array[i + 1].Magnitude + 1))
                    max_log_g = Math.Log(result_array[i + 1].Magnitude + 1);
                if (max_log_r < Math.Log(result_array[i + 2].Magnitude + 1))
                    max_log_r = Math.Log(result_array[i + 2].Magnitude + 1);

            }
            for (int i = 0; i < height * width * 4; i += 4)
            {
                //buffer_bytes[i] = (byte)(int)(Math.Ceiling(Math.Log(result_array[i].Magnitude + 1) * (255.0 / max_log_b)));
                //buffer_bytes[i + 1] = (byte)(int)(Math.Ceiling(Math.Log(result_array[i + 1].Magnitude + 1) * (255.0 / max_log_g)));
                //buffer_bytes[i + 2] = (byte)(int)(Math.Ceiling(Math.Log(result_array[i + 2].Magnitude + 1) * (255.0 / max_log_r)));
                //buffer_bytes[i] = (byte)(int)(Math.Round((result_array[i].Real + result_array[i].Imaginary) / 2.0));
                //buffer_bytes[i + 1] = (byte)(int)(Math.Round((result_array[i].Real + result_array[i].Imaginary) / 2.0));
                //buffer_bytes[i + 2] = (byte)(int)(Math.Round((result_array[i].Real + result_array[i].Imaginary) / 2.0));
                buffer_bytes[i] = (byte)(int)(Math.Round(result_array[i].Magnitude) > 255 ? 255 : Math.Round(result_array[i].Magnitude));
                buffer_bytes[i + 1] = (byte)(int)(Math.Round(result_array[i + 1].Magnitude) > 255 ? 255 : Math.Round(result_array[i + 1].Magnitude));
                buffer_bytes[i + 2] = (byte)(int)(Math.Round(result_array[i + 2].Magnitude) > 255 ? 255 : Math.Round(result_array[i + 2].Magnitude));
                buffer_bytes[i + 3] = 255;

            }

            result_image = BitmapSource.Create(Img.Width, Img.Height, 48, 48, PixelFormats.Bgra32, null, buffer_bytes, Img.Width * 4);
        }
        public void CalculateLayers()
        {
            try
            {
                CalculateFourier();
                CalculateResultImage();
            }
            catch (Exception)
            {

            }
        }

        public void AddIMGView(IMGView img)
        {
            if (img != null)
                this.Img = img;
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

        }
}
