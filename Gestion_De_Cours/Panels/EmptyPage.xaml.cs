using System;
using System.Collections.Generic;
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

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for EmptyPage.xaml
    /// </summary>
    public partial class EmptyPage : Window
    {
        public EmptyPage(string UserControlName, string ID1, string ID2)
        {
            InitializeComponent();
            if (UserControlName == "StudentPayment")
            {
                StudentPayment paymentU = new StudentPayment("0", "", "");
                Grid.Children.Add(paymentU);
            }
            else if (UserControlName == "StudentPayment2")
            {
                StudentPayment paymentU = new StudentPayment("2", ID1, ID2);
                Grid.Children.Add(paymentU);
            }
            else if (UserControlName == "StudentPayment3")
            {
                StudentPayment paymentU = new StudentPayment("3", "", ID2);
                Grid.Children.Add(paymentU);
            }
            else if (UserControlName == "StudentPaymentMonthly")
            {
                StudentPayment paymentU = new StudentPayment("1", ID1, "");
                Grid.Children.Add(paymentU);
                this.Width = 700;
                this.Height = 600;
            }
            else if(UserControlName == "StudentList")
            {
                ShowTableU stude = new ShowTableU("Student");
            }
        }
    }
}
