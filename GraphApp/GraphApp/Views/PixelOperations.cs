using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphApp.Views
{
    public class PixelOperations : BaseView
    {
        public delegate byte Operation(byte val1, byte val2, double opacity = 1);
        public string Name { get; set; }
        public Operation _pixelOperation;
        public List<PixelOperations> Operations
        {
            get
            {
                return PixelOperations.GetPixelOperationsList();
            }
        }
        public static List<PixelOperations> _operations = new List<PixelOperations> {
                new PixelOperations()
                {
                    Name = "-",
                    _pixelOperation = (v1, v2, o) => (byte)(v1*o)
                },

                new PixelOperations()
                {
                    Name = "Multiply",
                    _pixelOperation = (v1, v2, o) => (byte)( v1 * v2 / 255.0 )
                },

                new PixelOperations()
                {
                    Name = "Divide",
                    _pixelOperation = (v1, v2, o) => (byte)( 1.0 * v2 / v1 )
                },

                new PixelOperations()
                {
                    Name = "Sum",
                    _pixelOperation = (v1, v2, o) => (byte)(v1+v2)
                },
                new PixelOperations()
                {
                    Name = "Sub",
                    _pixelOperation = (v1, v2, o) => (byte)(v1-v2)
                },
                new PixelOperations()
                {
                    Name = "Maximum",
                    _pixelOperation = (v1, v2, o) => (byte)Math.Max(v1,v2)
                },
                new PixelOperations()
                {
                    Name = "Avg",
                    _pixelOperation = (v1, v2, o) => (byte) ((v1+v2)/2)
                },

        };
        public static List<PixelOperations> GetPixelOperationsList()
        {
            return _operations ;
        } 
    }
}
