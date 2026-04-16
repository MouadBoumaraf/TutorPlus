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
using Gestion_De_Cours.Classes; 

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for TeacherPaymentGroup.xaml
    /// </summary>
    public partial class TeacherPaymentGroup : Window
    {
        string GID;
        int ses;
        public TeacherPaymentGroup(string GroupID, int Sessions)
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                GID = GroupID;
                ses = Sessions;
                DataTable dt = new DataTable();
                Methods.FillDGAttendance(ref AttendanceDG, ses, GID, ref dt);
                int sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string Price = row["TPrice"].ToString();
                    sum += int.Parse(Price);
                }
                Total.Text = sum.ToString();
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string CID = Connexion.GetInt(GID, "Groups", "ClassID", "GroupID").ToString();
                int TID = Connexion.GetInt(CID, "Class", "TID");
                Connexion.Insert("Insert into TPayment Values (" + TID + "," +
                                                                   GID + "," +
                                                                   ses + ",'" +
                                                                   Date.Text.Replace("/", "-") + "','" +
                                                                   Note.Text + "')");
                string PID = Connexion.GetID("TPayment");
                for (int i = 0; i < AttendanceDG.Items.Count; i++)
                {
                    DataRowView row = ((DataRowView)AttendanceDG.Items[i]);
                    string SID = row.Row["ID"].ToString();


                    Connexion.Insert("Insert into TPaymentStudent Values (" + PID + ","
                                                                            + SID + ","
                                                                            + row.Row["TSes"].ToString() + ","
                                                                            + row.Row["SPrice"].ToString() + ","
                                                                            + row.Row["TPrice"].ToString() + ","
                                                                            + row.Row["SuPrice"].ToString() + ")");
                    Connexion.Insert("Update Class_Student " +
                        "Set TWallet = TWallet - " + row.Row["TPrice"].ToString() +
                        "    ,SPeriods = SPeriods -" + row.Row["TSes"].ToString()
                        + " , SPayed = SPayed - " + row.Row["SPrice"].ToString() + " " +
                        "Where StudentID = " + SID + "And GroupID = " + GID);
                }
                Connexion.Insert("Update Groups Set TSessions  = Tsessions - " + ses + " WHere GroupID = " + GID);

                MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                this.Close();
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void AttendanceDG_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            //MessageBox.Show("");
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
    }
}
