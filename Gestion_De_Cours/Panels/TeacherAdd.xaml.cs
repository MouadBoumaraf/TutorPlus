using Microsoft.Win32;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.UControl;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WIA;

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for TeacherAdd.xaml
    /// </summary>
    public partial class TeacherAdd : Window
    {

        string Type;
        string path = "";
        public  string TID = "-1";
        OpenFileDialog open = new OpenFileDialog();
        public TeacherAdd(string TypeUC, string ID)
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
                Type = TypeUC;
                if (Type == "Show")
                {
                    TID = ID;
                    FName.Text = Connexion.GetString(ID, "Teacher", "TFirstName");
                    LName.Text = Connexion.GetString(ID, "Teacher", "TLastName");
                    Phone.Text = Connexion.GetString(ID, "Teacher", "TPhoneNumber");
                    CCP.Text = Connexion.GetString(ID, "Teacher", "TCCP");
                    Adress.Text = Connexion.GetString(ID, "Teacher", "TAdress");
                    Note.Text = Connexion.GetString(ID, "Teacher", "TNote");
                    Email.Text = Connexion.GetString(ID, "Teacher", "Email");
                    Gender.SelectedIndex = Connexion.GetInt(ID, "Teacher", "TGender");
                    Date.Text = Connexion.GetString(ID, "Teacher", "TBirthDate");
                    Connexion.FillDataGrid(ID, ref DGFreeTime, "FreeTime_Teacher");
                    string ShowPic = Connexion.GetImagesFile() + "\\T" + ID + ".jpg";
                    string query = "Select Class.CName + ' ' + Groups.GroupName as GroupName ," +
                      "Groups.GroupID as GID ," +
                      "TPayment.ID  as IDTPAY , " +
                      "TPayment.Ses as Sessions ," +
                      "TPayment.Date as Date," +
                      "Dbo.GetDateGroup(TPayment.FromSes +1 , TPayment.GID) as FromSesDate ," +
                      "Dbo.GetDateGroup(TPayment.FromSes + TPayment.Ses , TPayment.GID) as ToSesDate," +
                      "dbo.TotalStudentSessionsGroup(TPayment.FromSes ,TPayment.FromSes + TPayment.Ses,Groups.GroupID) as TotalSesTeacher," +
                      "TPayment.Total as TotalP  " +
                      "from TPayment  join Groups  On Groups.GroupID = TPayment.GID " +
                      "Join CLass on Class.ID = Groups.ClassID " +
                      "Where TPayment.TID =  " + TID;
                    Connexion.FillDG(ref DGPay, query);
                    if (System.IO.File.Exists(ShowPic))
                    {
                        path = ShowPic; 
                        Methods.insertPic(ref TPicture, ShowPic);
                    }
                    else
                    {
                        if (Gender.SelectedIndex == 1)
                        { 
                            Methods.insertPic(ref TPicture, Directory.GetCurrentDirectory() + @"\Images\Women.png");
                        }
                        else if (Gender.SelectedIndex == 0)
                        {
                            Methods.insertPic(ref TPicture, Directory.GetCurrentDirectory() + @"\Images\man.png");
                        }
                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TeacherM") != 1)
                    {
                        FName.IsReadOnly = true;
                        LName.IsReadOnly = true;
                        Phone.IsReadOnly = true;
                        CCP.IsReadOnly = true;
                        Adress.IsReadOnly = true;
                        Email.IsReadOnly = true;
                        Note.IsReadOnly = true;
                        Gender.IsEnabled = false;
                        Date.IsEnabled = false;

                    }
                    if (Connexion.GetInt(Connexion.WorkerID, "Users", "TPayV") != 1)
                    {
                        Payment.IsEnabled = false; 
                    }

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

        private void Button_Click_1(object sender, RoutedEventArgs e) // button for Freetime Add
        {
            try
            {
                bool isrepeated = true;
                if (Day.Text != "" && HFrom.Text != "" && MFrom.Text != "" && HTo.Text != "" && MTo.Text != "")
                {

                    int A1;
                    int B1;
                    int A2 = Int16.Parse(HFrom.Text);
                    int B2 = Int16.Parse(HTo.Text);

                    if (A2 < B2)
                    {
                        for (int i = 0; i < DGFreeTime.Items.Count; i++)
                        {
                            string DGDay = "";
                            if (Type == "Show")
                            {
                                DGDay = ((DataRowView)DGFreeTime.Items[i]).Row["Day"].ToString();
                            }
                            if (DGDay == Day.Text)
                            {
                                A1 = Int16.Parse(((DataRowView)DGFreeTime.Items[i]).Row["TimeStart"].ToString().Substring(0, 2));
                                B1 = Int16.Parse(((DataRowView)DGFreeTime.Items[i]).Row["TimeEnd"].ToString().Substring(0, 2));
                                if (A2 >= A1 && A2 <= B1)
                                {
                                    isrepeated = false;
                                    MessageBox.Show("This Duration is Already Included1");
                                }
                                else if (B2 > A1 && B2 < B1)
                                {
                                    isrepeated = false;
                                    MessageBox.Show("This Duration is Already Included2");
                                }
                                else if (A2 < A1 && B1 >= A1)
                                {
                                    isrepeated = false;
                                    MessageBox.Show("This Duration is Already Included3");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error This Period is impossible");
                        isrepeated = false;
                    }
                    if (isrepeated)
                    {
                        string Start = Convert.ToString(HFrom.Text) + ":" + Convert.ToString(MFrom.Text);
                        string End = Convert.ToString(HTo.Text) + ":" + Convert.ToString(MTo.Text);
                        Connexion.Insert("Insert into FreeTime_Teacher Values ('" + TID + "','" + Day.Text + "','" + Start + "','" + End + "')");
                        Connexion.FillDataGrid(TID, ref DGFreeTime, "FreeTime_Teacher");
                    }
                }
                else
                {
                    MessageBox.Show("Please Fill all the combobox");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        public string ResponseText
        {
            get { return TID; }
        }

        public void SubmitT(object sender, RoutedEventArgs e)
        {
            try
            {
                
                string Birthdate = Date.Text;
                if (Type == "Add")
                {
                    TID = Connexion.InsertTeacher(FName.Text.Replace("'", "''"), LName.Text.Replace("'", "''"), Phone.Text.Replace("'", "''"), Gender.SelectedIndex, Adress.Text.Replace("'", "''"), CCP.Text.Replace("'", "''"), Note.Text.Replace("'", "''"), Birthdate, Email.Text.Replace("'", "''")).ToString();
                    if (open.FileName != "")
                    {
                        System.IO.File.Copy(open.FileName, Connexion.GetImagesFile() + "\\" + "T" + TID + ".jpg");
                    }
                    Connexion.InsertHistory(0, TID, 1);
                    MessageBox.Show("Inserted Successfully");
                   
                        DialogResult = true;
                    
                    this.Close();
                }
                else
                {
                    Connexion.Insert("update  Teacher set " +
                        "TFirstName = N'" + FName.Text.Replace("'", "''") + "' , " +
                        "TLastName = N'" + LName.Text.Replace("'", "''") + "' ," +
                        "TPhoneNumber = N'" + Phone.Text.Replace("'", "''") + "'," +
                        "TGender = " + Gender.SelectedIndex + "  ," +
                        "TAdress = '" + Adress.Text.Replace("'", "''") + "', " +
                        "TCCP = '" + CCP.Text + "' , " +
                        "TNote = '" + Note.Text.Replace("'", "''") + "' , " +
                        "TBirthDate = '" + Birthdate + "' ," +
                        "Email = '" + Email.Text.Replace("'", "''") +
                        "' Where ID = " + TID);
                    Connexion.InsertHistory(2, TID, 1);
                    if (path != "")
                    {
                        if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\T" + TID + ".jpg"))
                        {
                            System.IO.File.Delete(Connexion.GetImagesFile() + "\\T" + TID + ".jpg");
                        }
                        System.IO.File.Copy(path,
                            Connexion.GetImagesFile() + "\\T" + TID + ".jpg");
                    }
                    MessageBox.Show("Updated Successfully");
                    this.Close();
                    FreeTime.IsSelected = true; 

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
            Methods.EnterText(e, ref Phone , ref FName);
        }

        private void Phone_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref CCP , ref LName);
        }

        private void CCP_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Adress , ref Phone);
        }

        private void Adress_KeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Email , ref CCP);

        }

        private void Note_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Email.Focus();
            }
        }

        private void BtnDeleteFree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string Day = ((DataRowView)DGFreeTime.SelectedItem).Row["Day"].ToString();
                string TimeStart = ((DataRowView)DGFreeTime.SelectedItem).Row["TimeStart"].ToString();
                string TimeEnd = ((DataRowView)DGFreeTime.SelectedItem).Row["TimeEnd"].ToString();
                Connexion.Insert("Delete From FreeTime_Teacher Where ID = '" + TID + "' And Day = '" + Day + "' And TimeStart = '" + TimeStart + "' And TimeEnd = '" + TimeEnd + "'");
                Connexion.FillDataGrid(TID, ref DGFreeTime, "FreeTime_Teacher");
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Email_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Methods.EnterText(e, ref Note, ref Adress);
        }

        private void Phone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (path != "")
            {
                WindowEditImage window = new WindowEditImage(path);
                window.ShowDialog();
                if (window.DialogResult == true && window.ResponseText != null ) 
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(window.ResponseText);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    TPicture.Source = bitmap;
                    path = window.ResponseText; 
                }
            }
        }

        public void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var deviceManager = new DeviceManager();

                // Create an empty variable to store the scanner instance
                DeviceInfo firstScannerAvailable = null;

                // Loop through the list of devices to choose the first available
                for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
                {
                    // Skip the device if it's not a scanner
                    if (deviceManager.DeviceInfos[i].Type != WiaDeviceType.ScannerDeviceType)
                    {
                        continue;
                    }

                    firstScannerAvailable = deviceManager.DeviceInfos[i];

                    break;
                }

                // Connect to the first available scanner
                var device = firstScannerAvailable.Connect();

                // Select the scanner
                var scannerItem = device.Items[1];

                // Retrieve a image in JPEG format and store it into a variable
                var imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatJPEG);
                path = @"C:\ProgramData\EcoleSetting\EcolePhotos\Scan.jpg";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                imageFile.SaveFile(path);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                TPicture.Source = bitmap;

            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            open.Filter = "Image Files(*.jpg; *.jpeg; *.bmp )|*.jpg; *.jpeg; *.bmp";
            if (open.ShowDialog() == true)
            {

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(open.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                path = open.FileName; 
                TPicture.Source = bitmap;
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            if(TID == "-1")
            {
                return;
            }
            SelectAllTeacherPaymentGroup TPG = new SelectAllTeacherPaymentGroup(TID);
            TPG.Show();
        }

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Print(object sender, RoutedEventArgs e)
        {

        }

        private void DGPay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                DataRowView row = (DataRowView)DGPay.SelectedItem;
                if (row != null)
                {
                    int TPAYID = int.Parse(row["IDTPAY"].ToString());
                    Panels.DetailedTPayment DTP = new DetailedTPayment(TPAYID);
                    DTP.ShowDialog();
                  
                 
                }

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }
    }
}
