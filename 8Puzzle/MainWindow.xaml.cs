using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace _8Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer _timer;
        TimeSpan _time;

        public MainWindow()
        {

            InitializeComponent();
            //openButton.Focusable = false;
            //save.Focusable = false;
            //load.Focusable = false;
            //reset.Focusable = false;
            //start.Focusable = false;
        }



        private Image[,] images = new Image[3, 3];
        private Image[] SaveSmallImage = new Image[9];
        private int[,] originalImage = new int[3, 3];
        private int[,] imagesValue = new int[3, 3];
        private double imageWidth;
        private double imageHeight;

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            //openButton.IsEnabled = true;
            var screen = new OpenFileDialog();


            if (screen.ShowDialog() == true)
            {

                imgPhoto.Source = new BitmapImage(new Uri(screen.FileName));

                var count = 0;
                var bitmap = new BitmapImage(new Uri(screen.FileName));
                var fullImageWidth = 300;
                imageWidth = (int)fullImageWidth / 3;
                imageHeight = (int)(fullImageWidth * bitmap.Height / bitmap.Width) / 3;

                var padding = 5;
                var rng = new Random();
                var pool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
                var pooli = new List<int> { 0, 0, 0, 1, 1, 1, 2, 2 };
                var poolj = new List<int> { 0, 1, 2, 0, 1, 2, 0, 1 };

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (true)
                        {
                            // Set vi tri

                            if (i == 2 && j == 2)
                            {
                                var image = new Image();
                                image.Stretch = Stretch.Fill;

                                container.Children.Add(image);

                                Canvas.SetLeft(image, i * (imageWidth + padding));
                                Canvas.SetTop(image, j * (imageHeight + padding));
                                images[2, 2] = image;
                                imagesValue[2, 2] = count;
                                originalImage[2, 2] = count;

                                goto End;
                            }

                            var k = rng.Next(pool.Count); // Chon ngau nhien mot chi muc trong pool

                            var cropped = new CroppedBitmap(bitmap, new Int32Rect(
                                (pooli[k] * (int)bitmap.Width / 3), (poolj[k] * (int)bitmap.Height / 3),
                                (int)bitmap.Width / 3, (int)bitmap.Height / 3));

                            // Tao giao dien
                            var imageView = new Image();

                            imageView.Source = cropped;
                            imageView.Width = imageWidth;
                            imageView.Height = imageHeight;
                            imageView.Margin = new Thickness(350, 25, 25, 50);

                            imageView.MouseLeftButtonDown += ImageView_MouseLeftButtonDown;
                            imageView.MouseMove += ImageView_MouseMove;
                            imageView.MouseLeftButtonUp += ImageView_MouseLeftButtonUp;
                            container.MouseLeftButtonUp += ImageView_MouseLeftButtonUp;
                            container.MouseMove += ImageView_MouseMove;
                            container.MouseLeftButtonUp += ImageView_MouseLeftButtonUp;

                            container.Children.Add(imageView);

                            images[i, j] = imageView;
                            imagesValue[i, j] = count;
                            originalImage[pooli[k], poolj[k]] = count;
                            count++;
                            Canvas.SetLeft(imageView, i * (imageWidth + padding));
                            Canvas.SetTop(imageView, j * (imageHeight + padding));

                            pool.RemoveAt(k);
                            pooli.RemoveAt(k);
                            poolj.RemoveAt(k);
                        }
                    }
                }
            End:
                SetImagesEnable(false);
            }
        }

        private Image SelectedImage = null;

        private Point ImageLocation;

        private void ImageView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDrawing = false;
            if (SelectedImage != null)
            {
                //var nearlestImage = GetNearlestImage(SelectedImage);
                var nearlestImage = images[2, 2];
                double x1 = Canvas.GetLeft(SelectedImage) + imageWidth / 2;
                double x2 = Canvas.GetLeft(nearlestImage) + imageWidth / 2;
                double y1 = Canvas.GetTop(SelectedImage) + imageHeight / 2;
                double y2 = Canvas.GetTop(nearlestImage) + imageHeight / 2;

                var pos1 = new Point(x1, y1);
                var pos2 = new Point(x2, y2);
                var pos3 = new Point(ImageLocation.X + imageWidth / 2, ImageLocation.Y + imageHeight / 2);

                if (getDistance(pos1, pos3) < getDistance(pos1, pos2))
                {
                    Canvas.SetLeft(SelectedImage, ImageLocation.X);
                    Canvas.SetTop(SelectedImage, ImageLocation.Y);
                    SelectedImage = null;
                    return;
                }

                int i1 = -1, j1 = -1, i2 = -1, j2 = -1;
                i1 = (int)((ImageLocation.X + imageWidth / 2) / imageWidth);
                j1 = (int)((ImageLocation.Y + imageHeight / 2) / imageHeight);


                i2 = (int)((x2) / imageWidth);
                j2 = (int)((y2) / imageHeight);

                var temp = imagesValue[i1, j1];
                imagesValue[i1, j1] = imagesValue[i2, j2];
                imagesValue[i2, j2] = temp;

                Canvas.SetLeft(SelectedImage, Canvas.GetLeft(nearlestImage));
                Canvas.SetTop(SelectedImage, Canvas.GetTop(nearlestImage));

                Canvas.SetLeft(nearlestImage, ImageLocation.X);
                Canvas.SetTop(nearlestImage, ImageLocation.Y);


                SelectedImage = null;
                if (isVictory())
                {
                    _timer.Stop();
                    MessageBox.Show("win");
                    this.Close();
                }
            }
        }

        private Image GetNearlestImage(Image selectedImage)
        {
            Image Result = null;
            double minDistance = double.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {

                    if (selectedImage != images[i, j])
                    {
                        double x1 = Canvas.GetLeft(selectedImage) + imageWidth / 2;
                        double x2 = Canvas.GetLeft(images[i, j]) + imageWidth / 2;
                        double y1 = Canvas.GetTop(selectedImage) + imageHeight / 2;
                        double y2 = Canvas.GetTop(images[i, j]) + imageHeight / 2;
                        var pos1 = new Point(x1, y1);
                        var pos2 = new Point(x2, y2);
                        double temp = getDistance(pos1, pos2);
                        if (temp < minDistance)
                        {
                            minDistance = temp;
                            Result = images[i, j];
                        }
                    }


                }
            }
            return Result;
        }

        private double getDistance(Point pos1, Point pos2)
        {
            double x1 = pos1.X;
            double x2 = pos2.X;
            double y1 = pos1.Y;
            double y2 = pos2.Y;
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        private bool isVictory()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (imagesValue[i, j] != originalImage[i, j])
                        return false;
                }
            }
            return true;
        }

        private void ImageView_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDrawing && SelectedImage != null)
            {
                var newLocation = e.GetPosition(this);

                double newX = Canvas.GetLeft(SelectedImage) + (newLocation.X - OldLocation.X);
                double newY = Canvas.GetTop(SelectedImage) + (newLocation.Y - OldLocation.Y);

                if (newX <= imageWidth * 2 + 10 && newX >= 0)
                    Canvas.SetLeft(SelectedImage, newX);
                if (newY <= imageHeight * 2 + 10 && newY >= 0)
                    Canvas.SetTop(SelectedImage, newY);
                OldLocation = newLocation;
            }
        }
        private bool IsDrawing = false;
        private Point OldLocation = new Point();
        private void ImageView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsDrawing)
            {
                SelectedImage = sender as Image;
                ImageLocation = new Point(Canvas.GetLeft(SelectedImage), Canvas.GetTop(SelectedImage));
                IsDrawing = true;
                OldLocation = e.GetPosition(this);
                SelectedImage.BringToFront();
            }

        }


        private void Container_KeyDown(object sender, KeyEventArgs e)
        {
            if (!images[0, 0].IsEnabled)
                return;
            switch (e.Key)
            {
                case Key.Down:
                    {
                        //save.IsEnabled = false;
                        var location = new Point(Canvas.GetLeft(images[2, 2]), Canvas.GetTop(images[2, 2]));
                        int j1 = (int)((location.Y + imageHeight / 2) / imageHeight);
                        int i1 = (int)((location.X + imageWidth / 2) / imageWidth);
                        if (j1 == 0)
                        {
                            break;
                        }

                        j1--;
                        int i2 = -1, j2 = -1;
                        var temp = imagesValue[i1, j1 + 1];
                        imagesValue[i1, j1 + 1] = imagesValue[i1, j1];
                        imagesValue[i1, j1] = temp;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var loc = new Point(Canvas.GetLeft(images[i, j]), Canvas.GetTop(images[i, j]));
                                i2 = (int)((loc.X + imageWidth / 2) / imageWidth);
                                j2 = (int)((loc.Y + imageHeight / 2) / imageHeight);
                                if (i2 == i1 && j2 == j1)
                                {
                                    i2 = i;
                                    j2 = j;
                                    goto label;
                                }
                            }
                        }
                    label:


                        Canvas.SetLeft(images[2, 2], Canvas.GetLeft(images[i2, j2]));
                        Canvas.SetTop(images[2, 2], Canvas.GetTop(images[i2, j2]));

                        Canvas.SetLeft(images[i2, j2], location.X);
                        Canvas.SetTop(images[i2, j2], location.Y);

                        if (isVictory())
                        {
                            _timer.Stop();
                            MessageBox.Show("win");
                            this.Close();
                        }

                        break;
                    }
                case Key.Up:
                    {
                        //save.IsEnabled = false;
                        var location = new Point(Canvas.GetLeft(images[2, 2]), Canvas.GetTop(images[2, 2]));
                        int j1 = (int)((location.Y + imageHeight / 2) / imageHeight);
                        int i1 = (int)((location.X + imageWidth / 2) / imageWidth);
                        if (j1 == 2)
                        {
                            break;
                        }

                        j1++;

                        var temp = imagesValue[i1, j1 - 1];
                        imagesValue[i1, j1 - 1] = imagesValue[i1, j1];
                        imagesValue[i1, j1] = temp;
                        int i2 = -1, j2 = -1;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var loc = new Point(Canvas.GetLeft(images[i, j]), Canvas.GetTop(images[i, j]));
                                i2 = (int)((loc.X + imageWidth / 2) / imageWidth);
                                j2 = (int)((loc.Y + imageHeight / 2) / imageHeight);
                                if (i2 == i1 && j2 == j1)
                                {
                                    i2 = i;
                                    j2 = j;
                                    goto label;
                                }
                            }
                        }
                    label:

                        Canvas.SetLeft(images[2, 2], Canvas.GetLeft(images[i2, j2]));
                        Canvas.SetTop(images[2, 2], Canvas.GetTop(images[i2, j2]));

                        Canvas.SetLeft(images[i2, j2], location.X);
                        Canvas.SetTop(images[i2, j2], location.Y);

                        if (isVictory())
                        {
                            _timer.Stop();
                            MessageBox.Show("win");
                            this.Close();
                        }

                        break;
                    }
                case Key.Right:
                    {
                        //save.IsEnabled = false;
                        var location = new Point(Canvas.GetLeft(images[2, 2]), Canvas.GetTop(images[2, 2]));
                        int j1 = (int)((location.Y + imageHeight / 2) / imageHeight);
                        int i1 = (int)((location.X + imageWidth / 2) / imageWidth);
                        if (i1 == 0)
                        {
                            break;
                        }

                        i1--;

                        var temp = imagesValue[i1 + 1, j1];
                        imagesValue[i1 + 1, j1] = imagesValue[i1, j1];
                        imagesValue[i1, j1] = temp;
                        int i2 = -1, j2 = -1;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var loc = new Point(Canvas.GetLeft(images[i, j]), Canvas.GetTop(images[i, j]));
                                i2 = (int)((loc.X + imageWidth / 2) / imageWidth);
                                j2 = (int)((loc.Y + imageHeight / 2) / imageHeight);
                                if (i2 == i1 && j2 == j1)
                                {
                                    i2 = i;
                                    j2 = j;
                                    goto label;
                                }
                            }
                        }
                    label:

                        Canvas.SetLeft(images[2, 2], Canvas.GetLeft(images[i2, j2]));
                        Canvas.SetTop(images[2, 2], Canvas.GetTop(images[i2, j2]));

                        Canvas.SetLeft(images[i2, j2], location.X);
                        Canvas.SetTop(images[i2, j2], location.Y);

                        if (isVictory())
                        {
                            _timer.Stop();
                            MessageBox.Show("win");
                            this.Close();
                        }

                        break;
                    }
                case Key.Left:
                    {
                        //save.IsEnabled = false;
                        var location = new Point(Canvas.GetLeft(images[2, 2]), Canvas.GetTop(images[2, 2]));
                        int j1 = (int)((location.Y + imageHeight / 2) / imageHeight);
                        int i1 = (int)((location.X + imageWidth / 2) / imageWidth);
                        if (i1 == 2)
                        {
                            break;
                        }

                        i1++;
                        int i2 = -1, j2 = -1;

                        var temp = imagesValue[i1 - 1, j1];
                        imagesValue[i1 - 1, j1] = imagesValue[i1, j1];
                        imagesValue[i1, j1] = temp;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var loc = new Point(Canvas.GetLeft(images[i, j]), Canvas.GetTop(images[i, j]));
                                i2 = (int)((loc.X + imageWidth / 2) / imageWidth);
                                j2 = (int)((loc.Y + imageHeight / 2) / imageHeight);
                                if (i2 == i1 && j2 == j1)
                                {
                                    i2 = i;
                                    j2 = j;
                                    goto label;
                                }
                            }
                        }
                    label:

                        Canvas.SetLeft(images[2, 2], Canvas.GetLeft(images[i2, j2]));
                        Canvas.SetTop(images[2, 2], Canvas.GetTop(images[i2, j2]));

                        Canvas.SetLeft(images[i2, j2], location.X);
                        Canvas.SetTop(images[i2, j2], location.Y);

                        if (isVictory())
                        {
                            _timer.Stop();
                            MessageBox.Show("win");
                            this.Close();
                        }

                        break;
                    }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetImagesEnable(true);
                CountDown();
            }
            catch
            {
                MessageBox.Show("Vui lòng chọn ảnh", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetImagesEnable(bool v)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    images[i, j].IsEnabled = v;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ImageBrush bgBrush = (ImageBrush)this.FindResource("bgImage");
            this.Background = bgBrush;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _timer.Stop();
                var screen = new SaveFileDialog();

                if (screen.ShowDialog() == true)
                {
                    var doc = new XmlDocument();

                    var root = doc.CreateElement("Game");
                    root.SetAttribute("IsDraweing", IsDrawing.ToString());
                    // root.SetAttribute("IsVictory", isVictory.ToString());

                    var state = doc.CreateElement("State");
                    root.AppendChild(state);
                    int count = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        var line = doc.CreateElement("Line");
                        line.SetAttribute("Value", $"{imagesValue[i, 0]} {imagesValue[i, 1]} {imagesValue[i, 2]}");
                        state.AppendChild(line);
                        SaveSmallImage[count] = images[i, 0]; count++;
                        SaveSmallImage[count] = images[i, 1]; count++;
                        SaveSmallImage[count] = images[i, 2]; count++;

                    }
                    var SaveTime = doc.CreateElement("Time");
                    root.AppendChild(SaveTime);
                    SaveTime.SetAttribute("Time", _time.ToString());

                    doc.AppendChild(root);

                    doc.Save(screen.FileName);
                    MessageBox.Show("Save successfully");
                }
            }
            catch
            {
                MessageBox.Show("Vui lòng chọn ảnh", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            if (screen.ShowDialog() == true)
            {
                MessageBox.Show("Chưa hoàn thiện");
            }
        }

        void Reset()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        void CountDown()
        {
            _time = TimeSpan.FromSeconds(180);
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                tbTime.Text = _time.ToString("c");
                if (_time == TimeSpan.Zero)
                {
                    _timer.Stop();
                    MessageBox.Show("Lose");
                    this.Close();
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));

            }, Application.Current.Dispatcher);

            _timer.Start();
        }
    }
}
