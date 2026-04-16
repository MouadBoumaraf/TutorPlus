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

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for DetailedTPayment.xaml
    /// </summary>
    public partial class DetailedTPayment : Window
    {
        DataTable dt;
        DataTable dt2;
        int l = 0;
        public DetailedTPayment(int TPayment )
        {

            int lang = Connexion.Language();
            InitializeComponent();

            SetLang();
            dt = new DataTable();
            Methods.FillDGAttendanceOld(TPayment,ref dt , ref AttendanceDG);
            string GID = Connexion.GetString("Select GID from TPayment Where ID = " + TPayment);
            Methods.FillDGSesStuTPaymentOld(ref DGSessions, TPayment, ref dt2);
            if (lang == 1)
            {
                this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
            }
            l = 1;

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

        private void DGSessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PrintMethod2_Checked(object sender, RoutedEventArgs e)
        {
            DGSessions.Visibility = Visibility.Collapsed;
            AttendanceDG.Visibility = Visibility.Visible;
            this.Height = 500;
            this.Width = 800;

        }

        private void PrintMethod1_Checked(object sender, RoutedEventArgs e)
        {
            if(l == 0)
            {
                return;
            }
            DGSessions.Visibility = Visibility.Visible;
            AttendanceDG.Visibility = Visibility.Collapsed;
            this.Height = 300;
            this.Width = 350;
        }
    }
}
