using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GraphApp.Models
{
    public class GradationOperations
    {
        public delegate int Operation(Point p1, Point p2, int val);
        public Operation _operation;
        public string Name { get; set; }

        public List<GradationOperations> Operations{
            get
            {
                return GradationOperations.GetGradationOperationsList();
            }
        }

        public static List<GradationOperations> _operations = new List<GradationOperations>
        {
            new GradationOperations()
            {
                Name = "Linear",
                _operation = (p1, p2, val) => LinInterp(p1, p2, val),
            },

            new GradationOperations()
            {
                Name = "Bilinear",
                _operation = null,
            },

            new GradationOperations()
            {
                Name = "Cubic",
                _operation = null,
            },
        };

        public static int LinInterp(Point p1, Point p2, int x)
        {
            int y_diff = (int)(p2.Y - p1.Y);
            int x_diff = (int)(p2.X - p1.X);
            double k = (double)y_diff / (double)x_diff;
            return (int)(k * (x - p1.X) + p1.Y);
        }
        public static List<GradationOperations> GetGradationOperationsList()
        {
            return _operations;
        }

        public GradationOperations() { }
    }
}
