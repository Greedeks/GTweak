using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;

namespace GTweak.Assets.UserControl.ColorWheelPicker
{
    public partial class ColorPicker
    {
        internal static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        internal Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        internal string SelectedColorString
        {
            get => $"{SelectedColor.R} {SelectedColor.G} {SelectedColor.B}";

            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        string[] parts = value.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 3)
                        {
                            byte r = byte.Parse(parts[0]);
                            byte g = byte.Parse(parts[1]);
                            byte b = byte.Parse(parts[2]);

                            SelectedColor = Color.FromRgb(r, g, b);
                        }
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }
            }
        }

        private const double WheelSize = 150.0;
        private const double WheelRadius = WheelSize / 2.0;

        private bool _isDragging, _isInitialized, _updatingFromProperty;
        private double _selectedX, _selectedY, _brightness = 1.0;

        public ColorPicker()
        {
            InitializeComponent();
            PART_BrightnessSlider.ValueChanged += (s, e) => UpdateFromSlider();

            Loaded += (s, e) =>
            {
                if (!_isInitialized)
                {
                    SetColorFromColor(SelectedColor);
                    _isInitialized = true;
                }

                Window win = Window.GetWindow(this);
                if (win != null)
                {
                    win.LocationChanged += delegate { UpdatePopupPos(); };
                    win.SizeChanged += delegate { UpdatePopupPos(); };
                }
            };
        }
        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker ctrl = (ColorPicker)d;
            if (ctrl._updatingFromProperty)
            {
                return;
            }

            ctrl.SetColorFromColor((Color)e.NewValue);
        }

        private void PART_ToggleButton_Checked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = true;

        private void PART_ToggleButton_Unchecked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = false;

        private void UpdatePopupPos()
        {
            if (ColorPopup.IsOpen)
            {
                var method = typeof(System.Windows.Controls.Primitives.Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(ColorPopup, null);
            }
        }

        private void UpdateFromSlider()
        {
            _brightness = PART_BrightnessSlider.Value;
            UpdateColor(GetColorFromWheel(_selectedX, _selectedY, _brightness));
        }

        private void Wheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PART_Wheel.CaptureMouse();
            _isDragging = true;
            UpdateColorByMouse(e.GetPosition(PART_Wheel));
        }

        private void Wheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                UpdateColorByMouse(e.GetPosition(PART_Wheel));
            }
        }

        private void Wheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            PART_Wheel.ReleaseMouseCapture();
        }

        private void UpdateColorByMouse(Point pos)
        {
            double dx = pos.X - WheelRadius, dy = pos.Y - WheelRadius;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist > WheelRadius)
            {
                dx *= WheelRadius / dist;
                dy *= WheelRadius / dist;
            }

            _selectedX = WheelRadius + dx;
            _selectedY = WheelRadius + dy;
            UpdateColor(GetColorFromWheel(_selectedX, _selectedY, _brightness));
        }

        private void ColorPopup_Closed(object sender, EventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                PART_ToggleButton.IsChecked = false;
            }
            else if (!PART_ToggleButton.IsMouseOver)
            {
                PART_ToggleButton.IsChecked = false;
            }
        }

        private void UpdateColor(Color color)
        {
            if (color.A >= 128)
            {
                Canvas.SetLeft(PART_Marker, _selectedX - PART_Marker.Width / 2);
                Canvas.SetTop(PART_Marker, _selectedY - PART_Marker.Height / 2);

                PART_ColorPreview.Fill = new SolidColorBrush(color);
                PART_ColorTextBlock.Text = $"{color.R} {color.G} {color.B}";

                _updatingFromProperty = true;
                SelectedColor = color;
                _updatingFromProperty = false;
            }
        }

        private void SetColorFromColor(Color color)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            _brightness = max;
            PART_BrightnessSlider.Value = _brightness;

            double hueDeg = 0;

            if (delta != 0)
            {
                if (max == r)
                {
                    hueDeg = 60 * (((g - b) / delta) % 6);
                }
                else if (max == g)
                {
                    hueDeg = 60 * (((b - r) / delta) + 2);
                }
                else
                {
                    hueDeg = 60 * (((r - g) / delta) + 4);
                }
            }

            if (hueDeg < 0)
            {
                hueDeg += 360;
            }

            double s = (max == 0) ? 0 : delta / max;
            double angle = Math.PI - (Math.PI / 3.0) * (hueDeg / 60.0);

            _selectedX = WheelRadius - (s * WheelRadius * Math.Cos(angle));
            _selectedY = WheelRadius + (s * WheelRadius * Math.Sin(angle));

            UpdateColor(color);
        }

        public static Color GetColorFromWheel(double x, double y, double v)
        {
            double nx = (x - WheelRadius) / WheelRadius;
            double ny = (y - WheelRadius) / WheelRadius;
            double sat = Math.Min(1.0, Math.Sqrt(nx * nx + ny * ny));

            double hue = (Math.Atan2(ny, nx) + Math.PI) / (2 * Math.PI) * 6.0;

            double c = v * sat;
            double x_c = c * (1 - Math.Abs(hue % 2.0 - 1));
            double m = v - c;

            (double r, double g, double b) = hue switch
            {
                double h when h < 1 => (c, x_c, 0.0),
                double h when h < 2 => (x_c, c, 0.0),
                double h when h < 3 => (0.0, c, x_c),
                double h when h < 4 => (0.0, x_c, c),
                double h when h < 5 => (x_c, 0.0, c),
                _ => (c, 0.0, x_c)
            };

            byte ToByte(double val) => (byte)((val + m) * 255);

            return Color.FromRgb(ToByte(r), ToByte(g), ToByte(b));
        }

        private void ColorPopup_Opened(object sender, EventArgs e)
        {
            PopupTransform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-5, 0, 0.3, useCubicEase: true));
            PopupBorder.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.15));
        }
    }
}
