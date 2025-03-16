using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphApp.Views
{
    static class MultiDimArray
    {
        public static T[,] Empty2D<T>() => new T[0, 0];
        public static T[,,] Empty3D<T>() => new T[0, 0, 0];
    }
}
