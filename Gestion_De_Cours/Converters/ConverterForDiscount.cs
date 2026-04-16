using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Gestion_De_Cours.Classes;

namespace Gestion_De_Cours.Converters
{
    class ConverterForDiscount : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Use the studentID parameter here
            // Generate a list of integers based on the studentID and intValue
            List<object> result = new List<object>();
            int count;
            int startvalue = 0;
            int DiscountID = int.Parse(values[0].ToString());
            int StudentID = Connexion.GetInt("Select StudentID from Discounts WHere ID = " + DiscountID);
            int ClassID = Connexion.GetInt("Select ClassID from Discounts Where ID=" + DiscountID);
            int GroupID = Connexion.GetGroupID(StudentID.ToString(), ClassID.ToString());
            int StartSes = 99;
            if (values[1].ToString() == "0")
            {
                StartSes = Connexion.GetInt("Select Session from Class_Student Where GroupID = " + GroupID + " and StudentID = " + StudentID);
            }
            else if (values[1].ToString() == "1")
            {
                StartSes = Connexion.GetInt("Select StudentSes from Discounts  where ID = " + DiscountID);
            }
            int EndSes = Connexion.GetInt("Select Case When EndSession is null then -1 else EndSession end as f  from Class_Student Where GroupID = " + GroupID + " and StudentID = " + StudentID);
            if(EndSes == -1)
            {
                EndSes = Connexion.GetInt("Select Sessions from groups Where groupID = " + GroupID);
            }

            for (int i = StartSes; i <= EndSes; i++)
            {
                result.Add(i);
            }
            return result;
           
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
