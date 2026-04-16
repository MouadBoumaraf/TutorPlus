using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for TwoOptionsButtons.xaml
    /// </summary>
    public partial class TwoOptionsButtons : Window
    {
        public TwoOptionsButtons(string text , string button1 , string button2)
        {
            InitializeComponent();
            TB.Text = text;
            Option1.Content = button1;
            Option2.Content = button2;
        }
        public int Result { get; private set; }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            Result = 1;
            DialogResult = true; 
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            Result = 2;
            DialogResult = true;
        }

    }
}
