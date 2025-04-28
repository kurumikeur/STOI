using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using GraphApp.Views;

namespace GraphApp.Models
{
    public class FiltOperations
    {
        public delegate void Operation(int x_area, int y_area);
        public Operation _operation;
        public string Name { get; set; }


        public List<FiltOperations> Operations
        {
            get
            {
                return FiltOperations.GetFiltOperationsList();
            }
        }

        public static List<FiltOperations> _operations = new List<FiltOperations>
        {
            new FiltOperations()
            {
                Name = "Linear",
                _operation = null
            },

            new FiltOperations()
            {
                Name = "Median",
                _operation = null
            },

        };

        
        public static List<FiltOperations> GetFiltOperationsList()
        {
            return _operations;
        }

        

        public FiltOperations() { }
    }
}
