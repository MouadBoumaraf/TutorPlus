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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using Gestion_De_Cours.Classes;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for ShortCuts.xaml
    /// </summary>
    public partial class ShortCuts : UserControl
    {
        public ShortCuts()
        {
            InitializeComponent();
            List<DisplayButton> buttonsToRemove = new List<DisplayButton>();

            foreach (UIElement element in TargetPanel.Children.OfType<DisplayButton>())
            {
                // Check if the element is a DisplayButton and remove it from TargetPanel
                DisplayButton displayButton = (DisplayButton)element;

                if (Connexion.GetInt("Select Case when " + displayButton.Tag.ToString() + " is null then 0 else "+ displayButton.Tag.ToString()  + " end as f from EcoleSetting") == 1)
                {
                    buttonsToRemove.Add(displayButton);
                }
               
            }

            // Remove the DisplayButtons that do not meet the condition from TargetPanel
            foreach (DisplayButton button in buttonsToRemove)
            {
                TargetPanel.Children.Remove(button);
                button.Width = 300;
                button.Height = 200;
                button.FontSize = 20;
                // Add the removed DisplayButton to ShortCutPanel
                ShortCutPanel.Children.Add(button);
            }
        }

        private void GroupBox_Drop(object sender, DragEventArgs e)
        {

        }

        private void TargetPanel_Drop(object sender, DragEventArgs e)
        {
            var obj = e.Data.GetData(typeof(DisplayButton)) as DisplayButton;
            ((WrapPanel)obj.Parent).Children.Remove(obj);
            Connexion.Insert("Update EcoleSetting Set " + obj.Tag + " = 0");
            obj.Width = 150;
            obj.Height = 100;
            obj.FontSize = 15;
            TargetPanel.Children.Add(obj);
        }

        private void ShortCutPanel_Drop(object sender, DragEventArgs e)
        {
            var obj = e.Data.GetData(typeof(DisplayButton)) as DisplayButton;
            ((WrapPanel)obj.Parent).Children.Remove(obj);
            obj.Width = 300;
            obj.Height = 200;
            obj.FontSize = 20;
            Connexion.Insert("Update EcoleSetting Set " + obj.Tag + " = 1");
            ShortCutPanel.Children.Add(obj);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander.BorderThickness = new Thickness(2);
            Methods.ChangeGridHeight(ref row, "150");
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander.BorderThickness = new Thickness(0);
            Methods.ChangeGridHeight(ref row, "30");
        }
    }
}