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

namespace Gestion_De_Cours.PopUps
{
    /// <summary>
    /// Interaction logic for StudentClassesAdd.xaml
    /// </summary>
    public partial class StudentClassesAdd : Window
    {
        public StudentClassesAdd(List<string> listGID , string SID)
        {
            InitializeComponent();
        /*    Connexion.FillDT("Select  Case When Class.MultipleGroups = 'Single' Then" +
                        " Class.CName " +
                        "When Class.MultipleGroups = 'Multiple' Then Class.CName + ' ' + Groups.GroupName " +
                        "End as GName, Groups.GroupID " + ")*/

        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            DataRowView rowview = (DataRowView)checkBox.DataContext;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
