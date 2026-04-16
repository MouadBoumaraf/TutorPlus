using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Backup.xaml
    /// </summary>
    public partial class Backup : Window
    {
        int typefortb;
        public Backup(string text, int type,string textboxtext = null )
        {
            InitializeComponent();
            typefortb = type; 
            Block.Text = text;
            Block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size desiredSize = Block.DesiredSize;
            // Check if the desired width exceeds the available width
            if (desiredSize.Width > this.ActualWidth)
            {
                // Adjust the width of the container (assuming 'this' refers to the container)
                this.Width = desiredSize.Width + 20; // Add some extra space for padding
            }
            if (typefortb == 2)
            {
                CB.Visibility = Visibility.Visible;
                ResponseTextBox.Visibility = Visibility.Collapsed;
                string folderPath = @"C:\ProgramData\EcoleSetting\DataBases";
                string[] bakFiles = Directory.GetFiles(folderPath, "*.bak");
                foreach (string bakFile in bakFiles)
                {
                    // Get only the file name without the full path
                    string fileName = System.IO.Path.GetFileName(bakFile);
                    CB.Items.Add(fileName);
                }
            }
            else
            {
                ResponseTextBox.Focus();
            }
            if(textboxtext != null)
            {
                ResponseTextBox.Text = textboxtext;
            }
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(typefortb == 2)
            {
                ResponseTextBox.Text = CB.Text;
            }
            DialogResult = true;
        }

        private void ResponseTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (typefortb == 1)
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!this.DialogResult.HasValue) // user closed with X or Alt+F4
            {
                this.DialogResult = false;
            }
        }
        private void ResponseTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (typefortb == 2)
                {
                    ResponseTextBox.Text = CB.Text;
                }
                DialogResult = true;
            }
        }
    }
}
