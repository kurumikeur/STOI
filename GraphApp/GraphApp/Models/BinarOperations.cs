using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GraphApp.Views;

namespace GraphApp.Models
{
    public class BinarOperations
    {
        public delegate byte[] Operation(BinarWindowView BWV);
        public Operation _operation;
        public string Name { get; set; }

        public List<BinarOperations> Operations
        {
            get
            {
                return BinarOperations.GetBinarOperationsList();
            }
        }

        public static List<BinarOperations> _operations = new List<BinarOperations>
    {
        new BinarOperations()
        {
            Name = "Gavr",
            _operation = (BWV) => Gavr(BWV),
        },

        new BinarOperations()
        {
            Name = "Otsu",
            _operation = (BWV) => Otsu(BWV),
        },

        new BinarOperations()
        {
            Name = "Niblek",
            _operation = (BWV) => Niblek(BWV),
        },
        new BinarOperations()
        {
            Name = "Sauvola",
            _operation = (BWV) => Sauvola(BWV),
        },
        new BinarOperations()
        {
            Name = "Wolf",
            _operation = (BWV) => Wolf(BWV),
        },
    };
        public static List<BinarOperations> GetBinarOperationsList()
        {
            return _operations;
        }

        public static byte[] Gavr(BinarWindowView BWV)
        {
            IMGView Img = BWV.Img;
            int img_size = Img.Width * Img.Height;
            byte[] buffer_bytes = (byte[])BWV.ConvertedBytes.Clone();
            int avg = 0;
            for(int i = 0; i < img_size; i++)
            {
                avg += buffer_bytes[i];
            };
            avg /= (img_size);
            Parallel.For(0, img_size, index =>
            {
                if (buffer_bytes[index] <= avg)
                    buffer_bytes[index] = 0;
                else
                    buffer_bytes[index] = 255;
            });

            return buffer_bytes;
        }
        public static byte[] Otsu(BinarWindowView BWV)
        {
            IMGView Img = BWV.Img;
            int img_size = Img.Width * Img.Height;
            byte[] buffer_bytes = (byte[])BWV.ConvertedBytes.Clone();
            List<double> histo_values = BWV.CalculateHistogramBytes();
            int max = histo_values.IndexOf(histo_values.Max());
            int max_t = 0;
            double max_omega = 0;
            double ut = 0;
            for (int i = 0; i < 256; i++)
            {
                ut += i * histo_values[i];
            }
            ;
            double w1 = 0, w2 = 0, _u1 = 0, u1 = 0, u2 = 0, omega = 0;
            for (int t = 0; t < 256; t++)
            {
                w1 += histo_values[t];
                w2 = 1 - w1;
                _u1 += t * histo_values[t];
                u1 = _u1 / w1;
                u2 = (ut - u1 * w1) / w2;
                omega = w1 * w2 * Math.Pow(u1 - u2, 2);
                if (omega > max_omega)
                {
                    max_omega = omega;
                    max_t = t;
                }
            }
            Parallel.For(0, img_size, index =>
            {
                if (buffer_bytes[index] <= max_t)
                    buffer_bytes[index] = 0;
                else
                    buffer_bytes[index] = 255;
            });

            return buffer_bytes;
        }

        public static byte[] Niblek(BinarWindowView BWV, string param = "Niblek")
        {
            IMGView Img = BWV.Img;
            int img_size = Img.Width * Img.Height;
            int stride = Img.Width;
            int area = BWV.Area;
            byte[] buffer_bytes = BWV.ConvertedBytes ;
            byte[] image_bytes = new byte[img_size];
            double t = 0;
            int pix_i = 0, pix_j = 0, count = 0;
            double avg = 0, avg2 = 0;
            bool isWolf = param == "Wolf" ? true : false;
            int min_pix = 255;
            double max_sigma = 0;
            if (isWolf)
            {
                for (int n = area + stride * area; n < img_size - area - stride * area; n++)
                {
                    avg = 0;
                    avg2 = 0;
                    count = 0;
                    for (int i = -area; i <= area; i++)
                    {
                        pix_i = n + stride * i;
                        for (int j = -area; j <= area; j++)
                        {
                            pix_j = pix_i + j;
                            avg += buffer_bytes[pix_j];
                            avg2 += buffer_bytes[pix_j] * buffer_bytes[pix_j];
                            ++count;
                            if (buffer_bytes[pix_j] < min_pix)
                                min_pix = buffer_bytes[pix_j];
                        }
                    }
                    avg /= count;
                    avg2 /= count;
                    double dx = (avg2 - avg * avg);
                    double sigma = Math.Sqrt(dx);
                    if (sigma > max_sigma)
                        max_sigma = sigma;

                }

                for (int n = 0; n < img_size; n++)
                {
                    if (n >= area + stride * area && n <= img_size - area - stride * area)
                        continue;
                    avg = 0;
                    avg2 = 0;
                    count = 0;
                    for (int i = -area; i <= area; i++)
                    {
                        pix_i = n + stride * i;
                        if (pix_i >= img_size || pix_i < 0)
                            continue;

                        for (int j = -area; j <= area; j++)
                        {
                            pix_j = pix_i + j;
                            if (pix_j / stride != (pix_j - j) / stride)
                                continue;
                            if (pix_j >= img_size || pix_j < 0)
                                continue;
                            avg += buffer_bytes[pix_j];
                            avg2 += buffer_bytes[pix_j] * buffer_bytes[pix_j];
                            if (buffer_bytes[pix_j] < min_pix)
                                min_pix = buffer_bytes[pix_j];
                            ++count;
                        }
                    }
                    avg /= count;
                    avg2 /= count;
                    double dx = (avg2 - avg * avg);
                    double sigma = Math.Sqrt(dx);
                    if (sigma > max_sigma)
                        max_sigma = sigma;
                }
            }

            for (int n = area + stride * area; n < img_size - area - stride * area; n++)
            {
                avg = 0;
                avg2 = 0;
                count = 0;
                for (int i = -area; i <= area; i++)
                {
                    pix_i = n + stride * i;
                    for (int j = -area; j <= area; j++)
                    {
                        pix_j = pix_i + j;
                        avg += buffer_bytes[pix_j];
                        avg2 += buffer_bytes[pix_j] * buffer_bytes[pix_j];
                        ++count;
                    }
                }
                avg /= count;
                avg2 /= count;
                double dx = (avg2 - avg * avg);
                double sigma = Math.Sqrt(dx);
                switch (param)
                {
                    case "Niblek":
                        t = avg + BWV.K * sigma;
                        break;
                    case "Sauvola":
                        t = avg * (1 + BWV.K * (sigma / 128 - 1));
                        break;
                    case "Wolf":
                        t = (1 - BWV.K) * avg + BWV.K * min_pix + BWV.K * (sigma / max_sigma) * (avg - min_pix);
                        break;
                }


                if (buffer_bytes[n] <= Math.Round(t))
                    image_bytes[n] = 0;
                else
                    image_bytes[n] = 255;
            }

            for (int n = 0; n < img_size; n++) {
                if (n >= area + stride * area && n <= img_size - area - stride * area)
                    continue;
                avg = 0;
                avg2 = 0;
                count = 0;
                for (int i = -area; i <= area; i++)
                {
                    pix_i = n + stride * i;
                    if (pix_i >= img_size || pix_i < 0)
                        continue;

                    for (int j = -area; j <= area; j++)
                    {
                        pix_j = pix_i + j;
                        if (pix_j / stride != (pix_j - j) / stride)
                            continue;
                        if (pix_j >= img_size || pix_j < 0)
                            continue;                  
                        avg += buffer_bytes[pix_j];
                       avg2 += buffer_bytes[pix_j] * buffer_bytes[pix_j];
                       ++count;
                    }
                }
                avg /= count;
                avg2 /= count;
                double dx = (avg2 - avg * avg);
                double sigma = Math.Sqrt(dx);
                switch (param)
                {
                    case "Niblek":
                        t = avg + BWV.K * sigma;
                        break;
                    case "Sauvola":
                        t = avg * (1 + BWV.K * (sigma / 128 - 1));
                        break;
                    case "Wolf":
                        t = (1 - BWV.K) * avg + BWV.K * min_pix + BWV.K * (sigma / max_sigma) * (avg - min_pix);
                        break;
                }


                if (buffer_bytes[n] <= Math.Round(t))
                    image_bytes[n] = 0;
                else
                    image_bytes[n] = 255;

            }
            return image_bytes;
        }
        public static byte[] Sauvola(BinarWindowView BWV)
        {
            return Niblek(BWV, "Sauvola");
        }
        public static byte[] Wolf(BinarWindowView BWV)
        {
            return Niblek(BWV, "Wolf");
        }
        public BinarOperations() { }
    }
}
