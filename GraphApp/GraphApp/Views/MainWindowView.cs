using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GraphApp.Views
{
    public class MainWindowView : BaseView
    {
        private byte[] buffer_bytes;
        private BitmapSource _result_image;
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
        private ObservableCollection<IMGView> _imgs = new ObservableCollection<IMGView>();
        public ObservableCollection<IMGView> Imgs
        {
            get
            {
                return _imgs;
            }
            set {
                _imgs = value;
            }
        }


        public void AddIMGView(IMGView _imgview)
        {
            Imgs.Add(_imgview);

        }

        public void MoveUp(object obj)
        {
            var imgview = (IMGView)obj;
            var index = Imgs.IndexOf(imgview);
            if (index != 0) 
                Imgs.Move(index, index - 1);
        }

        public void MoveDown(object obj)
        {
            var imgview = (IMGView)obj;
            var index = Imgs.IndexOf(imgview);
            if (index != Imgs.Count() - 1)
                Imgs.Move(index, index + 1);
        }

        public void DeleteImg(object obj)
        {
            try
            {
                Imgs.Remove(obj as IMGView);
            }
            catch (Exception ex) 
            { 

            }
        }

        public void CalculateLayers()
        {
            var properties = (from img in Imgs
                              select new
                              {
                                  r = img.R,
                                  g = img.G,
                                  b = img.B,
                                  offset_X = img.Offset_X,
                                  offset_Y = img.Offset_Y,
                                  width = img.Width,
                                  height = img.Height,
                                  operation = img.operation,
                                  opacity = img.Opacity
                              }).ToArray();
            if (properties.Length == 0)
            {
                result_image = null;
                return;
            }
            int max_width = Imgs.Max(x => x.Width);
            int max_height = Imgs.Max(y => y.Height);
            buffer_bytes = new byte[max_width * max_height * 4];

            byte buff = new byte() ;            
            for (int n = 0; n < Imgs.Count; n++)
            {
                var color_channels = new (int cc, bool enabled)[]
                {
                    (2, properties[n].r),
                    (1, properties[n].g),
                    (0, properties[n].b),
                    (3, true)
                };
                for (int i = 1; i <= Imgs[n].Height && i + properties[n].offset_Y < max_height; ++i)
                {
                    for (int j = 1; j <= Imgs[n].Width && j + properties[n].offset_X < max_width; ++j)
                    {
                        foreach (var color_channel in color_channels.Where(x => x.enabled)) { 

                            buff = (byte) (properties[n].operation._pixelOperation(Imgs[n].ImgBytes[IMGView.GetImageByte(j, i, color_channel.cc, Imgs[n].Width)], 
                                                                                       buffer_bytes[IMGView.GetImageByte(j + properties[n].offset_X, i + properties[n].offset_Y, color_channel.cc, max_width    )]));

                            buffer_bytes[IMGView.GetImageByte(j + properties[n].offset_X, i + properties[n].offset_Y, color_channel.cc, max_width)] = (byte)((byte) (buff * properties[n].opacity) 
                                                                                                          +(byte) (buffer_bytes[IMGView.GetImageByte(j + properties[n].offset_X, i + properties[n].offset_Y, color_channel.cc, max_width)]*(1 - properties[n].opacity)
                                                                                                            )) ;
                        }
                    }
                }
            }

            result_image = BitmapSource.Create(max_width, max_height, 96, 96, PixelFormats.Bgra32, null, buffer_bytes, max_width * 4);
        }
    }
}
