using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;
using FastReport;
using Gestion_De_Cours.Classes;
using Gestion_De_Cours.Panels;

namespace Gestion_De_Cours.UControl
{
    /// <summary>
    /// Interaction logic for AllAttendance.xaml
    /// </summary>
    public partial class AllAttendance : UserControl
    {
        DataSet ds = new DataSet();
        int triggerEvent = 1;
        int stop = 0;
        DataTable groupBoxInfoTable = new DataTable();
        DataTable dtAttendances = new DataTable();
        private DispatcherTimer closeTimer;
        private DispatcherTimer inactivityTimer;
        public bool FINISH = false;
        public bool ALREADYOPENED = false;
        string querySession = "";
        int datecount = 0;
        DataTable dtAttendanceUncreated = new DataTable();
        DataTable dtattendanceExtra = new DataTable();
        DataTable DtAttend = new DataTable();
        string QueryForAttendance ;
        public AllAttendance()
        {
            InitializeComponent();
            SetLang();
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromSeconds(3);
            inactivityTimer.Tick += InactivityTimer_Tick;

            // Start the timer
            inactivityTimer.Start();

            // Attach event handlers to reset the timer on user activity
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;
           // DPTodayDate.Text = DateTime.Now.ToString("dd-MM-yyyy");


        }

   
        private void CreateExtraAttend(string AID)
        {
            GroupBox groupBox = new GroupBox
            {
                Name = "GBE" + AID,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Width = 500,
                Height = Commun.ScreenHeight - 220,
                Header = new StackPanel
                {
                }
            };
            string CID = Connexion.GetString("Select CID from Attendance_Extra where id = " + AID);
            string name = Connexion.GetString("Select CName from class where ID = " + CID) + this.Resources["ExtraSession"].ToString();
            StackPanel spHeader = new StackPanel();
            spHeader.Orientation = Orientation.Horizontal;
            TextBlock tbHeader = new TextBlock();
            tbHeader.Text = name;
            tbHeader.HorizontalAlignment = HorizontalAlignment.Center;
            tbHeader.FontSize = 15;
            tbHeader.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0));
            tbHeader.Margin = new Thickness(10, 0, 10, 0);
            spHeader.Children.Add(tbHeader);
            /*   Button bHeader = new Button();
                bHeader.Height = 27;
                bHeader.Width = 30;
                bHeader.Margin = new Thickness(0, 5, 0, 5);
                bHeader.Name = "BE" + AID;
                RegisterName(bHeader.Name, bHeader);
                Image BImage = new Image();
                BImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Print.png"));


                BImage.Width = 30;
                BImage.Height = 25;
                bHeader.Content = BImage;
                spHeader.Children.Add(bHeader);*/
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 50;
            comboBox.Height = 30;
            comboBox.HorizontalAlignment = HorizontalAlignment.Left;
            comboBox.VerticalAlignment = VerticalAlignment.Top;
            comboBox.Margin = new Thickness(10);
            comboBox.Name = "CBA" + AID;
            try
            {
                UnregisterName(comboBox.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(comboBox.Name, comboBox);
            // Create a list of items with text and image

            var items = new[]
            {
                new { Text = this.Resources["Print"].ToString() ,  ImagePath = Directory.GetCurrentDirectory() + @"\Images\PrintContent.png" },
                new { Text = this.Resources["ViewClass"].ToString() , ImagePath = Directory.GetCurrentDirectory() + @"\Images\ClassContent.png" },
                new { Text = this.Resources["ViewAttendance"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\AttendanceContent.png" },
                new { Text =  this.Resources["ViewExtraStudents"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\AddSignContent.png" },
                new { Text =  this.Resources["SetAbsenceForRest"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\absent512Content.png" }
            };

            // Iterate through items and add them to the ComboBox
            foreach (var item in items)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();

                // Create StackPanel to hold Image and Text
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                // Add Image
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(item.ImagePath));
                image.Width = 20;
                image.Height = 20;
                image.Margin = new Thickness(5);
                stackPanel.Children.Add(image);

                // Add Text
                TextBlock textBlock = new TextBlock();
                textBlock.Text = item.Text;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(textBlock);

                // Set the content of the ComboBoxItem to the StackPanel
                comboBoxItem.Content = stackPanel;

                // Add ComboBoxItem to the ComboBox
                comboBox.Items.Add(comboBoxItem);
            }

            spHeader.Children.Add(comboBox);
            groupBox.Header = spHeader;
            var cb = ((StackPanel)groupBox.Header).Children.OfType<ComboBox>().FirstOrDefault();
            if (cb != null)
            {
                cb.SelectionChanged += CB_options_Selection_Changed;
            }


            groupBox.Header = spHeader;
            try
            {
                UnregisterName(groupBox.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(groupBox.Name, groupBox);
            var button = ((StackPanel)groupBox.Header).Children.OfType<Button>().FirstOrDefault();
            if (button != null)
            {
                button.Click += printAttend;
            }
            // Create a DataGrid inside the Border
            DataGrid dataGrid = new DataGrid
            {
                Name = "DGE" + AID, // Set x:Name
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                SelectionMode = DataGridSelectionMode.Extended,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                IsReadOnly = true,
                AutoGenerateColumns = false,
                Height = Commun.ScreenHeight - 220,
                Width = 500,
                VerticalAlignment = VerticalAlignment.Top,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible
            };
            dataGrid.MouseDoubleClick += ListStudents_MouseDoubleClick;
            try
            {
                UnregisterName(dataGrid.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(dataGrid.Name, dataGrid);
            DataGridTextColumn textColumn = new DataGridTextColumn
            {
                Width = DataGridLength.Auto, // Set the Width to Auto
                Header = new TextBlock
                {
                    Text = this.Resources["Name"].ToString(), // Set your header text
                    TextAlignment = TextAlignment.Center
                },
                Binding = new Binding("Name"), // Set the Binding property
                CellStyle = new System.Windows.Style(typeof(DataGridCell))
            };

            // Create the style triggers for cell foreground color
            System.Windows.Style cellStyle = new System.Windows.Style(typeof(DataGridCell));
            cellStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Gender"),
                Value = 1,
                Setters = { new Setter(TextBlock.ForegroundProperty, Brushes.Salmon) }
            });
            cellStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Gender"),
                Value = 0,
                Setters = { new Setter(TextBlock.ForegroundProperty, Brushes.Blue) }
            });

            textColumn.CellStyle = cellStyle;

            // Add the DataGridTextColumn to the DataGrid
            dataGrid.Columns.Add(textColumn);
            DataGridTextColumn sessionsColumn = new DataGridTextColumn
            {
                Width = DataGridLength.Auto,
                Header = new TextBlock
                {
                    Text = this.Resources["Payments"].ToString(), // Set your header text
                    TextAlignment = TextAlignment.Center
                },
                Binding = new Binding("Sessions")
            };
            System.Windows.Style cellStyleSes = new System.Windows.Style(typeof(DataGridCell));
            cellStyleSes.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));

            // Use the converter to dynamically change the foreground color
            /* cellStyleSes.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("Sessions")
             {
                 Converter = new ForegroundConverter(CID)
             }));

             sessionsColumn.CellStyle = cellStyleSes;
            */
            // Add the DataGridTextColumn to the DataGrid
            dataGrid.Columns.Add(sessionsColumn);
            DataGridTextColumn statusTextColumn = new DataGridTextColumn
            {
                Width = DataGridLength.Auto,
                Header = new TextBlock
                {
                    Text = this.Resources["Presense"].ToString(),
                    TextAlignment = TextAlignment.Center
                },
                Binding = new Binding("StatusText"),
                CellStyle = new System.Windows.Style(typeof(DataGridCell))
            };



            statusTextColumn.CellStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Status"),
                Value = 1,
                Setters = { new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(102, 187, 106))) } // Green background for Status = 1
            });

            // Add the DataGridTextColumn to the DataGrid
            DataGridTemplateColumn paymentColumn = new DataGridTemplateColumn
            {
                Width = DataGridLength.Auto,
                Header = new TextBlock
                {
                    Text = this.Resources["Payment"].ToString(),
                    TextAlignment = TextAlignment.Center
                }
            };

            FrameworkElementFactory paymentFactory = new FrameworkElementFactory(typeof(Button));
            paymentFactory.SetValue(Button.WidthProperty, 40.0);
            paymentFactory.SetValue(Button.NameProperty, "BtnPayment");
            paymentFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnPayment_Click));

            FrameworkElementFactory paymentImage = new FrameworkElementFactory(typeof(Image));
            paymentImage.SetValue(Image.SourceProperty, new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Payment.png")));

            paymentImage.SetValue(Image.WidthProperty, 50.0);
            paymentImage.SetValue(Image.HeightProperty, 20.0);
            paymentImage.SetValue(Image.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            paymentImage.SetValue(Image.VerticalAlignmentProperty, VerticalAlignment.Stretch);

            paymentFactory.AppendChild(paymentImage);
            DataTemplate paymentTemplate = new DataTemplate { VisualTree = paymentFactory };
            paymentColumn.CellTemplate = paymentTemplate;

            // Add the DataGridTemplateColumn to the DataGrid
            dataGrid.Columns.Add(paymentColumn);

            // Create a DataGridTemplateColumn for the "Justification" column
            DataGridTemplateColumn justificationColumn = new DataGridTemplateColumn
            {
                Width = DataGridLength.Auto,
                Header = new TextBlock
                {
                    Text = this.Resources["Justification"].ToString(),
                    TextAlignment = TextAlignment.Center
                }
            };
            // Add the DataGridTemplateColumn to the DataGrid
            dataGrid.Columns.Add(justificationColumn);
            string query = "Select Students.ID as ID,Students.FirstName + ' ' + Students.LastName as Name , Students.LastName + ' ' + Students.FirstName As RName , Students.Barcode as Barcode , Attendance_Extra_students.Status as Status  ,  Case When Attendance_Extra_students.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' When Attendance_Extra_students.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' end as StatusText , Attendance_Extra_students.Price as Sessions From Attendance_Extra_Students join Students on Attendance_Extra_Students.SID =  Students.ID Where Attendance_Extra_Students.ID = " + AID;
            DataTable dt = new DataTable("DTE" + AID);
            Connexion.FillDT(ref dt, query);
            ds.Tables.Add(dt);
            dataGrid.ItemsSource = ds.Tables["DTE" + AID].DefaultView;
            groupBox.Content = dataGrid;

            // Add the Border to the StackPanel
            SPAttendance.Children.Insert(0, groupBox);
            dataGrid.SelectionChanged += ListStudents_SelectionChanged;
        }


        private void CreateAttend(string AID)
        {
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);

            string CID = Connexion.GetClassID(GID).ToString();
            string name = Connexion.GetString(" Select case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName When MultipleGroups = 'Single' then Class.CName End as Name from Groups Join Class on Class.ID = Groups.ClassID  where Groups.GroupID =" + GID);
            GroupBox groupBox = new GroupBox
            {
                Name = "GBA" + AID,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Width = 500,
                Height = Commun.ScreenHeight - 220,
                Header = new StackPanel
                {
                }
            };
            StackPanel spHeader = new StackPanel();
            spHeader.Orientation = Orientation.Horizontal;
            TextBlock tbHeader = new TextBlock();
            tbHeader.Text = name; 
            tbHeader.HorizontalAlignment = HorizontalAlignment.Center;
            tbHeader.VerticalAlignment = VerticalAlignment.Center;
            tbHeader.FontSize = 15;
            tbHeader.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0));
            tbHeader.Margin = new Thickness(10, 0, 0, 0);
            TextBlock tbHeaderTotal = new TextBlock();
            tbHeaderTotal.Text =  Connexion.GetTotalStuOutof(AID);
            tbHeaderTotal.HorizontalAlignment = HorizontalAlignment.Center;
            tbHeaderTotal.VerticalAlignment = VerticalAlignment.Center;
            tbHeaderTotal.FontSize = 15;
            tbHeaderTotal.Foreground = new SolidColorBrush(Color.FromRgb(0, 128, 0));
            tbHeaderTotal.Margin = new Thickness(3, 0, 10, 0);
            tbHeaderTotal.Name = "LBA" + AID;
            try
            {
                this.UnregisterName(tbHeaderTotal.Name);
            }
            catch (ArgumentException)
            {
                // The name was not registered, no action needed
            }
            RegisterName(tbHeaderTotal.Name, tbHeaderTotal);
            spHeader.Children.Add(tbHeader);
            spHeader.Children.Add(tbHeaderTotal);
            try
            {
                UnregisterName(groupBox.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(groupBox.Name, groupBox);
            /* Button bHeader = new Button();
             bHeader.Height = 27;
             bHeader.Width = 30;
             bHeader.Margin = new Thickness(0, 5, 0, 5);
             bHeader.Name = "BA" + AID;
             RegisterName(bHeader.Name, bHeader);


             Image BImage = new Image();
             BImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Print.png"));
             BImage.Width = 30;
             BImage.Height = 25;
             bHeader.Content = BImage;

             var button = ((StackPanel)groupBox.Header).Children.OfType<Button>().FirstOrDefault();
             if (button != null)
             {
                 button.Click += printAttend;
             }*/
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 50;
            comboBox.Height = 30;
            comboBox.HorizontalAlignment = HorizontalAlignment.Left;
            comboBox.VerticalAlignment = VerticalAlignment.Top;
            comboBox.Margin = new Thickness(10);
            comboBox.Name = "CBA" + AID;
            try
            {
                UnregisterName(comboBox.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(comboBox.Name, comboBox);
            // Create a list of items with text and image
 
            var items = new[]
            {
                new { Text = this.Resources["AddStudent"].ToString() ,  ImagePath = Directory.GetCurrentDirectory() + @"\Images\AddSignContent.png" },
                new { Text = this.Resources["Print"].ToString() ,  ImagePath = Directory.GetCurrentDirectory() + @"\Images\PrintContent.png" },
                new { Text = this.Resources["ViewClass"].ToString() , ImagePath = Directory.GetCurrentDirectory() + @"\Images\ClassContent.png" },
                new { Text = this.Resources["ViewAttendance"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\AttendanceContent.png" },
                new { Text =  this.Resources["ViewExtraStudents"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\AddSignContent.png" },
                new { Text =  this.Resources["SetAbsenceForRest"].ToString(), ImagePath = Directory.GetCurrentDirectory() + @"\Images\absent512Content.png" }
            };
            
            // Iterate through items and add them to the ComboBox
            foreach (var item in items)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();

                // Create StackPanel to hold Image and Text
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;

                // Add Image
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(item.ImagePath));
                image.Width = 20;
                image.Height = 20;
                image.Margin = new Thickness(5);
                stackPanel.Children.Add(image);

                // Add Text
                TextBlock textBlock = new TextBlock();
                textBlock.Text = item.Text;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                stackPanel.Children.Add(textBlock);

                // Set the content of the ComboBoxItem to the StackPanel
                comboBoxItem.Content = stackPanel;

                // Add ComboBoxItem to the ComboBox
                comboBox.Items.Add(comboBoxItem);
            }

            spHeader.Children.Add(comboBox);
            groupBox.Header = spHeader;
            var cb = ((StackPanel)groupBox.Header).Children.OfType<ComboBox>().FirstOrDefault();
            if (cb != null)
            {
                cb.SelectionChanged += CB_options_Selection_Changed;
            }

            // Create a DataGrid inside the Border
            // Create ListView
            // Create and configure the ListView
            ListView listView = new ListView
            {
                Name = "LVA" + AID,
                Height = Commun.ScreenHeight - 220,
                Width = 500,
                FontSize = 15,
                SelectionMode = SelectionMode.Extended
            };

            // Create a style for ListViewItem (equivalent to your <ListView.Resources> block)
            System.Windows.Style itemStyle = new System.Windows.Style(typeof(ListViewItem));

            // Default setters
            itemStyle.Setters.Add(new Setter(ListViewItem.BackgroundProperty, Brushes.White));
            itemStyle.Setters.Add(new Setter(ListViewItem.ForegroundProperty, Brushes.Black));
            itemStyle.Setters.Add(new Setter(ListViewItem.FontSizeProperty, 15.0));

            // Trigger for IsSelected = True
            Trigger selectedTrigger = new Trigger
            {
                Property = ListViewItem.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter(ListViewItem.BackgroundProperty,
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F8E9"))));
            selectedTrigger.Setters.Add(new Setter(ListViewItem.ForegroundProperty, Brushes.Black));

            // Trigger for IsMouseOver = True
            Trigger hoverTrigger = new Trigger
            {
                Property = ListViewItem.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(ListViewItem.BackgroundProperty, 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A5D6A7"))));

            // Add triggers
            itemStyle.Triggers.Add(selectedTrigger);
            itemStyle.Triggers.Add(hoverTrigger);

            // Apply style to ListView
            listView.ItemContainerStyle = itemStyle;

            // Optional: Double-click handler
            listView.MouseDoubleClick += ListStudents_MouseDoubleClick;

            // Register name (like you did with DataGrid)
            try { UnregisterName(listView.Name); } catch { }
            RegisterName(listView.Name, listView);

            // Create GridView
            GridView gridView = new GridView();
            listView.View = gridView;

            // ---------------- IMAGE COLUMN ----------------

            GridViewColumn unpaidColumn = new GridViewColumn { Header = "" };
            DataTemplate template = new DataTemplate();

            FrameworkElementFactory ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
            ellipseFactory.SetValue(Ellipse.WidthProperty, 16.0);
            ellipseFactory.SetValue(Ellipse.HeightProperty, 16.0);
            ellipseFactory.SetValue(Ellipse.MarginProperty, new Thickness(4));
            ellipseFactory.SetValue(Ellipse.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            ellipseFactory.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Bind Fill to Sessions using the converter
            Binding fillBinding = new Binding("Sessions")
            {
                Converter = new UnpaidToBrushConverter()
            };
            ellipseFactory.SetBinding(Ellipse.FillProperty, fillBinding);

            template.VisualTree = ellipseFactory;
            unpaidColumn.CellTemplate = template;
            gridView.Columns.Add(unpaidColumn);
            // ---------------- NAME COLUMN ----------------
            GridViewColumn nameColumn = new GridViewColumn
            {
                Header = this.Resources["Name"].ToString(),
                DisplayMemberBinding = new Binding("Name")
            };
            // Instead of DisplayMemberBinding we can do Template for color:
            DataTemplate nameTemplate = new DataTemplate();
            FrameworkElementFactory nameText = new FrameworkElementFactory(typeof(TextBlock));
            nameText.SetBinding(TextBlock.TextProperty, new Binding("Name"));

            // Gender-based coloring
            System.Windows.Style nameStyle = new System.Windows.Style(typeof(TextBlock));
            nameStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Gender"),
                Value = 1,
                Setters = { new Setter(TextBlock.ForegroundProperty, Brushes.Salmon) }
            });
            nameStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Gender"),
                Value = 0,
                Setters = { new Setter(TextBlock.ForegroundProperty, Brushes.Blue) }
            });
            nameText.SetValue(TextBlock.StyleProperty, nameStyle);

            nameTemplate.VisualTree = nameText;
            nameColumn.CellTemplate = nameTemplate;
            gridView.Columns.Add(nameColumn);

            // ---------------- PAYMENTS COLUMN ----------------
            GridViewColumn payColumn = new GridViewColumn
            {
                Header = this.Resources["Payments"].ToString(),
                DisplayMemberBinding = new Binding("Sessions")
            };
            gridView.Columns.Add(payColumn);

            // ---------------- STATUS COLUMN ----------------
            GridViewColumn statusColumn = new GridViewColumn
            {
                Header = this.Resources["Presense"].ToString(),
                Width = 75
            };

            DataTemplate statusTemplate = new DataTemplate();
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(System.Windows.Controls.Border));
            border.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(12));
            border.SetValue(System.Windows.Controls.Border.PaddingProperty, new Thickness(6, 2, 6, 2));
            border.SetValue(System.Windows.Controls.Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            // Text inside border
            FrameworkElementFactory statusText = new FrameworkElementFactory(typeof(TextBlock));
            statusText.SetBinding(TextBlock.TextProperty, new Binding("StatusText"));
            statusText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            statusText.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            border.AppendChild(statusText);

            // Style triggers for Status
            System.Windows.Style borderStyle = new System.Windows.Style(typeof(System.Windows.Controls.Border));
            borderStyle.Setters.Add(new Setter(System.Windows.Controls.Border.BackgroundProperty, Brushes.Transparent));

            borderStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Status"),
                Value = 0,
                Setters = { new Setter(System.Windows.Controls.Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(239, 83, 80))) }
            });
            borderStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Status"),
                Value = 1,
                Setters = { new Setter(System.Windows.Controls.Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(76, 175, 80))) }
            });
            borderStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Status"),
                Value = 2,
                Setters = { new Setter(System.Windows.Controls.Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 152, 0))) }
            });
            borderStyle.Triggers.Add(new DataTrigger
            {
                Binding = new Binding("Status"),
                Value = 3,
                Setters =
                {
                    new Setter(System.Windows.Controls.Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 214, 0))),
                    new Setter(TextBlock.ForegroundProperty, Brushes.Black)
                }
            });

            border.SetValue(System.Windows.Controls.Border.StyleProperty, borderStyle);
            statusTemplate.VisualTree = border;
            statusColumn.CellTemplate = statusTemplate;

            gridView.Columns.Add(statusColumn);

            // ---------------- REASON COLUMN ----------------
            GridViewColumn reasonColumn = new GridViewColumn
            {
                Header = this.Resources["ReasonofAbsent"].ToString(),
                DisplayMemberBinding = new Binding("Reason")
            };
            gridView.Columns.Add(reasonColumn);

          
            string query = Commun.GetQueryDataTable("Attendance", AID);

            DataTable dt = new DataTable("DTA" + AID);
            Connexion.FillDT(ref dt, query);
            ds.Tables.Add(dt);
            listView.ItemsSource = ds.Tables["DTA" + AID].DefaultView;

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Name = "ContextA" + AID;
            try
            {
                UnregisterName(contextMenu.Name);
            }
            catch (Exception excep)
            {

            }
            RegisterName(contextMenu.Name, contextMenu);
            MenuItem PaymentMenuItem = new MenuItem();
            PaymentMenuItem.Header = this.Resources["Payment"].ToString();
            PaymentMenuItem.Click += BtnPayment_Click;

            // Set the icon for Payment
            Image PaymentImage = new Image();
            PaymentImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Payment.png")); // Use your image path here
            PaymentImage.Width = 16;
            PaymentImage.Height = 16;
            PaymentMenuItem.Icon = PaymentImage;

            MenuItem JustifMenuItem = new MenuItem();
            JustifMenuItem.Header = this.Resources["Justification"].ToString();
            JustifMenuItem.Click += BtnJustif_Click;

            // Set the icon for Justif
            Image JustifImage = new Image();
            JustifImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Absent.png")); // Use your image path here
            JustifImage.Width = 16;
            JustifImage.Height = 16;
            JustifMenuItem.Icon = JustifImage;

            // Create Discount Menu Item
            MenuItem discountMenuItem = new MenuItem();
            discountMenuItem.Header = this.Resources["Discounts"].ToString();
            discountMenuItem.Click += DiscountMenuItem_Click;

            // Set the icon for Discount
            Image discountImage = new Image();
            discountImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\DiscountContent.png")); // Use your image path here
            discountImage.Width = 16;
            discountImage.Height = 16;
            discountMenuItem.Icon = discountImage;

            // Create Leave Group Menu Item
            MenuItem leaveGroupMenuItem = new MenuItem();
            leaveGroupMenuItem.Header = this.Resources["LeaveGroup"].ToString();
            leaveGroupMenuItem.Click += LeaveMenuItem_Click;

            // Set the icon for Leave Group
            Image leaveGroupImage = new Image();
            leaveGroupImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\LeaveContent.png")); // Use your image path here
            leaveGroupImage.Width = 16;
            leaveGroupImage.Height = 16;
            leaveGroupMenuItem.Icon = leaveGroupImage;

            // Create Change Payment Menu Item
            MenuItem changePaymentMenuItem = new MenuItem();
            changePaymentMenuItem.Header = this.Resources["ChangePrice"].ToString();
            changePaymentMenuItem.Click += ChangePaymentItem_Click;

            // Set the icon for Change Payment
            Image changePaymentImage = new Image();
            changePaymentImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Images\Payment512Content.png")); // Use your image path here
            changePaymentImage.Width = 16;
            changePaymentImage.Height = 16;
            changePaymentMenuItem.Icon = changePaymentImage;

            // Add MenuItems to ContextMenu
            contextMenu.Items.Add(PaymentMenuItem);
            contextMenu.Items.Add(JustifMenuItem);
            contextMenu.Items.Add(discountMenuItem);
            contextMenu.Items.Add(leaveGroupMenuItem);
            contextMenu.Items.Add(changePaymentMenuItem);

            // Attach ContextMenu to DataGrid
            listView.ContextMenu = contextMenu;

            // Optionally, you can add a MouseRightButtonUp event to trigger the ContextMenu
            listView.MouseRightButtonUp += DataGrid_MouseRightButtonUp;
            groupBox.Content = listView;

            // Add the Border to the StackPanel
            SPAttendance.Children.Insert(0, groupBox);
            listView.SelectionChanged += ListStudents_SelectionChanged;
        }
        private void DiscountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
            {
                return;
            }
            string AID = contextMenu.Name.Substring(8);
            ListView LV = (ListView)FindName("LVA" + AID);
            
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID =" + AID);
            string CID = Connexion.GetClassID(GID).ToString();
            if (LV.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select a single row ");
                return;
            }
            DataRowView rowS = (DataRowView)LV.SelectedItem;
            string SID = rowS["ID"].ToString();
            Panels.Discount discpanel = new Panels.Discount(SID, CID);
            discpanel.ShowDialog();
            int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
            {
                int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
            }

            if (monthlypayment == 1)
            {
                string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                int monthNumber = dateAttendance.Month;
                int year = dateAttendance.Year;
                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
            }
            else
            {
                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                {
                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                }
                else
                {
                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                }
            }

            string query = @"
                SELECT 
                    SubQuery.Name,
                    SubQuery.RName,
                    SubQuery.Gender,
                    SubQuery.Sessions,
                    SubQuery.ID,
                    SubQuery.Status,
                    SubQuery.StatusText,
                    SubQuery.Reason,
                    CASE 
                        WHEN SubQuery.Sessions < 0 THEN N'" + Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png' 
                        ELSE '' 
                    END AS paidimg
                FROM 
                    (
                        SELECT 
                            Students.FirstName + ' ' + Students.LastName AS Name,
                            Students.LastName + ' ' + Students.FirstName AS RName,
                            Students.Gender AS Gender,
                            " + querySession + @" AS Sessions,
                            Students.ID AS ID,
                            Attendance_Student.Status AS Status,
                            CASE 
                                WHEN Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 2 THEN N'" + this.Resources["GroupChange"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 3 THEN N'" + this.Resources["Justified"].ToString() + @"' 
                            END AS StatusText,
                            CASE 
                                WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                            END AS Reason
                        FROM 
                            Attendance 
                        JOIN 
                            Attendance_Student ON Attendance.ID = Attendance_Student.ID
                        JOIN 
                            Students ON Students.ID = Attendance_Student.StudentID
                        LEFT JOIN 
                            Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                        JOIN 
                            Groups ON Groups.GroupID = Attendance.GroupID
                        WHERE 
                            Attendance.ID = " + AID + @"
                    ) AS SubQuery;
                ";
            DataTable foundTable = ds.Tables["DTA" + AID];
            Connexion.FillDT(ref foundTable, query);
            ds.Tables.Remove("DTA" + AID);
            string Name = "LVA" + AID;
            LV.ItemsSource = foundTable.DefaultView;
            ds.Tables.Add(foundTable);

        }
        private void LeaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            string AID = contextMenu.Name.Substring(8);
            if (contextMenu == null)
            {
                return;
            }

            ListView LV = (ListView)FindName("LVA" + AID);
            if (LV.SelectedItems.Count > 1)
            {
                MessageBox.Show("Please select a single row ");
                return;
            }
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID =" + AID);
            DataTable dtlist = new DataTable();
            DataRowView rowS = (DataRowView)LV.SelectedItem;
            if (rowS == null)
            {
                return;
            }
            DataRow rows = (DataRow)LV.SelectedItem;
            Commun.LeaveGroup(rowS["SID"].ToString(), GID);
            int CID = Connexion.GetClassID(GID);
            int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
            {
                int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
            }

            if (monthlypayment == 1)
            {
                string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                int monthNumber = dateAttendance.Month;
                int year = dateAttendance.Year;
                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
            }
            else
            {
                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                {
                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                }
                else
                {
                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                }
            }

            string query = @"
                SELECT 
                    SubQuery.Name,
                    SubQuery.RName,
                    SubQuery.Gender,
                    SubQuery.Sessions,
                    SubQuery.ID,
                    SubQuery.Status,
                    SubQuery.StatusText,
                    SubQuery.Reason,
                    CASE 
                        WHEN SubQuery.Sessions < 0 THEN N'" + Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png' 
                        ELSE '' 
                    END AS paidimg
                FROM 
                    (
                        SELECT 
                            Students.FirstName + ' ' + Students.LastName AS Name,
                            Students.LastName + ' ' + Students.FirstName AS RName,
                            Students.Gender AS Gender,
                            " + querySession + @" AS Sessions,
                            Students.ID AS ID,
                            Attendance_Student.Status AS Status,
                            CASE 
                                WHEN Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 2 THEN N'" + this.Resources["GroupChange"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 3 THEN N'" + this.Resources["Justified"].ToString() + @"' 
                            END AS StatusText,
                            CASE 
                                WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                            END AS Reason
                        FROM 
                            Attendance 
                        JOIN 
                            Attendance_Student ON Attendance.ID = Attendance_Student.ID
                        JOIN 
                            Students ON Students.ID = Attendance_Student.StudentID
                        LEFT JOIN 
                            Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                        JOIN 
                            Groups ON Groups.GroupID = Attendance.GroupID
                        WHERE 
                            Attendance.ID = " + AID + @"
                    ) AS SubQuery;
                ";
            DataTable foundTable = ds.Tables["DTA" + AID];
            Connexion.FillDT(ref foundTable, query);
            ds.Tables.Remove("DTA" + AID);
            LV.ItemsSource = foundTable.DefaultView;
            ds.Tables.Add(foundTable);
        }
        private void ChangePaymentItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
            {
                return;
            }
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
            {
                return;
            }
            string AID = contextMenu.Name.Substring(8);
            ListView LV = (ListView)FindName("LVA" + AID);
            if (LV.SelectedItems.Count > 1)
            {
                if (MessageBox.Show("هل أنت متأكد أنك تريد تغيير السعر لجميع الطلاب المحددين؟", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                   
            }
            OptionPanels.TwoTextBoxPage twooption = new OptionPanels.TwoTextBoxPage(this.Resources["EnterNewPrice"].ToString(), this.Resources["SPriceLB"].ToString(), this.Resources["TPriceLB"].ToString(), "", "");
            bool? dialogResult = twooption.ShowDialog();
            string[] result;
            int resultint;
            if (dialogResult == true)
            {
                 result = twooption.Result;
            }
            else
            {
                return;
            }
            string GID = Connexion.GetString("Select GroupID from Attendance Where ID =" + AID);
            string CID = Connexion.GetClassID(GID).ToString();
            foreach (var selectedItem in LV.SelectedItems)
            {
                if (selectedItem is DataRowView row)
                {
                    string SID = row["ID"].ToString();

                    if (result[0] != "" && int.TryParse(result[0], out resultint))
                    {
                        int OldTotal = Connexion.GetInt("Select dbo.CalcPriceSum(" + SID + " , " + CID + ")");
                        int Diff = -OldTotal + resultint;
                        Commun.ChangeInitialPrice(SID, GID, Diff);
                    }
                    if (result[1] != "" && int.TryParse(result[1], out resultint))
                    {

                    }
                }
            }
            int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
            {
                int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
            }

            if (monthlypayment == 1)
            {
                string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                int monthNumber = dateAttendance.Month;
                int year = dateAttendance.Year;
                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
            }
            else
            {
                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                {
                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                }
                else
                {
                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                }
            }

            string query = "Select Students.FirstName + ' ' + Students.LastName as Name ,Students.LastName + ' ' + students.FirstName as RName ,  Students.Gender as Gender , " + querySession + " as Sessions  , Students.ID as ID , Attendance_Student.Status as Status ," +
                " Case " +
                "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                 "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                 "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                 "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , " +
                "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                "join Students on Students.ID = Attendance_Student.StudentID " +
                "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;
            DataTable foundTable = ds.Tables["DTA" + AID];
            Connexion.FillDT(ref foundTable, query);
            ds.Tables.Remove("DTA" + AID);
            string Name = "LVA" + AID;
            LV.ItemsSource = foundTable.DefaultView;
            ds.Tables.Add(foundTable);
        }
        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                string AID = menuItem.Name.Substring(8);
                 ListView LV =  (ListView)FindName("LVA"+AID);
                LV.ContextMenu.IsOpen = true;
            }

        }

        private void  CB_options_Selection_Changed(object sender , SelectionChangedEventArgs e)
        {
            try
            {
                inactivityTimer.Stop();
                ComboBox comboBox = sender as ComboBox;

                if (comboBox == null)
                {
                    return;
                }
                // Get the name of the ComboBox
                string comboBoxName = comboBox.Name;
                string AID = comboBoxName.Substring(3);
                string Type = comboBoxName.Substring(2, 1);
                // Get the selected index of the ComboBox
                int selectedIndex = comboBox.SelectedIndex;
                if(selectedIndex == 0)
                {
                    DataTable dt = null;
                    Panels.ChangeGroup changegroup = new Panels.ChangeGroup(AID, "1", ref dt);
                    changegroup.ShowDialog();
                }
                else if (selectedIndex == 1) // add student
                {
                    Report r = new Report();
                    if (Type == "A")
                    {
                        FastReports.PrintAttendance(AID);
                    }
                    else if (Type == "E")
                    {
                        FastReports.PrintAttendance(AID, "2");
                    }
                }
                else if (selectedIndex == 2) //ViewClass
                {
                    if (Type == "A")
                    {
                        string gid = Connexion.GetString("Select GroupID from Attendance Where ID = "+ AID);
                        int cid = Connexion.GetClassID(gid);
                        string Multi = Connexion.GetString("Select MultipleGroups from Class Where id =" + cid);
                        var AddW = new ClassAdd("Show", cid.ToString(), Multi) ;
                        AddW.ShowDialog();
                    }
                    else if (Type == "E")
                    {
                        string cid= Connexion.GetString("Select CID from Attendance_Extra Where ID = " + AID);
                        string Multi = Connexion.GetString("Select MultipleGroups from Class Where id =" + cid);
                        var AddW = new ClassAdd("Show", cid, Multi);
                        AddW.ShowDialog();
                    }
                }
                else if(selectedIndex == 3) //view attendance
                {
                    if(Type == "A")
                    {
                        var AddS = new AttendanceAdd(AID, "Show", "1");
                        AddS.ShowDialog();
                    }
                    else if (Type== "E")
                    {
                        var AddS = new AttendanceAdd(AID, "Show", "3");
                        AddS.ShowDialog();
                    }
                }
                else if (selectedIndex == 4) // extra students
                {

                    if (Type == "A")
                    {
                        var AddS = new ExtraStudentsAttendView(AID);
                        AddS.ShowDialog();
                        TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                        textblock.Text = Connexion.GetTotalStuOutof(AID);
                    }
                }
                else if (selectedIndex == 5)// mark absent 
                {
                    if (Type == "A")
                    {
                        if (MessageBox.Show("Are you Sure you want to mark the rest of the students as absent", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            DataTable dtAbsent = new DataTable();
                            Connexion.FillDT(ref dtAbsent, "Select * from Attendance_Student Where Status is null and ID = " + AID);
                            string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                            string CID = Connexion.GetClassID(GID).ToString();
                            string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                            foreach(DataRow row in dtAbsent.Rows)
                            {
                                string SID = row["StudentID"].ToString();
                                Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 0);
                            }
                            TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                            textblock.Text = Connexion.GetTotalStuOutof(AID);
                            DataTable foundTable = ds.Tables["DTA" + AID];
                            string query = "";
                             GID = Connexion.GetString("Select groupid from attendance where ID =" + AID);
                            int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                            {
                                int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                                monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                            }

                            if (monthlypayment == 1)
                            {
                                string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                                DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                                int monthNumber = dateAttendance.Month;
                                int year = dateAttendance.Year;
                                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
                            }
                            else
                            {
                                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                                {
                                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                                }
                                else
                                {
                                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                                }
                            }

                            query = "Select Students.FirstName + ' ' + Students.LastName as Name ,Students.LastName + ' ' + students.FirstName as RName ,  Students.Gender as Gender , " + querySession + " as Sessions  , Students.ID as ID , Attendance_Student.Status as Status ," +
                               " Case " +
                               "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                                "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                                "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                                "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText , " +
                               "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                               "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                               "join Students on Students.ID = Attendance_Student.StudentID " +
                               "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                               "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;

                            Connexion.FillDT(ref foundTable, query);
                            ds.Tables.Remove("DTA" + AID);
                            string Name = "LVA" + AID;
                            ListView LV = (ListView)FindName(Name);
                            LV.ItemsSource = foundTable.DefaultView;
                            ds.Tables.Add(foundTable);
                        }
                    }
                }
                comboBox.SelectedIndex = -1;
                inactivityTimer.Start();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
        //    TBSearch.Focus();
        }

        private void ResetInactivityTimer(object sender, EventArgs e)
        {
            // Reset the timer on user activity
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        private void ListStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                if (triggerEvent == 1)
                {
                    foreach (DataTable table in ds.Tables)
                    {
                        if (table.TableName.Substring(3) != ((FrameworkElement)sender).Name.ToString().Substring(3) || table.TableName.Substring(2, 1) != 
                        ((FrameworkElement)sender).Name.ToString().Substring(2, 1))
                        {
                            ListView LV = (ListView)FindName("LV" + table.TableName.Substring(2, 1) + table.TableName.Substring(3));
                            triggerEvent = 0;
                            LV.UnselectAll();
                            triggerEvent = 1;
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        void ButtonForCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Button clickedButton = (Button)sender; // Cast sender to Button
                string GID = clickedButton.Name.Substring(2);

                string stackPanelName = "SPC" + GID;
                GroupBox groupBox = FindName("GBC" + GID) as GroupBox;

                if (groupBox != null)
                {
                    StackPanel stackPanel = groupBox.FindName(stackPanelName) as StackPanel;
                    if (stackPanel != null)
                    {
                        stackPanel.Children.Clear();
                        stackPanel.VerticalAlignment = VerticalAlignment.Top;
                        SPAttendance.Children.Remove(groupBox);
                        UnregisterName(groupBox.Name);

                        DateTime selectedDate = DPTodayDate.SelectedDate.Value;
                        string formattedDate = selectedDate.ToString("dd-MM-yyyy");
                        bool checkexist = Connexion.IFNULL("Select * from Attendance where GroupID  = " + GID + " and date = '" + DPTodayDate.Text.ToString().Replace('/', '-')  + "'");
                        string AID = "";
                        if (checkexist)
                        {
                            int GTID = Connexion.GetInt("Select ID from Class_Time Where GID = " + GID + " and Day = DATEPART(DW ,Convert(datetime,'" + formattedDate + "', 105)) -1 ");

                            AID = Commun.InsertAttendance(GID, GTID.ToString(), formattedDate).ToString();
                        }
                        else
                        {
                            AID = Connexion.GetString("Select * from Attendance where GroupID  = " + GID + " and date = '" + DPTodayDate.Text.ToString().Replace('/', '-'));
                        }
                        Commun.FillAttendance(AID);
                        CreateAttend(AID);
                        foreach (DataRow row in dtAttendances.Rows)
                        {
                            if (row["ID"].ToString() == GID && row["Created"].ToString() == "0" && row["Type"].ToString() == "3")
                            {
                                row["Created"] = "1";
                                row["AID"] = AID;
                                row["Type"] = "1";
                            }
                        }
                        string name = Connexion.GetString("Select case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName When MultipleGroups = 'Single' then Class.CName End as Name from Class Join Groups on Groups.ClassID = Class.ID where GroupID = " + GID);
                        groupBoxInfoTable.Rows.Add(AID, name, "A");
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem menuItem = sender as MenuItem;
                ListView listview = null;
                if (menuItem != null)
                {
                    ContextMenu contextMenu = menuItem.Parent as ContextMenu;
                    if (contextMenu != null)
                    {
                        listview = contextMenu.PlacementTarget as ListView;
                        // Now you can use dataGrid safely
                    }
                    else
                    {
                        return;
                    }
                }
                DataRowView row = (DataRowView)listview.SelectedItem;
                if (listview.Name.Substring(2, 1) == "A")
                {
                    string AID = listview.Name.Substring(3);
                    string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                    string CID = Connexion.GetClassID(GID).ToString();

                    if (listview.SelectedItems.Count > 1)
                    {
                        MessageBox.Show("Please Select one student only");
                        return;
                    }
                    if (row != null)
                    {
                        inactivityTimer.Stop();
                        int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                        if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                        {
                            int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                            monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                        }
                        if (monthlypayment == 1)
                        {
                            EmptyPage Empty = new EmptyPage("StudentPaymentMonthly", row["ID"].ToString(), "");
                            Empty.ShowDialog();
                        }
                        else
                        {
                            EmptyPage Empty = new EmptyPage("StudentPayment2", row["ID"].ToString(), CID);
                            Empty.ShowDialog();
                        }
                        inactivityTimer.Start();
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnJustif_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (stop != 0)
                {
                    return;
                }

                // With this safer version:
                MenuItem menuItem = sender as MenuItem;
                ListView dataGrid = null;
                if (menuItem != null)
                {
                    ContextMenu contextMenu = menuItem.Parent as ContextMenu;
                    if (contextMenu != null)
                    {
                         dataGrid = contextMenu.PlacementTarget as ListView;
                        // Now you can use dataGrid safely
                    }
                    else
                    {
                        return;
                    }
                }
                DataRowView row = (DataRowView)dataGrid.SelectedItem;
                if (dataGrid.SelectedItems.Count > 1)
                {
                    MessageBox.Show("يرجى اختيار تلميذ واحد فقط");
                    return;
                }
                if (row != null)
                {
                    string SID = row["ID"].ToString();
                    string AID = dataGrid.Name.Substring(3);
                    string GID = Connexion.GetString("Select GroupID From Attendance Where ID = " + AID);
                    string CID = Connexion.GetClassID(GID).ToString();
                    string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                    int rowIndex = dataGrid.Items.IndexOf(row);
                    DataRow[] rowToUpdate = new DataRow[] { ds.Tables["DTA" + AID].Rows[rowIndex] };
                    Commun.SetStatusAttendanceupg(SID, AID,CID,GID, date, 3, ref rowToUpdate);
                    TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                    textblock.Text = Connexion.GetTotalStuOutof(AID);
                    stop = 0;
                    inactivityTimer.Start();
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private T FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null)
            {
                if (element is T tElement)
                    return tElement;
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }
        private void SetLang()
        {
            ResourceDictionary ResourceDic = new ResourceDictionary();

            if (Connexion.Language() == 1) // Arabic
            {
                ResourceDic.Source = new Uri("../Dictionary\\AR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.RightToLeft;
               // this.Resources["AppFont"] = new FontFamily("Segoe UI");
                // Apply Arabic font globally
                this.Resources["AppFont"] = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Droid Arabic Kufi");
            }
            else if (Connexion.Language() == 0) // English
            {
                ResourceDic.Source = new Uri("../Dictionary\\EN.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;

                this.Resources["AppFont"] = new FontFamily("Segoe UI"); // fallback
            }
            else if (Connexion.Language() == 2) // French
            {
                ResourceDic.Source = new Uri("../Dictionary\\FR.xaml", UriKind.Relative);
                this.FlowDirection = FlowDirection.LeftToRight;

                this.Resources["AppFont"] = new FontFamily("Segoe UI");
            }

            this.Resources.MergedDictionaries.Add(ResourceDic);
        }

        void FindGroupBoxNames(DependencyObject element, int t)
        {
            try
            {
                if (t == 1)
                {
                    groupBoxInfoTable.Rows.Clear();
                }

                int count = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(element, i);

                    // Check if the child is a GroupBox
                    if (child is GroupBox groupBox)
                    {
                        if (groupBox.Visibility == Visibility.Visible)
                        {

                            StackPanel stackPanel = groupBox.Header as StackPanel;
                            string groupBoxHeaderText = null;

                            if (stackPanel != null)
                            {
                                foreach (UIElement child2 in stackPanel.Children)
                                {
                                    if (child2 is TextBlock textBlock)
                                    {
                                        groupBoxHeaderText = textBlock.Text;
                                        break;
                                    }
                                }
                            }
                            if (groupBoxHeaderText != null)
                            {
                                string type = groupBox.Name.Substring(2, 1);
                                groupBoxInfoTable.Rows.Add(groupBox.Name.Substring(3), groupBoxHeaderText, type);
                            }
                        }
                    }

                    // Recursively search for GroupBoxes in the child's subtree
                    FindGroupBoxNames(child, 0);
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        ListView FindListviewInGroupBox(DependencyObject groupBox)
        {
            ListView foundListview = null;
            // Traverse the visual tree inside the GroupBox
            int count = VisualTreeHelper.GetChildrenCount(groupBox);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(groupBox, i);

                // Check if the child is a DataGrid
                if (child is ListView)
                {
                    foundListview = (ListView)child;
                    break; // Stop searching once the DataGrid is found
                }
                else
                {
                    // If the child is not a DataGrid, recursively search within it
                    foundListview = FindListviewInGroupBox(child);
                    if (foundListview != null)
                    {
                        break; // Stop searching if a DataGrid is found within the child
                    }
                }
            }

            return foundListview;

        }

        private void CBStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void addAttendInCBGroups(string AID, string name, string type)
        {

            groupBoxInfoTable.Rows.Add(AID, name, type);
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {

                CheckBox checkBox = (CheckBox)sender;
                DataRowView rowview = (DataRowView)checkBox.DataContext;

                if (rowview["ID"].ToString() == "0")
                {
                    foreach (DataRow row in dtAttendances.Rows)
                    {
                        if (row["ID"].ToString() != "0")
                        {

                            row["Checked"] = 1;
                            /*if (row["Type"].ToString() == "3")
                            {

                                GroupBox gb = (GroupBox)FindName("GBC" + row["ID"].ToString());
                                gb.Visibility = Visibility.Visible;

                            }
                            else
                            {

                                string type = "";
                                if (row["Type"].ToString() == "1")
                                {
                                    type = "A";
                                    groupBoxInfoTable.Rows.Add(row["AID"].ToString(), row["Name"].ToString(), type);
                                }
                                else if (row["Type"].ToString() == "2")
                                {
                                    type = "E";
                                    groupBoxInfoTable.Rows.Add(row["AID"].ToString(), row["Name"].ToString(), type);
                                }
                                string Name = "GB" + type + row["AID"].ToString();
                                GroupBox gb = (GroupBox)FindName(Name);
                                gb.Visibility = Visibility.Visible;

                            }*/
                        }
                    }
                }
                else
                {

                    if (rowview["Type"].ToString() == "3")
                    {

                        GroupBox gb = (GroupBox)FindName("GBC" + rowview["ID"].ToString());
                        gb.Visibility = Visibility.Visible;

                    }
                    else
                    {

                        string type = "";
                        if (rowview["Type"].ToString() == "1")
                        {
                            type = "A";
                            if (rowview["Created"].ToString() == "0")
                            {
                                CreateAttend(rowview["AID"].ToString());
                                rowview["Created"] = "1";
                            }
                            groupBoxInfoTable.Rows.Add(rowview["AID"].ToString(), rowview["Name"].ToString(), type);
                        }
                        else if (rowview["Type"].ToString() == "2")
                        {
                            type = "E";
                            if (rowview["Created"].ToString() == "0")
                            {
                                CreateExtraAttend(rowview["AID"].ToString());
                                rowview["Created"] = "1";
                            }
                            groupBoxInfoTable.Rows.Add(rowview["AID"].ToString(), rowview["Name"].ToString(), type);
                        }
                        string Name = "GB" + type + rowview["AID"].ToString();
                        GroupBox gb = (GroupBox)FindName(Name);
                        gb.Visibility = Visibility.Visible;

                    }
                }

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {

                CheckBox checkBox = (CheckBox)sender;
                DataRowView rowview = (DataRowView)checkBox.DataContext;
                if (rowview["ID"].ToString() == "0")
                {
                    foreach (DataRow row in dtAttendances.Rows)
                    {
                        if (row["ID"].ToString() != "0")
                        {
                            row["Checked"] = 0;
                            if (row["Type"].ToString() == "3")
                            {

                                GroupBox gb = (GroupBox)FindName("GBC" + row["ID"].ToString());
                                gb.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                string type = "";
                                if (row["Type"].ToString() == "1")
                                {
                                    type = "A";
                                    foreach (DataRow rowdel in groupBoxInfoTable.Select("Name = '" + row["AID"].ToString() + "' AND Type ='" + type + "' ")) // Replace with your condition.
                                    {
                                        rowdel.Delete();
                                    }

                                }
                                else if (row["Type"].ToString() == "2")
                                {
                                    type = "E";
                                    foreach (DataRow rowdel in groupBoxInfoTable.Select("Name = '" + row["AID"].ToString() + "' AND Type ='" + type + "' ")) // Replace with your condition.
                                    {
                                        rowdel.Delete();
                                    }
                                }
                                string Name = "GB" + type + row["AID"].ToString();
                                GroupBox gb = (GroupBox)FindName(Name);
                                gb.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
                else
                {
                    if (rowview["Created"].ToString() == "0")
                    {

                        GroupBox gb = (GroupBox)FindName("GBC" + rowview["ID"].ToString());
                        gb.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        string type = "";
                        if (rowview["Type"].ToString() == "1")
                        {
                            type = "A";
                        }
                        else if (rowview["Type"].ToString() == "2")
                        {
                            type = "E";
                        }
                        foreach (DataRow rowdel in groupBoxInfoTable.Select("Name = '" + rowview["AID"].ToString() + "' AND Type ='" + type + "' ")) // Replace with your condition.
                        {
                            rowdel.Delete();
                        }
                        string Name = "GB" + type + rowview["AID"].ToString();
                        GroupBox gb = (GroupBox)FindName(Name);
                        gb.Visibility = Visibility.Collapsed;
                    }
                }
                groupBoxInfoTable.AcceptChanges();
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }

        }

        private void Button_Click_Present(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (DataTable table in ds.Tables)
                {
                    if (table.TableName.Substring(2, 1) == "A")
                    {
                        ListView LV = (ListView)FindName("LVA" + table.TableName.Substring(3));
                        string AID = table.TableName.Substring(3);
                        string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                        string CID = Connexion.GetClassID(GID).ToString() ;
                        string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                        foreach (DataRowView selectedItemrow in LV.SelectedItems)
                        {
                            string SID = selectedItemrow["ID"].ToString();
                            int rowIndex = LV.Items.IndexOf(selectedItemrow);
                            DataRow[] rows = table.Select($"{"ID"} = '{SID}'");
                            Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 1, ref rows);
                        }
                        TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                        textblock.Text = Connexion.GetTotalStuOutof(AID);
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Button_Click_Absent(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (DataTable table in ds.Tables)
                {
                    if (table.TableName.Substring(2, 1) == "A")
                    {
                        ListView LV = (ListView)FindName("LVA" + table.TableName.Substring(3));
                        string AID = table.TableName.Substring(3);
                        string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                        string CID = Connexion.GetClassID(GID).ToString();
                        string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                        foreach (DataRowView selectedItemrow in LV.SelectedItems)
                        {
                            string SID = selectedItemrow["ID"].ToString();
                            int rowIndex = LV.Items.IndexOf(selectedItemrow);
                            DataRow[] rows = table.Select($"{"ID"} = '{SID}'");
                            Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 0, ref rows);
                        }
                        TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                        textblock.Text = Connexion.GetTotalStuOutof(AID);
                    }

                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void TBSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    DataSet CopyDS = new DataSet();
                    CopyDS = ds.Copy();
                    string queryforexception;
                    if (TBSearch.Text.Length == 0)
                    {
                        return;
                    }
                    int found = 0;
                    int red = 0;
                    if (TBSearch.Text.Last() == '$')
                    {
                        if (Connexion.IFNULL("Select * from Students Where BarCode = '" + TBSearch.Text + "'"))
                        {
                            MessageBox.Show("Barcode Wrong");
                            TBSearch.Text = "";
                        }
                        else
                        {
                            string SID = Connexion.GetInt(TBSearch.Text, "Students", "ID", "BarCode").ToString();
                            found = 0;
                            DataSet tablestoUpdate = new DataSet();
                            foreach (DataTable table in ds.Tables)
                            {
                                if (table.TableName.Substring(2, 1) == "A")
                                {
                                    ListView LV = (ListView)FindName("LVA" + table.TableName);
                                    string AID = table.TableName.Substring(3);
                                    string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                                    string CID = Connexion.GetClassID(GID).ToString();
                                    string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                                    string Name = "GBA" + AID;
                                    GroupBox gb = (GroupBox)FindName(Name);
                                    if (gb.Visibility == Visibility.Collapsed)
                                    {
                                        continue;
                                    }
                                  
                                    if (!Connexion.IFNULL("Select * from Attendance_Student Where StudentID= " + SID + " and ID =" + AID))
                                    {

                                        found = 1;
                                        DataRow[] rows = table.Select($"{"ID"} = '{SID}'");
                                        Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 1, ref rows);
                                        TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                                        textblock.Text = Connexion.GetTotalStuOutof(AID);
                                        if (Connexion.GetInt("Select dbo.CalcPriceSum(" + SID + "," + Connexion.GetClassID(GID) + ")") < 0)
                                        {
                                            red = 1;
                                        }
                                    }
                                }
                                else if (table.TableName.Substring(2, 1) == "E")
                                {
                                    DataGrid DG = (DataGrid)FindName("DGE" + table.TableName.Substring(3));
                                    string AID = table.TableName.Substring(3);
                                    string CID = Connexion.GetString("Select CID from Attendance_Extra Where ID = " + AID);
                                    string Name = "GBE" + AID;
                                    GroupBox gb = (GroupBox)FindName(Name);
                                    //CONDITIONS TO SKIPPING SEARCH IN THOSE EXTRA SESSIONS
                                    if (gb.Visibility == Visibility.Collapsed)
                                    {
                                        continue;
                                    }
                                    if (!Connexion.IFNULL("Select * from Attendance_Extra_Students Where SID = " + SID + " and ID = " + AID))
                                    {
                                        continue;
                                    }
                                    int yearid = Connexion.GetInt("Select CYear From Class Where ID = " + CID);
                                    string query = "SELECT Students.ID ,Students.Gender,(FirstName + ' ' + LastName) as Name  , '" + Connexion.GetImagesFile() + "\\" + "S' + Convert(Varchar(50),ID)  + '.jpg' as Picture from Students  Where  Students.Status = 1 and Year = " + yearid;
                                    DataTable dtSpec = new DataTable();
                                    Connexion.FillDT(ref dtSpec, "Select SpecID from Class_Speciality Where ID = " + CID);
                                    string Specid = "";
                                    for (int i = 0; i < dtSpec.Rows.Count; i++)
                                    {
                                        Specid = dtSpec.Rows[i]["SpecID"].ToString();
                                        if (i == 0)
                                        {
                                            query += " AND SPECIALITY = " + Specid;
                                        }
                                        else
                                        {
                                            query += " OR SPECIALITY = " + Specid;
                                        }
                                    }
                                    query += " And Students.ID = " + SID;
                                    if (Connexion.IFNULL(query))
                                    {
                                        continue;
                                    }
                                    string studentName = Connexion.GetString("Select FirstName + ' ' + LastName as Name from Students where ID = " + SID);
                                    string ClassName = Connexion.GetString("select CName from Class  where ID = " + CID);
                                    string message = $"يوجد حصة إضافية في القسم (" + ClassName + ")، هل تريد إضافة تلميذ (" + studentName + ") إليها؟";
                                    MessageBoxOptions options = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
                                    string caption = "تأكيد الإضافة";

                                    // Display the MessageBox with Yes/No buttons
                                    MessageBoxResult result = MessageBox.Show(
                                        message,        // Message content
                                        caption,        // Title
                                        MessageBoxButton.YesNo,  // Buttons
                                        MessageBoxImage.Question, // Icon
                                        MessageBoxResult.No,      // Default button
                                        options                  // RTL and right alignment options
                                    );
                                    if (result != MessageBoxResult.Yes)
                                    {
                                        continue;
                                    }
                                    if (Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and ClassID = " + CID + " and EndSession is null "))
                                    {
                                        string messageprice = $"سعر الحصة للتلميذ (" + studentName + ")";
                                        OptionPanels.TextPopups popup = new OptionPanels.TextPopups();
                                        bool? dialogResult = popup.ShowDialog();

                                        if (dialogResult == true)
                                        {
                                            int resultprice = popup.Result;
                                            int tprice = Connexion.GetInt("Select TPayment from Class Where ID  = " + CID) / 4;
                                            Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + "," + resultprice + "," + tprice + ",1)");
                                            DataRow dr = table.NewRow();
                                            dr["ID"] = SID;
                                            dr["Name"] = studentName;
                                            dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);

                                            dr["Sessions"] = resultprice;
                                            dr["Status"] = 1;
                                            dr["StatusText"] = this.Resources["Present"].ToString();
                                            dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
                                            if (int.Parse(dr["Sessions"].ToString()) >= 0)
                                            {
                                                dr["paidimg"] = "";
                                            }
                                            else
                                            {
                                                red = 1;
                                                dr["paidimg"] = Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png";
                                            }
                                            table.Rows.InsertAt(dr, 0);
                                          
                                        }
                                        else
                                        {
                                            MessageBox.Show("No value was entered or the operation was canceled.");

                                        }
                                    }
                                    else
                                    {
                                        Connexion.Insert("Insert into Attendance_extra_Students values(" + AID + "," + SID + ",0,0,1)");
                                        DataRow dr = table.NewRow();
                                        dr["ID"] = SID;
                                        dr["Name"] = studentName;
                                        dr["RName"] = Connexion.GetString("Select lastName + ' ' + FirstName as Name from Students Where ID =" + SID);

                                        dr["Sessions"] = 0;
                                        dr["Status"] = 1;
                                        dr["StatusText"] = this.Resources["Present"].ToString();
                                        dr["Barcode"] = Connexion.GetString("Select Barcode from Students Where ID = " + SID);
                                        if (int.Parse(dr["Sessions"].ToString()) >= 0)
                                        {
                                            dr["paidimg"] = "";
                                        }
                                        else
                                        {
                                            red = 1;
                                            dr["paidimg"] = Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png";
                                        }
                                        table.Rows.InsertAt(dr, 0);
                                       
                                    }

                                }
                            }
                            if (found != 1)//START THIS
                            {
                                List<string> Classes = new List<string>();
                                int YearID = Connexion.GetInt("Select Year from Students Where ID = " + SID);
                                int IfSpeciality = Connexion.GetInt("Select IsSpeciality from Years  Join Levels on Levels.ID = Years.LevelID Where Years.ID = " + YearID);
                                foreach (DataTable table in ds.Tables)
                                {
                                    ListView LV = (ListView)FindName("LVA" + table.TableName.Substring(3));
                                    string AID = table.TableName.Substring(3);
                                    string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                                    string Name = "GBA" + AID;
                                    GroupBox gb = (GroupBox)FindName(Name);
                                    if (gb.Visibility == Visibility.Collapsed)
                                    {
                                        continue;
                                    }
                                    string CID = Connexion.GetClassID(GID).ToString();

                                    if (Connexion.GetInt("Select CYear from Class Where ID = " + CID) == YearID)
                                    {
                                        if (IfSpeciality == 1)
                                        {
                                            DataTable ClassSpecialities = new DataTable();
                                            Connexion.FillDT(ref ClassSpecialities, "Select * from " +
                                                "Class_Speciality Where ID = " + CID);
                                            string SpecID = Connexion.GetString("Select Speciality " +
                                                "from Students Where ID = " + SID);
                                            foreach (DataRow rowClassSpec in ClassSpecialities.Rows)
                                            {
                                                if (rowClassSpec["SpecID"].ToString() != SpecID)
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                        string CName = Connexion.GetString("Select CName from CLass Where ID = " + CID);
                                        if (!Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and ClassID = " + CID + "and GroupID != " + GID))
                                        {
                                            string StudentGID = Connexion.GetString("Select * from Class_Student Where StudentID =" + SID + " and ClassID = " + CID + " and GroupID !=" + GID);

                                            MessageBoxResult resultMessage = MessageBox.Show("عذرًا، هذا التلميذ مسجل بالفعل في مجموعة أخرى:" + CName, "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                            if (resultMessage != MessageBoxResult.Yes)
                                            {
                                                continue;
                                            }
                                            int sessiongroup = Connexion.GetInt("Select Sessions From Groups where GroupID =" + StudentGID);
                                            int SessionA = Connexion.GetInt("Select Session from Attendance Where ID = " + AID) - 1;
                                            if (sessiongroup < SessionA) //G2->G1 // verify if the group is before or after 
                                            {

                                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + SID + ",1)");
                                                Commun.SetStatusAttendance(SID, AID, 1);
                                                TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                                                textblock.Text = Connexion.GetTotalStuOutof(AID);
                                                Connexion.Insert("Insert into Attendance_Change values (" + SID + "," + StudentGID + "," + GID + "," + SessionA + ")");
                                                //RadioButton btn = sender as RadioButton;
                                                //btn.Background = Brushes.Green;
                                                //btn.Foreground = Brushes.Green;
                                            }
                                            else
                                            {

                                                SqlConnection con = Connexion.Connect();
                                                SqlCommand CommandID = new SqlCommand("Select ID From Attendance Where GroupID = '" + StudentGID + "' And Session = " + SessionA, con);
                                                CommandID.ExecuteNonQuery();
                                                Int32 result2 = Convert.ToInt32(CommandID.ExecuteScalar());
                                                Commun.SetStatusAttendance(SID, result2.ToString(), 2);

                                                Connexion.Insert("Insert into Attendance_Change values (" + SID + "," + StudentGID + "," + GID + "," + SessionA + ")");
                                                Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + SID + " , 1 )");
                                                Commun.SetStatusAttendance(SID, AID, 1);
                                                TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                                                textblock.Text = Connexion.GetTotalStuOutof(AID);
                                            }

                                        }
                                        else
                                        {


                                            string SName = Connexion.GetString("Select FirstName + ' '+ LastName from students Where  ID = " + SID);
                                            // string message = string.Format((string)this.Resources["NotRegAddStu"].ToString(), SName, CName);
                                            string message = $"هل تريد إضافته/إضافتها؟(" + CName + ") غير مسجل داخل المجموعة ( " + SName + " ): التلميذ";
                                            MessageBoxResult result = MessageBox.Show(message, "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);


                                            if (result != MessageBoxResult.Yes)
                                            {
                                                continue;
                                            }


                                            int Session = Connexion.GetInt("Select Session from Attendance Where ID = " + AID) - 1;
                                            if (!Connexion.IFNULL("Select * from Class_Student Where StudentID = " + SID + " and GroupID = " + GID))
                                            {

                                                Connexion.Insert("Update  Class_Student Set Session = " + Session + "  Where StudentID = " + SID + " and GroupID = " + GID);
                                            }
                                            else
                                            {
                                                Connexion.Insert("Insert into Class_Student Values (" + SID + " , " + CID + " , " + GID + " , " + Session + " , NULL,0,0 )");

                                            }
                                            Session += 2;
                                            Connexion.Insert("Insert into Attendance_Student(ID,StudentID,Status) Values (" + AID + "," + SID + " , 1 )");
                                            Commun.SetStatusAttendance(SID, AID, 1);
                                            TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                                            textblock.Text = Connexion.GetTotalStuOutof(AID);
                                            int monthlypayment2 = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                                            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                                            {
                                                monthlypayment2 = Connexion.GetInt("Select Monthly from years where id = " + YearID);
                                            }
                                            if (monthlypayment2 == 1)
                                            {
                                                Methods.InsertStudentClassMonthly(SID, CID);
                                                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID) ";
                                            }
                                            else
                                            {
                                                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                                                {
                                                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                                                }
                                                else
                                                {
                                                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                                                }
                                                Commun.CheckDiscountAddClass(SID, this.Resources, 0, -1);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    string queryyy = "Select Students.FirstName + ' ' + Students.LastName as Name, Students.LastName + ' ' + Students.FirstName as RName, Students.Gender as Gender, " + querySession + " as Sessions, Students.ID as ID, Attendance_Student.Status as Status," +
                                      " Case " +
                                      "When Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' " +
                                      "When Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' " +
                                      "When Attendance_Student.Status = 2 Then N'" + this.Resources["GroupChange"].ToString() + "' " +
                                      "When Attendance_Student.Status = 3 Then N'" + this.Resources["Justified"].ToString() + "' END as StatusText, " +
                                      "case When Attendance_Student.Status = 3 then Justif.Reason end as Reason  " +
                                      "from Attendance join Attendance_Student on Attendance.ID = Attendance_Student.ID   " +
                                      "join Students on Students.ID = Attendance_Student.StudentID " +
                                      "left  join justif on (Justif.AID = Attendance.ID and Justif.SID = Students.ID)  " +
                                      "join Groups on Groups.GroupID = Attendance.GroupID where Attendance.ID = " + AID;
                                    DataTable copyTable = table.Copy();
                                    Connexion.FillDT(ref copyTable, queryyy);
                                    DataRow row = copyTable.Select($"{"ID"} = '{SID}'").FirstOrDefault(); ;

                                    DataRow newRow = copyTable.NewRow();
                                    newRow.ItemArray = row.ItemArray; // Copy all data from the current row
                                    copyTable.Rows.Remove(row);          // Remove the original row
                                    copyTable.Rows.InsertAt(newRow, 0);  // Insert the copied row at the top
                                    tablestoUpdate.Tables.Add(copyTable);

                                }
                            }
                            else
                            {
                                string name = Connexion.GetString("Select FirstName +' ' + LastName from Students Where id = " + SID);
                                if (red == 1)
                                {
                                    PopUps.RedMessagebox msg = new PopUps.RedMessagebox(name);
                                    msg.ShowDialog();
                                }
                                else
                                {
                                    MessageBox.Show(name);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (DataTable table in ds.Tables)
                        {
                            if (table.TableName.Substring(2, 1) == "A")
                            {
                                ListView LV = (ListView)FindName("LVA" + table.TableName);
                                string AID = table.TableName.Substring(3);
                                string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                                string CID = Connexion.GetClassID(GID).ToString();
                                string date = Connexion.GetString("Select Date from Attendance Where ID = " + AID);
                                string Name = "GBA" + AID;
                                GroupBox gb = (GroupBox)FindName(Name);
                                if (gb.Visibility == Visibility.Collapsed)
                                {
                                    continue;
                                }
                                DataTable copyTable = table.Copy();
                                string Condition = "1 > 0 ";
                                string searchText = Regex.Replace(TBSearch.Text, @"\s+", " ").Replace("'", "''");
                                // Construct the filter expression
                                Condition += $" and (Name LIKE '%{searchText}%' OR RName LIKE '%{searchText}%') ";
                                copyTable.DefaultView.RowFilter = Condition;
                                int rowCount = copyTable.DefaultView.Count;
                                if (rowCount == 0)
                                {
                                    continue;
                                }
                                if (rowCount > 1)
                                {
                                    if (MessageBox.Show("تم عرض عدة تلاميذ في البحث، هل تريد وضع علامة الحضور للجميع؟", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                    {
                                        return;
                                    }
                                }
                                string SID = "";
                                string names = "";
                                foreach (DataRowView rowView in copyTable.DefaultView)
                                {
                                    DataRow row = rowView.Row;
                                    SID = row["ID"].ToString();
                                    names += Connexion.GetString(SID, "Students", "FirstName") + "  " + Connexion.GetString(SID, "Students", "LastName") + ",";
                                    DataRow[] rows = table.Select($"{"ID"} = '{SID}'");
                                    Commun.SetStatusAttendanceupg(SID, AID,CID,GID,date, 1,ref rows);
                                    TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                                    textblock.Text = Connexion.GetTotalStuOutof(AID);
                                }
                                PopupText.Text = names;
                                MyPopup.IsOpen = true;
                            }
                        }

                    }
                    triggerEvent = 0;
                    TBSearch.Text = "";
                    TBSearch.Focus();
                    string Condition2 = "1 > 0 ";
                    foreach (DataTable table in ds.Tables)
                    {
                        table.DefaultView.RowFilter = Condition2;
                    }
                    triggerEvent = 1;
                    return;
                }

            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);

            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (triggerEvent == 1)
                {
                    string queryforexception;
                    DataSet CopyDS = new DataSet();
                    CopyDS = ds.Copy();

                    if (TBSearch.Text == "")
                    {
                        foreach (DataTable table in CopyDS.Tables)
                        {
                            if (table.TableName.Substring(2, 1) == "A")
                            {

                                ListView LV = (ListView)FindName("LVA" + table.TableName.Substring(3));
                                string AID = table.TableName.Substring(3);
                                DataTable copyTable = table.Copy(); // Create a copy of the DataTable
                                string GID = Connexion.GetString("Select GroupID from Attendance Where ID = " + AID);
                                string Name = "GBA" + AID;
                                GroupBox gb = (GroupBox)FindName(Name);
                                if (gb.Visibility == Visibility.Collapsed)
                                {
                                    continue;
                                }
                                ds.Tables[table.TableName].DefaultView.RowFilter = "1 > 0 ";
                                LV.ItemsSource = ds.Tables[table.TableName].DefaultView;
                            }
                            else if (table.TableName.Substring(2, 1) == "E")
                            {
                                DataGrid DG = (DataGrid)FindName("DGE" + table.TableName.Substring(3));
                                string AID = table.TableName.Substring(3);
                                DataTable copyTable = table.Copy(); // Create a copy of the DataTable

                                string Name = "GBE" + AID;
                                GroupBox gb = (GroupBox)FindName(Name);
                                if (gb.Visibility == Visibility.Collapsed)
                                {
                                    continue;
                                }
                                ds.Tables[table.TableName].DefaultView.RowFilter = "1 > 0 ";
                                DG.ItemsSource = ds.Tables[table.TableName].DefaultView;
                            }
                        }

                        return;
                    }
                    else if (TBSearch.Text.Last() == '$')
                    {


                    }
                    string text = TBSearch.Text;
                    bool isNumeric = int.TryParse(text, out _);

                    if (isNumeric)
                    {

                    }
                    else
                    {
                        string Condition = "1 > 0 ";
                        //   Condition += "And (Name Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%' OR RName Like '%" + Regex.Replace(CodebarTxt.Text, @"\s+", " ") + "%') ";
                        string searchText = Regex.Replace(TBSearch.Text, @"\s+", " ").Replace("'", "''");

                        // Construct the filter expression
                        Condition += $" and (Name LIKE '%{searchText}%' OR RName LIKE '%{searchText}%') ";
                        foreach (DataTable table in ds.Tables)
                        {
                            string type = "";
                            if (table.TableName.Substring(2, 1) == "A")
                            {
                                type = "A";
                            }
                            else if (table.TableName.Substring(2, 1) == "E")
                            {
                                type = "E";
                            }

                            string AID = table.TableName.Substring(3);
                            string Name = "GB" + type + AID;
                            GroupBox gb = (GroupBox)FindName(Name);
                            if (gb.Visibility == Visibility.Collapsed)
                            {
                                continue;
                            }
                            table.DefaultView.RowFilter = Condition;
                        }
                    }
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void CustomComboBox_SelectionChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                // Support both WPF ComboBox and DevExpress ComboBoxEdit
                var comboBox = sender as System.Windows.Controls.ComboBox;
                if (comboBox == null)
                {
                    // Try DevExpress ComboBoxEdit
                    var dxComboBox = sender as DevExpress.Xpf.Editors.ComboBoxEdit;
                    if (dxComboBox != null)
                    {
                        var selectedItem = dxComboBox.SelectedItem as DataRowView;
                        if (selectedItem != null)
                        {
                            DataRow selectedDataRow = selectedItem.Row;
                            selectedDataRow["Checked"] = selectedDataRow["Checked"].ToString() == "1" ? 0 : 1;
                            dxComboBox.SelectedItem = null;
                        }
                    }
                    return;
                }

                // WPF ComboBox logic
                DataRowView selectedItemWpf = comboBox.SelectedItem as DataRowView;
                if (selectedItemWpf != null)
                {
                    DataRow selectedDataRow = selectedItemWpf.Row;
                    selectedDataRow["Checked"] = selectedDataRow["Checked"].ToString() == "1" ? 0 : 1;
                    comboBox.SelectedItem = null;
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Row1.Height = new GridLength(150);
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Row1.Height = new GridLength(80);
        }

        private void MyPopup_Opened(object sender, EventArgs e)
        {
            closeTimer = new DispatcherTimer();
            closeTimer.Interval = TimeSpan.FromSeconds(1); // Set the timer interval to 1 second
            closeTimer.Tick += ClosePopup; // Hook up an event handler to run when the timer elapses
            closeTimer.Start(); // Start the timer
        }
        private void ClosePopup(object sender, EventArgs e)
        {
            // This method is called when the timer elapses (after 1 second)
            // Close your popup here
            MyPopup.IsOpen = false;

            // Stop the timer
            closeTimer.Stop();
        }

        private void printAttend(object sender, EventArgs e)
        {
            try
            {
                Report r = new Report();
                Button but = (Button)sender;
                string AID = but.Name.Substring(2);
                if (but.Name.Substring(1, 1) == "A")
                {
                    FastReports.PrintAttendance(AID);
                }
                else if (but.Name.Substring(1, 1) == "E")
                {
                    FastReports.PrintAttendance(AID, "2");
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }
        private void ViewAttend(object sender, EventArgs e)
        {
            try
            {
                Button but = (Button)sender;
                string AID = but.Name.Substring(2);
                if (but.Name.Substring(1, 1) == "A")
                {
                    var AddS = new AttendanceAdd(AID, "Show", "1");
                    AddS.ShowDialog();
                }
                else if (but.Name.Substring(1, 1) == "E")
                {
                    var AddS = new AttendanceAdd(AID, "Show", "3");
                    AddS.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Methods.ExceptionHandle(ex);
            }
        }

   

        private void DPTodayDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ALREADYOPENED)
            {
                DateTime selectedDate = DPTodayDate.SelectedDate.Value;
                string formattedDate = selectedDate.ToString("dd-MM-yyyy");
                filldata(formattedDate);
                fill(formattedDate);
            }  
        }

        public class ForegroundConverter : IValueConverter
        {
            string classid;
            public ForegroundConverter(string classId)
            {
                classid = classId;
            }
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {

                int monthlypaymentt = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + classid);
                    monthlypaymentt = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }
                if (monthlypaymentt == 1)
                {
                    if (value is int intValue)
                    {
                        return intValue >= 0 ? Brushes.Green : Brushes.Red;
                    }
                    return Brushes.Black; // Default color if value is not an integer
                }
                else
                {
                    if (value is int intValue)
                    {
                        return intValue < 0 ? Brushes.Red : Brushes.Green;
                    }
                    return Brushes.Black; // Default color if value is not an integer
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public void filldata(string date)
        {
           
            Connexion.FillDT(ref DtAttend, "Select *, case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName When MultipleGroups = 'Single' then Class.CName End as Name ,  Attendance.ID as AID , groups.GroupID as GID from Attendance join Groups on Groups.GroupID = Attendance.GroupID join Class on Class.ID = Groups.ClassID  Where Date = '" + date + "'");
            Connexion.FillDT(ref dtAttendanceUncreated, "Select DISTINCT   GroupID as GID ,CLassID as CID , Case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName When MultipleGroups = 'Single' then Class.CName End as Name from Class_Time  join  Groups on Groups.GroupID = Class_Time.GID join Class on Class.ID = Groups.ClassID where GroupID not in (Select GroupID from Attendance where Attendance.Date = '" + date + "') and(DATEPART(DW, Convert(datetime, '" + date + "', 105)) - 1 ) % 7 = Class_Time.Day");
            Connexion.FillDT(ref dtattendanceExtra, "Select *,Class.CName as Name ,Attendance_Extra.CID as CID,  Attendance_Extra.ID as AID  from Attendance_Extra  join Class on Class.ID = Attendance_Extra.CID  Where Date = '" + date + "'");
            FINISH = true;
      

        }
        public void fill(string date)
        {
            try
            {
                List<GroupBox> groupsToRemove = new List<GroupBox>();

                foreach (var child in SPAttendance.Children)
                {
                    if (child is GroupBox groupBox)
                    {
                        groupsToRemove.Add(groupBox);
                    }
                }

                // Iterate over the list of items to remove
                foreach (var groupBoxToRemove in groupsToRemove)
                {
                    SPAttendance.Children.Remove(groupBoxToRemove);

                    if (groupBoxToRemove.Content is StackPanel stackPanelforContent)
                    {

                        foreach (var ChildInGroupBox in stackPanelforContent.Children)
                        {
                            if (ChildInGroupBox is ListView LVDelete)
                            {
                                UnregisterName(LVDelete.Name);
                                LVDelete.Name = null;
                            }
                            else if (ChildInGroupBox is Button ButonCreate)
                            {

                                ButonCreate.Name = null;
                            }
                        }
                        UnregisterName(stackPanelforContent.Name);
                        stackPanelforContent.Name = null;
                    }
                    else
                    {
                        if (groupBoxToRemove.Content is ListView LVStudents)
                        {
                            UnregisterName(LVStudents.Name);

                        }
                    }

                    if (groupBoxToRemove.Header is StackPanel stackPanel)
                    {
                        foreach (var stackPanelChild in stackPanel.Children)
                        {
                            if (stackPanelChild is Button ButtonDelete)
                            {
                                UnregisterName(ButtonDelete.Name);
                                ButtonDelete.Name = null;
                            }
                        }
                        if (stackPanel.Name != "")
                        {
                            UnregisterName(stackPanel.Name);
                            stackPanel.Name = null;
                        }
                    }
                    UnregisterName(groupBoxToRemove.Name);
                    groupBoxToRemove.Name = null;
                }
                SPAttendance.Children.Clear();

                ds = new DataSet();
                dtAttendances = new DataTable();
                groupBoxInfoTable = new DataTable();


                groupBoxInfoTable.Columns.Add("Name", typeof(string));
                groupBoxInfoTable.Columns.Add("Text", typeof(string));
                groupBoxInfoTable.Columns.Add("type", typeof(string));
                dtAttendances.Columns.Add("ID", typeof(string));
                dtAttendances.Columns.Add("AID", typeof(string));
                dtAttendances.Columns.Add("Name", typeof(string));
                dtAttendances.Columns.Add("Checked", typeof(int));
                dtAttendances.Columns.Add("Created", typeof(int));
                dtAttendances.Columns.Add("Type", typeof(int));

                DataRow newRow = dtAttendances.NewRow();
                newRow["ID"] = "0";
                newRow["Name"] = this.Resources["SelectAll"].ToString();
                newRow["Checked"] = 0;
                dtAttendances.Rows.Add(newRow);
               

                foreach (DataRow dr in DtAttend.Rows)
                {
                    DataRow newRow2 = dtAttendances.NewRow();

                    // Populate the values for the three columns
                    newRow2["ID"] = dr["GID"].ToString();
                    newRow2["AID"] = dr["AID"].ToString();
                    newRow2["Name"] = dr["Name"].ToString();
                    newRow2["Checked"] = 0;
                    newRow2["Created"] = 0;
                    newRow2["Type"] = 1;
                    // Add the new row to the DataTable
                    dtAttendances.Rows.Add(newRow2);

                    // Add the TextBlock and DataGrid to the Border


                }
               
             
                foreach (DataRow drUncreated in dtAttendanceUncreated.Rows)
                {
                    GroupBox groupBox = new GroupBox
                    {
                        Header = new TextBlock
                        {
                            Text = drUncreated["Name"].ToString(),

                            FontSize = 20
                        },
                        Name = "GBC" + drUncreated["GID"].ToString(),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(5),
                        Width = 350,
                        Height = Commun.ScreenHeight - 220
                    };
                    StackPanel SP = new StackPanel()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Name = "SPC" + drUncreated["GID"].ToString(),
                        Children =
                    {
                        new TextBlock
                        {
                            Text = this.Resources["ThisAttendanceNotCreated"].ToString(),
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 20) // Add some space between the text and the button
                        },
                        new Button
                        {
                            Content = this.Resources["CreateAttendance"].ToString(),
                            Name = "BC" + drUncreated["GID"].ToString(),
                            FontSize = 16,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = 150,
                            Height = 30,
                        }
                    }
                    };
                    try
                    {
                        UnregisterName(SP.Name);
                    }
                    catch(Exception excep)
                    {

                    }
                    RegisterName(SP.Name, SP);
                    groupBox.Content = SP;
                    DataRow newRow3 = dtAttendances.NewRow();
                    newRow3["ID"] = drUncreated["GID"].ToString();
                    newRow3["Name"] = drUncreated["Name"].ToString();
                    newRow3["Checked"] = 1;
                    newRow3["Created"] = 0;
                    newRow3["Type"] = 3;
                    // Add the new row to the DataTable
                    dtAttendances.Rows.Add(newRow3);
                    try
                    {
                        UnregisterName(groupBox.Name);
                    }
                    catch (Exception excep)
                    {

                    }
                    RegisterName(groupBox.Name, groupBox);
                    var createAttendanceButton = (Button)((StackPanel)groupBox.Content).Children[1];
                    createAttendanceButton.Click += ButtonForCreate_Click;
                    SPAttendance.Children.Add(groupBox);
                }
               
                foreach (DataRow drExtra in dtattendanceExtra.Rows)
                {
                    DataRow newRow2 = dtAttendances.NewRow();

                    // Populate the values for the three columns
                    newRow2["ID"] = drExtra["CID"].ToString();
                    newRow2["AID"] = drExtra["AID"].ToString();
                    newRow2["Name"] = drExtra["Name"].ToString() + this.Resources["ExtraSession"].ToString();
                    newRow2["Checked"] = 0;
                    newRow2["Created"] = 0;
                    newRow2["Type"] = 2;
                    // Add the new row to the DataTable
                    dtAttendances.Rows.Add(newRow2);

                }
                FindGroupBoxNames(SPAttendance, 1);
                CustomComboBox.ItemsSource = dtAttendances.DefaultView;
                ALREADYOPENED = true;
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        public void CreateEmptyPage(string GroupID)
        {

        }

        public void CreateAttendance(string AttendanceID)
        {

        }
        private void ListStudents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ListView dataGrid = null;
            if (menuItem != null)
            {
                ContextMenu contextMenu = menuItem.Parent as ContextMenu;
                if (contextMenu != null)
                {
                    dataGrid = contextMenu.PlacementTarget as ListView;
                    // Now you can use dataGrid safely
                }
                else
                {
                    return;
                }
            }
            inactivityTimer.Stop();
            DataRowView row = (DataRowView)dataGrid.SelectedItem;
            if (row != null)
            {
                StudentAdd s = new StudentAdd("Show", row["ID"].ToString());
                s.ShowDialog();
            }
            inactivityTimer.Start();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {/*
                    foreach (DataTable table in ds.Tables)
                    {

                        DataGrid DG = (DataGrid)FindName("A" + table.TableName);
                        foreach (DataRowView selectedItemrow in DG.SelectedItems)
                        {
                            string SID = selectedItemrow["ID"].ToString();
                            int rowIndex = DG.Items.IndexOf(selectedItemrow);
                            string AID = table.TableName;
                            int SessionA = Connexion.GetInt("Select Session from Attendance Where ID = " + AID);
                            int GID = Connexion.GetInt("Select GroupID from Attendance Where ID = " + AID);
                            DataRow rowToUpdate = table.Rows[rowIndex]; // Replace rowIndex with the index of the row you want to update
                            int oldstatus = Connexion.GetInt("Select case when Status is null then 0 else Status end as sta from Attendance_Student Where StudentID = " + SID + " and ID  = " + AID);
                            if (oldstatus == 2)
                            {
                                if (MessageBox.Show(this.Resources["MessageBoxOverrideGroupChange"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    int ToGroupID = Connexion.GetInt("Select ToGroupID from Attendance_Change Where FromGroupID = " + GID + " and StudentID = " + SID + " and Session = " + SessionA);
                                    int ToAttendanceID = Connexion.GetInt("Select ID from Attendance Where GroupID = " + ToGroupID + " and Session = " + SessionA);
                                    Connexion.Insert("Delete from Attendance_Student Where ID=" + ToAttendanceID + " and StudentID = " + SID);
                                    Connexion.Insert("Delete From Attendance_Change Where FromGroupID = " + GID + " and StudentID = " + SID + " and Session = " + SessionA);
                                }
                                else
                                {
                                    break; //
                                }
                            }
                            if (oldstatus == 3)
                            {
                                if (MessageBox.Show(this.Resources["MessageBoxOverrideJustif"].ToString(), "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    Connexion.Insert("Delete from justif where SID = " + SID + " and AID = " + AID);
                                    DataRow[] rows1 = table.Select($"{"ID"} = '{SID}'");
                                    foreach (DataRow row in rows1)
                                    {
                                        row["Reason"] = "";
                                    }
                                }
                                else
                                {
                                    return; //
                                }
                            }
                            Commun.SetStatusAttendance(SID, AID, 1);
                            DataRow[] rows = table.Select($"{"ID"} = '{SID}'");
                            int CID = Connexion.GetClassID(GID.ToString());
                            foreach (DataRow row in rows)
                            {
                                // Edit the values in the target column(s)
                                row["StatusText"] = this.Resources["Present"].ToString();
                                row["Status"] = 1;
                                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                                {
                                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                                }
                                if (monthlypayment != 1)
                                {
                                    if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                                    {
                                        row["Sessions"] = Connexion.GetInt("Select dbo.CalcPriceSum(" + SID + "," + CID + ") as Sessions");
                                    }
                                    else
                                    {
                                        row["Sessions"] = Connexion.GetInt("Select dbo.GettotalPayStudent(Students.ID," + CID + ") - dbo.CalculatePrice(Students.ID, " + GID + "," + Connexion.GetString("Select Tsessions from Groups where GroupID =  " + GID) + ", 'Su') as Sessions from Students Where ID =  " + SID);
                                    }
                                }
                            }
                        }

                    
                    }*/
                }
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnExtraSes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = $"جلسة إضافية";
                DataTable dt = new DataTable();
                Connexion.FillDT(ref dt, "Select ID,CName from Class ");
                OptionPanels.ComboBoxPopup popup = new OptionPanels.ComboBoxPopup(dt, message, "CName", "ID");
                bool? dialogResult = popup.ShowDialog();
                ;
                if (dialogResult != true)
                {
                    return;
                }
                string result = popup.Result;
                DateTime selectedDate = DPTodayDate.SelectedDate.Value;
                string formattedDate = selectedDate.ToString("dd-MM-yyyy");
                string AID = Connexion.GetInt("Insert into Attendance_Extra (CID,Date) OUTPUT Inserted.ID  Values(" + result + ",'" + formattedDate + "')").ToString();
                CreateExtraAttend(AID);
                DataRow dr = dtAttendances.NewRow();
                dr["ID"] = result;
                dr["AID"] = AID;
                dr["Name"] = Connexion.GetString("Select CName from Class Where ID = " + result) + this.Resources["ExtraSession"].ToString();
                dr["Checked"] = 1;
                dr["Created"] = 1;
                dr["Type"] = 2;
                // Add the new row to the DataTable
                dtAttendances.Rows.Add(dr);
                groupBoxInfoTable.Rows.Add(AID, Connexion.GetString("Select CName from Class Where ID = " + result) + this.Resources["ExtraSession"].ToString(), "E");
            }
            catch (Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (DataRow row in dtAttendances.Rows)
                {
                    if (row["Type"].ToString() == "1")
                    {
                        if (row["Created"].ToString() == "1")
                        {
                            string AID = row["AID"].ToString();
                            DataTable foundTable = ds.Tables["DTA" + AID];
                            string query = "";
                            string GID = Connexion.GetString("Select groupid from attendance where ID =" + AID);
                            int CID = Connexion.GetClassID(GID);
                            int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                            if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                            {
                                int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                                monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                            }

                            if (monthlypayment == 1)
                            {
                                string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                                DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                                int monthNumber = dateAttendance.Month;
                                int year = dateAttendance.Year;
                                querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
                            }
                            else
                            {
                                if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                                {
                                    querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                                }
                                else
                                {
                                    querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                                }
                            }

                            query = @"
                SELECT 
                    SubQuery.Name,
                    SubQuery.RName,
                    SubQuery.Gender,
                    SubQuery.Sessions,
                    SubQuery.ID,
                    SubQuery.Status,
                    SubQuery.StatusText,
                    SubQuery.Reason,
                    CASE 
                        WHEN SubQuery.Sessions < 0 THEN N'" + Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png' 
                        ELSE '' 
                    END AS paidimg
                FROM 
                    (
                        SELECT 
                            Students.FirstName + ' ' + Students.LastName AS Name,
                            Students.LastName + ' ' + Students.FirstName AS RName,
                            Students.Gender AS Gender,
                            " + querySession + @" AS Sessions,
                            Students.ID AS ID,
                            Attendance_Student.Status AS Status,
                            CASE 
                                WHEN Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 2 THEN N'" + this.Resources["GroupChange"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 3 THEN N'" + this.Resources["Justified"].ToString() + @"' 
                            END AS StatusText,
                            CASE 
                                WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                            END AS Reason
                        FROM 
                            Attendance 
                        JOIN 
                            Attendance_Student ON Attendance.ID = Attendance_Student.ID
                        JOIN 
                            Students ON Students.ID = Attendance_Student.StudentID
                        LEFT JOIN 
                            Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                        JOIN 
                            Groups ON Groups.GroupID = Attendance.GroupID
                        WHERE 
                            Attendance.ID = " + AID + @"
                    ) AS SubQuery;
                ";
                            Connexion.FillDT(ref foundTable, query);
                            ds.Tables.Remove("DTA" + AID);
                            string Name = "LVA" + AID;
                            ListView LV = (ListView)FindName(Name);
                            LV.ItemsSource = foundTable.DefaultView;
                            ds.Tables.Add(foundTable);
                            TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                            textblock.Text = Connexion.GetTotalStuOutof(AID);
                        }
                    }
                    else if (row["Type"].ToString() == "2")
                    {
                        if (row["Created"].ToString() == "1")
                        {
                            string AID = row["AID"].ToString();
                            DataTable foundTable = ds.Tables["DTE" + AID];
                            string query = "Select Students.ID as ID,Students.FirstName + ' ' + Students.LastName as Name , Students.LastName + ' ' + Students.FirstName As RName , Students.Barcode as Barcode , Attendance_Extra_students.Status as Status  ,  Case When Attendance_Extra_students.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + "' When Attendance_Extra_students.Status = 1 THEN N'" + this.Resources["Present"].ToString() + "' end as StatusText , Attendance_Extra_students.Price as Sessions From Attendance_Extra_Students join Students on Attendance_Extra_Students.SID =  Students.ID Where Attendance_Extra_Students.ID = " + AID;

                            Connexion.FillDT(ref foundTable, query);
                            ds.Tables.Remove("DTE" + AID);
                            string Name = "DGE" + AID;
                            DataGrid dg = (DataGrid)FindName(Name);
                            dg.ItemsSource = foundTable.DefaultView;
                            ds.Tables.Add(foundTable);

                        }
                    }
                    else if (row["Type"].ToString() == "3")
                    {
                        string GID = row["ID"].ToString();
                        bool checkexist = Connexion.IFNULL("Select * from Attendance where GroupID  = " + row["ID"].ToString() + " and date = '" + DPTodayDate.Text.ToString().Replace('/', '-') + "'");
                        if (!checkexist)
                        {
                            string stackPanelName = "SPC" + GID;
                            GroupBox groupBox = FindName("GBC" + GID) as GroupBox;

                            if (groupBox != null)
                            {
                                StackPanel stackPanel = groupBox.FindName(stackPanelName) as StackPanel;
                                if (stackPanel != null)
                                {
                                    stackPanel.Children.Clear();
                                    stackPanel.VerticalAlignment = VerticalAlignment.Top;
                                    SPAttendance.Children.Remove(groupBox);
                                    UnregisterName(groupBox.Name);
                                    string AID = Connexion.GetString("Select * from Attendance where GroupID  = " + row["ID"].ToString() + " and date = '" + DPTodayDate.Text.ToString().Replace('/', '-'));
                                    CreateAttend(AID);
                                    row["Created"] = "1";
                                    row["AID"] = AID;
                                    row["Type"] = "1";


                                }
                            }
                        }
                    }

                }
                DataTable dtallattend = new DataTable();
                Connexion.FillDT(ref dtallattend, "Select ID from Attendance Where Date = '" + DPTodayDate.Text.ToString().Replace('/', '-') + "'");
                foreach (DataRow rows in dtallattend.Rows)
                {
                    string AID = rows["ID"].ToString();
                    var rowExists = dtAttendances.AsEnumerable().Any
                        (row => row["AID"] != DBNull.Value && row["Type"] != DBNull.Value &&
                                             row["AID"].ToString() == AID && row["Type"].ToString() == "1");
                    if (!rowExists)
                    {
                        DataRow dr = dtAttendances.NewRow();
                        string GID = Connexion.GetString("Select GroupID from Attendance Where ID= " + AID);
                        string CID = Connexion.GetString("Select ClassID from Groups Where GroupID = " + GID);
                        string name = Connexion.GetString("Select case when MultipleGroups = 'Multiple' then Class.CName + ' ' + Groups.GroupName When MultipleGroups = 'Single' then Class.CName End as Name from Class Join Groups on Groups.ClassID = Class.ID where GroupID = " + GID);
                        dr["ID"] = Connexion.GetString("Select GroupID from Attendance Where ID= " + AID);
                        dr["AID"] = AID;
                        dr["Name"] = name;
                        dr["Checked"] = 1;
                        dr["Created"] = 1;
                        dr["Type"] = 1;
                        CreateAttend(AID.ToString());
                        dtAttendances.Rows.Add(dr);
                        groupBoxInfoTable.Rows.Add(AID, Name, "A");

                    }
                }
                DataTable dtallattendextra = new DataTable();
                Connexion.FillDT(ref dtallattendextra, "Select ID from Attendance_Extra Where Date = '" + DPTodayDate.Text.ToString().Replace('/', '-') + "'");
                foreach (DataRow rows in dtallattendextra.Rows)
                {
                    string AID = rows["ID"].ToString();
                    var rowExists = dtAttendances.AsEnumerable().Any
                      (row => row["AID"] != DBNull.Value && row["Type"] != DBNull.Value &&
                                           row["AID"].ToString() == AID && row["Type"].ToString() == "2");
                    if (!rowExists)
                    {
                        DataRow dr = dtAttendances.NewRow();
                        string CID = Connexion.GetString("Select CID from Attendance_Extra where ID = " + AID);
                        dr["ID"] = CID;
                        dr["AID"] = AID;
                        dr["Name"] = Connexion.GetString("Select CName from Class Where ID = " + CID) + this.Resources["ExtraSession"].ToString();
                        dr["Checked"] = 1;
                        dr["Created"] = 1;
                        dr["Type"] = 2;
                        CreateExtraAttend(AID.ToString());
                        dtAttendances.Rows.Add(dr);
                        groupBoxInfoTable.Rows.Add(AID, Connexion.GetString("Select CName from Class Where ID = " + CID) + this.Resources["ExtraSession"].ToString(), "E");
                    }
                }
                MessageBox.Show(this.Resources["RefreshDone"].ToString());
            }
            catch(Exception er)
            {
                Methods.ExceptionHandle(er);
            }
        }
        public class UnpaidToBrushConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                int sessions = 0;
                if (value != null && int.TryParse(value.ToString(), out sessions))
                {
                    return sessions < 0 ? Brushes.Red : Brushes.Transparent;
                }
                return Brushes.Transparent;
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        private void Button_Click_ChangeStudents(object sender, RoutedEventArgs e)
        {
            Panels.ChangeGroup changegroup = new Panels.ChangeGroup("", "1", ref groupBoxInfoTable);
            changegroup.ShowDialog();
            if (changegroup.DialogResult == true)
            {
                string AID = changegroup.AID;

                DataTable foundTable = ds.Tables["DTA" + AID];
                string query = "";
                string GID = Connexion.GetString("Select groupid from attendance where ID =" + AID);
                int CID = Connexion.GetClassID(GID);
                int monthlypayment = Connexion.GetInt("Select PaymentMonth from EcoleSetting");
                if (Connexion.GetInt("Select PaymentMonth from EcoleSetting") == 2)
                {
                    int YID = Connexion.GetInt("Select CYear from Class Where ID = " + CID);
                    monthlypayment = Connexion.GetInt("Select Monthly from years where id = " + YID);
                }

                if (monthlypayment == 1)
                {
                    string dateAttendanceString = Connexion.GetString(AID, "Attendance", "Date");
                    DateTime dateAttendance = DateTime.Parse(dateAttendanceString);
                    int monthNumber = dateAttendance.Month;
                    int year = dateAttendance.Year;
                    querySession = "dbo.CalculateMonthlyPaymentRemaining(Students.ID ) ";
                }
                else
                {
                    if (Connexion.GetInt("Select CalcPrice from EcoleSetting") == 1)
                    {
                        querySession = "dbo.CalcPriceSum(Students.ID,Groups.ClassID)";
                    }
                    else
                    {
                        querySession = "dbo.GettotalPayStudent(Students.ID , Groups.ClassID) - dbo.CalculatePrice(Students.ID,Groups.GroupID, Groups.TSessions,'Su')";
                    }
                }

                query = @"
                SELECT 
                    SubQuery.Name,
                    SubQuery.RName,
                    SubQuery.Gender,
                    SubQuery.Sessions,
                    SubQuery.ID,
                    SubQuery.Status,
                    SubQuery.StatusText,
                    SubQuery.Reason,
                    CASE 
                        WHEN SubQuery.Sessions < 0 THEN N'" + Directory.GetCurrentDirectory() + @"\Images\reddotcontent.png' 
                        ELSE '' 
                    END AS paidimg
                FROM 
                    (
                        SELECT 
                            Students.FirstName + ' ' + Students.LastName AS Name,
                            Students.LastName + ' ' + Students.FirstName AS RName,
                            Students.Gender AS Gender,
                            " + querySession + @" AS Sessions,
                            Students.ID AS ID,
                            Attendance_Student.Status AS Status,
                            CASE 
                                WHEN Attendance_Student.Status = 0 THEN N'" + this.Resources["Absent"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 1 THEN N'" + this.Resources["Present"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 2 THEN N'" + this.Resources["GroupChange"].ToString() + @"' 
                                WHEN Attendance_Student.Status = 3 THEN N'" + this.Resources["Justified"].ToString() + @"' 
                            END AS StatusText,
                            CASE 
                                WHEN Attendance_Student.Status = 3 THEN Justif.Reason 
                            END AS Reason
                        FROM 
                            Attendance 
                        JOIN 
                            Attendance_Student ON Attendance.ID = Attendance_Student.ID
                        JOIN 
                            Students ON Students.ID = Attendance_Student.StudentID
                        LEFT JOIN 
                            Justif ON (Justif.AID = Attendance.ID AND Justif.SID = Students.ID)
                        JOIN 
                            Groups ON Groups.GroupID = Attendance.GroupID
                        WHERE 
                            Attendance.ID = " + AID + @"
                    ) AS SubQuery;
                ";
                Connexion.FillDT(ref foundTable, query);
                ds.Tables.Remove("DTA" + AID);
                string Name = "LVA" + AID;
                ListView LV = (ListView)FindName(Name);
                LV.ItemsSource = foundTable.DefaultView;
                ds.Tables.Add(foundTable);
                TextBlock textblock = FindName("LBA" + AID) as TextBlock;
                textblock.Text = Connexion.GetTotalStuOutof(AID);

            }
        }
    }
}
