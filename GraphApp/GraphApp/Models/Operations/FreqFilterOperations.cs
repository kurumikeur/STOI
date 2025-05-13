using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media.Animation;
using GraphApp.Views;

namespace GraphApp.Models.Operations
{
    public class FreqFilterOperations
    {
        public delegate Complex[] Operation(Complex[] fourier_array, int inner_r, int outer_r, int stride, int offset_X, int offset_Y);
        public Operation _operation;
        public string Name { get; set; }
        public static List<FreqFilterOperations> _operations = new List<FreqFilterOperations>
        {
            new FreqFilterOperations()
            {
                Name = "-",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => NoOp(fourier_array),
            },

            new FreqFilterOperations()
            {
                Name = "Высокочастотный",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => HighFreq(fourier_array, inner_r, stride),
            },

            new FreqFilterOperations()
            {
                Name = "Низкочастотный",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => LowFreq(fourier_array, inner_r, stride),
            },
            new FreqFilterOperations()
            {
                Name = "Режекторный",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => RejectFilter(fourier_array, inner_r, outer_r, stride),
            },
            new FreqFilterOperations()
            {
                Name = "Полосовой",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => LineFilter(fourier_array, inner_r, outer_r, stride),
            },
            new FreqFilterOperations()
            {
                Name = "Узкопол. режект.",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => LRejectFilter(fourier_array, inner_r, outer_r, stride, offset_X, offset_Y),
            },
            new FreqFilterOperations()
            {
                Name = "Узкопол. полосов.",
                _operation = (fourier_array, inner_r, outer_r, stride, offset_X, offset_Y) => LLineFilter(fourier_array, inner_r, outer_r, stride, offset_X, offset_Y),
            },
        };
        public List<FreqFilterOperations> Operations
        {
            get
            {
                return FreqFilterOperations.GetFreqFilterOperationsList();
            }
        }

        

        public static Complex[] NoOp(Complex[] fourier_array)
        {
            return (Complex[])fourier_array.Clone();
        }
        public static Complex[] HighFreq(Complex[] fourier_array, int r, int stride)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4; 
            Complex[] buff_array = new Complex[fourier_array.Length];
            int index = 0;
            foreach(var complex in fourier_array)
            {
                buff_array[index] = new Complex(complex.Real, complex.Imaginary);
                ++index;
            }
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    if (Math.Pow(r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_array[center_index + i * stride + j * 4]     = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(0, 0);
                    }
                }
            }

            return buff_array;
        }
        public static Complex[] LowFreq(Complex[] fourier_array, int r, int stride)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4;
            Complex[] buff_array = new Complex[fourier_array.Length];
            Complex buff_val1;
            Complex buff_val2;
            Complex buff_val3;
            for (int i = -r; i <= r; i++)
            {
                for (int j = -r; j <= r; j++)
                {
                    if (Math.Pow(r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_val1 = fourier_array[center_index + i * stride + j * 4];
                        buff_val2 = fourier_array[center_index + i * stride + j * 4 + 1];
                        buff_val3 = fourier_array[center_index + i * stride + j * 4 + 2];
                        buff_array[center_index + i * stride + j * 4] = new Complex(buff_val1.Real, buff_val1.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(buff_val2.Real, buff_val2.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(buff_val3.Real, buff_val3.Imaginary);
                    }
                }
            }

            return buff_array;
        }
        public static Complex[] RejectFilter(Complex[] fourier_array, int inner_r, int outer_r, int stride)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4;
            Complex[] buff_array = new Complex[fourier_array.Length];
            Complex buff_val1;
            Complex buff_val2;
            Complex buff_val3;
            int index = 0;
            foreach (var complex in fourier_array)
            {
                buff_array[index] = new Complex(complex.Real, complex.Imaginary);
                ++index;
            }

            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_array[center_index + i * stride + j * 4] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(0, 0);
                    }
                }
            }

            return buff_array;
        }
        public static Complex[] LineFilter(Complex[] fourier_array, int inner_r, int outer_r, int stride)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4;
            Complex[] buff_array = new Complex[fourier_array.Length];
            Complex buff_val1;
            Complex buff_val2;
            Complex buff_val3;
            int index = 0;
            
            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_val1 = fourier_array[center_index + i * stride + j * 4];
                        buff_val2 = fourier_array[center_index + i * stride + j * 4 + 1];
                        buff_val3 = fourier_array[center_index + i * stride + j * 4 + 2];
                        buff_array[center_index + i * stride + j * 4] = new Complex(buff_val1.Real, buff_val1.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(buff_val2.Real, buff_val2.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(buff_val3.Real, buff_val3.Imaginary);
                    }
                }
            }
            return buff_array;
        }
        public static Complex[] LRejectFilter(Complex[] fourier_array, int inner_r, int outer_r, int stride, int offset_x, int offset_y)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4;         
            Complex[] buff_array = new Complex[fourier_array.Length];
            Complex buff_val1;
            Complex buff_val2;
            Complex buff_val3;
            int index = 0;
            foreach (var complex in fourier_array)
            {
                buff_array[index] = new Complex(complex.Real, complex.Imaginary);
                ++index;
            }

            center_index -= offset_y * stride - offset_x * 4;
            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_array[center_index + i * stride + j * 4] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(0, 0);
                    }
                }
            }
            center_index += 2 * (offset_y * stride - offset_x * 4);
            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_array[center_index + i * stride + j * 4] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(0, 0);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(0, 0);
                    }
                }
            }

            return buff_array;
        }

        public static Complex[] LLineFilter(Complex[] fourier_array, int inner_r, int outer_r, int stride, int offset_x, int offset_y)
        {
            int center_index = (int)Math.Ceiling(fourier_array.Length / 2.0) - (fourier_array.Length / 2) % 4;
            Complex[] buff_array = new Complex[fourier_array.Length];
            Complex buff_val1;
            Complex buff_val2;
            Complex buff_val3;
            int index = 0;

            center_index -= offset_y * stride - offset_x * 4;
            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_val1 = fourier_array[center_index + i * stride + j * 4];
                        buff_val2 = fourier_array[center_index + i * stride + j * 4 + 1];
                        buff_val3 = fourier_array[center_index + i * stride + j * 4 + 2];
                        buff_array[center_index + i * stride + j * 4] = new Complex(buff_val1.Real, buff_val1.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(buff_val2.Real, buff_val2.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(buff_val3.Real, buff_val3.Imaginary);
                    }
                }
            }
            center_index += 2 * (offset_y * stride - offset_x * 4);
            for (int i = -outer_r; i <= outer_r; i++)
            {
                for (int j = -outer_r; j <= outer_r; j++)
                {
                    if (Math.Pow(outer_r, 2) >= Math.Pow(i, 2) + Math.Pow(j, 2) && Math.Pow(inner_r, 2) <= Math.Pow(i, 2) + Math.Pow(j, 2))
                    {
                        buff_val1 = fourier_array[center_index + i * stride + j * 4];
                        buff_val2 = fourier_array[center_index + i * stride + j * 4 + 1];
                        buff_val3 = fourier_array[center_index + i * stride + j * 4 + 2];
                        buff_array[center_index + i * stride + j * 4] = new Complex(buff_val1.Real, buff_val1.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 1] = new Complex(buff_val2.Real, buff_val2.Imaginary);
                        buff_array[center_index + i * stride + j * 4 + 2] = new Complex(buff_val3.Real, buff_val3.Imaginary);
                    }
                }
            }

            return buff_array;
        }


        public static List<FreqFilterOperations> GetFreqFilterOperationsList()
        {
            return _operations;
        }

    }
}
