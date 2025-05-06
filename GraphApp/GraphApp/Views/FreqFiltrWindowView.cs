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

namespace GraphApp.Views
{
    public class FreqFiltrWindowView : BaseView
    {
        private BitmapSource _result_image;
        private BitmapSource _result_fourier_image;
        private Complex[] _fourier_array = new Complex[0];
        private byte[] _result_bytes;
        private IMGView _img;
        private string _status = "";
        private string _input = null;
        private List<List<double>> _values = new List<List<double>>();
        private int median_area = 1;
        private int gauss_area = 1;
        private double sigma = 1;

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
        public int Median_area
        {
            get { return median_area; }
            set { median_area = value % 2 == 0 ? value + 1 : value; median_area = median_area <= 0 ? 1 : median_area; OnPropertyChanged(nameof(Median_area)); }
        }
        public int Gauss_area
        {
            get { return gauss_area; }
            set { gauss_area = value % 2 == 0 ? value + 1 : value; gauss_area = gauss_area <= 0 ? 1 : gauss_area; OnPropertyChanged(nameof(Gauss_area)); }
        }
        public double Sigma
        {
            get { return sigma; }
            set { sigma = value; OnPropertyChanged(nameof(Sigma)); }
        }


        private void CalculateFourierImage()
        {
            int[] buffer_ints= new int[Img.Height * Img.Width * 4];
            byte[] buffer_bytes = new byte[Img.Height * Img.Width * 4];
            int[] b_channel_ints = new int[Img.Height * Img.Width];
            int[] g_channel_ints = new int[Img.Height * Img.Width];
            int[] r_channel_ints = new int[Img.Height * Img.Width];
            int width = Img.Width;
            int height = Img.Height;
            for (int i = 0; i < buffer_bytes.Length / 4; i++)
            {
                b_channel_ints[i] = (int)Img.ImgBytes[i + 0] * (int)Math.Pow(-1, (i / (Img.Width)) + ((i % (Img.Width))));
                g_channel_ints[i] = (int)Img.ImgBytes[i + 1] * (int)Math.Pow(-1, (i / (Img.Width)) + ((i % (Img.Width))));
                r_channel_ints[i] = (int)Img.ImgBytes[i + 2] * (int)Math.Pow(-1, (i / (Img.Width)) + ((i % (Img.Width))));
                buffer_bytes[i * 4 + 3] = 255;
            }
            for (int i = 0; i < buffer_bytes.Length; i++)
            {
                if (i % 4 == 3)
                {
                    buffer_ints[i] = 255;
                    continue;
                }
                buffer_ints[i] = (int)Img.ImgBytes[i] * (int)Math.Pow(-1, (i / (Img.Width * 4)) + ((i % (Img.Width * 4)) / 4));
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
                        _fourier_array[n * width * 4 + w * 4 + 0] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width + w * 4 + 0]);
                        _fourier_array[n * width * 4 + w * 4 + 1] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width + w * 4 + 1]);
                        _fourier_array[n * width * 4 + w * 4 + 2] += (new Complex(Math.Cos(fiM), Math.Sin(fiM)) * buff_array[m * width + w * 4 + 2]);
                    }
                    _fourier_array[n * width * 4 + w * 4 + 0] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 0];
                    _fourier_array[n * width * 4 + w * 4 + 1] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 1];
                    _fourier_array[n * width * 4 + w * 4 + 2] = 1.0 / N * _fourier_array[n * width * 4 + w * 4 + 2];
                }

            });

            double max_val = 0;
            for (int i = 0; i < height * width * 4; i++)
            {
                double avg = Math.Sqrt(Math.Pow(_fourier_array[i].Imaginary, 2) + Math.Pow(_fourier_array[i].Real, 2));
                if (max_val < Math.Log(avg + 1))
                    max_val = Math.Log(avg + 1);

            }

            for (int i = 0; i < height * width * 4; i++)
            {
                if (i % 4 == 3) { 
                    buffer_bytes[i] = 255;
                    continue;
                }
                double avg = Math.Sqrt(Math.Pow(_fourier_array[i].Imaginary, 2) + Math.Pow(_fourier_array[i].Real, 2)) * 25;
                buffer_bytes[i] = (byte)(int)(Math.Round(avg));
                
            }



            result_fourier_image = BitmapSource.Create(Img.Width, Img.Height, 96, 96, PixelFormats.Bgra32, null, buffer_bytes, Img.Width * 4);
        }
        //private void CalculateMedianSquare(string position, string side, byte[] buffer_bytes, double[] buffer_doubles)
        //{
        //    bool isCenterX = false, isCenterY = false;
        //    int x_area = 0, y_area = 0;
        //    x_area = Median_area;
        //    y_area = Median_area;

        //    if (Img == null || _input == null)
        //        return;
        //    int width = Img.Width, stride = width * 4;
        //    int height = Img.Height;
        //    int buffer_size = width * height * 4;
        //    int start_pix_y, start_pix_x, last_pix_y, last_pix_x;
        //    int k_y, k_x;
        //    switch (position)
        //    {
        //        case "Top":
        //            k_y = 1;
        //            start_pix_y = 0;
        //            last_pix_y = stride * y_area;
        //            break;
        //        case "Center":
        //            isCenterY = true;
        //            k_y = 1;
        //            start_pix_y = stride * y_area;
        //            last_pix_y = buffer_size - stride * y_area;
        //            break;
        //        case "Bottom":
        //            k_y = -1;
        //            start_pix_y = buffer_size - stride * y_area;
        //            last_pix_y = buffer_size;
        //            break;
        //        default:
        //            return;
        //    }
        //    switch (side)
        //    {
        //        case "Left":
        //            k_x = 1;
        //            start_pix_x = 0;
        //            last_pix_x = x_area * 4;
        //            break;
        //        case "Center":
        //            k_x = 1;
        //            isCenterX = true;
        //            start_pix_x = x_area * 4;
        //            last_pix_x = stride - x_area * 4;
        //            break;
        //        case "Right":
        //            k_x = -1;
        //            start_pix_x = stride - x_area * 4;
        //            last_pix_x = stride;
        //            break;
        //        default:
        //            return;
        //    }

        //    Parallel.For(start_pix_y / stride, last_pix_y / stride, i =>
        //    {
        //        i *= stride;
        //        int offset_y = -k_y * ((height + (k_y * i / stride)) % height);
        //        for (int n = (i + start_pix_x); n < (i + last_pix_x); n += 4)
        //        {
        //            List<int> B_val = new List<int>();
        //            List<int> G_val = new List<int>();
        //            List<int> R_val = new List<int>();
        //            List<int> A_val = new List<int>();
        //            int k1_x = k_x;
        //            int k1_y = k_y;
        //            int offset_x = -k_x * ((stride + (k_x * n % stride)) % stride) / 4;
        //            int pix_x, pix_y, pix;
        //            for (int x = -x_area; x <= x_area; x++)
        //            {
        //                if (!isCenterX)
        //                {
        //                    if (-k_x * x >= offset_x)
        //                        k1_x = -k_x;
        //                    else
        //                        k1_x = k_x;
        //                }
        //                pix_x = k1_x * k_x * x * 4;

        //                for (int y = -y_area; y <= y_area; y++)
        //                {
        //                    if (!isCenterY)
        //                    {
        //                        if (-k_y * y >= offset_y)
        //                            k1_y = -k_y;
        //                        else
        //                            k1_y = k_y;
        //                    }
        //                    pix_y = k1_y * k_y * y * stride;
        //                    pix = pix_x + pix_y + n;
        //                    B_val.Add(buffer_bytes[pix]);
        //                    G_val.Add(buffer_bytes[pix + 1]);
        //                    R_val.Add(buffer_bytes[pix + 2]);
        //                    A_val.Add(buffer_bytes[pix + 3]);
        //                }
        //            }
        //            int median = B_val.Count / 2;
        //            B_val.Sort();
        //            G_val.Sort();
        //            R_val.Sort();
        //            A_val.Sort();
        //            _result_bytes[n] = (byte)(B_val.Count() % 2 != 0 ? B_val[median] : (B_val[median] + B_val[median + 1]) / 2);
        //            _result_bytes[n + 1] = (byte)(G_val.Count() % 2 != 0 ? G_val[median] : (G_val[median] + G_val[median + 1]) / 2);
        //            _result_bytes[n + 2] = (byte)(R_val.Count() % 2 != 0 ? R_val[median] : (R_val[median] + R_val[median + 1]) / 2);
        //            _result_bytes[n + 3] = (byte)(A_val.Count() % 2 != 0 ? A_val[median] : (A_val[median] + A_val[median + 1]) / 2);

        //        }
        //    });
        //}
        //private void CalculateSquare(string position, string side, byte[] buffer_bytes, double[] buffer_doubles)
        //{
        //    bool isCenterX = false, isCenterY = false;
        //    int x_area = 0, y_area = 0;
        //    if (Img == null || _input == null)
        //        return;
        //    int width = Img.Width, stride = width * 4;
        //    int height = Img.Height;
        //    int buffer_size = width * height * 4;
        //    int start_pix_y, start_pix_x, last_pix_y, last_pix_x;
        //    int k_y, k_x;
        //    switch (position)
        //    {
        //        case "Top":
        //            k_y = 1;
        //            start_pix_y = 0;
        //            last_pix_y = stride * y_area;
        //            break;
        //        case "Center":
        //            isCenterY = true;
        //            k_y = 1;
        //            start_pix_y = stride * y_area;
        //            last_pix_y = buffer_size - stride * y_area;
        //            break;
        //        case "Bottom":
        //            k_y = -1;
        //            start_pix_y = buffer_size - stride * y_area;
        //            last_pix_y = buffer_size;
        //            break;
        //        default:
        //            return;
        //    }
        //    switch (side)
        //    {
        //        case "Left":
        //            k_x = 1;
        //            start_pix_x = 0;
        //            last_pix_x = x_area * 4;
        //            break;
        //        case "Center":
        //            k_x = 1;
        //            isCenterX = true;
        //            start_pix_x = x_area * 4;
        //            last_pix_x = stride - x_area * 4;
        //            break;
        //        case "Right":
        //            k_x = -1;
        //            start_pix_x = stride - x_area * 4;
        //            last_pix_x = stride;
        //            break;
        //        default:
        //            return;
        //    }

        //    Parallel.For(start_pix_y / stride, last_pix_y / stride, i =>
        //    {
        //        i *= stride;
        //        int offset_y = -k_y * ((height + (k_y * i / stride)) % height);
        //        for (int n = (i + start_pix_x); n < (i + last_pix_x); n += 4)
        //        {
        //            int k1_x = k_x;
        //            int k1_y = k_y;
        //            int offset_x = -k_x * ((stride + (k_x * n % stride)) % stride) / 4;
        //            int pix_x, pix_y, pix;
        //            for (int x = -x_area; x <= x_area; x++)
        //            {
        //                if (!isCenterX)
        //                {
        //                    if (-k_x * x >= offset_x)
        //                        k1_x = -k_x;
        //                    else
        //                        k1_x = k_x;
        //                }
        //                pix_x = k1_x * k_x * x * 4;

        //                for (int y = -y_area; y <= y_area; y++)
        //                {
        //                    if (!isCenterY)
        //                    {
        //                        if (-k_y * y >= offset_y)
        //                            k1_y = -k_y;
        //                        else
        //                            k1_y = k_y;
        //                    }
        //                    pix_y = k1_y * k_y * y * stride;
        //                    pix = pix_x + pix_y + n;
        //                    buffer_doubles[n] += buffer_bytes[pix] * _values[y + y_area][x + x_area];
        //                    buffer_doubles[n + 1] += buffer_bytes[pix + 1] * _values[y + y_area][x + x_area];
        //                    buffer_doubles[n + 2] += buffer_bytes[pix + 2] * _values[y + y_area][x + x_area];
        //                    buffer_doubles[n + 3] += buffer_bytes[pix + 3] * _values[y + y_area][x + x_area];
        //                }
        //            }
        //            _result_bytes[n] = (byte)((int)Math.Round(buffer_doubles[n]));
        //            _result_bytes[n + 1] = (byte)((int)Math.Round(buffer_doubles[n + 1]));
        //            _result_bytes[n + 2] = (byte)((int)Math.Round(buffer_doubles[n + 2]));
        //            _result_bytes[n + 3] = (byte)((int)Math.Round(buffer_doubles[n + 3]));

        //        }
        //        ;
        //    });
        //}
        //private void CalculateSquares()
        //{
        //    int x_area = _values[0].Count() / 2;
        //    int y_area = _values.Count() / 2;
        //    if (Img == null || _input == null)
        //        return;
        //    int width = Img.Width, stride = width * 4;
        //    int height = Img.Height;
        //    int buffer_size = width * height * 4;
        //    byte[] buffer_bytes = Img.ImgBytes;
        //    double[] buffer_doubles = new double[buffer_size];
        //    _result_bytes = new byte[buffer_size];

        //    CalculateSquare("Top", "Left", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Top", "Center", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Top", "Right", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Center", "Left", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Center", "Center", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Center", "Right", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Bottom", "Left", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Bottom", "Center", buffer_bytes, buffer_doubles);
        //    CalculateSquare("Bottom", "Right", buffer_bytes, buffer_doubles);

        //    CalculateMedianSquare("Top", "Left", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Top", "Center", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Top", "Right", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Center", "Left", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Center", "Center", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Center", "Right", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Bottom", "Left", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Bottom", "Center", buffer_bytes, buffer_doubles);
        //    CalculateMedianSquare("Bottom", "Right", buffer_bytes, buffer_doubles);
        //    result_image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, _result_bytes, stride);

        //}
        public void CalculateLayers()
        {
            CalculateFourierImage();
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
