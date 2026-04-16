using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for Discounts.xaml
    /// </summary>
    public partial class Discount : Window
    {
        public Discount(string SID = "" , string CID = "" )
        {
            try
            {
                InitializeComponent();
                int lang = Connexion.Language();
                SetLang();
                Connexion.FillCB(ref CBclass, " Select *,Class.CName as Name From Class  ");
                FILLDG("");
                if(CID != "")
                {
                    CBclass.EditValue = CID;
                }
                if(SID != "")
                {
                    CBStudent.EditValue = SID;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void FILLDG(string condition)
        {
            Connexion.FillDG(ref DGDiscount, "" +
                       "Select Discounts.CPrice As SPrice ," +
                       "Discounts.TPrice as TPrice , " +
                       "Discounts.StudentID, " +
                       "Discounts.ClassID, " +
                       "Discounts.ID , " +
                       "Discounts.Note As Note, " +
                       "(Students.FirstName + ' ' + students.LastName) As StudentName , " +
                       "Class.CName As ClassName ," +
                       "Discounts.Done as EndDate , " +
                       "Discounts.StudentDate as StudentDate ," +
                       "Discounts.TeacherDate as TeacherDate " +
                       "From Discounts " +
                       "Join Students on Students.ID = Discounts.StudentID " +
                       "Join Class ON Class.ID = Discounts.ClassID " +condition);
        }

        private void CBStudent_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                DataRowView rowS = (DataRowView)CBStudent.SelectedItem;
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                if (rowS != null && rowC != null)
                {   
                    string SID = rowS["ID"].ToString();
                    string ClassID = rowC["ID"].ToString();
                    if (Connexion.GetInt(ClassID, "Class", "TPaymentMethod") == 1)
                    {
                        SP2.Visibility = Visibility.Visible;
                        TPrice.Visibility = Visibility.Visible;
                        TPrice.Text = Connexion.GetInt(ClassID, "Class", "TPayment").ToString();
                    }
                    else
                    {
                        SP2.Visibility = Visibility.Collapsed;
                        TPrice.Visibility = Visibility.Collapsed;
                    }
                    FILLDG("Where StudentID = " + SID);
                    SPrice.Text = Connexion.GetInt(ClassID, "Class", "CPrice").ToString();
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

                this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");

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

        private void CBclass_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                if (rowC != null)
                {
                    CBStudent.SelectedIndex = -1;
                    SPrice.Text = "";
                    TPrice.Text = "";
                    Connexion.FillCB(ref CBStudent, "SELECT * ,(FirstName + ' ' + LastName) as Name  , '" + Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "//MyPhotos\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture From Students join Class_Student on Class_Student.StudentID = Students.ID Where Class_Student.ClassID = '" + rowC["ID"].ToString() + "'");
                    string ClassID = rowC["ID"].ToString();
                    if (Connexion.GetInt(ClassID, "Class", "TPaymentMethod") != 1)
                    {
                        SP2.Visibility = Visibility.Collapsed;
                        TPrice.Visibility = Visibility.Collapsed;
                    }
                    FILLDG("Where  Class.ID = " + ClassID);
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Istrue = true;
                DataRowView row = (DataRowView)CBStudent.SelectedItem;
                DataRowView rowC = (DataRowView)CBclass.SelectedItem;
                if (row == null || rowC == null)
                {
                    return;
                }
              
                string SID = row["ID"].ToString();
                string CID = rowC["ID"].ToString();
                string GID = Connexion.GetInt(SID, "Class_Student", "GroupID", "StudentID", "ClassID", CID).ToString();
                if (SDateStart.Text == "")
                {
                    MessageBox.Show("Please Select the Starting Session for the Discount ");
                    return;
                }
                string StuDateStart = SDateStart.Text.Replace("/", "-");
                if (!Connexion.IFNULL("Select  * From Discounts Where StudentID = '" + SID + "' and ClassID = " + CID + " and (Done != '' or done is not null)"))
                {
                    Istrue = false;
                    if (MessageBox.Show("This Student Already Have Discount , Do you want to override it?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Connexion.Insert("Update Discounts Set Done = " + StuDateStart + " Where StudentID = '" + SID + "' and ClassID = " + CID);
                    }
                }
                else
                {
                    Istrue = true;
                }
                if (Istrue)
                {
                    
                    int DID;
                    if (Connexion.GetInt(CID, "Class", "TPaymentMethod") == 1)
                    {
                        if (TDateStart.Text == "")
                        {
                            MessageBox.Show("Please Select the Start Session For Teacher Payment");
                            return; 
                        }
                        string TeaDateStart = TDateStart.Text.Replace("/", "-");
                        DID = Connexion.GetInt("Insert into discounts(StudentID,ClassID,CPrice,TPrice,Note,StudentDate,TeacherDate) output inserted.ID  Values " +
                               "(" + SID + ", " +
                               "" + CID + " , " +
                               "" + SPrice.Text + " , " +
                               "" + TPrice.Text + " , " +
                               " N'" + Note.Text + "', " +
                               "'" + StuDateStart + "' , " +
                               "'" + TeaDateStart +"')");
                    }
                    else
                    {
                        DID = Connexion.GetInt("Insert into discounts(StudentID,ClassID,CPrice,TPrice,Note,StudentDate,TeacherDate) output inserted.ID Values " +
                             "(" + SID + ", " +
                             "" + CID + " , " +
                             "" + SPrice.Text + " , " +
                             "null, " +
                             "N'" + Note.Text + "', " +
                             "'" + StuDateStart + "', " +
                             " null" +
                             "  )");
                    }
                    Connexion.Insert("UPDATE A_S " +
                        "SET " +
                        "A_S.price = CASE " +
                        "WHEN A_S.Status = 1 " +
                        "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                        "THEN dbo.GetPriceSession(A_S.StudentID, A_S.ID) " +
                        "ELSE 0 " +
                        "END, " +
                        "A_S.Tprice = CASE " +
                        "WHEN A_S.Status = 1 " +
                        "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                        "THEN dbo.GetTPriceSession(A_S.StudentID, A_S.ID) " +
                        "ELSE 0 " +
                        "END " +
                        "FROM Attendance_Student A_S " +
                        "INNER JOIN Attendance A " +
                        "ON A.ID = A_S.ID " +
                        "INNER JOIN Groups G " +
                        "ON G.GroupID = A.GroupID " +
                        "WHERE G.ClassID = " + CID + " And A_S.StudentID = " + SID);
                    Connexion.InsertHistory(0, DID.ToString(), 10);
                    FILLDG("Where StudentID = " + SID + " and Class.ID = " + CID);
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnLeave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)DGDiscount.SelectedItem;
                if (row == null)
                {
                    return;
                }
                if (MessageBox.Show("Are you Sure you want to Stop this discount", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string SID = row["StudentID"].ToString();
                    string CID = row["CLassID"].ToString();
                    string ID = row["ID"].ToString();
                  
                    Connexion.Insert("Update Discounts Set Done = '" + DateTime.Today.ToString("dd-MM-yyyy") + "' Where ID = " + ID);
                    Connexion.Insert("UPDATE A_S " +
                       "SET " +
                       "A_S.price = CASE " +
                       "WHEN A_S.Status = 1 " +
                       "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                       "THEN dbo.GetPriceSession(A_S.StudentID, A_S.ID) " +
                       "ELSE 0 " +
                       "END, " +
                       "A_S.Tprice = CASE " +
                       "WHEN A_S.Status = 1 " +
                       "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                       "THEN dbo.GetTPriceSession(A_S.StudentID, A_S.ID) " +
                       "ELSE 0 " +
                       "END " +
                       "FROM Attendance_Student A_S " +
                       "INNER JOIN Attendance A " +
                       "ON A.ID = A_S.ID " +
                       "INNER JOIN Groups G " +
                       "ON G.GroupID = A.GroupID " +
                       "WHERE G.ClassID = " + CID + " And A_S.StudentID = " + SID);
                    Connexion.InsertHistory(1, ID, 10);
                    MessageBox.Show("Discount Ended Succesfully");
                }
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
                DataRowView row = (DataRowView)DGDiscount.SelectedItem;
                if (row == null)
                {
                    return;
                }
                if (MessageBox.Show("Are you Sure you want to Delete this discount", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string SID = row["StudentID"].ToString();
                    string CID = row["CLassID"].ToString();
                    string ID = row["ID"].ToString();
                    Connexion.Insert("delete from  Discounts  Where ID = " + ID);
                    Connexion.Insert("UPDATE A_S " +
                       "SET " +
                       "A_S.price = CASE " +
                       "WHEN A_S.Status = 1 " +
                       "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                       "THEN dbo.GetPriceSession(A_S.StudentID, A_S.ID) " +
                       "ELSE 0 " +
                       "END, " +
                       "A_S.Tprice = CASE " +
                       "WHEN A_S.Status = 1 " +
                       "OR(A_S.Status = 0 AND(SELECT absent FROM EcoleSetting) = 1) " +
                       "THEN dbo.GetTPriceSession(A_S.StudentID, A_S.ID) " +
                       "ELSE 0 " +
                       "END " +
                       "FROM Attendance_Student A_S " +
                       "INNER JOIN Attendance A " +
                       "ON A.ID = A_S.ID " +
                       "INNER JOIN Groups G " +
                       "ON G.GroupID = A.GroupID " +
                       "WHERE G.ClassID = " + CID + " And A_S.StudentID = " + SID);
                    Connexion.InsertHistory(1, ID, 10);
                    MessageBox.Show("Discount Deleted Succesfully");
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

    }
}
