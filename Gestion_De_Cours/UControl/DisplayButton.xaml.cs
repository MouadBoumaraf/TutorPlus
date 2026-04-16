using Gestion_De_Cours.Panels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for DisplayButton.xaml
    /// </summary>
    public partial class DisplayButton : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DisplayButton));
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(DisplayButton), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(DisplayButton));

        public string Tag
        {
            get { return (string)GetValue(TagProperty); }
            set { SetValue(TagProperty, value); }
        }
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(string), typeof(DisplayButton));

        public DisplayButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }

        }

        private void Click1(object sender, RoutedEventArgs e)
        {
            if (Tag == "AddStudent")
            {
                var AddS = new StudentAdd("Add", "");
                AddS.ShowDialog();

            }
            else if (Tag == "ScanBarCode")
            {
                var scanningCodeBar = new ScanningCodeBar();
                scanningCodeBar.Show();
            }
            else if (Tag == "AddPayment")
            {
                StudentPayment paymentU = new StudentPayment("0", "" , "");
                EmptyPage Page = new EmptyPage("StudentPayment" , "" , "");
                Page.Show();
            }

        }

        private void Click2(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click 2");
        }
    }
}
