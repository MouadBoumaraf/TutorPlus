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
    /// Interaction logic for TextPopups.xaml
    /// </summary>
    public partial class TextPopups : Window
    {
        public int Result { get; private set; }
        public TextPopups()
        {
            InitializeComponent();
            Result = -1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TB.Text,out int res))
            {
                MessageBox.Show("Please enter a valid value");
                return;
            }
            Result = int.Parse(TB.Text);
            DialogResult = true;
            this.Close();
        }
    }
}
