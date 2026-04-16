using System.Data.SqlClient;
using Microsoft.Win32;
using Gestion_De_Cours.Classes;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for WorkerAdd.xaml
    /// </summary>
    public partial class WorkerAdd : Window
    {

        string Type;
        static string WID;
        OpenFileDialog open = new OpenFileDialog();
        public WorkerAdd(string TypeUC, string ID)
        {
            try
            {
                InitializeComponent();
                Type = TypeUC;
                SetLang();
                if (Type == "Show")
                {
                    WID = ID;
                    FName.Text = Connexion.GetString(ID, "Workers", "FirstName");
                    LName.Text = Connexion.GetString(ID, "Workers", "LastName");
                    Phone.Text = Connexion.GetString(ID, "Workers", "Number");
                    CCP.Text = Connexion.GetString(ID, "Workers", "CCP");
                    Adress.Text = Connexion.GetString(ID, "Workers", "Adress");
                    Note.Text = Connexion.GetString(ID, "Workers", "Note");
                    Gender.SelectedIndex = Connexion.GetInt(ID, "Workers", "Gender");
                    string BirthDate = Connexion.GetString(ID, "Workers", "BirthDate");
                    Date.Text = BirthDate;
                    WPaymentMethod.SelectedIndex = Connexion.GetInt(ID, "Workers", "Payment_Method");
                    WSalary.Text = Connexion.GetString(ID, "Workers", "Salary");
                    Connexion.FillDG(ref DGWorkTime, "Select ID,WID, " +
                        " Case  When Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                         "  When Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                         "  When Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                         "  When Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                         "  When Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                         "  When Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                         "  When Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                          " End  as Day , TimeStart,TimeEnd from Workers_Time where WID = " + WID);
                    //Filling the privilage panel 
                    SView.IsChecked = (Connexion.GetInt(ID, "Users", "StudentV") == 1) ? true : false;
                    SAdd.IsChecked = (Connexion.GetInt(ID, "Users", "StudentA") == 1) ? true : false;
                    SModify.IsChecked = (Connexion.GetInt(ID, "Users", "StudentM") == 1) ? true : false;
                    SDelete.IsChecked = (Connexion.GetInt(ID, "Users", "StudentD") == 1) ? true : false;
                    SPView.IsChecked = (Connexion.GetInt(ID, "Users", "SpayV") == 1) ? true : false;
                    SPAdd.IsChecked = (Connexion.GetInt(ID, "Users", "SPayA") == 1) ? true : false;
                    SPModify.IsChecked = (Connexion.GetInt(ID, "Users", "SPayM") == 1) ? true : false;
                    SPDelete.IsChecked = (Connexion.GetInt(ID, "Users", "SPayD") == 1) ? true : false;
                    CView.IsChecked = (Connexion.GetInt(ID, "Users", "ClassV") == 1) ? true : false;
                    CAdd.IsChecked = (Connexion.GetInt(ID, "Users", "ClassA") == 1) ? true : false;
                    CModify.IsChecked = (Connexion.GetInt(ID, "Users", "ClassM") == 1) ? true : false;
                    CDelete.IsChecked = (Connexion.GetInt(ID, "Users", "ClassD") == 1) ? true : false;
                    TView.IsChecked = (Connexion.GetInt(ID, "Users", "TeacherV") == 1) ? true : false;
                    TAdd.IsChecked = (Connexion.GetInt(ID, "Users", "TeacherA") == 1) ? true : false;
                    TModify.IsChecked = (Connexion.GetInt(ID, "Users", "TeacherM") == 1) ? true : false;
                    TDelete.IsChecked = (Connexion.GetInt(ID, "Users", "TeacherD") == 1) ? true : false;
                    TPView.IsChecked = (Connexion.GetInt(ID, "Users", "TPayV") == 1) ? true : false;
                    TPAdd.IsChecked = (Connexion.GetInt(ID, "Users", "TPayA") == 1) ? true : false;
                    TPModify.IsChecked = (Connexion.GetInt(ID, "Users", "TPayM") == 1) ? true : false;
                    TPDelete.IsChecked = (Connexion.GetInt(ID, "Users", "TPayD") == 1) ? true : false;
                    Stat.IsChecked = (Connexion.GetInt(ID, "Users", "Stat") == 1) ? true : false;
                    History.IsChecked = (Connexion.GetInt(ID, "Users", "History") == 1) ? true : false;
                    EcoleInfo.IsChecked = (Connexion.GetInt(ID, "Users", "EcoleInfo") == 1) ? true : false;
                    CashRegister.IsChecked = (Connexion.GetInt(ID, "Users", "CashRegister") == 1) ? true : false;
                    WView.IsChecked = (Connexion.GetInt(ID, "Users", "WorkerV") == 1) ? true : false;
                    WAdd.IsChecked = (Connexion.GetInt(ID, "Users", "WorkerA") == 1) ? true : false;
                    WModify.IsChecked = (Connexion.GetInt(ID, "Users", "WorkerM") == 1) ? true : false;
                    WDelete.IsChecked = (Connexion.GetInt(ID, "Users", "WorkerD") == 1) ? true : false;
                    AView.IsChecked = (Connexion.GetInt(ID, "Users", "AttendanceV") == 1) ? true : false;
                    AAdd.IsChecked = (Connexion.GetInt(ID, "Users", "AttendanceA") == 1) ? true : false;
                    AModify.IsChecked = (Connexion.GetInt(ID, "Users", "AttendanceM") == 1) ? true : false;
                    ADelete.IsChecked = (Connexion.GetInt(ID, "Users", "AttendanceD") == 1) ? true : false;
                    TBUsername.Text = Connexion.GetString(ID, "Users", "UserName");
                    TBPass.Password = Connexion.GetString(ID, "Users", "Password").Substring(1);
                    if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\W" + ID + ".jpg"))
                    {
                        string ShowPic = Connexion.GetImagesFile() + "\\W" + ID + ".jpg";
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(ShowPic);
                        bitmap.EndInit();
                        WPicture.Source = bitmap;
                    }
                    else
                    {
                        if (Gender.SelectedIndex == 1)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\Women.png");
                            bitmap.EndInit();
                            WPicture.Source = bitmap;
                        }
                        else if (Gender.SelectedIndex == 0)
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Images\man.png");
                            bitmap.EndInit();
                            WPicture.Source = bitmap;
                        }
                    }
                }
                else
                {
                    TabWorkTime.IsEnabled = false;
                    TabPrivilage.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MTo.SelectedIndex != -1)
                {
                    Connexion.Insert("Insert into Workers_Time Values (" + WID + "," + Day.SelectedIndex + ",'" + HFrom.SelectedItem.ToString() + ":" + MFrom.SelectedItem.ToString() + "','" + HTo.SelectedItem.ToString() + ":" + MTo.SelectedItem.ToString() + "')");
                    Connexion.FillDG(ref DGWorkTime, "Select ID,WID, " +
                         " Case  When Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                          "  When Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                          "  When Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                          "  When Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                          "  When Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                          "  When Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                          "  When Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                           " End  as Day , TimeStart,TimeEnd from Workers_Time where WID = " + WID);
                    Day.SelectedIndex = -1;
                    MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                }

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }
        public void SubmitW(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Type == "Add")
                {
                    if(FName.Text !="" && LName.Text != "" && Phone.Text != "")
                    {
                        WID = Connexion.GetInt("Insert into " +
                            "Workers  OUTPUT Inserted.ID values (N'"
                                     + FName.Text + "',N'"
                                     + LName.Text + "','"
                                     + Phone.Text + "','"
                                     + Gender.SelectedIndex + "','"
                                     + Date.Text + "','"
                                     + DateTime.Today.ToString("g") + "','"
                                     + CCP.Text + "','"
                                     + WSalary.Text + "','"
                                     + WPaymentMethod.SelectedIndex + "','"
                                     + Adress.Text + "','"
                                     + Note.Text + "',1)").ToString();

                        Connexion.InsertHistory(0, WID, 2);
                        if (open.FileName != "")
                        {
                            System.IO.File.Copy(open.FileName, 
                                Connexion.GetImagesFile() + "\\W" + WID + ".jpg");
                        }
                        MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                        TabWorkTime.IsEnabled = true;
                        TabPrivilage.IsEnabled = true;
                        TabWorkTime.IsSelected = true;
                    }
                    else
                    {
                        MessageBox.Show("Please fill in the information");
                    }
                }
                else
                {
                    Connexion.Insert("update Workers  " +
                          "Set FirstName = N'"+ FName.Text + "'," +
                          "LastName = N'"+ LName.Text + "'," +
                          "Number = '" + Phone.Text + "'," +
                          "Gender = '" + Gender.SelectedIndex + "'," +
                          "BirthDate ='" + Date.Text + "'," +
                          "CCP = '"+ CCP.Text + "'," +
                          "Salary ='" + WSalary.Text + "'," +
                          "Payment_Method = '"+ WPaymentMethod.SelectedIndex + "'," +
                          "Adress = '" + Adress.Text + "'," +
                          "Note = '"+ Note.Text + "' Where ID = " + WID);
                    Connexion.InsertHistory(2, WID, 2);
                    if (open.FileName != "")
                    {
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\W" + WID + ".jpg"))
                        {
                            System.IO.File.Delete(Connexion.GetImagesFile() + "\\W" + WID + ".jpg");
                        }
                        System.IO.File.Copy(open.FileName,
                            Connexion.GetImagesFile() + "\\W" + WID + ".jpg");
                    }
                    MessageBox.Show("Updated Succesfully");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        public void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                open.Filter = "Image Files(*.jpg; *.jpeg; *.bmp )|*.jpg; *.jpeg; *.bmp";
                if (open.ShowDialog() == true)
                {

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(open.FileName);
                    bitmap.EndInit();
                    WPicture.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void FName_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref LName);
        }

        private void LName_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Phone);
        }

        private void Phone_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref CCP);
        }

        private void CCP_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Adress);
        }

        private void Adress_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Note);

        }

        private void Note_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void BtnDeleteTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Type == "Show")
                {
                    if( (DataRowView)DGWorkTime.SelectedItem ==null)        
                    {
                        return;
                    }
                    string ID = ((DataRowView)DGWorkTime.SelectedItem).Row["ID"].ToString();
                    Connexion.Insert("Delete From Workers_Time Where ID = " + ID );
                    Connexion.FillDG(ref DGWorkTime, "Select ID,WID, " +
                        " Case  When Day = 0 Then N'" + this.Resources["Sunday"].ToString() + "' " +
                         "  When Day = 1 Then N'" + this.Resources["Monday"].ToString() + "' " +
                         "  When Day = 2 Then N'" + this.Resources["Tuesday"].ToString() + "'" +
                         "  When Day = 3 Then N'" + this.Resources["Wednesday"].ToString() + "'" +
                         "  When Day = 4 Then N'" + this.Resources["Thursday"].ToString() + "' " +
                         "  When Day = 5 Then N'" + this.Resources["Friday"].ToString() + "'" +
                         "  When Day = 6 Then N'" + this.Resources["Saturday"].ToString() + "' " +
                          " End  as Day , TimeStart,TimeEnd from Workers_Time where WID = " + WID);
                }
                
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {

                if (TBUsername.Text != "" && TBPass.Password.ToString() != "")
                {
                    if (Connexion.GetInt("Select count(*) from Users Where ID = " + WID) == 0)
                    {

                        if (Connexion.GetInt("Select count(*) from Users Where UserName ='" + TBUsername.Text.ToLower() + "'") == 0)
                        {
                            Connexion.Insert("Insert into Users values ('"
                                + WID + "',N'"
                                + TBUsername.Text.ToLower() + "',N'@"
                                + TBPass.Password.ToString().ToLower() + "','"
                                + ((SView.IsChecked == true) ? "1" : "0") + "','"
                                + ((SAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((SModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((SDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((SPView.IsChecked == true) ? "1" : "0") + "','"
                                + ((SPAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((SPModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((SPDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((CView.IsChecked == true) ? "1" : "0") + "','"
                                + ((CAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((CModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((CDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((TView.IsChecked == true) ? "1" : "0") + "','"
                                + ((TAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((TModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((TDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((TPView.IsChecked == true) ? "1" : "0") + "','"
                                + ((TPAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((TPModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((TPDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((CashRegister.IsChecked == true) ? "1" : "0") + "','"
                                + ((Stat.IsChecked == true) ? "1" : "0") + "','"
                                + ((History.IsChecked == true) ? "1" : "0") + "','"
                                + ((EcoleInfo.IsChecked == true) ? "1" : "0") + "','"
                                + ((WView.IsChecked == true) ? "1" : "0") + "','"
                                + ((WAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((WModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((WDelete.IsChecked == true) ? "1" : "0") + "','"
                                + ((AView.IsChecked == true) ? "1" : "0") + "','"
                                + ((AAdd.IsChecked == true) ? "1" : "0") + "','"
                                + ((AModify.IsChecked == true) ? "1" : "0") + "','"
                                + ((ADelete.IsChecked == true) ? "1" : "0") + "')"
                                );
                            MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                        }
                        else
                        {
                            MessageBox.Show("This Username is already taken insert another one");
                            TBUsername.Text = "";
                            TBPass.Password = "";
                        }
                    }
                    else
                    {
                        Connexion.Insert("Update  Users Set UserName ='" + TBUsername.Text.ToLower() + "',Password = '!" + TBPass.Password.ToString().ToLower() + "' ," +
                            "StudentV = '" + ((SView.IsChecked == true) ? "1" : "0") + "'," +
                            "StudentA = '" + ((SAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "StudentM = '" + ((SModify.IsChecked == true) ? "1" : "0") + "'," +
                            "StudentD = '" + ((SDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "SPayV = '" + ((SPView.IsChecked == true) ? "1" : "0") + "'," +
                            "SPayA = '" + ((SPAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "SPayM = '" + ((SPModify.IsChecked == true) ? "1" : "0") + "'," +
                            "SPayD = '" + ((SPDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "ClassV = '" + ((CView.IsChecked == true) ? "1" : "0") + "'," +
                            "ClassA = '" + ((CAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "ClassM = '" + ((CModify.IsChecked == true) ? "1" : "0") + "'," +
                            "ClassD = '" + ((CDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "TeacherV = '" + ((TView.IsChecked == true) ? "1" : "0") + "'," +
                            "TeacherA = '" + ((TAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "TeacherM = '" + ((TModify.IsChecked == true) ? "1" : "0") + "'," +
                            "TeacherD = '" + ((TDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "TPayV = '" + ((TPView.IsChecked == true) ? "1" : "0") + "'," +
                            "TPayA = '" + ((TPAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "TPayM = '" + ((TPModify.IsChecked == true) ? "1" : "0") + "'," +
                            "TPayD = '" + ((TPDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "CashRegister = '" + ((CashRegister.IsChecked == true) ? "1" : "0") + "'," +
                            "Stat = '" + ((Stat.IsChecked == true) ? "1" : "0") + "'," +
                            "History = '" + ((History.IsChecked == true) ? "1" : "0") + "'," +
                            "EcoleInfo = '" + ((EcoleInfo.IsChecked == true) ? "1" : "0") + "'," +
                            "WorkerV = '" + ((WView.IsChecked == true) ? "1" : "0") + "'," +
                            "WorkerA = '" + ((WAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "WorkerM = '" + ((WModify.IsChecked == true) ? "1" : "0") + "'," +
                            "WorkerD = '" + ((WDelete.IsChecked == true) ? "1" : "0") + "'," +
                            "AttendanceV = '" + ((AView.IsChecked == true) ? "1" : "0") + "'," +
                            "AttendanceA = '" + ((AAdd.IsChecked == true) ? "1" : "0") + "'," +
                            "AttendanceM = '" + ((AModify.IsChecked == true) ? "1" : "0") + "'," +
                            "AttendanceD = '" + ((ADelete.IsChecked == true) ? "1" : "0") + "' where ID = " + WID
                            ) ;
                        MessageBox.Show("Updated Succesfully");
                        Connexion.InsertHistory(2, WID, 2);
                    }
                }
            }
            catch(Exception ex)
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

        private void Day_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Day.SelectedIndex != -1)
                {
                    HFrom.Items.Clear();
                    HFrom.Items.Add("06");
                    HFrom.Items.Add("07");
                    HFrom.Items.Add("08");
                    HFrom.Items.Add("09");
                    HFrom.Items.Add("10");
                    HFrom.Items.Add("11");
                    HFrom.Items.Add("12");
                    HFrom.Items.Add("13");
                    HFrom.Items.Add("14");
                    HFrom.Items.Add("15");
                    HFrom.Items.Add("16");
                    HFrom.Items.Add("17");
                    HFrom.Items.Add("18");
                    HFrom.Items.Add("19");
                    HFrom.Items.Add("16");
                    HFrom.Items.Add("17");
                    HFrom.Items.Add("18");
                    HFrom.Items.Add("19");
                    HFrom.Items.Add("20");
                    HFrom.Items.Add("21");
                    HFrom.Items.Add("22");
                    HFrom.Items.Add("23");
                }
            }
            catch (Exception r)
            {
                Methods.ExceptionHandle(r);
            }
        }

        private void MFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HTo.Items.Clear();
            if (HFrom.SelectedIndex != -1)
            {
                string hourfrom = HFrom.SelectedItem.ToString();
                int startHour = int.Parse(hourfrom); // Convert the string to an integer

                for (int hour = startHour +1; hour <= 23; hour++)
                {
                    HTo.Items.Add(hour.ToString("D2")); // Format the hour as "07", "08", etc.
                }
            }
        }

        private void HFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                
                MFrom.Items.Clear();
                MFrom.Items.Add("00");
                MFrom.Items.Add("05");
                MFrom.Items.Add("10");
                MFrom.Items.Add("15");
                MFrom.Items.Add("20");
                MFrom.Items.Add("25");
                MFrom.Items.Add("30");
                MFrom.Items.Add("35");
                MFrom.Items.Add("40");
                MFrom.Items.Add("45");
                MFrom.Items.Add("50");
                MFrom.Items.Add("55");
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void HTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                MTo.Items.Clear();
                MTo.Items.Add("00");
                MTo.Items.Add("05");
                MTo.Items.Add("10");
                MTo.Items.Add("15");
                MTo.Items.Add("20");
                MTo.Items.Add("25");
                MTo.Items.Add("30");
                MTo.Items.Add("35");
                MTo.Items.Add("40");
                MTo.Items.Add("45");
                MTo.Items.Add("50");
                MTo.Items.Add("55");
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void MTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
