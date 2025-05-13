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
using GraphApp.Models;
using GraphApp.Views;
using Microsoft.Win32;



namespace GraphApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowView MWV { get; set; }
        public GradWindowView GWV { get; set; }
        public BinarWindowView BWV { get; set; }
        public FiltrWindowView FWV { get; set; }
        public FreqFilterWindowView FFWV { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Title = "Teto";
            this.DataContext = this;
            MWV = new MainWindowView();
            GWV = new GradWindowView();
            BWV = new BinarWindowView();
            FWV = new FiltrWindowView();
            FFWV = new FreqFilterWindowView();

        }

        //private void FiltInputBoxKeyHandler(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        FWV.Input.Append('\n');
        //    }
        //}

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
                catch (Exception ex)
                {

                }
            }
        }
        private void Button_Click_SaveFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "PNG Image|*.png";
            dlg.Title = "Save an Image File";
            dlg.ShowDialog();
            var Image = MWV.result_image;
            if (dlg.FileName != "")
            {
                using (System.IO.FileStream fs = (System.IO.FileStream)dlg.OpenFile())
                {

                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(Image));
                    encoder.Save(fs);
                    fs.Close();
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

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            MWV.DeleteImg(((FrameworkElement)e.Source).DataContext);
        }
        private void Button_Click_MoveUp(object sender, RoutedEventArgs e)
        {
            MWV.MoveUp(((FrameworkElement)e.Source).DataContext);
        }
        private void Button_Click_MoveDown(object sender, RoutedEventArgs e)
        {
            MWV.MoveDown(((FrameworkElement)e.Source).DataContext);
        }
        private void Button_Click_GradController(object sender, RoutedEventArgs e)
        {
            var val = ((Button)sender).Tag as string;
            switch (val)
            {
                case "OpenFile":
                    {
                        GWV.OpenFile();
                    }
                    break;
                case "SaveFile":
                    {
                        GWV.SaveFile();
                    }
                    break;
                case "AddPoint":
                    {
                        GWV.AddPoint();
                        break;
                    }
                case "ResetPoints":
                    {
                        GWV.ResetPoints();
                    }
                    break;
                default:
                    break;
            }
        }
        private void Button_Click_BinarController(object sender, RoutedEventArgs e)
        {
            var val = ((Button)sender).Tag as string;
            switch (val)
            {
                case "OpenFile":
                    {
                        BWV.OpenFile();
                    }
                    break;
                case "SaveFile":
                    {
                        BWV.SaveFile();
                    }
                    break;
                default:
                    break;
            }
        }
        private void Button_Click_FiltrController(object sender, RoutedEventArgs e)
        {
            var val = ((Button)sender).Tag as string;
            switch (val)
            {
                case "Calculate":
                    FWV.CalculateLayers();
                    break;
                case "Gauss":
                    FWV.CalculateGauss();
                    break;
                case "OpenFile":
                    FWV.OpenFile();
                    break;
                case "SaveFile":
                    FWV.SaveFile();
                    break;
                default:
                    break;
            }
        }
        private void Button_Click_FreqFiltrController(object sender, RoutedEventArgs e)
        {
            var val = ((Button)sender).Tag as string;
            switch (val)
            {
                case "Calculate":
                    FFWV.CalculateLayers();
                    break;
                case "OpenFile":
                    FFWV.OpenFile();
                    break;
                case "SaveFile":
                    FFWV.SaveFile();
                    break;
                default:
                    break;
            }
        }
    }
}
