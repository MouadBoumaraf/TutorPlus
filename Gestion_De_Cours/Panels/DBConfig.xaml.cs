using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
    /// Interaction logic for DBConfig.xaml
    /// </summary>
    public partial class DBConfig : Window
    {
        public DBConfig()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string EcoleData = @"C:\ProgramData\EcoleSetting\SqlSettings.txt";
                SqlConnection con = new SqlConnection("Data Source=" + ServerName.Text + ";Initial Catalog=master ;Persist Security Info=True;User ID=" + UserName.Text + ";Password=" + PasswordName.Text + "");
                con.Open();
                Connexion.RestoreMainDataBase(DBName.Text, con);
                con.Close();
                StreamWriter sw = File.CreateText(EcoleData);
                {
                  
                    sw.WriteLine(DBName.Text);
                    sw.WriteLine(ServerName.Text);
                    sw.WriteLine(UserName.Text);
                    sw.WriteLine(PasswordName.Text);
                    sw.Close();
                }
            }
            catch(Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
    }
}
