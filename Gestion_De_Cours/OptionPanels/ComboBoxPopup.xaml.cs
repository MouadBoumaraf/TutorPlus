using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for ComboBoxPopup.xaml
    /// </summary>
    public partial class ComboBoxPopup : Window
    {
        public string Result { get; private set; }
        string id;
        public ComboBoxPopup(DataTable dt , string text , string desplaymemberpath , string idreturn )
        {
            InitializeComponent();
            CB.ItemsSource = dt.DefaultView;
            CB.DisplayMemberPath = desplaymemberpath;
            Lbl.Content = text; 
            id = idreturn;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CB.SelectedIndex == -1 )
            {
                MessageBox.Show("Please enter a valid value");
                return;
            }
            DataRowView row = (DataRowView)CB.SelectedItem;
            Result = row[id].ToString();
            DialogResult = true;
            this.Close();
        }
    }
}
