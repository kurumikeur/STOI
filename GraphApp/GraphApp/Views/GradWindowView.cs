﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Numerics;
using System.Windows;
using System.Windows.Media.TextFormatting;
using GraphApp.Models;
using GraphApp;
using System.IO;
using Point = System.Drawing.Point;
using System.Security.Cryptography;



namespace GraphApp.Views
{
    public class GradWindowView : BaseView
    {
        private BitmapSource _result_image = BitmapSource.Create(1, 1, 1, 1, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        private BitmapSource _result_histo = BitmapSource.Create(1, 1, 1, 1, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
        private BitmapSource _result_graph;
        private IMGView _img = new IMGView();
        private GradationOperations _gradoperation = GradationOperations.GetGradationOperationsList()[0];
        private ObservableCollection<Point> _points = new ObservableCollection<Point>();
        private int _x;
        private int _y;

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
        public BitmapSource result_histo
        {
            get
            {
                return _result_histo;
            }
            set
            {
                _result_histo = value;
                OnPropertyChanged(nameof(result_histo));
            }
        }
        public BitmapSource result_graph
        {
            get 
            { 
                return _result_graph; 
            }
            set 
            { 
                _result_graph = value; 
                OnPropertyChanged(nameof(result_graph)); 
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
        public GradationOperations gradOperation
        {
            get { return _gradoperation; }
            set { _gradoperation = value; OnPropertyChanged(nameof(_gradoperation)); }
        }
        public ObservableCollection<Point> Points
        {
            get { return _points; }
            set { _points = value; }
        }
        public int X { 
            get { return _x; } 
            set {
                if (value < 0)
                    _x = 0;
                else if (value > 255)
                    _x = 255;
                else
                    _x = value;
            }  
        }
        public int Y { 
            get { return _y; } 
            set {
                if (value < 0)
                    _y = 0;
                else if (value > 255)
                    _y = 255;
                else
                    _y = value;
            } 
        }

        private void CalculateHistogram()
        {
            List<int> buffer = new List<int>(new int[256]);
            int index, max;
            double k;
            int histo_height = 256 * 3, histo_width = 256 * 3;
            int bitmap_bytes = histo_height * histo_width * (PixelFormats.Gray8.BitsPerPixel / 8);
            byte[] buffer_bytes = new byte[bitmap_bytes];

            for (int i = 1; i <= Img.Height; ++i)
            {
                for (int j = 1; j <= Img.Width; ++j)
                {
                    int b = Img.ImgBytes[IMGView.GetImageByte(j, i, 0, Img.Width)];
                    int g = Img.ImgBytes[IMGView.GetImageByte(j, i, 1, Img.Width)];
                    int r = Img.ImgBytes[IMGView.GetImageByte(j, i, 2, Img.Width)];
                    index = (int)(Math.Round((b + g + r) / 3.0 % 256));
                    buffer[index] += 1;
                }
            }
            max = (int)buffer.Max();
            k = (double)histo_height / (double)max;

            for (int i = 0; i < 256 * 3; i += 3)
            {
                int step = histo_width, threshold = (int)((histo_height - buffer[i / 3] * k) * histo_width + i / 3);
                
                for (int j = i; j < threshold && j < bitmap_bytes; j += step)
                {
                    buffer_bytes[j] = (byte)255;
                    buffer_bytes[j + 1] = (byte)255;
                    buffer_bytes[j + 2] = (byte)255;
                }
            }
            result_histo = BitmapSource.Create(histo_width, histo_height, 96, 96, PixelFormats.Gray8, null, buffer_bytes, histo_width);
        }
        public void CalculateGraph()
        {
            int k = 1;
            int graph_height = 256 * k, graph_width = 256 * k, stride = graph_width * 3;
            int buff_size = graph_height * graph_width * 3;
            byte[] buffer_bytes = new byte[buff_size];
            Parallel.For(0, graph_height * graph_width * 3, index =>
            {
                buffer_bytes[index] = 255;
            });
           
            foreach (Point p2 in Points) //
            {
                int index = Points.IndexOf(p2);
                if (index == 0)
                    continue;
                Point p1 = Points[index - 1];
                List<Point> DrawPoints = LineDrawing.BresenhamLine(p1, p2);
                foreach (Point p in DrawPoints)
                {
                    int pix = ((graph_height - 1) - p.Y) * stride + p.X * 3;
                    buffer_bytes[pix] = 0;
                    buffer_bytes[pix + 1] = 0;
                    buffer_bytes[pix + 2] = 0;
                }
             }

            //foreach (Point p2 in Points) //
            //{
            //    int index = Points.IndexOf(p2);
            //    if (index == 0)
            //        continue;
            //    Point p1 = Points[index - 1];
            //    int y1 = (int)p1.Y, y2 = (int)p2.Y;
            //    int x1 = (int)p1.X, x2 = (int)p2.X;
            //    int diff_y = y2 - y1, diff_x = x2 - x1;

            //    int step = diff_y >= diff_x ? diff_y / diff_x : diff_x / diff_y;
            //    int neg = diff_y * diff_x > 0 ? -1 : 1;
            //    int pix = ((graph_height - 1) - y1) * stride + x1 * 3, last_pix = ((graph_height - 1) - y2) * stride + x2 * 3;

            //    while (pix != last_pix)
            //    {
            //        if (pix < 0 || pix > buff_size)
            //            break;
            //        for (int i = step; i > 0; i--)
            //        {
            //            buffer_bytes[pix] = 0;
            //            buffer_bytes[pix + 1] = 0;
            //            buffer_bytes[pix + 2] = 0;
            //            pix = diff_y >= diff_x ? pix + stride * neg : pix + 3;
            //        }
            //        pix = diff_y >= diff_x ? pix + 3 : pix + stride * neg;
            //    }
            //}

            foreach (Point p in Points) //Красные квадраты в точках
            {
                int center_pix = (graph_height - 1 - ((int)p.Y) * k) * stride + (int)p.X * k * 3;
                int area = 3;
                for (int i = -area; i <= area; i++)
                {
                    int line_pix = center_pix + stride * i;
                    if (line_pix >= buff_size || line_pix < 0)
                        continue;
                    for (int j = -area; j <= area; j++) 
                    {
                        int point_pix = line_pix + j * 3;
                        if (point_pix / stride != line_pix / stride)
                            continue;
                        buffer_bytes[point_pix] = 0;
                        buffer_bytes[point_pix + 1] = 0;
                        buffer_bytes[point_pix + 2] = 255;
                    }
                }
            }
            result_graph = BitmapSource.Create(graph_height, graph_width, 96, 96, PixelFormats.Bgr24, null, buffer_bytes, stride);

        }
        public void CalculateLayers()
        {
            CalculateGraph();
            if (Img.ImgData == null)
            {
                return;
            }
            List<int> buffer = new List<int>(new int[256]);

            var color_channels = new (int cc, bool enabled)[]
            {
                (2, Img.R),
                (1, Img.G),
                (0, Img.B),
                (3, true)
            };

            byte[] buffer_bytes = (byte[])Img.ImgBytes.Clone();
            int buffer_size = Img.Width * Img.Height - 1;
            Parallel.For(0, buffer_size, index =>
            {
                for (int i = 0; i < 4; i++)
                {
                    int byte_index = index * 4 + i;
                    int val = buffer_bytes[byte_index];
                    Point p2 = Points.Where(p => p.X > val).FirstOrDefault();
                    if (Points.IndexOf(p2) - 1 < 0)
                        break;
                    Point p1 = Points[Points.IndexOf(p2) - 1];
                    buffer_bytes[byte_index] = (byte)gradOperation._operation(p1, p2, val);
                }
            });
            
            CalculateHistogram();
            result_image = BitmapSource.Create(Img.Width, Img.Height, 96, 96, PixelFormats.Bgra32, null, buffer_bytes, Img.Width * 4);
        }
        public void AddIMGView(IMGView _imgv)
        {
            Img = _imgv;
        }
        public void AddPoint()
        {
            int index = Points.IndexOf(Points.Where(x => x.X > X).FirstOrDefault());
            if (Points.Where(x => x.X == X).Any())
            {
                return;
            }
            if (index == -1)
            {
                Points.Add(new Point(X, Y));
                return;
            }
            Points.Insert((int)index, new Point ( X, Y ));
            Points.OrderBy(x => x.X);
            CalculateLayers();
        }
        public void ResetPoints()
        {
            Points.Clear();
            Points.Add(new Point(0, 0));
            Points.Add(new Point(255, 255));
            CalculateLayers();
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

        public GradWindowView()
        {
            Points.Add(new Point(0, 0));
            Points.Add(new Point(255, 255));
            CalculateGraph();
        }
        
    }
}
