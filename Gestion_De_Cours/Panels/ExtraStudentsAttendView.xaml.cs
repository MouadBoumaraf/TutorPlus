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
    /// Interaction logic for ExtraStudentsAttendView.xaml
    /// </summary>
    public partial class ExtraStudentsAttendView : Window
    {
        string ID = "";
        int CID =-1 ;
        public ExtraStudentsAttendView(string AID)
        {
            try
            {
                InitializeComponent();
                SetLang();
                ID = AID;
                string GID = Connexion.GetString("Select GroupID from Attendance where ID =" + AID);
                CID = Connexion.GetClassID(GID);
                string Name = Connexion.GetString("Select Case When MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName else Class.CName end as f from Class Join Groups on Groups.ClassID = Class.ID Where Groups.GroupID = " + GID);
                LBName.Content = Name;
                Connexion.FillDG(ref DGExtra, "Select * from Attendance_StudentsOneSes where AID =" + AID);
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        public void SetLang()
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (TBName.Text != "")
                {
                    int TPayment = Connexion.GetInt("Select TPayment from Class Where ID = " + CID) / 4;
                    int Ammount = Connexion.GetInt("Select CPrice from Class Where ID = " + CID) / 4;
                    int Inserted = Connexion.GetInt("Insert into Attendance_StudentsOneSes(AID,Name,Price,TPrice) OUTPUT Inserted.ID Values(" + ID + ",N'" + TBName.Text + "'," + Ammount + "," + TPayment + ")");
                    MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                    Connexion.FillDG(ref DGExtra, "Select * from Attendance_StudentsOneSes where AID =" + ID);
                    Connexion.InsertHistory(0, Inserted.ToString(), 15);

                }
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataRowView row = (DataRowView)DGExtra.SelectedItem;
                if (row == null)
                {
                    return;
                }
                FastReports.PrintPaymentStudent(ref DGExtra, "", "ExtraStu");
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                DataRowView row = (DataRowView)DGExtra.SelectedItem;
                if (row == null)
                {
                    return;
                }
                string ASOID = row["ID"].ToString();
                Connexion.Insert("Delete from Attendance_StudentsOneSes where ID = " + ASOID);
                MessageBox.Show("Deleted Succesfully");
                Connexion.FillDG(ref DGExtra, "Select * from Attendance_StudentsOneSes where AID =" + ID);
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
    }
}
