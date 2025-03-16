using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphApp.Views
{
    public class BaseView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);

        }
        protected void OnPropertyChanged(string PropertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));
        }
    }
}
