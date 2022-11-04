using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Picon
{
    public partial class ImageViewWindow : Window
    {
        private Point WorkingScreenDimensions { get; set; }

        private OpenFileDialog OpenFileDialogInstance { get; set; }
        private BitmapImage BitmapImageInstance { get; set; }
        private Vector ImageNativeResolution { get; set; }
        private double ImageConstantScale { get; set; }
        private Vector ImageRealResolution { get; set; }

        private double ScaleDelta { get; set; }
        private double Scale { get; set; } = 0.934679262589549f;
        private int ScaleStep { get; set; } = 38;
        private Point ScalePosition { get; set; }

        private bool Drag { get; set; }
        private Point DragStartPosition { get; set; } = new Point(0, 0);
        private Vector RuntimeOffset { get; set; } = new Vector(0, 0);
        private Point Offset { get; set; } = new Point(0, 0);

        public ImageViewWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            KeyDown += HandleEsc;

            WorkingScreenDimensions = new Point(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            string imagePath = App.Instance.ImagePath;
            
            if (!CheckInput(imagePath) || !ProcessInput(imagePath))
                imagePath = RequestPath();

            if (!CheckInput(imagePath) || !ProcessInput(imagePath))
            {
                Close();
                return;
            }
            
            ProcessImage();
        }

        private bool CheckInput(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            
            if (!File.Exists(path))
                return false;

            return true;
        }

        private bool ProcessInput(string path)
        {
            Uri imageUri = new Uri(path);
            BitmapImageInstance = new BitmapImage(imageUri);

            return true;
        }
        
        private string RequestPath()
        {
            if (OpenFileDialogInstance == null)
            {
                OpenFileDialogInstance = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "*.PNG;*.JPG;*.BMP;*.ICO|*.PNG;*.JPG;*.BMP;*.ICO"
                };
            }

            if (OpenFileDialogInstance.ShowDialog() != true)
                return string.Empty;

            return OpenFileDialogInstance.FileName;
        }

        private void ProcessImage()
        {
            ImageNativeResolution = new Vector(BitmapImageInstance.Width, BitmapImageInstance.Height);
            ImageConstantScale = Math.Min(WorkingScreenDimensions.X / ImageNativeResolution.X, WorkingScreenDimensions.Y / ImageNativeResolution.Y);
            ImageRealResolution = Vector.Multiply(ImageNativeResolution, ImageConstantScale);

            MainImage.Source = BitmapImageInstance;

            Opacity = 1;
        }
        
        private void Update()
        {
            Offset -= new Vector(
                (ScalePosition.X - ImageRealResolution.X / 2) * ScaleDelta,
                (ScalePosition.Y - ImageRealResolution.Y / 2) * ScaleDelta
            );
            
            Scale += ScaleDelta;
            ScaleDelta = 0;
            
            Offset +=  RuntimeOffset;
            RuntimeOffset = new Vector();
            
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform(Offset.X / Scale, Offset.Y / Scale));
            transform.Children.Add(new ScaleTransform(Scale, Scale));
            MainImage.RenderTransform = transform;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is Image clickedImage && MainImage == clickedImage))
                return;

            DragStartPosition = e.GetPosition(this);
            Drag = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!Drag)
                return;

            RuntimeOffset = e.GetPosition(this) - DragStartPosition;
            DragStartPosition = e.GetPosition(this);

            Update();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            RuntimeOffset = e.GetPosition(this) - DragStartPosition;
            DragStartPosition = e.GetPosition(this);
            
            Drag = false;
            
            Update();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScalePosition = e.GetPosition(MainImage);

            ScaleDelta = GetNextScale(e.Delta > 0) - Scale;

            Update();
        }
        
        private double GetNextScale(bool decrease)
        {
            const int scaleStepsCount = 50; // 0 -> scaleStepsCount (including, (scaleStepsCount + 1) total)
            const double minScale = 10f;
            const double maxScale = 0.25f;
            
            double result = Scale;

            if (ScaleStep == 0 && decrease || ScaleStep == scaleStepsCount && !decrease)
                return result;
            
            ScaleStep += decrease ? -1 : 1;

            double x = (double) ScaleStep / scaleStepsCount;
            double ease = Math.Sin(x * Math.PI / 2); // easeOutSine function
            result = minScale + (maxScale - minScale) * ease; // simply lerp from minScale to maxScale by result modifer

            return result;
        }

        private void OnBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}