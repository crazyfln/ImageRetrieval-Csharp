using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;  
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Windows.Media.Animation;
using System.Data;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : System.Windows.Window
    {
        public MyView view;
        public Model model;
        public Controller controller;

        private BflyObject Q;
        private Q_possibility_object[] Q_possibilities;
        private D_small_display_object[] D_img_objects;
        private Image[] img;
        private int D_object_enlarged;
        private int num_of_D_images;
        private int ID;
        private int currentX, currentY;
        private Image[] Q_img;

        #region CurrentMidIndexProperty
        public static readonly DependencyProperty CurrentMidIndexProperty = DependencyProperty.Register(
            "CurrentMidIndex", typeof(double), typeof(Window1),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(CurrentMidIndexPropertyChangedCallback)));

        private static void CurrentMidIndexPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Window1 win = sender as Window1;
            if (win != null)
                win.ReLayoutInteractiveVisual3D();
        }

        public double CurrentMidIndex
        {
            get
            {
                return (double)this.GetValue(CurrentMidIndexProperty);
            }
            set
            {
                this.SetValue(CurrentMidIndexProperty, value);
                Q.pid = Q_possibilities[(int)value].pid;
                this.controller.parse_meta(ref Q);
                //Q.photo_top.Remove(0, Q.photo_top.Length);
                //Q.photo_top.Append(Q_possibilities[value].img_path);
                this.change_Q_display();
            }
        }
        #endregion 
        
        #region ModelAngleProperty
        public static readonly DependencyProperty ModelAngleProperty = DependencyProperty.Register(
            "ModelAngle", typeof(double), typeof(Window1),
            new FrameworkPropertyMetadata(70.0, new PropertyChangedCallback(ModelAnglePropertyChangedCallback)));

        public static void ModelAnglePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Window1 win = sender as Window1;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double ModelAngle
        {
            get
            {
                return (double)this.GetValue(ModelAngleProperty);
            }
            set
            {
                this.SetValue(ModelAngleProperty, value);
            }
        }
           
        #endregion

        #region XDistanceBetweenModelsProperty
        public static readonly DependencyProperty XDistanceBetweenModelsProperty = DependencyProperty.Register(
            "XDistanceBetweenModels", typeof(double), typeof(Window1),
            new FrameworkPropertyMetadata(0.5, XDistanceBetweenModelsPropertyChangedCallback));

        private static void XDistanceBetweenModelsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Window1 win = sender as Window1;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        /// <summary>
        /// »ñÈ¡»òÉèÖÃX·½ÏòÉÏÁ½¸öÄ£ÐÍ¼äµÄ¾àÀë
        /// </summary>
        public double XDistanceBetweenModels
        {
            get
            {
                return (double)this.GetValue(XDistanceBetweenModelsProperty);
            }
            set
            {
                this.SetValue(XDistanceBetweenModelsProperty, value);
            }
        }
        #endregion

        #region ZDistanceBetweenModelsProperty
        public static readonly DependencyProperty ZDistanceBetweenModelsProperty = DependencyProperty.Register(
            "ZDistanceBetweenModels", typeof(double), typeof(Window1),
            new FrameworkPropertyMetadata(0.5, ZDistanceBetweenModelsPropertyChangedCallback));

        private static void ZDistanceBetweenModelsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Window1 win = sender as Window1;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        /// <summary>
        /// »ñÈ¡»òÉèÖÃZ·½ÏòÉÏÁ½¸öÄ£ÐÍ¼äµÄ¾àÀë
        /// </summary>
        public double ZDistanceBetweenModels
        {
            get
            {
                return (double)this.GetValue(ZDistanceBetweenModelsProperty);
            }
            set
            {
                this.SetValue(ZDistanceBetweenModelsProperty, value);
            }
        }
        #endregion

        #region MidModelDistanceProperty
        public static readonly DependencyProperty MidModelDistanceProperty = DependencyProperty.Register(
            "MidModelDistance", typeof(double), typeof(Window1),
            new FrameworkPropertyMetadata(1.5, MidModelDistancePropertyChangedCallback));
        
        private static void MidModelDistancePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            Window1 win = sender as Window1;
            if (win != null)
            {
                win.ReLayoutInteractiveVisual3D();
            }
        }

        public double MidModelDistance
        {
            get
            {
                return (double)this.GetValue(MidModelDistanceProperty);
            }
            set
            {
                this.SetValue(MidModelDistanceProperty, value);
            }
        }
        #endregion


        public Window1()
        {
            InitializeComponent();
            InitComponent();

            view = new MyView(this);
            model = new Model();
            controller = new Controller();
            setup_Q_objects();
            setup_D_objects();
            Q = new BflyObject();
            
            this.generate_queries();
            
        }

        private void InitComponent()
        {
            //this.LoadImageToViewport3D(this.GetUserImages());

            //test
           image1.MouseLeftButtonDown += new MouseButtonEventHandler(image1_MouseLeftButtonDown);
           this.grid1.MouseDown += new MouseButtonEventHandler(Grid1_MouseDown);
           image2.MouseLeftButtonDown += new MouseButtonEventHandler(image2_MouseLeftButtonDown);
           Q_img = new Image[Constants.NUM_Q_POSSIBILITIES];
           for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
               Q_img[i] = new Image();
        }

        void Grid1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.CurrentMidIndex++;
            }
            else
            {
                this.CurrentMidIndex--;
            }
        }

        private void LoadImageToViewport3D(List<string> images)
        {
            if (images == null)
            {
                return;
            }

            for (int i = 0; i < images.Count; i++)
            {
                string imageFile = images[i];

                InteractiveVisual3D iv3d = this.CreateInteractiveVisual3D(imageFile, i);

                this.viewport3D.Children.Add(iv3d);
            }

            this.ReLayoutInteractiveVisual3D();
        }

        private List<string> GetUserImages( Q_possibility_object[] Q_possibilities )
        {
            List<string> images = new List<string>();

            //string path = "C:\\ucr_db\\ucr_img_db";
            //string path = "C:\\Users\\Owner\\Pictures";
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            //DirectoryInfo dir = new DirectoryInfo(path);
            //FileInfo[] files = dir.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
            if (Q_possibilities != null)
            {
                foreach (Q_possibility_object file in Q_possibilities)
                {
                    images.Add(file.img_path);
                }
            }
            int a = images.Count;
            return images;
        }

        private Visual CreateVisual(string imageFile, int index)
        {
            BitmapImage bmp = null;

            try
            {
                bmp = new BitmapImage(new Uri(imageFile));
            }
            catch
            {
            }

            //Image img = new Image();
            
            Q_img[index].Source = bmp;
            Q_img[index].Width = bmp.Width;
            Q_img[index].Height = bmp.Height;

            Border outBordre = new Border();
            outBordre.BorderBrush = Brushes.White;
            outBordre.BorderThickness = new Thickness(0.5);
            outBordre.Child = Q_img[index];

            outBordre.MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                this.CurrentMidIndex = index;
                e.Handled = true;
            };

            return outBordre;

        }
        
        private Geometry3D CreateGeometry3D()
        {
            MeshGeometry3D geometry = new MeshGeometry3D();

            geometry.Positions = new Point3DCollection();
            geometry.Positions.Add(new Point3D(-1, 1, 0));
            geometry.Positions.Add(new Point3D(-1, -1, 0));
            geometry.Positions.Add(new Point3D(1, -1, 0));
            geometry.Positions.Add(new Point3D(1, 1, 0));

            geometry.TriangleIndices = new Int32Collection();
            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(1);
            geometry.TriangleIndices.Add(2);
            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(2);
            geometry.TriangleIndices.Add(3);

            geometry.TextureCoordinates = new PointCollection();
            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));
            geometry.TextureCoordinates.Add(new Point(1, 1));
            geometry.TextureCoordinates.Add(new Point(1, 0));

            return geometry;
        }

        private InteractiveVisual3D CreateInteractiveVisual3D(string imageFile, int index)
        {
            InteractiveVisual3D iv3d = new InteractiveVisual3D();
            iv3d.Visual = this.CreateVisual(imageFile, index);
            iv3d.Geometry = this.CreateGeometry3D();
            iv3d.Transform = this.CreateEmptyTransform3DGroup();

            return iv3d;
        }

        private Transform3DGroup CreateEmptyTransform3DGroup()
        {
            Transform3DGroup group = new Transform3DGroup();
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)));
            group.Children.Add(new TranslateTransform3D(new Vector3D()));
            group.Children.Add(new ScaleTransform3D());

            return group;
        }

        private void GetTransformOfInteractiveVisual3D(int index, double midIndex, out double angle, out double offsetX, out double offsetZ)
        {
            double disToMidIndex = index - midIndex;

            angle = 0;
            if (disToMidIndex < 0)
            {
                angle = this.ModelAngle;
            }
            else if (disToMidIndex > 0)
            {
                angle = (-this.ModelAngle);
            }

            offsetX = 0;
            if (Math.Abs(disToMidIndex) <= 1)
            {
                offsetX = disToMidIndex * this.MidModelDistance;
            }
            else if (disToMidIndex != 0)
            {
                offsetX = disToMidIndex * this.XDistanceBetweenModels + (disToMidIndex > 0 ? this.MidModelDistance : -this.MidModelDistance);
            }

            offsetZ = Math.Abs(disToMidIndex) * -this.ZDistanceBetweenModels;
        }

        private void ReLayoutInteractiveVisual3D()
        {
            int j = 0;
            for (int i = 0; i < this.viewport3D.Children.Count; i++)
            {
                InteractiveVisual3D iv3d = this.viewport3D.Children[i] as InteractiveVisual3D;
                if (iv3d != null)
                {
                    double angle = 0;
                    double offsetX = 0;
                    double offsetZ = 0;
                    this.GetTransformOfInteractiveVisual3D(j++, this.CurrentMidIndex, out angle, out offsetX, out offsetZ);
                    
                    NameScope.SetNameScope(this, new NameScope());
                    this.RegisterName("iv3d", iv3d);
                    Duration time = new Duration(TimeSpan.FromSeconds(0.3));

                    DoubleAnimation angleAnimation = new DoubleAnimation(angle, time);
                    DoubleAnimation xAnimation = new DoubleAnimation(offsetX, time);
                    DoubleAnimation zAnimation = new DoubleAnimation(offsetZ, time);
                   
                    Storyboard story = new Storyboard();
                    story.Children.Add(angleAnimation);
                    story.Children.Add(xAnimation);
                    story.Children.Add(zAnimation);

                    Storyboard.SetTargetName(angleAnimation, "iv3d");
                    Storyboard.SetTargetName(xAnimation, "iv3d");
                    Storyboard.SetTargetName(zAnimation, "iv3d");

                    Storyboard.SetTargetProperty(
                        angleAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[0].(RotateTransform3D.Rotation).(AxisAngleRotation3D.Angle)"));

                    Storyboard.SetTargetProperty(
                        xAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[1].(TranslateTransform3D.OffsetX)"));
                    Storyboard.SetTargetProperty(
                        zAnimation,
                        new PropertyPath("(ModelVisual3D.Transform).(Transform3DGroup.Children)[1].(TranslateTransform3D.OffsetZ)"));

                    story.Begin(this);
                }
            }
        }

        private void generate_queries()
        {
            int[] num_specimens = new int[Constants.NUM_Q_POSSIBILITIES];
            DataTable table = controller.generate_queries(ref num_specimens);
            this.update_Q_displays(table, num_specimens);
            this.LoadImageToViewport3D(this.GetUserImages(Q_possibilities));
            Q.pid = Q_possibilities[0].pid;
            this.new_query();

        }

        private bool update_Q_displays(DataTable table, int[] num_specimens)
        {
            StringBuilder temp;
            try
            {
                for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
                {
                    temp = new StringBuilder("");
                    temp.Append(Constants.SMALL_IMG_FOLDER);
                    temp.Append(table.Rows[i]["photo_top"].ToString());
                    temp.Append(".jpg");
                    Q_possibilities[i].img_path = temp.ToString();

                    temp = new StringBuilder("");
                    temp.Append("Group ");
                    temp.Append((int)table.Rows[i]["group_index"]);
                    temp.Append("\n");
                    temp.Append(num_specimens[i]);
                    temp.Append(" images");
                    Q_possibilities[i].label = temp.ToString();

                    Q_possibilities[i].pid = (int)table.Rows[i]["pid"];
                }
            }
            catch (Exception e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string);
            }

            return true;
        }

        private bool update_D_displays(DataTable table)
        {
            StringBuilder temp;
            StringBuilder temp1;
            try
            {
                for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
                {
                    temp = new StringBuilder("");
                    temp.Append(Constants.LARGE_IMG_FOLDER);
                    temp.Append(table.Rows[i]["photo_top"].ToString());
                    temp.Append(".jpg");
                    D_img_objects[i].img_path = temp.ToString();
                    if ((int)table.Rows[i]["group_index"] == Q.group_index)
                        D_img_objects[i].label_color = "Green";
                    else
                        D_img_objects[i].label_color = "Black";
                    
                    temp1 = new StringBuilder("");
                    temp1.Append("Group ");
                    temp1.Append((int)table.Rows[i]["group_index"]);
                    temp1.Append("\r\n pid:");
                    temp1.Append((int)table.Rows[i]["pid"]);
                    D_img_objects[i].label = temp1.ToString();
                    try
                    {
                        D_img_objects[i].parse_metadata(table.Rows[i]);
                    }
                    catch (Exception e)
                    {
                        string e_string = e.ToString();
                    }
                    D_img_objects[i].rf_neg = false;
                    D_img_objects[i].rf_pos = false;
                }
            }
            catch (Exception e)
            {
                string e_string = e.ToString();
                MessageBox.Show(e_string);
            }

            return true;
        }

        private void setup_D_objects()
        {
            D_img_objects = new D_small_display_object[Constants.NUM_D_RESULTS];
            for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
            {
                D_img_objects[i] = new D_small_display_object();
            }
        }

        private void setup_Q_objects()
        {
            Q_possibilities = new Q_possibility_object[Constants.NUM_Q_POSSIBILITIES];
            for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
            {
                Q_possibilities[i] = new Q_possibility_object();
            }

        }

        private void new_query()
        {
            DataTable table = controller.new_query(Q);
            change_Q_display();
            update_D_displays(table);
            CreateImages(6, 2);
            change_large_D_img(0);
            parse_all_D_features();
            this.currentX = 6;
            this.currentY = 2;
        }

        private void change_Q_display()
        {
            BitmapImage bmp = null;

            try
            {
                bmp = new BitmapImage(new Uri(Q.photo_top.ToString()));
            }
            catch
            {
            }
            image1.Source = bmp;
            //image1.Width = bmp.Width;
            //image1.Height = bmp.Height;
            textBox1.Text = Q.print_info();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            CreateImages(6, 2);
            this.currentX = 6;
            this.currentY = 2;
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            CreateImages(10, 3);
            this.currentX = 10;
            this.currentY = 3;
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            CreateImages(15, 4);
            this.currentX = 15;
            this.currentY = 4;
        }
        private void CreateImages(int x, int y)
        {
            num_of_D_images = x*y;
            canvas1.Children.Clear();
            double width = (this.canvas1.Width - (x + 1) * 5) / x;
            double height = (this.canvas1.Height - (y + 1) * 5) / y;
            img = new Image[x*y];
            BitmapImage bit = new BitmapImage(new Uri(@"Resources\check_mark.png", UriKind.Relative));
            for (int i = 0; i < x * y; i++)
            {
                img[i] = new Image();
                img[i].Source = bit;
                img[i].Visibility = Visibility.Hidden;
                img[i].Width = width;
                img[i].Height = height;
            }
            double w = img[0].Width;
            double h = img[0].Height;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    ImageBrush brush = new ImageBrush( new BitmapImage(new Uri(D_img_objects[i+x*j].img_path)));
                    brush.Stretch = Stretch.Uniform;
                    Button img_btn = new Button(); 
                    try
                    {
                        img_btn.Width = width;
                        img_btn.Height = height;
                        img_btn.Focusable = false;
                        img_btn.Name = String.Format("btn{0}", (i + x * j) );
                    }
                    catch (Exception e)
                    {
                        string e_string = e.ToString();
                    }
                    img_btn.Background = brush;
                
                    //try
                    //{
                    //    img[i + x * j].Source = new BitmapImage(new Uri(@"Resources\glossy_ecommerce_026.png", UriKind.Relative));
                    //    img[i + x * j].Visibility = Visibility.Hidden;
                    //}
                    //catch (Exception e)
                    //{
                    //    string e_string = e.ToString();
                    //}
                    Canvas.SetTop(img[i + x * j], j * height + 5);
                    Canvas.SetLeft(img[i + x * j], i * width + 5);
                    Canvas.SetTop( img_btn, j * height + 5 );
                    Canvas.SetLeft( img_btn, i * width + 5 );
                    canvas1.Children.Add(img_btn);
                    //canvas1.Children.Add(img[i + 6 * j]);
                }
            }
           
            for (int i = 0; i < x * y; i++)
            {
                canvas1.Children.Add(img[i]);
                ((Button)canvas1.Children[i]).Click += new System.Windows.RoutedEventHandler(this.D_small_MouseLeftButtonDown);
            }

        }

        private void image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.new_query();
        }

        private void image2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Q.pid = D_img_objects[ID].pid;
            this.new_query();
            change_Q_display();
            image2.Source = new BitmapImage(new Uri(@"Resources\2.png", UriKind.Relative));
        }

        private void D_small_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            char[] chars = btn.Name.ToCharArray();
            
            Console.WriteLine("{0}", btn.Name[3]);
            int btnName;
            string num = null;
            foreach (char item in btn.Name)
            {
                if (item >= 48 && item <= 58)
                {
                    num += item;
                }
            }
            btnName = int.Parse(num);
            D_object_enlarged = btnName;
            change_large_D_img(btnName);
            if( img[btnName].Visibility == Visibility.Hidden )
            {
                img[btnName].Visibility = Visibility.Visible;
                D_img_objects[btnName].rf_pos = true;
                D_img_objects[btnName].rf_neg = false;
                //Canvas.SetZIndex(img[btnName], 10);
                //Canvas.SetZIndex(((Button)canvas1.Children[btnName]), 0);
            }
            else if (img[btnName].Visibility == Visibility.Visible)
            {
                img[btnName].Visibility = Visibility.Hidden;
                D_img_objects[btnName].rf_pos = false;
                D_img_objects[btnName].rf_neg = true;
            }          
        }

        private void change_large_D_img( int id )
        {
            ID = id;
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage(new Uri(D_img_objects[id].photo_top.ToString()));
            }
            catch
            {
            }
            image2.Source = bmp;
            //image1.Width = bmp.Width;
            //image1.Height = bmp.Height;
            if (D_img_objects[id].label_color == "Green")
                textBox2.Foreground = Brushes.Green;
            else
                textBox2.Foreground = Brushes.Black;
            textBox2.Text = D_img_objects[id].print_info();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.relevance_switch();
            this.process_RF();
        }

        private void relevance_switch()
        {
            for (int i = 0; i < num_of_D_images; i++)
            {
                this.img[i].Visibility = Visibility.Hidden;
            }
        }

        private void process_RF()
        {
            int neg = 0;
            D_small_display_object[] d_img_objects = new D_small_display_object[currentX * currentY];
            for (int i = 0; i < currentX*currentY; i++)
            {
                if (!D_img_objects[i].rf_pos)
                {
                    D_img_objects[i].rf_neg = true;
                    neg++;
                }
                d_img_objects[i] = new D_small_display_object();
                d_img_objects[i] = D_img_objects[i];
            }
            
       
            DataTable table = controller.process_RF(Q, d_img_objects);
            textBox1.Text = Q.print_info();

            update_D_displays(table);
            change_large_D_img(0);
            parse_all_D_features();
            CreateImages(currentX, currentY);
            //for (int i = 0; i < currentX * currentY/*Constants.NUM_D_RESULTS*/; i++)
            //{
            //    BitmapImage bmp = null;
                
            //    try
            //    {
            //        bmp = new BitmapImage(new Uri( D_img_objects[i].img_path));
            //    }
            //    catch
            //    {
            //    }
            //    img[i].Source = bmp;
            //}
            
        }

        private void parse_all_D_features()
        {
            for (int i = 0; i < Constants.NUM_D_RESULTS; i++)
            {
                D_img_objects[i].parse_all_features();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            int[] num_specimens = new int[Constants.NUM_Q_POSSIBILITIES];
            DataTable table = controller.generate_queries(ref num_specimens);
            this.update_Q_displays(table, num_specimens);

            for (int i = 0; i < Constants.NUM_Q_POSSIBILITIES; i++)
            {
                BitmapImage bmp = null;

                try
                {
                    bmp = new BitmapImage(new Uri(Q_possibilities[i].img_path));
                }
                catch
                {
                }
                Q_img[i].Source = bmp;
            }
            Q.pid = Q_possibilities[0].pid;
            this.new_query();
        }

    }
}
