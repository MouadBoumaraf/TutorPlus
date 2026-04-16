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
    /// Interaction logic for DiscountStandard.xaml
    /// </summary>
    public partial class DiscountStandard : Window
    {
        DataTable DTYearChecked = new DataTable();
        public DiscountStandard()
        {
            InitializeComponent();
            int lang = Connexion.Language();
            DTYearChecked.Columns.Add("ID");
            SetLang();
            Connexion.FillCB(ref CLevel, "Select * from Levels");
            string query = @"SELECT 
            DiscType1.ID,
            'Type 1' as Type,
            S1.Subject + ',' + S2.Subject + ',' + S3.Subject + ',' +
            S4.Subject + ',' + S5.Subject + ',' + S6.Subject as Subjects,
            SD1.Subject + ' ' + CAST(Price1 AS NVARCHAR(50)) + ' ' + CAST(TPrice1 AS NVARCHAR(50)) + ' , ' +
            SD2.Subject + ' ' + CAST(Price2 AS NVARCHAR(50)) + ' ' + CAST(TPrice2 AS NVARCHAR(50)) + ' , ' +
            SD3.Subject + ' ' + CAST(Price3 AS NVARCHAR(50)) + ' ' + CAST(TPrice3 AS NVARCHAR(50)) + ' , ' +
            SD4.Subject + ' ' + CAST(Price4 AS NVARCHAR(50)) + ' ' + CAST(TPrice4 AS NVARCHAR(50)) + ' , ' +
            SD5.Subject + ' ' + CAST(Price5 AS NVARCHAR(50)) + ' ' + CAST(TPrice5 AS NVARCHAR(50)) + ' , ' +
            SD6.Subject + ' ' + CAST(Price6 AS NVARCHAR(50)) + ' ' + CAST(TPrice6 AS NVARCHAR(50)) + ' , ' as SubjectDiscount
            FROM DiscType1
            JOIN Subjects S1 on DiscType1.Subject1 = S1.ID
            JOIN Subjects S2 on DiscType1.Subject2 = S2.ID
            JOIN Subjects S3 on DiscType1.Subject3 = S3.ID
            JOIN Subjects S4 on DiscType1.Subject4 = S4.ID
            JOIN Subjects S5 on DiscType1.Subject5 = S5.ID
            JOIN Subjects S6 on DiscType1.Subject6 = S6.ID
            JOIN Subjects SD1 on DiscType1.SubjectDisc1 = SD1.ID
            JOIN Subjects SD2 on DiscType1.SubjectDisc2 = SD2.ID
            JOIN Subjects SD3 on DiscType1.SubjectDisc3 = SD3.ID
            JOIN Subjects SD4 on DiscType1.SubjectDisc4 = SD4.ID
            JOIN Subjects SD5 on DiscType1.SubjectDisc5 = SD5.ID
            JOIN Subjects SD6 on DiscType1.SubjectDisc6 = SD6.ID";
            Connexion.FillDG(ref DGDiscounts, query);
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

        private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(typeComboBox.SelectedIndex == 0)
            {
                SPType1.Visibility = Visibility.Visible;
                SPType2.Visibility = Visibility.Collapsed; 
                Row1.Height = new GridLength(190);
                Row2.Height = new GridLength(1, GridUnitType.Star);
                CYear.Visibility = Visibility.Visible;
                CBYear.Visibility = Visibility.Collapsed;
                this.Height = 450;
                this.Width = 1050;
            }
            else
            {
                SPType2.Visibility = Visibility.Visible;
                SPType1.Visibility = Visibility.Collapsed;
                CYear.Visibility = Visibility.Collapsed;
                CBYear.Visibility = Visibility.Visible;
                Row1.Height = new GridLength(50);
                Row2.Height = new GridLength(1, GridUnitType.Star);
                this.Height = 310;
                this.Width = 1100;
                Connexion.FillDG(ref DGDiscounts, "Select '' as Type, Levels.Level, Years.Year, AmmountSubjects, price as MPrice, TPrice as MTPrice from DiscType2 join levels on Levels.ID = DiscType2.LevelID   join Years on Years.ID = DiscType2.YearID");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            DataRowView rowLevel = (DataRowView)CLevel.SelectedItem;
            //DataRowView rowSpeciality = (DataRowView)CSpeciality.SelectedItem;
           // string SpecID = "";
         
            if (typeComboBox.SelectedIndex == 0)
            {
                DataRowView rowYear = (DataRowView)CYear.SelectedItem;
                DataRowView rowSubject1 = (DataRowView)subjectComboBox1.SelectedItem;
                DataRowView rowSubject2 = (DataRowView)subjectComboBox2.SelectedItem;
                DataRowView rowSubject3 = (DataRowView)subjectComboBox3.SelectedItem;
                DataRowView rowSubject4 = (DataRowView)subjectComboBox4.SelectedItem;
                DataRowView rowSubject5 = (DataRowView)subjectComboBox5.SelectedItem;
                DataRowView rowSubject6 = (DataRowView)subjectComboBox6.SelectedItem;
                DataRowView rowDiscSubject1 = (DataRowView)subjectDiscComboBox1.SelectedItem;
                DataRowView rowDiscSubject2 = (DataRowView)subjectDiscComboBox2.SelectedItem;
                DataRowView rowDiscSubject3 = (DataRowView)subjectDiscComboBox3.SelectedItem;
                DataRowView rowDiscSubject4 = (DataRowView)subjectDiscComboBox4.SelectedItem;
                DataRowView rowDiscSubject5 = (DataRowView)subjectDiscComboBox5.SelectedItem;
                DataRowView rowDiscSubject6 = (DataRowView)subjectDiscComboBox6.SelectedItem;
                string idSubject1 = rowSubject1?.Row["ID"].ToString() ?? "NULL";
                string idSubject2 = rowSubject2?.Row["ID"].ToString() ?? "NULL";
                string idSubject3 = rowSubject3?.Row["ID"].ToString() ?? "NULL";
                string idSubject4 = rowSubject4?.Row["ID"].ToString() ?? "NULL";
                string idSubject5 = rowSubject5?.Row["ID"].ToString() ?? "NULL";
                string idSubject6 = rowSubject6?.Row["ID"].ToString() ?? "NULL";


                string idDiscSubject1 = rowDiscSubject1?.Row["ID"].ToString() ?? "NULL";
                string idDiscSubject2 = rowDiscSubject2?.Row["ID"].ToString() ?? "NULL";
                string idDiscSubject3 = rowDiscSubject3?.Row["ID"].ToString() ?? "NULL";
                string idDiscSubject4 = rowDiscSubject4?.Row["ID"].ToString() ?? "NULL";
                string idDiscSubject5 = rowDiscSubject5?.Row["ID"].ToString() ?? "NULL";
                string idDiscSubject6 = rowDiscSubject6?.Row["ID"].ToString() ?? "NULL";
                if (rowSubject1 == null)
                {
                    MessageBox.Show("Please Select atleast one Subject");
                    return;
                }
                int DiscType1ID = Connexion.GetInt("Insert into DiscType1  OUTPUT Inserted.ID Values " +
                    "( " + rowLevel["ID"].ToString() + " , " +
                    "" + rowYear["ID"].ToString() + " , " +
                    "" + idSubject1 + "," +
                    "" + idSubject2 + ", " +
                    "" + idSubject3 + " , " +
                    "" + idSubject4 + " , " +
                    "" + idSubject5 + " , " +
                    "" + idSubject6 + " , " +
                    "" + idDiscSubject1 + ", " +
                    "'" + TBPrice1.Text + "' , " +
                    "'" + TBTPrice1.Text + "' , " +
                    "" + idDiscSubject2 + ", " +
                    "'" + TBPrice2.Text + "' , " +
                    "'" + TBTPrice2.Text + "' , " +
                    "" + idDiscSubject3 + " , " +
                    "'" + TBPrice3.Text + "' , " +
                    "'" + TBTPrice3.Text + "' , " +
                    "" + idDiscSubject4 + " , " +
                    "'" + TBPrice4.Text + "' , " +
                    "'" + TBTPrice4.Text + "' , " +
                    "" + idDiscSubject5 + " , " +
                    "'" + TBPrice5.Text + "' , " +
                    "'" + TBTPrice5.Text + "' , " +
                    "" + idDiscSubject6 + " , " +
                    "'" + TBPrice6.Text + "' , " +
                    "'" + TBTPrice6.Text + "' ,'-1' ) " );
                string query = "SELECT cs.StudentID  FROM class_Student cs  INNER JOIN class c ON cs.ClassID = c.ID join Students on cs.StudentID = Students.ID ";
               
                query += "Where ";
              
                query += " c.CSubject IN (";

                int count = 0;
                if(idSubject1 != "NULL")
                {
                    query += idSubject1 ;
                    count++;
                }
                if(idSubject2 != "NULL")
                {
                    query += "," + idSubject2 ;
                    count++;
                }
                if (idSubject3 != "NULL")
                {
                    query += "," + idSubject3 ;
                    count++;
                }
                if (idSubject4 != "NULL")
                {
                    query += "," + idSubject4;
                    count++;
                }
                if (idSubject5 != "NULL")
                {
                    query += "," + idSubject5 ;
                    count++;
                }
                if (idSubject6 != "NULL")
                {
                    query += "," + idSubject6 ;
                    count++;
                }
                query += ") GROUP BY cs.StudentID HAVING COUNT(DISTINCT CASE WHEN c.CSubject = "+ idSubject1 +" THEN 1" ;
                int count2 = 1;
                if (idSubject2 != "NULL")
                {
                    count2++;
                    query += " WHEN c.CSubject = "+ idSubject2 +" THEN " + count2 ;
                }
                if (idSubject3 != "NULL")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + idSubject3 + " THEN " + count2;
                }
                if (idSubject4 != "NULL")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + idSubject4 + " THEN " + count2;
                }
                if (idSubject5 != "NULL")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + idSubject5 + " THEN " + count2;
                }
                if (idSubject6 != "NULL")
                {
                    count2++;
                    query += " WHEN c.CSubject = " + idSubject6 + " THEN " + count2;
                }
                query += " else 0 end )  = " + count; 

                DataTable dtStudents = new DataTable();
                Connexion.FillDT(ref dtStudents , query);
                foreach (DataRow row in dtStudents.Rows)
                {
                    string SID = row["StudentID"].ToString();
                    OptionPanels.ThreeButtonPage page = new OptionPanels.ThreeButtonPage(this.Resources["HowApplyDisc?"].ToString(), this.Resources["DiscStartSes"].ToString(), this.Resources["DiscLastPayment"].ToString(), this.Resources["DiscFromNow?"].ToString());
                    page.ShowDialog();
                    int result = page.Result;
                    if (result == 1 || result == 2 || result == 3)
                    {
                        result = 3;
                    }
                    Commun.CheckDiscountAddClass(SID, this.Resources, 1 , result);
                }
                MessageBox.Show(this.Resources["InsertedSucc"].ToString());
            }
            else
            {
                DataRowView rowYear = (DataRowView)CBYear.SelectedItem;
                OptionPanels.ThreeButtonPage page = new OptionPanels.ThreeButtonPage(this.Resources["HowApplyDisc?"].ToString(), this.Resources["DiscStartSes"].ToString(), this.Resources["DiscLastPayment"].ToString(), this.Resources["DiscFromNow?"].ToString());
                page.ShowDialog();
                int result = page.Result;
                if(result == -1)
                {
                    MessageBox.Show("No option was Selected");
                    return;
                }
                for (int i = DTYearChecked.Rows.Count - 1; i >= 0; i--) // Iterate in reverse to avoid issues when removing rows
                {
                    DataRow dtRow = DTYearChecked.Rows[i];

                    string YearID = dtRow["ID"].ToString();
                    if (CBAmmountOFSubjects.SelectedIndex != -1 && rowLevel != null && TBMinusPrice.Text != "" && TBMinusTPrice.Text != "")
                    {
                        Connexion.Insert("Insert into DiscType2 Values " +
                          "( " + rowLevel["ID"].ToString() + " , " +
                          "" + YearID + " , " +
                          "" + CBAmmountOFSubjects.Text + "," +
                          "" + TBMinusPrice.Text + "," +
                          "" + TBMinusTPrice.Text + " , '-1')");

                        string query = "SELECT cs.StudentID FROM class_Student cs INNER JOIN class c ON cs.ClassID = c.ID where c.CYear = " + YearID + " GROUP BY cs.StudentID HAVING COUNT(DISTINCT c.CSubject) >= " + CBAmmountOFSubjects.Text;
                      

                        DataTable dtStudents = new DataTable();
                        Connexion.FillDT(ref dtStudents, query);
                        foreach (DataRow row in dtStudents.Rows)
                        {
                            DataTable dtSubjects = new DataTable();
                            string SID = row["StudentID"].ToString();
                            if (result == 1 || result == 2 || result == 3)
                            {
                                result = 3;
                            }
                            Commun.CheckDiscountAddClass(SID, this.Resources, 1, result);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Fill out all information");
                        return;
                    }
                }
                    MessageBox.Show(this.Resources["InsertedSucc"].ToString());
                    Connexion.FillDG(ref DGDiscounts, "Select '' as Type, Levels.Level, Years.Year, AmmountSubjects, price as MPrice, TPrice as MTPrice from DiscType2 join levels on Levels.ID = DiscType2.LevelID   join Years on Years.ID = DiscType2.YearID");
                

            }
        }

        private void CLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CLevel.SelectedItem;
            if (row != null)
            {

                if (typeComboBox.SelectedIndex == 0)
                {
                    Connexion.FillCB(ref CYear, "Select * from Years Where LevelID = " + row["ID"].ToString());
                }
                else
                {
                    DTYearChecked.Rows.Clear();
                    DataTable dtyear = new DataTable();
                    Connexion.FillDT(ref dtyear, "Select * , 1 as IsChecked from Years Where LevelID = " + row["ID"].ToString());
                    CBYear.ItemsSource = dtyear.DefaultView;
                    for (int i = dtyear.Rows.Count - 1; i >= 0; i--) // Iterate in reverse to avoid issues when removing rows
                    {
                        string dtRow = dtyear.Rows[i]["ID"].ToString() ;
                        DTYearChecked.Rows.Add(dtRow);

                    }
                }

                    /*    if(row["IsSpeciality"].ToString() == "0")
                        {
                            SpecLB.Visibility = Visibility.Collapsed;
                            CSpeciality.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            SpecLB.Visibility = Visibility.Visible;
                            CSpeciality.Visibility = Visibility.Visible;
                        }*/
                }
        }

        private void CYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowLevel = (DataRowView)CLevel.SelectedItem;
            if (row != null)
            {
                string yearid = row["ID"].ToString();
                string IsSpec = rowLevel["IsSpeciality"].ToString();
              
                if (typeComboBox.SelectedIndex == 0)
                {
                    Connexion.FillDTItem("Subjects Where YearID = " + yearid, ref subjectComboBox1);
                    Connexion.FillDTItem("Subjects Where YearID = " + yearid, ref subjectDiscComboBox1);

                }
                else
                {
              //     int max =  Connexion.GetInt( "Select Count(*) from Subjects Where YearID = " + yearid);
                    
                }
            }
        }

        private void subjectComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectComboBox1.SelectedItem;
            if (rowSubject != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString(), ref subjectComboBox2);
            }
        }

        private void subjectComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectComboBox2.SelectedItem;
            if (rowSubject2 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString(), ref subjectComboBox3);
            }
        }

        private void subjectComboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectComboBox3.SelectedItem;
            if (rowSubject3 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString(), ref subjectComboBox4);
            }
        }

        private void subjectComboBox4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectComboBox3.SelectedItem;
            DataRowView rowSubject4 = (DataRowView)subjectComboBox4.SelectedItem;
            if (rowSubject4 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString() + " and ID !=" + rowSubject4["ID"].ToString(), ref subjectComboBox5);
            }
        }

        private void subjectComboBox5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectComboBox3.SelectedItem;
            DataRowView rowSubject4 = (DataRowView)subjectComboBox4.SelectedItem;
            DataRowView rowSubject5 = (DataRowView)subjectComboBox5.SelectedItem;
            if (rowSubject5 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString() + " and ID !=" + rowSubject4["ID"].ToString() + " and ID != " + rowSubject5["ID"].ToString(), ref subjectComboBox6);
            }
        }
  

        private void subjectComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void subjectDiscComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectDiscComboBox1.SelectedItem;
            if (rowSubject != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString(), ref subjectDiscComboBox2);
            }
        }

        private void subjectDiscComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectDiscComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectDiscComboBox2.SelectedItem;
            if (rowSubject2 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString(), ref subjectDiscComboBox3);
            }
        }

        private void subjectDiscComboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectDiscComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectDiscComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectDiscComboBox3.SelectedItem;
            if (rowSubject3 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString() , ref subjectDiscComboBox4);
            }
        }

        private void subjectDiscComboBox4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectDiscComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectDiscComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectDiscComboBox3.SelectedItem;
            DataRowView rowSubject4 = (DataRowView)subjectDiscComboBox4.SelectedItem;
            if (rowSubject4 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString() + " and ID != " + rowSubject4["ID"].ToString(), ref subjectDiscComboBox5);
            }
        }

        private void subjectDiscComboBox5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CYear.SelectedItem;
            DataRowView rowSubject = (DataRowView)subjectDiscComboBox1.SelectedItem;
            DataRowView rowSubject2 = (DataRowView)subjectDiscComboBox2.SelectedItem;
            DataRowView rowSubject3 = (DataRowView)subjectDiscComboBox3.SelectedItem;
            DataRowView rowSubject4 = (DataRowView)subjectDiscComboBox4.SelectedItem;
            DataRowView rowSubject5 = (DataRowView)subjectDiscComboBox5.SelectedItem;
            if (rowSubject5 != null)
            {
                string yearid = row["ID"].ToString();
                Connexion.FillDTItem("Subjects Where YearID = " + yearid + " and ID != " + rowSubject["ID"].ToString() + " and ID != " + rowSubject2["ID"].ToString() + " and ID != " + rowSubject3["ID"].ToString() + " and ID != " + rowSubject4["ID"].ToString() + " and ID != " + rowSubject5["ID"].ToString()
                    , ref subjectDiscComboBox6);
            } 
        }

        private void subjectDiscComboBox6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void checkBoxYear_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView dataRow = (DataRowView)checkBox.DataContext;
            DataRow row = dataRow.Row;
            DTYearChecked.Rows.Add(row["ID"].ToString());
        }

        private void checkBoxYear_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView dataRow = (DataRowView)checkBox.DataContext;
            DataRow row = dataRow.Row;

            foreach (DataRow dtRow in DTYearChecked.Rows)
            {
                if (dtRow["ID"].ToString() == row["ID"].ToString())
                {
                    // Remove the matching row
                    DTYearChecked.Rows.Remove(dtRow);
                    break; // Exit the loop once the row is found and removed
                }
            }
        }

        private void CBAmmountOFSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
