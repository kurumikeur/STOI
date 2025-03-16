using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;



namespace GraphApp.Views
{
    public class IMGView : BaseView
    {
        private BitmapSource _imgData;
        private PixelOperations _operation = PixelOperations.GetPixelOperationsList()[0];

        private string _name = "";
        private bool _r = true;
        private bool _g = true;
        private bool _b = true;
        private double _opacity = 1;
        private int _offset_x = 0;
        private int _offset_y = 0;

        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] ImgBytes { set; get; }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(B));
            }
        }
        public bool R
        {
            get => _r;
            set
            {
                _r = value;
                OnPropertyChanged(nameof(R));
            }
        }
        public bool G
        {
            get => _g;
            set
            {
                _g = value;
                OnPropertyChanged(nameof(G));
            }
        }
        public bool B
        {
            get => _b;
            set
            {
                _b = value;
                OnPropertyChanged(nameof(B));
            }
        }
        public double Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }
        public int Offset_X
        {
            get => _offset_x;
            set
            {
                _offset_x = value;
                OnPropertyChanged(nameof(Offset_X));
            }
        }
        public int Offset_Y
        {
            get => _offset_y;
            set
            {
                _offset_y = value;
                OnPropertyChanged(nameof(Offset_Y));
            }
        }



        public PixelOperations operation
        {
            get => _operation;
            set
            {
                _operation = value;
                OnPropertyChanged(nameof(operation));
            }
        }
        public BitmapSource ImgData
        {
            set
            {

                FormatConvertedBitmap converted = new FormatConvertedBitmap();
                converted.BeginInit();
                converted.Source = value;
                converted.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
                converted.EndInit();

                int stride = (int)converted.PixelWidth * (converted.Format.BitsPerPixel / 8);
                Width = converted.PixelWidth;
                Height = converted.PixelHeight;
                byte[] b = new byte[converted.PixelHeight * stride];
                converted.CopyPixels(b, stride, 0);

                ImgBytes = b;

                var src = BitmapSource.Create(converted.PixelWidth, converted.PixelHeight, converted.DpiX, converted.DpiY, converted.Format,
                    converted.Palette, b, stride);
                _imgData = src;

                OnPropertyChanged(nameof(ImgData));
            }
            get => _imgData;
        }

        public static int GetImageByte(int x, int y, int channel, int Width)
        {
            return (((y - 1) * Width + (x-1)) * 4 + channel);
        }
    }
}
