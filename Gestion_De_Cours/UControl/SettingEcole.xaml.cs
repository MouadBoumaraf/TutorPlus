using Microsoft.Win32;
using Gestion_De_Cours.Classes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gestion_De_Cours.Panels;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for SettingEcole.xaml
    /// </summary>
    public partial class SettingEcole : UserControl
    {
        OpenFileDialog open = new OpenFileDialog();
        string Appfile = "";
        public SettingEcole()
        {
            try
            {
                int lang = Connexion.Language();
                InitializeComponent();
                if (lang == 1)
                {
                    this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
                }
                SetLang();
                Connexion.FillCB(ref CBInscLevel, "Select * from Levels");
                bool Exist = Connexion.IFNULL("Select * from EcoleSetting");
                if (Exist == false)
                {
                    DataTable dt = new DataTable();
                    Connexion.FillDT(ref dt, "Select * from EcoleSetting");
                    TBNameFR.Text = dt.Rows[0]["NameFR"].ToString();
                    TBNameAR.Text = dt.Rows[0]["NameAR"].ToString();
                    TBAdress1.Text = dt.Rows[0]["Adress"].ToString();
                    TBAdress2.Text = dt.Rows[0]["Adress2"].ToString();
                    TBNumber1.Text = dt.Rows[0]["Number"].ToString();
                    TBNumber2.Text = dt.Rows[0]["Number2"].ToString();
                    TBCCP.Text = dt.Rows[0]["CCP"].ToString();
                    TBFax.Text = dt.Rows[0]["Fax"].ToString();
                  
                  //  TBPhotoFile.Text = Connexion.GetImagesFile();
                    //TBAppFile.Text = dt.Rows[0]["ApplicationFile"].ToString();
                    
                    if(lang == 1)
                    {
                        TBlockEcole.Text = dt.Rows[0]["NameAR"].ToString();
                    }
                    else
                    {
                        TBlockEcole.Text = dt.Rows[0]["NameFR"].ToString();
                    }
                    Appfile = dt.Rows[0]["ApplicationFile"].ToString();
                    if (dt.Rows[0]["Absent"].ToString() == "1")
                    {
                        YesAbs.IsChecked = true;
                    }
                    if (dt.Rows[0]["Absent"].ToString() == "0")
                    {
                        NoAbs.IsChecked = true;
                    }
                    if(dt.Rows[0]["PaymentMonth"].ToString() == "1")
                    {
                        YesMonthes.IsChecked = true;
                    }
                    if (dt.Rows[0]["PaymentMonth"].ToString() == "0")
                    {
                        NoMonthes.IsChecked = true;
                    }
                    if (dt.Rows[0]["PaymentMonth"].ToString() == "2")
                    {
                        CustomMonthes.IsChecked = true;
                    }
                    if (dt.Rows[0]["ClassPay"].ToString() == "1")
                    {
                        YesClass.IsChecked = true;
                    }
                    if (dt.Rows[0]["ClassPay"].ToString() == "0")
                    {
                        NoClass.IsChecked = true;
                    }

                }
                else
                {
                    Appfile = Connexion.GetFile();
                }
                Connexion.FillCB(ref CBLevelYears, "Select * from Levels");
                Connexion.FillCB(ref CBLevelSpec, "Select * from Levels Where IsSpeciality = 1");
                Connexion.FillCB(ref CBLevelSub, "Select * from Levels");

                Connexion.FillDG(ref DGLevel, "Select *, Case When IsSpeciality = 1 then N'" + Properties.Resources.Yes + "' When IsSpeciality = 0 then N'" + Properties.Resources.No + "' END as IsSpec from Levels");
                Connexion.FillDG(ref DGYear, "Select *,Years.ID as YearID from Years join Levels on Levels.ID = Years.LevelID");
                Connexion.FillDG(ref DGSpec, "Select Years.Year , Years.ID as YearID,  Specialities.Speciality ,Specialities.ID as SpecID ,Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID");
                Connexion.FillDG(ref DGSubjects, "Select " +
                    " Years.ID as YearID," +
                    "Subject,Speciality,SpecialityID, " +
                    "Years.Year ," +
                    "levels.IsSpeciality ," +
                    "Subjects.ID as SubjectID ," +
                    "Level," +
                    "LevelID " +
                    "from Years " +
                    "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                    "Full outer join Specialities on Specialities.YearID = Years.ID " +
                    "Join SubjectSPec on " +
                    "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                    "Union (" +
                    "Select Years.ID as YearID ," +
                    "Subject , " +
                    "null as Speciality, " +
                    "null as SpecialityID , " +
                    "Years.Year, " +
                    "Levels.IsSpeciality  , " +
                    "Subjects.ID as SubjectID , " +
                    "Level,LevelID  " +
                    "from Years " +
                    "join Levels on Levels.ID = years.LevelID " +
                    "Join  Subjects on Subjects.YearID = Years.ID " +
                    "where IsSpeciality = '0')");
                Connexion.FillDG(ref DGRoom, "Select * from Rooms");
                if (System.IO.File.Exists(Connexion.GetImagesFile() + "\\EcoleLogo.jpg"))
                {
                    string ShowPic = Connexion.GetImagesFile() + "\\EcoleLogo.jpg";
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(ShowPic);
                    bitmap.EndInit();
                    EcolePic.Source = bitmap;
                }
            }
            catch(Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Year(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CBLevelYears.SelectedIndex != -1)
                {
                    bool Istrue = true;
                    string year = YearTB.Text.ToLower();
                    for (int i = 0; i < DGYear.Items.Count; i++)
                    {

                        string YearDG = ((DataRowView)DGYear.Items[i]).Row["Year"].ToString().ToLower();

                        if (year == YearDG)
                        {
                            Istrue = false;
                            MessageBox.Show("This Year is already inserted");
                        }
                    }
                    if (Istrue == true)
                    {
                        DataRowView row = (DataRowView)CBLevelYears.SelectedItem;
                        Connexion.Insert("Insert into Years(Year,LevelID) values(N'" + YearTB.Text.Replace("'", "''") + "'," + row["ID"].ToString() + ")");
                        Connexion.FillDG(ref DGYear, "Select *,Years.ID as YearID from Years join Levels on Levels.ID = Years.LevelID Where LevelID = " + row["ID"].ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Please Select A level");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Level(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Istrue = true;
                for (int i = 0; i < DGLevel.Items.Count; i++)
                {
                    string YearDG = ((DataRowView)DGLevel.Items[i]).Row["Level"].ToString().ToLower();

                    if (LevelTB.Text.ToLower() == YearDG)
                    {
                        Istrue = false;
                        MessageBox.Show("This Level is already inserted");
                    }
                }
                if (Istrue == true)
                {
                    int isSpec;
                    if (Yes.IsChecked == true)
                    {
                        isSpec = 1;
                    }
                    else if (No.IsChecked == true)
                    {
                        isSpec = 0;
                    }
                    else
                    {
                        MessageBox.Show("Please check one of the Checkboxes");
                        return;
                    }
                    Connexion.Insert("Insert into Levels values(N'" + LevelTB.Text.Replace("'", "''") + "' , " + isSpec + " , 0)");
                    Connexion.FillDG(ref DGLevel, "Select * ,Case When IsSpeciality = 1 then N'" + Properties.Resources.Yes + "' When IsSpeciality = 0 then N'" + Properties.Resources.No + "' END as IsSpec from Levels");

                    Connexion.FillCB(ref CBLevelYears, "Select * from Levels");
                    Connexion.FillCB(ref CBLevelSpec, "Select * from Levels Where IsSpeciality = 1");
                    Connexion.FillCB(ref CBLevelSub, "Select * from Levels");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void ComboBox_Level_Year(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBLevelYears.SelectedItem;
                if (row != null)
                {
                    Connexion.FillDG(ref DGYear, "Select *,Years.ID as YearID from Years join Levels on Levels.ID = Years.LevelID Where LevelID = " + row["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBLevelSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBLevelSpec.SelectedItem;
                if (row != null)
                {
                    Connexion.FillCB(ref CBYearSpec, "Select * from Years Where LevelID = " + row["ID"].ToString());
                    Connexion.FillDG(ref DGSpec, "Select Years.Year ,Years.ID as YearID , Specialities.Speciality ,Specialities.ID as SpecID , Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID Where Levels.IsSpeciality = 1 and Levels.ID = " + row["ID"].ToString());
                    CBYearSpec.SelectedIndex = -1;
                    SpecTB.Text = "";
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBYearSpec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBYearSpec.SelectedItem;
                if (row != null)
                {
                    Connexion.FillDG(ref DGSpec, "Select Years.Year , Years.ID as YearID,  Specialities.Speciality ,Specialities.ID as SpecID , Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID Where Years.ID = " + row["ID"].ToString() + " and Levels.ID = " + row["LevelID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Spec(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBYearSpec.SelectedItem;
                if (row != null)
                {
                    bool Istrue = true;
                    string yearID = row["ID"].ToString().ToLower().Replace("'", "''");
                    string SpecName = SpecTB.Text.ToLower().Replace("'", "''");
                    for (int i = 0; i < DGSpec.Items.Count; i++)
                    {

                        string YearIDDG = ((DataRowView)DGSpec.Items[i]).Row["YearID"].ToString().ToLower();
                        string SpecDG = ((DataRowView)DGSpec.Items[i]).Row["Speciality"].ToString().ToLower();

                        if (yearID == YearIDDG && SpecName == SpecDG)
                        {
                            Istrue = false;
                            MessageBox.Show("This Register is already inserted");
                        }
                    }
                    if (Istrue == true)
                    {
                        Connexion.Insert("Insert into Specialities values (" + row["ID"].ToString() + ", N'" + SpecTB.Text.Replace("'", "''") + "')");
                        Connexion.FillDG(ref DGSpec, "Select Years.Year  ,Years.ID as YearID,  Specialities.Speciality ,Specialities.ID as SpecID , Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID Where Years.ID = " + row["ID"].ToString() + " and LevelID = " + row["LevelID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }

        }

        private void CBLevelSub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CBYearSub.SelectedIndex = -1;
                CBSpecSub.SelectedIndex = -1;
                CBSubjectSub.SelectedIndex = -1;
                SubTB.Text = "";
                DataRowView row = (DataRowView)CBLevelSub.SelectedItem;
                if (row != null)
                {
                    if (row["IsSpeciality"].ToString() == "0")
                    {
                        CBSubjectSub.Visibility = Visibility.Collapsed;
                        SubTB.Visibility = Visibility.Visible;
                        SPSpec.Visibility = Visibility.Collapsed;
                        SPSpec2.Visibility = Visibility.Collapsed;
                        DGTCSpec.Visibility = Visibility.Collapsed;
                        Connexion.FillDG(ref DGSubjects, "" +
                            "Select Levels.Level ," + 
                            "       Years.ID as YearID, " +
                            "       Years.Year , " +
                            "       '' as Speciality , " +
                            "       Subjects.Subject ,  " +
                            "       Subjects.ID  as subjectID " +
                            "from subjects " +
                            "       Join Years on Years.ID = Subjects.YearID  " +
                            "       Join Levels on Levels.ID = Years.LevelID " +
                            "Where LevelID = " + row["ID"].ToString());
                    }
                    else if (row["IsSpeciality"].ToString() == "1")
                    {
                        CBSubjectSub.Visibility = Visibility.Visible;
                        SubTB.Visibility = Visibility.Collapsed;
                        SPSpec.Visibility = Visibility.Visible;
                        SPSpec2.Visibility = Visibility.Visible;
                        DGTCSpec.Visibility = Visibility.Visible;
                        Connexion.FillDG(ref DGSubjects, "Select " +
                          " Years.ID as YearID," +
                           "Subject,Speciality,SpecialityID, " +
                           "Years.Year ," +
                            "levels.IsSpeciality ," +
                         "Subjects.ID as SubjectID ," +
                         "Level," +
                         "LevelID " +
                    "from Years " +
                         "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                         "Full outer join Specialities on Specialities.YearID = Years.ID " +
                         "Join SubjectSPec on " +
                          "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) " +
                          "join Levels on Years.LevelID = Levels.ID " +
                    "Where LevelID = " + row["ID"].ToString());
                    }
                    Connexion.FillCB(ref CBYearSub, "Select * from Years Where LevelID = " + row["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBYearSub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                CBSpecSub.SelectedIndex = -1;
                CBSubjectSub.SelectedIndex = -1;
                SubTB.Text = "";
                DataRowView row = (DataRowView)CBYearSub.SelectedItem;
                if (row != null)
                {
                    if (SPSpec.Visibility == Visibility.Visible)
                    {
                        Connexion.FillCB(ref CBSpecSub, "Select * from Specialities Where YearID = " + row["ID"].ToString());
                        Connexion.FillDG(ref DGSubjects, "Select " +
                            " Years.ID as YearID," +
                            "Subject,Speciality,SpecialityID, " +
                            "Years.Year ," +
                            "levels.IsSpeciality ," +
                            "Subjects.ID as SubjectID ," +
                            "Level," +
                            "LevelID " +
                    "from Years " +
                            "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                             "Full outer join Specialities on Specialities.YearID = Years.ID " +
                             "Join SubjectSPec on " +
                             "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                    "Where LevelID = " + row["LevelID"].ToString()
                      + " And Years.ID = " + row["ID"].ToString());
                    }
                    else
                    {
                        Connexion.FillDG(ref DGSubjects,
                       "Select Levels.Level ,"+
                       "       Years.ID as YearID," +
                       "       Years.Year , " +
                       "       Subjects.Subject ," +
                       "       Subjects.ID as subjectID " +
                       "from subjects " +
                       "   Join Years on Years.ID = Subjects.YearID " +
                       "   Join Levels on Levels.ID = Years.LevelID " +
                       "Where LevelID = " + row["LevelID"].ToString()
                     + " And Years.ID = " + row["ID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBSpecSub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SubTB.Text = "";
                CBSubjectSub.SelectedIndex = -1;
                NoSub.IsChecked = true;
                DataRowView row = (DataRowView)CBSpecSub.SelectedItem;
                DataRowView row2 = (DataRowView)CBYearSub.SelectedItem;
                if (row != null && row2 != null)
                {
                    Connexion.FillDG(ref DGSubjects,
                    "Select " +
                         " Years.ID as YearID," +
                          "Subject,Speciality,SpecialityID, " +
                         "Years.Year ," +
                         "levels.IsSpeciality ," +
                         "Subjects.ID as SubjectID ," +
                          "Level," +
                         "LevelID " +
                          "from Years " +
                         "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                         "Full outer join Specialities on Specialities.YearID = Years.ID " +
                         "Join SubjectSPec on " +
                         "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                         "Where LevelID = " + row2["LevelID"].ToString()
                    + " And Years.ID = " + row2["ID"].ToString() +
                      " And SubjectSpec.SpecialityID = " + row["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void CBSubjectSub_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBSpecSub.SelectedItem;
                DataRowView row2 = (DataRowView)CBYearSub.SelectedItem;
                DataRowView row3 = (DataRowView)CBYearSub.SelectedItem;
                if (row != null && row2 != null && row3 != null)
                {
                    Connexion.FillDG(ref DGSubjects,
                    "Select " +
                            " Years.ID as YearID," +
                            "Subject,Speciality,SpecialityID, " +
                           "Years.Year ," +
                           "levels.IsSpeciality ," +
                           "Subjects.ID as SubjectID ," +
                           "Level," +
                           "LevelID " +
                    "from Years " +
                             "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                             "Full outer join Specialities on Specialities.YearID = Years.ID " +
                             "Join SubjectSPec on " +
                             "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                    "Where LevelID = " + row2["LevelID"].ToString()
                  + " And Years.ID = " + row2["ID"].ToString() +
                    " And SubjectSpec.SpecialityID = " + row["ID"].ToString() +
                    " And SubjectSpec.SubjectID = " + row3["ID"].ToString());
                }
                if (YesSub.IsChecked == true && row != null && row2 != null && CBSubjectSub.SelectedItem == null)
                {
                    Connexion.FillCB(ref CBSubjectSub, "Select * from Subjects left join SubjectSPec on SubjectSpec.SpecialityID = " + row["ID"].ToString() + " Where SubjectSpec.SpecialityID is null and YearID = " + row2["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void YesSub_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBYearSub.SelectedItem;
                DataRowView row2 = (DataRowView)CBSpecSub.SelectedItem;
                if (row != null)
                {
                    SubTB.Visibility = Visibility.Collapsed;
                    CBSubjectSub.Visibility = Visibility.Visible;

                    Connexion.FillCB(ref CBSubjectSub, "select Distinct   Subjects.ID , Subject from SubjectS    join SubjectSPec on Subjects.ID = SubjectSPec.SubjectID  Join Years on Subjects.YearID = Years.ID Where Years.ID = " + row["ID"].ToString() + " Except  select   Subjects.ID , Subject from SubjectS  join SubjectSPec on Subjects.ID = SubjectSPec.SubjectID Join Years on Subjects.YearID = Years.ID Where SubjectSPec.SpecialityID = " + row2["ID"].ToString());
                }
                else
                {
                    YesSub.IsChecked = false;
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void NoSub_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBSpecSub.SelectedItem;
                if (row != null)
                {
                    CBSubjectSub.Visibility = Visibility.Collapsed;
                    SubTB.Visibility = Visibility.Visible;
                }
                else
                {
                    NoSub.IsChecked = false;
                    e.Handled = false;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Sub(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)CBSpecSub.SelectedItem;
                DataRowView row2 = (DataRowView)CBYearSub.SelectedItem;
                bool Istrue = true;
                if (row2 != null)
                {
                    string YearID = row2["ID"].ToString().ToLower();
                    string Subject = SubTB.Text.ToLower();

                    if (SubTB.Visibility == Visibility.Visible)
                    {
                        for (int i = 0; i < DGSubjects.Items.Count; i++)
                        {

                            string YearIDDG = ((DataRowView)DGSubjects.Items[i]).Row["YearID"].ToString().ToLower();
                            string SubjectDG = ((DataRowView)DGSubjects.Items[i]).Row["Subject"].ToString().ToLower();

                            if (YearID == YearIDDG && Subject == SubjectDG)
                            {
                                Istrue = false;
                                MessageBox.Show("This Register is already inserted");
                            }
                        }
                    }
                }
                if (SPSpec.Visibility == Visibility.Collapsed)
                {
                    if (Istrue == true)
                    {
                        if (row2 != null && SubTB.Text != "")
                        {
                            Connexion.Insert("Insert into Subjects values (" + row2["ID"].ToString() + ",N'" + SubTB.Text + "')");
                            Connexion.FillDG(ref DGSubjects,
                                "Select Levels.Level ,"+
                                "years.ID as YearID, Years.Year , " +
                                "Subjects.Subject ," +
                                "Subjects.ID  as SubjectID " +
                                "from subjects " +
                                "Join Years on Years.ID = Subjects.YearID " +
                                "Join Levels on Levels.ID = Years.LevelID " +
                                "Where LevelID = " + row2["LevelID"].ToString() + " " +
                                "And Years.ID = " + row2["ID"].ToString());
                        }
                    }
                }
                else
                {
                    if (YesSub.IsChecked == true)
                    {
                        DataRowView row3 = (DataRowView)CBSubjectSub.SelectedItem;
                        Connexion.Insert("Insert into SuBjectSpec Values (" + row["ID"].ToString() + " , " + row3["ID"].ToString() + ")");
                        Connexion.FillDG(ref DGSubjects,
                    "Select " +
                          " Years.ID as YearID," +
                          "Subject,Speciality,SpecialityID, " +
                         "Years.Year ," +
                         "levels.IsSpeciality ," +
                          "Subjects.ID as ID ," +
                          "Level," +
                          "LevelID " +
                     "from Years " +
                            "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                           "Full outer join Specialities on Specialities.YearID = Years.ID " +
                          "Join SubjectSPec on " +
                             "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                    "Where LevelID = " + row2["LevelID"].ToString() + " " +
                            "And Years.ID = " + row2["ID"].ToString() + " " +
                            "And SubjectSpec.SpecialityID = " + row["ID"].ToString() + " " +
                            "And SubjectSpec.SubjectID  = " + row3["ID"]);
                    }
                    else if (NoSub.IsChecked == true)
                    {
                        if (Istrue == true)
                        {
                            Connexion.Insert("Insert into Subjects values (" + row2["ID"].ToString() + ",N'" + SubTB.Text + "')");
                            string SubID = Connexion.GetID("Subjects");
                            Connexion.Insert("Insert into SuBjectSpec Values (" + row["ID"].ToString() + " , " + SubID + " )");
                            Connexion.FillDG(ref DGSubjects,
                          "Select " +
                           " Years.ID as YearID," +
                           "Subject,Speciality,SpecialityID, " +
                           "Years.Year ," +
                           "levels.IsSpeciality ," +
                            "Subjects.ID as SubjectID ," +
                            "Level," +
                            "LevelID " +
                            "from Years " +
                            "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                            "Full outer join Specialities on Specialities.YearID = Years.ID " +
                            "Join SubjectSPec on " +
                            "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                           "Where LevelID = " + row2["LevelID"].ToString() + " " +
                           "And Years.ID = " + row2["ID"].ToString() + " " +
                           "And SubjectSpec.SpecialityID = " + row["ID"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void Button_Click_Room(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Istrue = true;
                for (int i = 0; i < DGRoom.Items.Count; i++)
                {
                    string YearDG = ((DataRowView)DGRoom.Items[i]).Row["Room"].ToString().ToLower();

                    if (RoomTB.Text.ToLower() == YearDG)
                    {
                        Istrue = false;
                        MessageBox.Show("This Room is already inserted");
                    }
                }
                int seats; 
                if (!int.TryParse(RoomSeatsTB.Text,out seats))
                {
                    Istrue = false;
                }
                 if (RoomSeatsTB.Text == "")
                {
                    Istrue = true;
                }
                if (Istrue == true)
                {

                    Connexion.Insert("Insert into Rooms values (N'" + RoomTB.Text.Replace("'", "''") + "','"+ RoomSeatsTB.Text + "')");
                    Connexion.FillDG(ref DGRoom, "Select * from Rooms");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool Exist = Connexion.IFNULL("Select * from EcoleSetting");
                string ab = "";
                if (YesAbs.IsChecked == true)
                {
                    ab = "1";
                }
                else if (NoAbs.IsChecked == true)
                {
                    ab = "0";
                }
                else
                {
                    return;
                }
                string lass = "";
                if (YesClass.IsChecked == true)
                {
                    lass = "1";
                }
                else if (NoClass.IsChecked == true)
                {
                    lass = "0";
                }
                else
                {
                    return;
                }
                string calcprice = "1";
                if (YesCalcMethod.IsChecked == true)
                {
                    Connexion.Insert("update Attendance_Student Set price = (case when Status= 1 or ( Status = 0 and (Select absent from EcoleSetting) = 1 ) then  dbo.GetPriceSession(StudentID,ID) else 0 end ) , Tprice =(case when Status= 1 or ( Status = 0 and (Select absent from EcoleSetting) = 1 ) then  dbo.GetTPriceSession(StudentID,ID) else 0 end )");
                    calcprice = "1";
                }
                else if (NoCalcMethod.IsChecked == true)
                {
                    calcprice = "0";
                }
                string MonthPayment = "0"; 
                if(YesMonthes.IsChecked == true)
                {
                    MonthPayment = "1";
                }
                else if(NoMonthes.IsChecked == true)
                {
                    MonthPayment = "0";
                }
                else if (CustomMonthes.IsChecked == true)
                {
                    MonthPayment = "2";
                }
                MonthPayment = "0";
                if (Exist == true)
                {
                    Connexion.Insert("Insert into EcoleSetting(NameFR , NameAR , Adress ,Adress2  , Number , Number2, CCP , Fax , InsFees , Absent, ClassPay  ,[TextSize] , Language,PaymentMonth,CalcPrice) values (" +
                        "N'" + TBNameFR.Text.Replace("'", "''") + "'," +
                        "N'" + TBNameAR.Text.Replace("'", "''") + "'," +
                        "N'" + TBAdress1.Text.Replace("'", "''") + "'," +
                        "N'" + TBAdress2.Text.Replace("'", "''") + "'," +
                        "'" + TBNumber1.Text.Replace("'", "''") + "'," +
                        "'" + TBNumber2.Text.Replace("'", "''") + "'," +
                        "'" + TBCCP.Text.Replace("'", "''") + "'," +
                        "N'" + TBFax.Text.Replace("'", "''") + "'," +
                        "'" + TBFees.Text.Replace("'", "''") + "'," +
                        "'" + ab + "'," +
                        "'" + lass + "'," +
                        "'15' , "+ Connexion.Language() + ","+ MonthPayment + ","+ calcprice + " )");
                }
                else
                {
                    Connexion.Insert("Update EcoleSetting Set " +
                        "NameFr =N'" + TBNameFR.Text.Replace("'", "''") + "'," +
                        "NameAR =N'" + TBNameAR.Text.Replace("'", "''") + "'," +
                        "Adress = N'" + TBAdress1.Text.Replace("'", "''") + "'," +
                        "Adress2 = N'" + TBAdress2.Text.Replace("'", "''") + "'," +
                        "Number = N'" + TBNumber1.Text.Replace("'", "''") + "'," +
                        "Number2 = N'" + TBNumber2.Text.Replace("'", "''") + "'," +
                        "CCP = N'" + TBCCP.Text.Replace("'", "''") + "'," +
                        "Fax = N'" + TBFax.Text.Replace("'", "''") + "'," +
                        "insfees = N'" + TBFees.Text.Replace("'", "''") + "'," +
                        "Absent = '" + ab + "'," +
                        "ClassPay = '" + lass + "',CalcPrice = '"+ calcprice + "' , " +
                        "[TextSize] = '15',PaymentMonth=" +
                        MonthPayment + " ");
                }
                if (open.FileName != "")
                {
                    if (System.IO.File.Exists(@"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg"))
                    {
                        System.IO.File.Delete(@"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");
                        System.IO.File.Copy(open.FileName, @"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");

                    }
                    else
                    {
                        System.IO.File.Copy(open.FileName, @"C:\ProgramData\EcoleSetting\EcolePhotos\EcoleLogo.jpg");
                    }
                }
                MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                if (Connexion.Language() == 1)
                {
                    TBlockEcole.Text = TBNameAR.Text.ToString();
                }
                else
                {
                    TBlockEcole.Text = TBNameFR.Text.ToString();
                }
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
                open.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
                if (open.ShowDialog() == true)
                {

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(open.FileName);
                    bitmap.EndInit();
                    EcolePic.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnDelYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string YearID = ((DataRowView)DGYear.SelectedItem).Row["YearID"].ToString();
                if (!Connexion.IFNULL("Select CYear from Class Where CYear =" + YearID))
                {
                    MessageBox.Show("Can't Delete this Year,There are Classes Created with this year ");
                    return;
                }
                Connexion.Insert("Delete from Years Where ID = " + YearID);
                if (CBLevelYears.SelectedIndex == -1)
                {
                    Connexion.FillDG(ref DGYear, "Select *,Years.ID as YearID from Years join Levels on Levels.ID = Years.LevelID");
                }
                else
                {
                    DataRowView row = (DataRowView)CBLevelYears.SelectedItem;
                    Connexion.FillDG(ref DGYear, "Select *,Years.ID as YearID from Years join Levels on Levels.ID = Years.LevelID Where LevelID = " + row["ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want To Delete This Level?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string LevelID = ((DataRowView)DGLevel.SelectedItem).Row["ID"].ToString();
                    if (!Connexion.IFNULL("Select CLevel from Class Where CLevel =" + LevelID))
                    {
                        MessageBox.Show("Can't Delete this Level,There are Classes Created with this Level ");
                        return;
                    }

                    Connexion.Insert("Delete from Levels Where ID = " + LevelID);
                    Connexion.FillDG(ref DGLevel, "Select *,Case When IsSpeciality = 1 then N'" + Properties.Resources.Yes + "' When IsSpeciality = 0 then N'" + Properties.Resources.No + "' END as IsSpec from Levels");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelSpec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want To Delete This Speciality?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string SpecID = ((DataRowView)DGSpec.SelectedItem).Row["SpecID"].ToString();
                    if (!Connexion.IFNULL("Select SpecID from Class_Speciality Where SpecID =" + SpecID))
                    {
                        MessageBox.Show("Can't Delete this Speciality,There are Classes Created with this Speciality ");
                        return;
                    }
                    Connexion.Insert("Delete from Specialities Where ID = " + SpecID);
                    if (CBLevelSpec.SelectedIndex == -1)
                    {
                        Connexion.FillDG(ref DGSpec, "Select Years.Year , Years.ID as YearID,  Specialities.Speciality ,Specialities.ID as SpecID ,Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID");
                    }
                    else if (CBYearSpec.SelectedIndex == -1)
                    {
                        DataRowView row = (DataRowView)CBLevelSpec.SelectedItem;

                        Connexion.FillCB(ref CBYearSpec, "Select * from Years Where LevelID = " + row["ID"].ToString());
                        Connexion.FillDG(ref DGSpec, "Select Years.Year , Years.ID as YearID , Specialities.Speciality ,Specialities.ID as SpecID , Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID Where Levels.IsSpeciality = 1 and Levels.ID = " + row["ID"].ToString());

                    }
                    else
                    {
                        DataRowView row = (DataRowView)CBYearSpec.SelectedItem;

                        Connexion.FillDG(ref DGSpec, "Select Years.Year , Years.ID as YearID,  Specialities.Speciality ,Specialities.ID as SpecID , Levels.Level  from Specialities Join Years  on Years.ID = Specialities.YearID Join Levels On Years.LevelID = Levels.ID Where Years.ID = " + row["ID"].ToString() + " and Levels.ID = " + row["LevelID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelSubject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want To Delete This Subjectt?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string SubjectID = ((DataRowView)DGSubjects.SelectedItem).Row["SubjectID"].ToString();
                    if (!Connexion.IFNULL("Select CSubject from Class Where CSubject =" + SubjectID))
                    {
                        MessageBox.Show("Can't Delete this Subject,There are Classes Created with this Subject ");
                        return;
                    }

                    int YearID = Connexion.GetInt(SubjectID, "Subjects", "YearID");
                    int IsSpec = Connexion.GetInt(YearID.ToString(), "Levels", "IsSpeciality");
                    if (IsSpec == 0)
                    {
                        Connexion.Insert("Delete from Subjects Where ID = " + SubjectID);
                    }
                    else
                    {
                        Connexion.Insert("Delete from SubjectSpec Where SubjectID = " + SubjectID);
                    }
                    if (CBLevelSub.SelectedIndex == -1)
                    {
                        Connexion.FillDG(ref DGSubjects,
                     "Select " +
                          " Years.ID as YearID," +
                          "Subject,Speciality,SpecialityID, " +
                          "Years.Year ," +
                          "levels.IsSpeciality ," +
                          "Subjects.ID as SubjectID ," +
                          "Level," +
                          "LevelID " +
                          "from Years " +
                          "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                          "Full outer join Specialities on Specialities.YearID = Years.ID " +
                          "Join SubjectSPec on " +
                          "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                     "Union (" +
                         "Select Years.ID as YearID ," +
                         "Subject , " +
                         "null as Speciality, " +
                         "null as SpecialityID , " +
                         "Years.Year, " +
                         "Levels.IsSpeciality  , " +
                         "Subjects.ID as SubjectID , " +
                         "Level,LevelID  " +
                         "from Years " +
                         "join Levels on Levels.ID = years.LevelID " +
                         "Join  Subjects on Subjects.YearID = Years.ID " +
                    "where IsSpeciality = '0')");
                    }
                    else if (CBYearSub.SelectedIndex == -1)
                    {
                        DataRowView row = (DataRowView)CBLevelSub.SelectedItem;
                        if (row != null)
                        {
                            if (row["IsSpeciality"].ToString() == "0")
                            {

                                Connexion.FillDG(ref DGSubjects, "" +
                                    "Select Levels.Level ," +
                                    "       Years.ID as YearID, " +
                                    "       Years.Year , " +
                                    "       '' as Speciality , " +
                                    "       Subjects.Subject ,  " +
                                    "       Subjects.ID as SubjectID  " +
                                    "from subjects " +
                                    "       Join Years on Years.ID = Subjects.YearID  " +
                                    "       Join Levels on Levels.ID = Years.LevelID " +
                                    "Where LevelID = " + row["ID"].ToString());
                            }
                            else if (row["IsSpeciality"].ToString() == "1")
                            {

                                Connexion.FillDG(ref DGSubjects,
                            "Select " +
                                  " Years.ID as YearID," +
                                  "Subject,Speciality,SpecialityID, " +
                                  "Years.Year ," +
                                  "levels.IsSpeciality ," +
                                  "Subjects.ID as SubjectID ," +
                                   "Level," +
                                   "LevelID " +
                             "from Years " +
                             "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                             "Full outer join Specialities on Specialities.YearID = Years.ID " +
                             "Join SubjectSPec on " +
                             "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                            "Where LevelID = " + row["ID"].ToString());
                            }
                        }
                    }
                    else if (CBSpecSub.SelectedIndex == -1)
                    {
                        DataRowView row = (DataRowView)CBYearSub.SelectedItem;

                        if (SPSpec.Visibility == Visibility.Visible)
                        {
                            Connexion.FillDG(ref DGSubjects,
                           "Select " +
                                " Years.ID as YearID," +
                             "Subject,Speciality,SpecialityID, " +
                             "Years.Year ," +
                              "levels.IsSpeciality ," +
                            "Subjects.ID as SubjectID ," +
                             "Level," +
                             "LevelID " +
                             "from Years " +
                            "Full outer Join Subjects on Subjects.YearID  = Years.ID " +
                            "Full outer join Specialities on Specialities.YearID = Years.ID " +
                             "Join SubjectSPec on " +
                             "(SubjectSPec.SpecialityID = Specialities.ID and SubjectSPec.SubjectID = Subjects.ID) join Levels on Years.LevelID = Levels.ID " +
                            "Where LevelID = " + row["LevelID"].ToString()
                          + " And Years.ID = " + row["ID"].ToString());
                        }
                        else
                        {
                            Connexion.FillDG(ref DGSubjects,
                           "Select Levels.Level ," +
                           "       Years.ID as YearID," +
                           "       Years.Year , " +
                           "       Subjects.Subject ," +
                           "       Subjects.ID as SubjectiD " +
                           "from subjects " +
                           "   Join Years on Years.ID = Subjects.YearID " +
                           "   Join Levels on Levels.ID = Years.LevelID " +
                           "Where LevelID = " + row["LevelID"].ToString()
                         + " And Years.ID = " + row["ID"].ToString());
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

        private void BtnDelRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want To Delete This room?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string RoomID = ((DataRowView)DGRoom.SelectedItem).Row["ID"].ToString();
                    Connexion.Insert("Delete from Rooms Where ID = " + RoomID);
                    Connexion.FillDG(ref DGRoom, "Select * from Rooms");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
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

        private void Button_Click_Import(object sender, RoutedEventArgs e)
        {
            var AddS = new ImportLYSS("0");
            AddS.Show();

        }

        private void BtnArchive_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Backup("Enter the BackUp name",0);
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.ResponseText ;
                Connexion.Insert(
                    @"BACKUP DATABASE[" + Connexion.DB + @"] 
                    TO DISK = N'C:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MOUATHSQL\MSSQL\Backup\" + Connexion.DB +".Bak' " +
                    "WITH NOFORMAT," +
                    "NOINIT, " +
                    "NAME = N'" + Connexion.DB + "', " +
                    "SKIP, " +
                    "NOREWIND, " +
                    "NOUNLOAD,  " +
                    "STATS = 10 ");
                Connexion.Insert(
                    @"RESTORE DATABASE ["+ name + @"]  FROM   
                    DISK = N'C:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MOUATHSQL\MSSQL\Backup\"+ Connexion.DB + @".Bak' 
                    WITH  FILE = 1,  
                    MOVE N'" + Connexion.DB + @"' 
                    TO N'c:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MOUATHSQL\MSSQL\DATA\"+ name + @".mdf',  
                    MOVE N'"+Connexion.DB + @"_log' 
                    TO N'c:\Program Files (x86)\Microsoft SQL Server\MSSQL10_50.MOUATHSQL\MSSQL\DATA\"+ name +"_1.LDF',  " +
                    "NOUNLOAD,  STATS = 10");
                Connexion.Insert("ALTER DATABASE [" + name +"] MODIFY FILE ( NAME = "+Connexion.DB +" , NEWNAME = '"+ name +"');");
                Connexion.Insert("ALTER DATABASE [" + name + "] MODIFY FILE ( NAME = '" + Connexion.DB + "_log' , NEWNAME = '" + name + "_log');");
                MessageBox.Show("Database have been backed up Succesfully");
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CBInscLevel.SelectedItem; 
            if(row == null)
            {
                return;
            }
            TBFees.Text = Connexion.GetString("Select InscFees from Levels Where ID = " + row["ID"].ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)CBInscLevel.SelectedItem;
            if (row == null)
            {
                return;
            }
            Connexion.Insert("Update Levels Set InscFees = '" + TBFees.Text + "' Where ID = " + row["ID"].ToString());
            MessageBox.Show(this.Resources["InsertedSucc"].ToString());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(CustomMonthes.IsChecked == true)
            {
                var AddS = new ImportLYSS("1");
                AddS.Show();
            }
            else
            {
                MessageBox.Show("Please Check Custom Monthes ");
            }
        }

        private void DGRoom_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                TextBox textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    var binding = (e.Column as DataGridBoundColumn)?.Binding as Binding;
                    DataRowView rowView = e.Row.Item as DataRowView;
                    if (rowView != null)
                    {
                        if (binding != null)
                        {
                            int ID = (int)rowView["ID"];
                            if (binding.Path.Path == "Room") // Replace "Field1" with the actual property name for TextBox1
                            {
                                string RoomNew = textBox.Text;
                                Connexion.Insert("Update Rooms Set Room = N'" + RoomNew + "'  Where ID =" + ID);
                            }
                            else if (binding.Path.Path == "Seats") // Replace "Field2" with the actual property name for TextBox2
                            {
                                if (int.TryParse(textBox.Text, out int result))
                                {
                                    Connexion.Insert("Update Rooms Set Seats = N'" + result + "'  Where ID =" + ID);
                                }
                                else if (textBox.Text == "")
                                {
                                    Connexion.Insert("Update Rooms Set Seats = NULL  Where ID =" + ID);
                                }
                            }
                        }
                    }
                    
                  


                }
            }
        }
    }
}
