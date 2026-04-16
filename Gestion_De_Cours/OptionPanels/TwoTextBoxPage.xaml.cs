using Gestion_De_Cours.Classes;
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
    /// Interaction logic for TwoTextBoxPage.xaml
    /// </summary>
    public partial class TwoTextBoxPage : Window
    {
        public TwoTextBoxPage(string LBTitle , string LBFirst ,string LB2nd , string TB1DefaultValue , string TB2DefaultValue)
        {

            InitializeComponent();
            SetLang();
            LBMain.Content = LBTitle;
            LB1.Content = LBFirst;
            LB2.Content = LB2nd;
            TB1.Text = TB1DefaultValue;
            TB2.Text = TB2DefaultValue;
            

        }
        private void SetLang()
        {
            ResourceDictionary ResourceDic = new ResourceDictionary();
            if (Connexion.Language() == 1)
            {
                ResourceDic.Source = new Uri("../Dictionary\\AR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.RightToLeft;
            }
            else if (Connexion.Language() == 0)
            {
                ResourceDic.Source = new Uri("../Dictionary\\EN.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            else if (Connexion.Language() == 2)
            {
                ResourceDic.Source = new Uri("../Dictionary\\FR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;
            }
            this.Resources.MergedDictionaries.Add(ResourceDic);
        }

        public string[] Result { get; private set; }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = new string[2];
            Result[0] = TB1.Text;
            Result[1] = TB2.Text;
            DialogResult = true;
            this.Close();

        }
    }
}
