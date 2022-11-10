using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Picon.Subscripts;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Picon
{
    public partial class ImageViewWindow : Window
    {
        private Point WorkingScreenDimensions { get; set; }

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
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            
            Initialize();
        }

        private void Initialize()
        {
            KeyDown += HandleEsc;
            Topmost = true;

            WorkingScreenDimensions = new Point(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            PiconViewerBridge.OnPiconViewerInitialized(OpenFile);
        }

        private void OpenFile(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return;
            
            Uri imageUri = new Uri(imagePath);
            BitmapImageInstance = new BitmapImage(imageUri);
            
            ProcessImage();
        }

        private void ProcessImage()
        {
            ImageNativeResolution = new Vector(BitmapImageInstance.Width, BitmapImageInstance.Height);
            ImageConstantScale = Math.Min(WorkingScreenDimensions.X / ImageNativeResolution.X, WorkingScreenDimensions.Y / ImageNativeResolution.Y);
            ImageRealResolution = Vector.Multiply(ImageNativeResolution, ImageConstantScale);

            MainImage.Source = BitmapImageInstance;

            Opacity = 1;
            WindowState = WindowState.Maximized;
        }
        
        private void Update()
        {
            if (ScaleDelta > 0) // Zoom IN
            {
                Offset -= new Vector(
                    (ScalePosition.X - ImageRealResolution.X / 2) * ScaleDelta,
                    (ScalePosition.Y - ImageRealResolution.Y / 2) * ScaleDelta
                );
            }
            else if (ScaleDelta < 0) // Zoom OUT
            {
                double offsetAngle = Math.Atan2(Offset.Y, Offset.X);
                double offsetLength = Math.Sqrt(Offset.X * Offset.X + Offset.Y * Offset.Y);
                double offsetLengthDelta = Clamp(Math.Pow(Math.Abs(offsetLength), 1.1d / 2) * 3d, 0, offsetLength);
                
                Offset -= new Vector(Math.Cos(offsetAngle), Math.Sin(offsetAngle)) * offsetLengthDelta;
            }
            
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
                OnImageClosed();
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

        private static double Clamp(double value, double minValue, double maxValue)
        {
            if (value < minValue)
                return minValue;

            if (value > maxValue)
                return maxValue;

            return value;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            bool zoomIn = e.Delta > 0;
            
            ScaleDelta = GetNextScale(zoomIn) - Scale;

            ScalePosition = e.GetPosition(MainImage);

            ScalePosition = new Point(
                Clamp(ScalePosition.X, 0, ImageRealResolution.X),
                Clamp(ScalePosition.Y, 0, ImageRealResolution.Y)
            );

            Update();
        }
        
        private double GetNextScale(bool zoomIn)
        {
            const int scaleStepsCount = 50; // 0 -> scaleStepsCount (including, (scaleStepsCount + 1) total)
            const double minScale = 10f;
            const double maxScale = 0.25f;
            
            double result = Scale;

            if (ScaleStep == 0 && zoomIn || ScaleStep == scaleStepsCount && !zoomIn)
                return result;
            
            ScaleStep += zoomIn ? -1 : 1;

            double x = (double) ScaleStep / scaleStepsCount;
            double ease = Math.Sin(x * Math.PI / 2); // easeOutSine function
            result = minScale + (maxScale - minScale) * ease; // simply lerp from minScale to maxScale by result modifer

            return result;
        }

        private void OnImageClosed()
        {
            Opacity = 0;
            WindowState = WindowState.Minimized;
        }

        private void OnBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            OnImageClosed();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            OnImageClosed();
        }
    }
}