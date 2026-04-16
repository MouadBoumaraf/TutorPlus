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
using Gestion_De_Cours.Classes; 

namespace Gestion_De_Cours.Panels
{
    /// <summary>
    /// Interaction logic for ImportLYSS.xaml
    /// </summary>
  
    public partial class ImportLYSS : Window
    {
        int f;
        DataTable dtSpec;
        DataTable dtLevels;
        DataTable dtYears = new DataTable();
        DataTable dtSubject;
        string type = ""; 

        public ImportLYSS(string  Type)
        {
            f = 0;
            type = Type;
            InitializeComponent();
            if (type == "1")
            {
                DGSpec.Visibility = Visibility.Collapsed;
                DGLevel.Visibility = Visibility.Collapsed;
                DGSubject.Visibility = Visibility.Collapsed;
                Col1.Width = new GridLength(0); // Set width to 0
                Col3.Width = new GridLength(0);
                Col4.Width = new GridLength(0);
                this.Width = 350;
                Connexion.FillDT(ref dtYears, "Select *,Years.ID as YID,case when Monthly is null then 'false' when monthly = 0 then 'false' when monthly = 1 then 'true' end   as Checked from Years join Levels on Years.LevelID = Levels.ID");
                DGYears.ItemsSource = dtYears.DefaultView; 
            }
            else
            {
                dtLevels = new DataTable();
                dtLevels.Columns.Add("ID", typeof(int));
                dtLevels.Columns.Add("Level", typeof(string));
                dtLevels.Columns.Add("IsSpec", typeof(string));
                dtLevels.Columns.Add("Checked", typeof(bool));
                dtLevels.Rows.Add(new Object[] { 1, "Premaire", "No", true });
                dtLevels.Rows.Add(new Object[] { 2, "Cem", "No", true });
                dtLevels.Rows.Add(new Object[] { 3, "Lycée", "Yes", true });
                DGLevel.DataContext = dtLevels.DefaultView;


                //Levels

                //Years
                dtYears = new DataTable();
                dtYears.Columns.Add("ID", typeof(int));
                dtYears.Columns.Add("LevelID", typeof(int));
                dtYears.Columns.Add("Level", typeof(string));
                dtYears.Columns.Add("Year", typeof(string));
                dtYears.Columns.Add("Checked", typeof(bool));
                dtYears.Rows.Add(new Object[] { 1, 1, "Premaire", "1AP", true });
                dtYears.Rows.Add(new Object[] { 2, 1, "Premaire", "2AP", true });
                dtYears.Rows.Add(new Object[] { 3, 1, "Premaire", "3AP", true });
                dtYears.Rows.Add(new Object[] { 4, 1, "Premaire", "4AP", true });
                dtYears.Rows.Add(new Object[] { 5, 1, "Premaire", "5AP", true });
                dtYears.Rows.Add(new Object[] { 6, 2, "Cem", "1AM", true });
                dtYears.Rows.Add(new Object[] { 7, 2, "Cem", "2AM", true });
                dtYears.Rows.Add(new Object[] { 8, 2, "Cem", "3AM", true });
                dtYears.Rows.Add(new Object[] { 9, 2, "Cem", "4AM", true });
                dtYears.Rows.Add(new Object[] { 10, 3, "Lycée", "1AS", true });
                dtYears.Rows.Add(new Object[] { 11, 3, "Lycée", "2AS", true });
                dtYears.Rows.Add(new Object[] { 12, 3, "Lycée", "3AS", true });
                DGYears.DataContext = dtYears.DefaultView;
                //Specialities

                dtSpec = new DataTable();
                dtSpec.Columns.Add("ID", typeof(int));
                dtSpec.Columns.Add("YearID", typeof(int));
                dtSpec.Columns.Add("Year", typeof(string));
                dtSpec.Columns.Add("Speciality", typeof(string));
                dtSpec.Columns.Add("Checked", typeof(bool));
                dtSpec.Rows.Add(new Object[] { 1, 10, "1AS", "Scientifique", true });
                dtSpec.Rows.Add(new Object[] { 2, 10, "1AS", "Lettres", true });
                dtSpec.Rows.Add(new Object[] { 3, 11, "2AS", "Scientifique", true });
                dtSpec.Rows.Add(new Object[] { 4, 11, "2AS", "Langue", true });
                dtSpec.Rows.Add(new Object[] { 5, 11, "2AS", "gestion", true });
                dtSpec.Rows.Add(new Object[] { 6, 11, "2AS", "philosophie", true });
                dtSpec.Rows.Add(new Object[] { 7, 11, "2AS", "Math-Technique", true });
                dtSpec.Rows.Add(new Object[] { 8, 11, "2AS", "mathéleme", true });
                dtSpec.Rows.Add(new Object[] { 9, 12, "3AS", "Scientifique", true });
                dtSpec.Rows.Add(new Object[] { 10, 12, "3AS", "Langue", true });
                dtSpec.Rows.Add(new Object[] { 11, 12, "3AS", "gestion", true });
                dtSpec.Rows.Add(new Object[] { 12, 12, "3AS", "philosophie", true });
                dtSpec.Rows.Add(new Object[] { 13, 12, "3AS", "Math-Technique", true });
                dtSpec.Rows.Add(new Object[] { 14, 12, "3AS", "mathéleme", true });
                DGSpec.DataContext = dtSpec.DefaultView;
                //Subjects
                dtSubject = new DataTable();
                dtSubject.Columns.Add("ID", typeof(int));
                dtSubject.Columns.Add("YearID", typeof(int));
                dtSubject.Columns.Add("SpecID", typeof(int));
                dtSubject.Columns.Add("Year", typeof(string));
                dtSubject.Columns.Add("Speciality", typeof(string));
                dtSubject.Columns.Add("Subject", typeof(string));
                dtSubject.Columns.Add("Checked", typeof(bool));
                dtSubject.Columns.Add("Exist", typeof(string));
                //Premaire Subjects
                //1AP
                dtSubject.Rows.Add(new Object[] { 1, 1, 0, "1AP", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 2, 1, 0, "1AP", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 3, 1, 0, "1AP", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 3, 1, 0, "1AP", "", "Francais", true });
                //2AP
                dtSubject.Rows.Add(new Object[] { 4, 2, 0, "2AP", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 5, 2, 0, "2AP", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 6, 2, 0, "2AP", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 7, 2, 0, "2AP", "", "Francais", true });
                //3AP
                dtSubject.Rows.Add(new Object[] { 8, 3, 0, "3AP", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 9, 3, 0, "3AP", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 10, 3, 0, "3AP", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 11, 3, 0, "3AP", "", "Francais", true });
                //4AP
                dtSubject.Rows.Add(new Object[] { 12, 4, 0, "4AP", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 13, 4, 0, "4AP", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 14, 4, 0, "4AP", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 15, 4, 0, "4AP", "", "Francais", true });
                //5AP
                dtSubject.Rows.Add(new Object[] { 16, 5, 0, "5AP", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 17, 5, 0, "5AP", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 18, 5, 0, "5AP", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 19, 5, 0, "5AP", "", "Francais", true });
                //Cem Subjects
                //1AM
                dtSubject.Rows.Add(new Object[] { 20, 6, 0, "1AM", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 21, 6, 0, "1AM", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 22, 6, 0, "1AM", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 23, 6, 0, "1AM", "", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 24, 6, 0, "1AM", "", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 25, 6, 0, "1AM", "", "Science", true });
                //2AM
                dtSubject.Rows.Add(new Object[] { 26, 7, 0, "2AM", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 27, 7, 0, "2AM", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 28, 7, 0, "2AM", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 29, 7, 0, "2AM", "", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 30, 7, 0, "2AM", "", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 31, 7, 0, "2AM", "", "Science", true });
                //3AM
                dtSubject.Rows.Add(new Object[] { 31, 8, 0, "3AM", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 32, 8, 0, "3AM", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 33, 8, 0, "3AM", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 34, 8, 0, "3AM", "", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 35, 8, 0, "3AM", "", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 36, 8, 0, "3AM", "", "Science", true });
                //4AM
                dtSubject.Rows.Add(new Object[] { 37, 9, 0, "4AM", "", "Math", true });
                dtSubject.Rows.Add(new Object[] { 38, 9, 0, "4AM", "", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 39, 9, 0, "4AM", "", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 40, 9, 0, "4AM", "", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 41, 9, 0, "4AM", "", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 42, 9, 0, "4AM", "", "Science", true });
                //Lycee Subjects
                //1AS Scientific 
                dtSubject.Rows.Add(new Object[] { 43, 10, 1, "1AS", "Scientifique", "Math", true });
                dtSubject.Rows.Add(new Object[] { 44, 10, 1, "1AS", "Scientifique", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 45, 10, 1, "1AS", "Scientifique", "Science", true });
                dtSubject.Rows.Add(new Object[] { 46, 10, 1, "1AS", "Scientifique", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 47, 10, 1, "1AS", "Scientifique", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 48, 10, 1, "1AS", "Scientifique", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 49, 10, 1, "1AS", "Scientifique", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 50, 10, 1, "1AS", "Scientifique", "Hist & Geo", true });

                dtSubject.Rows.Add(new Object[] { 49, 10, 2, "1AS", "Lettres", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 50, 10, 2, "1AS", "Lettres", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 43, 10, 2, "1AS", "Lettres", "Math", true });
                dtSubject.Rows.Add(new Object[] { 44, 10, 2, "1AS", "Lettres", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 46, 10, 2, "1AS", "Lettres", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 47, 10, 2, "1AS", "Lettres", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 48, 10, 2, "1AS", "Lettres", "Francais", true });
                //2AS
                dtSubject.Rows.Add(new Object[] { 56, 11, 3, "2AS", "Scientifique", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 3, "2AS", "Scientifique", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 56, 11, 4, "2AS", "Langue", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 4, "2AS", "Langue", "Hist & Geo", true });

                dtSubject.Rows.Add(new Object[] { 56, 11, 8, "2AS", "mathéleme", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 8, "2AS", "mathéleme", "Hist & Geo", true });

                dtSubject.Rows.Add(new Object[] { 56, 11, 5, "2AS", "philosophie", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 5, "2AS", "philosophie", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 56, 11, 6, "2AS", "Gestion", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 6, "2AS", "Gestion", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 56, 11, 7, "2AS", "Math-Technique", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 57, 11, 7, "2AS", "Math-Technique", "Hist & Geo", true });


                dtSubject.Rows.Add(new Object[] { 58, 11, 3, "2AS", "Scientifique", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 59, 11, 3, "2AS", "Scientifique", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 3, "2AS", "Scientifique", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 58, 11, 5, "2AS", "Gestion", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 59, 11, 5, "2AS", "Gestion", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 5, "2AS", "Gestion", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 58, 11, 7, "2AS", "Math-Technique", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 59, 11, 7, "2AS", "Math-Technique", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 7, "2AS", "Math-Technique", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 58, 11, 8, "2AS", "mathéleme", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 59, 11, 8, "2AS", "mathéleme", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 8, "2AS", "mathéleme", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 59, 11, 6, "2AS", "Philo", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 6, "2AS", "Philo", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 58, 11, 4, "2AS", "Langue", "Arab", true });

                dtSubject.Rows.Add(new Object[] { 58, 11, 6, "2AS", "Philo", "Arab", true });

                dtSubject.Rows.Add(new Object[] { 64, 11, 8, "2AS", "mathéleme", "Math", true });
                dtSubject.Rows.Add(new Object[] { 65, 11, 8, "2AS", "mathéleme", "Physique", true });

                dtSubject.Rows.Add(new Object[] { 64, 11, 7, "2AS", "Math-Technique", "Math", true });
                dtSubject.Rows.Add(new Object[] { 65, 11, 7, "2AS", "Math-Technique", "Physique", true });

                dtSubject.Rows.Add(new Object[] { 66, 11, 6, "2AS", "Philo", "Philo", true });

                dtSubject.Rows.Add(new Object[] { 59, 11, 4, "2AS", "Langue", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 60, 11, 4, "2AS", "Langue", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 69, 11, 4, "2AS", "Langue", "Espagnol", true });
                dtSubject.Rows.Add(new Object[] { 70, 11, 4, "2AS", "Langue", "Allemand", true });

                dtSubject.Rows.Add(new Object[] { 64, 11, 3, "2AS", "Scientifique", "Math", true });
                dtSubject.Rows.Add(new Object[] { 65, 11, 3, "2AS", "Scientifique", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 73, 11, 3, "2AS", "Scientifique", "Science", true });

                dtSubject.Rows.Add(new Object[] { 74, 11, 7, "2AS", "Math-Technique", "Chimie", true });
                dtSubject.Rows.Add(new Object[] { 75, 11, 7, "2AS", "Math-Technique", "électronique", true });
                dtSubject.Rows.Add(new Object[] { 76, 11, 7, "2AS", "Math-Technique", "Génie civil", true });
                dtSubject.Rows.Add(new Object[] { 77, 11, 7, "2AS", "Math-Technique", "Mécanique", true });

                dtSubject.Rows.Add(new Object[] { 73, 11, 8, "2AS", "mathéleme", "Science", true });

                dtSubject.Rows.Add(new Object[] { 79, 11, 5, "2AS", "Gestion", "Economy", true });
                dtSubject.Rows.Add(new Object[] { 80, 11, 5, "2AS", "Gestion", "Comptabilité", true });
                dtSubject.Rows.Add(new Object[] { 81, 11, 5, "2AS", "Gestion", "Droit", true });

                //3AS
                dtSubject.Rows.Add(new Object[] { 82, 12, 9, "3AS", "Scientifique", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 9, "3AS", "Scientifique", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 82, 12, 10, "3AS", "Langue", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 10, "3AS", "Langue", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 88, 12, 10, "3AS", "Langue", "Math", true });

                dtSubject.Rows.Add(new Object[] { 82, 12, 14, "3AS", "mathéleme", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 14, "3AS", "mathéleme", "Hist & Geo", true });

                dtSubject.Rows.Add(new Object[] { 82, 12, 12, "3AS", "philosophie", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 12, "3AS", "philosophie", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 88, 12, 12, "3AS", "philosophie", "Math", true });

                dtSubject.Rows.Add(new Object[] { 82, 12, 11, "3AS", "Gestion", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 11, "3AS", "Gestion", "Hist & Geo", true });
                dtSubject.Rows.Add(new Object[] { 88, 12, 11, "3AS", "Gestion", "Math", true });


                dtSubject.Rows.Add(new Object[] { 82, 12, 13, "3AS", "Math-Technique", "Islamic", true });
                dtSubject.Rows.Add(new Object[] { 83, 12, 13, "3AS", "Math-Technique", "Hist & Geo", true });


                dtSubject.Rows.Add(new Object[] { 84, 12, 9, "3AS", "Scientifique", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 85, 12, 9, "3AS", "Scientifique", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 9, "3AS", "Scientifique", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 84, 12, 11, "3AS", "Gestion", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 85, 12, 11, "3AS", "Gestion", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 11, "3AS", "Gestion", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 84, 12, 13, "3AS", "Math-Technique", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 85, 12, 13, "3AS", "Math-Technique", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 13, "3AS", "Math-Technique", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 84, 12, 14, "3AS", "mathéleme", "Arab", true });
                dtSubject.Rows.Add(new Object[] { 85, 12, 14, "3AS", "mathéleme", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 14, "3AS", "mathéleme", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 85, 12, 12, "3AS", "Philo", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 12, "3AS", "Philo", "Francais", true });

                dtSubject.Rows.Add(new Object[] { 84, 12, 10, "3AS", "Langue", "Arab", true });

                dtSubject.Rows.Add(new Object[] { 84, 12, 10, "3AS", "Philo", "Arab", true });

                dtSubject.Rows.Add(new Object[] { 88, 12, 14, "3AS", "mathéleme", "Math", true });
                dtSubject.Rows.Add(new Object[] { 89, 12, 14, "3AS", "mathéleme", "Physique", true });

                dtSubject.Rows.Add(new Object[] { 88, 12, 13, "3AS", "Math-Technique", "Math", true });
                dtSubject.Rows.Add(new Object[] { 89, 12, 13, "3AS", "Math-Technique", "Physique", true });

                dtSubject.Rows.Add(new Object[] { 106, 12, 12, "3AS", "Philo", "Philo", true });

                dtSubject.Rows.Add(new Object[] { 85, 12, 10, "3AS", "Langue", "Anglais", true });
                dtSubject.Rows.Add(new Object[] { 86, 12, 10, "3AS", "Langue", "Francais", true });
                dtSubject.Rows.Add(new Object[] { 93, 12, 10, "3AS", "Langue", "Espagnol", true });
                dtSubject.Rows.Add(new Object[] { 94, 12, 10, "3AS", "Langue", "Allemand", true });

                dtSubject.Rows.Add(new Object[] { 88, 12, 9, "3AS", "Scientifique", "Math ", true });
                dtSubject.Rows.Add(new Object[] { 89, 12, 9, "3AS", "Scientifique", "Physique", true });
                dtSubject.Rows.Add(new Object[] { 97, 12, 9, "3AS", "Scientifique", "Science", true });

                dtSubject.Rows.Add(new Object[] { 98, 12, 13, "3AS", "Math-Technique", "Chimie", true });
                dtSubject.Rows.Add(new Object[] { 99, 12, 13, "3AS", "Math-Technique", "électronique", true });
                dtSubject.Rows.Add(new Object[] { 100, 12, 13, "3AS", "Math-Technique", "Génie civil", true });
                dtSubject.Rows.Add(new Object[] { 101, 12, 13, "3AS", "Math-Technique", "Mécanique", true });

                dtSubject.Rows.Add(new Object[] { 97, 12, 14, "3AS", "mathéleme", "Science", true });

                dtSubject.Rows.Add(new Object[] { 103, 12, 11, "3AS", "Gestion", "Economy", true });
                dtSubject.Rows.Add(new Object[] { 104, 12, 11, "3AS", "Gestion", "Comptabilité", true });
                dtSubject.Rows.Add(new Object[] { 105, 12, 11, "3AS", "Gestion", "Droit", true });

                dtSubject.Rows.Add(new Object[] { 106, 12, 11, "3AS", "Gestion", "Philo", true });
                dtSubject.Rows.Add(new Object[] { 106, 12, 14, "3AS", "mathéleme", "Philo", true });
                dtSubject.Rows.Add(new Object[] { 106, 12, 13, "3AS", "Math-Technique", "Philo", true });
                dtSubject.Rows.Add(new Object[] { 106, 12, 10, "3AS", "Scientifique", "Philo", true });
                dtSubject.Rows.Add(new Object[] { 106, 12, 9, "3AS", "Langue", "Philo", true });
                DGSubject.DataContext = dtSubject.DefaultView;
            }
            f = 1;
        }
        private void LevelChecking(string LevelID, string IFSpec, bool Check)
        {
            for(int k = 0; k < dtLevels.Rows.Count; k++)
            {
                if( dtLevels.Rows[k]["ID"].ToString() == LevelID)
                {
                    dtLevels.Rows[k]["Checked"] = Check;
                }
            }
            for (int i = 0; i < dtYears.Rows.Count; i++)
            {
                if (LevelID == dtYears.Rows[i]["LevelID"].ToString())
                {
                    dtYears.Rows[i]["Checked"] = Check;
                    YearChecking(dtYears.Rows[i]["ID"].ToString(), IFSpec, Check);
                }
            }
            DGYears.ItemsSource = dtYears.DefaultView;

        }

        private void YearChecking(string YearID, string IFSpec, bool Check)
        {
            for (int k = 0; k < dtYears.Rows.Count; k++)
            {
                if (dtYears.Rows[k]["ID"].ToString() == YearID)
                {
                    dtYears.Rows[k]["Checked"] = Check;
                }
            }
            if (IFSpec == "Yes")
            {
                for (int i = 0; i < dtSpec.Rows.Count; i++)
                {
                    if (dtSpec.Rows[i]["YearID"].ToString() == YearID)
                    {
                        dtSpec.Rows[i]["Checked"] = Check;
                    }
                }
                DGSpec.ItemsSource = dtSpec.DefaultView;
            }
            for (int i = 0; i < dtSubject.Rows.Count; i++)
            {
                if (dtSubject.Rows[i]["YearID"].ToString() == YearID)
                {
                    dtSubject.Rows[i]["Checked"] = Check;
                }
            }
            DGSubject.ItemsSource = dtSubject.DefaultView;

        }

        private void SpecialityChecking(string SpecID , bool Check)
        {
            for (int k = 0; k < dtSpec.Rows.Count; k++)
            {
                if (dtSpec.Rows[k]["ID"].ToString() == SpecID)
                {
                    dtSpec.Rows[k]["Checked"] = Check;
                }
            }
            for (int i = 0; i < dtSubject.Rows.Count; i++)
            {
                if (dtSubject.Rows[i]["SpecID"].ToString() == SpecID)
                {
                    dtSubject.Rows[i]["Checked"] = Check;
                }
            }
            DGSubject.ItemsSource = dtSubject.DefaultView;
        }

        private void CheckBoxLvl_Checked(object sender, RoutedEventArgs e)
        {
            if (f == 0)
            {
                return;
            }
            DataRowView row = (DataRowView)DGLevel.SelectedItem;
            if(row == null)
            {
                return; 
            }
         
            string levelID = row["ID"].ToString();
            LevelChecking(levelID, row["IsSpec"].ToString(), true);

        }


        private void CheckBoxLvl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (f == 0)
            {
                return;
            }
            DataRowView row = (DataRowView)DGLevel.SelectedItem;
            string levelID = row["ID"].ToString();
            LevelChecking(levelID , row["IsSpec"].ToString() , false);
        }

        private void CheckboxYears_Checked(object sender, RoutedEventArgs e)
        {
            if (f == 0)
            {
                return;
            }
            DataRowView row = (DataRowView)DGYears.SelectedItem;
            if (row == null)
            {
                return;
            }
            if (type == "0")
            {
                string IFSpec = "";
                for (int i = 0; i < dtLevels.Rows.Count; i++)
                {
                    if (dtLevels.Rows[i]["ID"].ToString() == row["LevelID"].ToString())
                    {
                        IFSpec = dtLevels.Rows[i]["IsSpec"].ToString();
                        i = dtLevels.Rows.Count;
                    }
                }
                YearChecking(row["ID"].ToString(), IFSpec, true);
            }
            else
            {
                row["Checked"] = "true";
            }
        }

        private void CheckboxYears_Unchecked(object sender, RoutedEventArgs e)
        {
            if (f == 0)
            {
                return;
            }
            DataRowView row = (DataRowView)DGYears.SelectedItem;
            if (type == "0")
            {
                if (row == null)
                {
                    return;
                }
                string IFSpec = "";
                for (int i = 0; i < dtLevels.Rows.Count; i++)
                {
                    if (dtLevels.Rows[i]["ID"].ToString() == row["LevelID"].ToString())
                    {
                        IFSpec = dtLevels.Rows[i]["IsSpec"].ToString();
                        i = dtLevels.Rows.Count;
                    }
                }
                YearChecking(row["ID"].ToString(), IFSpec, false);
            }
            else
            {
                row["Checked"] = "false";
            }
          
        }

        private void CheckBoxSubject_Checked(object sender, RoutedEventArgs e)
        {
           
            DataRowView row = (DataRowView)DGSubject.SelectedItem;
            if (type == "0")
            {
                if (row == null)
                {
                    return;
                }
                string subjectID = row["ID"].ToString();
                string SpecID = row["SpecID"].ToString();
                for (int k = 0; k < dtSubject.Rows.Count; k++)
                {
                    if (dtSubject.Rows[k]["ID"].ToString() == subjectID && dtSubject.Rows[k]["SpecID"].ToString() == SpecID)
                    {
                        dtSubject.Rows[k]["Checked"] = true;
                    }
                }
            }
        }

        private void CheckBoxSubject_Unchecked(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGSubject.SelectedItem;
            if( row == null)
            {
                return; 
            }
            string subjectID = row["ID"].ToString();
            string SpecID = row["SpecID"].ToString();
            for (int k = 0; k < dtSubject.Rows.Count; k++)
            {
                if (dtSubject.Rows[k]["ID"].ToString() == subjectID && dtSubject.Rows[k]["SpecID"].ToString() == SpecID)
                {
                    dtSubject.Rows[k]["Checked"] = false;
                }
            }
        }

        private void CheckBoxSpec_Checked(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGSpec.SelectedItem; 
            if(row == null)
            {
                return; 
            }
            for (int i = 0; i < dtSpec.Rows.Count; i++)
            {
                if (dtSpec.Rows[i]["ID"].ToString() == row["ID"].ToString())
                {
                    dtSpec.Rows[i]["Checked"] = true; 
                    i = dtLevels.Rows.Count;
                }
            }
            SpecialityChecking(row["ID"].ToString(), true);
            DGSpec.ItemsSource = dtSpec.DefaultView;

        }

        private void CheckBoxSpec_Unchecked(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DGSpec.SelectedItem;
            if (row == null)
            {
                return;
            }
            for (int i = 0; i < dtSpec.Rows.Count; i++)
            {
                if (dtSpec.Rows[i]["ID"].ToString() == row["ID"].ToString())
                {
                    dtSpec.Rows[i]["Checked"] = false;
                    i = dtLevels.Rows.Count;
                }
            }
            SpecialityChecking(row["ID"].ToString(), false);
            DGSpec.ItemsSource = dtSpec.DefaultView; 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (type == "0")
                {
                    for (int i = 0; i < dtLevels.Rows.Count; i++)
                    {
                        if (dtLevels.Rows[i]["Checked"].ToString() == "True")
                        {

                            string isSpec;
                            if (dtLevels.Rows[i]["IsSpec"].ToString() == "Yes")
                            {
                                isSpec = "1";
                            }
                            else
                            {
                                isSpec = "0";
                            }

                            Connexion.Insert("Insert into Levels Values ('" + dtLevels.Rows[i]["Level"].ToString() + "','" + isSpec + "', 0)");

                            string LevelID = Connexion.GetID("Levels");
                            string OldLevelID = dtLevels.Rows[i]["ID"].ToString();
                            int yearmethod;
                            if(Connexion.IFNULL("Select PaymentMonth from ecoleSetting"))
                            {
                                yearmethod = 0;
                            }
                            else
                            {
                                 yearmethod =  Connexion.GetInt("Select paymentMonth from ecolesetting ");
                                if( yearmethod == 2)
                                {
                                    yearmethod = 0;
                                }
                            }
                            dtLevels.Rows[i]["ID"] = LevelID;
                            for (int k = 0; k < dtYears.Rows.Count; k++)
                            {
                                if (dtYears.Rows[k]["LevelID"].ToString() == OldLevelID && dtYears.Rows[k]["Checked"].ToString() == "True")
                                {
                                    if (type == "0")
                                    {
                                        Connexion.Insert("Insert into Years Values ('" + dtYears.Rows[k]["Year"].ToString() + "','" + LevelID + "',"+ yearmethod+")");
                                    }
                                    else if (type == "1")
                                    {
                                    }
                                    string YearID = Connexion.GetID("Years");
                                    string OldYearID = dtYears.Rows[k]["ID"].ToString();
                                    if (isSpec == "0")
                                    {
                                        for (int f = 0; f < dtSubject.Rows.Count; f++)
                                        {
                                            if (dtSubject.Rows[f]["YearID"].ToString() == OldYearID && dtSubject.Rows[f]["Checked"].ToString() == "True")
                                            {
                                                Connexion.Insert("" +
                                                    "Insert into Subjects " +
                                                    "Values ('" + YearID + "'" +
                                                    ",'" + dtSubject.Rows[f]["Subject"].ToString() + "')");
                                                dtSubject.Rows[f]["Checked"] = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int Z = 0; Z < dtSpec.Rows.Count; Z++)
                                        {
                                            if (dtSpec.Rows[Z]["YearID"].ToString() == OldYearID && dtSpec.Rows[Z]["Checked"].ToString() == "True")
                                            {
                                                string SpecID = Connexion.GetInt("Insert into Specialities output inserted.ID values('" + YearID + "','" + dtSpec.Rows[Z]["Speciality"].ToString() + "')").ToString();
                                                string OldSpecID = dtSpec.Rows[Z]["ID"].ToString();
                                                for (int X = 0; X < dtSubject.Rows.Count; X++)
                                                {
                                                    if (dtSubject.Rows[X]["SpecID"].ToString() == OldSpecID && dtSubject.Rows[X]["Checked"].ToString() == "True")
                                                    {
                                                        if (dtSubject.Rows[X]["Exist"].ToString() == "")
                                                        {
                                                            Connexion.Insert("Insert into Subjects                                 values('" + YearID + "','" + dtSubject.Rows[X]["Subject"].ToString() + "')");
                                                            string SubjectID = Connexion.GetID("Subjects");
                                                            string OldSubjectID = dtSubject.Rows[X]["ID"].ToString();
                                                            Connexion.Insert("Insert into SubjectSpec                             values('" + SpecID + "','" + SubjectID + "')");
                                                            for (int O = 0; O < dtSubject.Rows.Count; O++)
                                                            {
                                                                if (dtSubject.Rows[O]["ID"].ToString() == OldSubjectID && dtSubject.Rows[O]["Checked"].ToString() == "True")
                                                                {
                                                                    dtSubject.Rows[O]["Exist"] = SubjectID;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Connexion.Insert("Insert into SubjectSpec                             values('" + SpecID + "','" + dtSubject.Rows[X]["Exist"].ToString() + "')");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Connexion.Insert("update EcoleSetting Set PaymentMonth = 2");
                    foreach (DataRowView rowView in DGYears.ItemsSource)
                    {
                        DataRow row = rowView.Row; // Get the DataRow from DataRowView

                        // Accessing specific columns
                        string IsChecked = row["Checked"].ToString(); // Replace "Name" with your actual column name
                       if(IsChecked == "true")
                       {
                            Connexion.Insert("Update Years Set Monthly = 1 where ID = " + row["YID"].ToString());
                       }
                       else  
                       {
                            Connexion.Insert("Update Years Set Monthly = 0 where ID = " + row["YID"].ToString());
                        }
                    }

                }
                MessageBox.Show("Inserted Succesfully");
                this.Close();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

    }
}
