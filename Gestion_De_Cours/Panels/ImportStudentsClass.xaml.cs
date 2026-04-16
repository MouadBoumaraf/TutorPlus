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
    /// Interaction logic for ImportStudentsClass.xaml
    /// </summary>
    public partial class ImportStudentsClass : Window
    {
        string groupID;
        string classID;
        DataTable dt = new DataTable();
        public ImportStudentsClass(string GID)
        {
            int lang = Connexion.Language();
            SetLang();
            if (lang == 1)
            {
                this.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
            }
            InitializeComponent();
            DGStudents.DataContext = dt.DefaultView ;
            groupID = GID;
            classID = Connexion.GetClassID(GID).ToString();
            int CYear = Connexion.GetInt("Select Class.CYear from class where ID =" + classID);
            Connexion.FillCB(ref CBClass, "Select Class.ID as CID , Groups.GroupID as GID,  case when Class.MultipleGroups = 'Single' then Class.CName  else Class.CName + '' + Groups.GroupName end as ClassName from class join groups on class.ID = Groups.ClassID Where Class.ID != "+ classID + " and Class.CYear = '"+ CYear +"' order by Class.ID ");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView)CBClass.SelectedValue;
           
            Connexion.FillDT(ref dt, "Select 1 as Checked ,Students.ID as ID,Students.FirstName as FName , Students.LastName as LName ,  CASE WHEN Students.id NOT IN(SELECT StudentID FROM class_Student where ClassID = '"+ classID + "') THEN '1' ELSE '0' END AS Status from Class_Student join Students on Students.ID = Class_Student.StudentID where Class_Student.GroupID = '" + row["GID"].ToString() + "' ");
        }

        private void CheckBoxLvl_Checked(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGStudents.SelectedItem;
            if (row != null)
            {

                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (dt.Rows[k]["ID"].ToString() == row["ID"].ToString())
                    {
                        dt.Rows[k]["Checked"] = 1;
                    }
                }
            }
        }

        private void CheckBoxLvl_Unchecked(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGStudents.SelectedItem;
            if (row != null)
            {

                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    if (dt.Rows[k]["ID"].ToString() == row["ID"].ToString())
                    {
                        dt.Rows[k]["Checked"] = 0;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Commun.CheckSeatsClass(groupID, this.Resources["WarningSeatsMax"].ToString()))
            {
                return;
            }
            for (int k = 0; k < dt.Rows.Count; k++)
            {
                if(dt.Rows[k]["Status"].ToString() != "0" && dt.Rows[k]["Checked"].ToString() != "0")
                {
                    int ses = Connexion.GetInt(groupID, "Groups", "Sessions", "GroupID");
                    if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + dt.Rows[k]["ID"].ToString() + " and ClassID = " + classID))
                    {
                        Connexion.Insert("Insert into Class_Student Values ('" + dt.Rows[k]["ID"].ToString() + "','" + classID + "' ,  '" + groupID + "'," + ses + ",NULL,0,0 )");
                        OptionPanels.ThreeButtonPage page = new OptionPanels.ThreeButtonPage(this.Resources["HowApplyDisc?"].ToString(), this.Resources["DiscStartSes"].ToString(), this.Resources["DiscLastPayment"].ToString(), this.Resources["DiscFromNow?"].ToString());
                        page.ShowDialog();
                        int result = page.Result;
                        if (result == 1 || result == 2 || result == 3)
                        {
                            result = 3;
                        }
                        Commun.CheckDiscountAddClass(dt.Rows[k]["ID"].ToString(), this.Resources, 0, result);
                    }
                }
            }
            MessageBox.Show(this.Resources["InsertedSucc"].ToString());
            this.Close();
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
            this.Resources.MergedDictionaries.Add(ResourceDic);
        }
    }
}
