using System;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using Gestion_De_Cours.Classes;

namespace Gestion_De_Cours.Converters
{
    public class RangeToListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {



            if (values.Length == 4 && values[0] is int intValue && values[1] is int studentID && values[2] is int GroupID)
            {
                // Use the studentID parameter here
                // Generate a list of integers based on the studentID and intValue
                List<object> result = new List<object>();
                int count;
                int startvalue = 0;
                if (values[3].ToString() == "0")
                {
                    if (Connexion.IFNULLVar("Select EndSession from Class_Student Where StudentID = " + studentID + " and GroupID = " + GroupID))
                    {
                        count = Connexion.GetInt("Select Sessions From Groups Where GroupID =  " + GroupID);
                    }
                    else
                    {
                        count = Connexion.GetInt("Select EndSession from Class_Student Where StudentID = " + studentID + " and GroupID = " + GroupID);

                    }
                }
                else
                {
                    startvalue = Connexion.GetInt("Select Session from Class_Student Where StudentID = " + studentID + " and GroupID = " + GroupID);
                    count = Connexion.GetInt("Select Sessions From Groups Where GroupID =  " + GroupID);
                }
                for (int i = startvalue; i <= count; i++)
                {
                    result.Add(i);
                }
                if (values[3].ToString() == "1")
                {
                   result.Add("Back Studying");
                }
                return result;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
