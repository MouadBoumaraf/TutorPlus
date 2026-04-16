using Gestion_De_Cours.Classes;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Gestion_De_Cours.PopUps
{
    /// <summary>
    /// Interaction logic for ListDelete.xaml
    /// </summary>
    public partial class ListDelete : Window
    {
    
        public DataTable datat { get; private set; }
        public ListDelete( DataTable dt , string type)
        {
            InitializeComponent();
            datat = dt;
            if(type == "AbsentStudents")
            {
                DG.Columns.Add(new DataGridTextColumn
                {
                    Header = "Name",
                    Binding = new System.Windows.Data.Binding("Name")
                });
                DG.Columns.Add(new DataGridTextColumn
                {
                    Header = "Last Session",
                    Binding = new System.Windows.Data.Binding("LastSession")
                });
            }
            // Adding Delete Column (with Button and Image)
            DataGridTemplateColumn justificationColumn = new DataGridTemplateColumn
            {
                Width = DataGridLength.Auto,
                Header = new TextBlock
                {
                    Text = "Delete",
                    TextAlignment = TextAlignment.Center
                }
            };


            FrameworkElementFactory justificationFactory = new FrameworkElementFactory(typeof(Button));
            justificationFactory.SetValue(Button.WidthProperty, 40.0);
            justificationFactory.SetValue(Button.NameProperty, "BtnJustif");
            justificationFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(Button_Click_Remove));

            FrameworkElementFactory justificationImage = new FrameworkElementFactory(typeof(Image));
            justificationImage.SetValue(Image.SourceProperty, new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\BinSched.png")));

            justificationImage.SetValue(Image.WidthProperty, 50.0);
            justificationImage.SetValue(Image.HeightProperty, 20.0);
            justificationImage.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            justificationImage.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            justificationFactory.AppendChild(justificationImage);
            DataTemplate justificationTemplate = new DataTemplate { VisualTree = justificationFactory };
            justificationColumn.CellTemplate = justificationTemplate;

            // Add the DataGridTemplateColumn to the DataGrid
            DG.Columns.Add(justificationColumn);
            DG.ItemsSource = datat.DefaultView;

        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string Condition = "1 > 0 ";
                if (TBSearch.Text != "")
                {

                  
                        Condition += " and ( Name like '%" + TBSearch.Text + "%' )";
                   

                }
                datat.DefaultView.RowFilter = Condition;

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)DG.SelectedItem;
            if(row == null)
            {
                return;
            }
            datat.Rows.Remove(row.Row);

            // Optionally, you can refresh the DataGrid if necessary, though it should auto-update.
            // Ensure the DataGrid is using the updated DataTable as its ItemsSource.
            DG.ItemsSource = datat.DefaultView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
