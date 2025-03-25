using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphApp.Views
{
    public class GradationOperations
    {
        public delegate double Operation(int v1, int v2, double o);
        public Operation _operation;
        public string Name;
        
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
                _operation = (v1, v2, o) => (double)(v1 * v2),
            }
        };

        public static List<GradationOperations> GetGradationOperationsList()
        {
            return _operations;
        }

        public GradationOperations() { }
    }
}
