using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;

namespace GraphApp.Views
{
    public class GradView : BaseView {
        private IMGView _grad_image;
        public IMGView grad_image
        {
            get { return _grad_image; }
            set {  _grad_image = value; OnPropertyChanged(nameof(grad_image)); }
        }
        private List<Point> _points;
        public List<Point> Points
        {
            get { return _points; }
        }

        private GradationOperations _operation = GradationOperations.GetGradationOperationsList()[0];
        public GradationOperations operation
        {
            get { return _operation; }
            set { _operation = value; OnPropertyChanged(nameof(operation)); }
        }



    }
}
