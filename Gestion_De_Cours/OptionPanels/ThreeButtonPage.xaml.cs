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

namespace Gestion_De_Cours.OptionPanels
{
    /// <summary>
    /// Interaction logic for ThreeButtonPage.xaml
    /// </summary>
    public partial class ThreeButtonPage : Window
    {
        public ThreeButtonPage(string Text , string b1,string b2, string b3)
        {
            InitializeComponent();
            TB.Text = Text;
            Option1.Content = b1;
            Option2.Content = b2;
            Option3.Content = b3;
            Result = -1;
        }
        public int Result  { get; private set; }
        private void Option1_Click(object sender, RoutedEventArgs e)
        {
            Result = 1;
            DialogResult = true;

        }

        private void Option2_Click(object sender, RoutedEventArgs e)
        {
            Result = 2;
            DialogResult = true;

        }

        private void Option3_Click(object sender, RoutedEventArgs e)
        {
            Result = 3;
            DialogResult = true;

        }
    }
}
