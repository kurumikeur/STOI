using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphApp.Views;


namespace GraphApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowView MWV;
        public MainWindow()
        {
            InitializeComponent();

            MWV = new MainWindowView();
            this.Title = "JokerPoker - Balala";
            this.DataContext = MWV;
            listBox.ItemsSource = MWV.Imgs;
           
            
        }

        private void Button_Click_OpenFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All files (*.*) |*.jpg;*.png;*.jpeg;*.gif| JPG Files (*.jpg) | JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|*.jpg|GIF Files (*.gif)|*.gif";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                FileInfo F = new FileInfo(filename);
                try
                {
                    MWV.AddIMGView(new IMGView { ImgData = new BitmapImage(new Uri(filename)), Name = F.Name });
                    MWV.CalculateLayers();
                }
                catch (Exception ex) { 
                    
                }
            }
        }

        private void Button_Click_Recalculate(object sender, RoutedEventArgs e)
        {
            try
            {
                MWV.CalculateLayers();
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
