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
    /// Interaction logic for Justif.xaml
    /// </summary>
    public partial class Justif : Window
    {
        public Justif()
        {
            // you can add a collapsed cb of groups if the student used to study
            // in another group then he changed 
            try
            {
                InitializeComponent();
                int lang = Connexion.Language();
                SetLang();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                Connexion.FillCB(ref CBclass, " Select *,Class.ID as ClassID, Class.CName as Name From Class ");
                FILLDG("");
            }
            catch (Exception e)
            {
                Methods.ExceptionHandle(e);
            }

        }

        private void FILLDG(string condition)
        {
            Connexion.FillDG(ref DGJustif, "Select " +
                    "Justif.ID," +
                    "Students.FirstName + ' ' + students.LastName as Sname , " +
                    "Class.CName as CName , " +
                    "Justif.Reason as Reason , " +
                    "Justif.Date as Date  , " +
                    "Justif.CID as CID , " +
                    "Justif.AID as AID ," +
                    "Students.ID as SID " +
                    "from justif Join Students on Students.ID = Justif.SID " +
                    "join Class on Class.ID = Justif.CID  " + condition);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                DataRowView rowS = (DataRowView)CBStudent.SelectedItem;
                if (rowS != null && Date.Text != "")
                {
                    string SID = rowS["ID"].ToString();
                    string CID = rowC["CLassID"].ToString();
                    int GID = Connexion.GetGroupID(SID, rowC["CLassID"].ToString());
                    int? AID = Connexion.GetIntnl("Select top 1 ID from Attendance Where GroupID = " + GID + " and Date = '" + Date.Text.Replace("/", "-") + "'");
                    int ID = Connexion.GetInt("Insert into justif(SID,Reason,Date,AID,CID) Output Inserted.ID values ('" + SID + "',N'" + TBReason.Text + " ','" + Date.Text.Replace("/", "-") + "', '" + AID + "',"+CID+")");
                    Connexion.InsertHistory(0, ID.ToString(), 11);
                    if (AID != null)
                    {
                        Commun.SetStatusAttendanceupg(SID, AID.ToString(),CID,GID.ToString(), Date.Text.Replace("/", "-"), 3);
                    }
                    MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                    CBStudent.SelectedIndex = -1;
                    FILLDG("Where Justif.SID  = " + rowS["ID"].ToString() + " and Class.ID =  " + CID);
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }

        private void CBStudent_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                DataRowView rowS = (DataRowView)CBStudent.SelectedItem;
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                if (rowS != null)
                {
                    string SID = rowS["ID"].ToString();
                    string CID = rowC["CLassID"].ToString();
                    int GID = Connexion.GetGroupID(SID, rowC["CLassID"].ToString());
                    FILLDG("Where Justif.SID  = " + rowS["ID"].ToString() + " and Justif.CID =  " + CID);
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
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

        private void BtnDelJustif_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGJustif.SelectedItem;
                if (row == null)
                {
                    return;
                }
                string CID = row["CID"].ToString();
                string ID = row["ID"].ToString();
                string SID = row["SID"].ToString();
                Connexion.Insert("Delete from Justif where ID = " + ID);
                Connexion.InsertHistory(1, ID.ToString(), 11);
                string AID = row["AID"].ToString();
                if(AID != "0")
                {
                    Commun.SetStatusAttendance(SID, AID.ToString(), 1);
                }
                MessageBox.Show("Deleted Succesfully");
                FILLDG("");
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }




        private void CBclass_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs  e)
        {
            try
            {
                CBStudent.SelectedIndex = -1;
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                if (rowC != null)
                {
                    string CID = rowC["CLassID"].ToString();
                    Connexion.FillCB(ref CBStudent, "SELECT * ,(FirstName + ' ' + LastName) as Name  , '" + Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Students join class_Student on Class_Student.StudentID = Students.ID where Class_Student.ClassID = " + CID);
                    FILLDG("Where  Justif.CID =  " + CID);
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
    }
}