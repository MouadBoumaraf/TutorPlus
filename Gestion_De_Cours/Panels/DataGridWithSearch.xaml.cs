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
    /// Interaction logic for DataGridWithSearch.xaml
    /// </summary>
    /// 

    public partial class DataGridWithSearch : Window
    {
        public object SelectedValue { get; private set; }
        string type;
        DataTable dtview; 
        public DataGridWithSearch(string Type , DataTable dt )
        {
            try
            {
                InitializeComponent();
                SetLang();
                dtview = dt;
                type = Type;
                DG.ItemsSource = dtview.DefaultView;
                if(Type == "Sessions")
                {
                    group.Visibility = Visibility.Collapsed;
                    Session.Visibility = Visibility.Collapsed;
                }
                else if(Type == "Groups")
                {
                    date.Visibility = Visibility.Collapsed;
                    Session.Visibility = Visibility.Collapsed;
                    Status.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
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
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Condition = "1 > 0 ";
                if (TBSearch.Text != "")
                {

                    if (type == "Sessions")
                    {
                        Condition += " and ( Ses = " + TBSearch.Text + " or date Like '%" + TBSearch.Text + "%')";
                    }
                    else if (type == "Groups")
                    {
                        Condition += " and GroupName ='" + TBSearch.Text + "' "; 
                    }

                }
                dtview.DefaultView.RowFilter = Condition;
               
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView rowview = DG.SelectedItem as DataRowView;
            if(rowview != null)
            {
                if(type == "Sessions")
                {
                    if (rowview["Ses"].ToString() == "-1")
                    {
                        SelectedValue = -1;

                    }
                    else if (rowview["Ses"].ToString() == "-2")
                    {
                        SelectedValue = -2;
                    }
                    else
                    {
                        SelectedValue = rowview["Date"].ToString();
                    }
                }
                else if (type == "Groups")
                {
                    SelectedValue = rowview["ID"].ToString();
                }
                this.DialogResult = true; // Optional: Indicates success
                this.Close();
            }
        }
    }
}
