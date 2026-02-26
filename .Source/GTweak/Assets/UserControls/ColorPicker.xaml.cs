using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControls
{
    public partial class ColorPicker
    {
        internal event EventHandler ColorPicked;

        internal static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        internal static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register(nameof(DefaultColor), typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.White));

        internal static readonly DependencyProperty SelectedColorStringProperty =
            DependencyProperty.Register(nameof(SelectedColorString), typeof(string), typeof(ColorPicker), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorStringChanged));

        internal Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        internal Color DefaultColor
        {
            get => (Color)GetValue(DefaultColorProperty);
            set => SetValue(DefaultColorProperty, value);
        }

        internal string SelectedColorString
        {
            get => (string)GetValue(SelectedColorStringProperty);
            set => SetValue(SelectedColorStringProperty, value);
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker picker && e.NewValue is Color newColor && !picker._isUpdatingFromProperty)
            {
                picker.UpdateHsvFromColor(newColor);
            }
        }

        private static void OnSelectedColorStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker picker && e.NewValue is string newValue && !string.IsNullOrWhiteSpace(newValue) && !picker._isUpdatingFromProperty)
            {
                string[] components = newValue.Split(ColorSeparators, StringSplitOptions.RemoveEmptyEntries);

                if (components.Length == 3 && byte.TryParse(components[0], out byte r) && byte.TryParse(components[1], out byte g) && byte.TryParse(components[2], out byte b))
                {
                    Color newColor = Color.FromRgb(r, g, b);
                    bool prevPropFlag = picker._isUpdatingFromProperty;
                    picker._isUpdatingFromProperty = true;
                    picker.SelectedColor = newColor;
                    picker.UpdateHsvFromColor(newColor);
                    picker._isUpdatingFromProperty = prevPropFlag;
                }
            }
        }

        private const double WheelSize = 150.0, WheelRadius = WheelSize / 2.0;

        private static readonly char[] ColorSeparators = { ' ', ',', ';' };
        private static readonly Regex HexRegex = new Regex("[^0-9a-fA-F]", RegexOptions.Compiled);
        private static readonly MethodInfo UpdatePositionMethod = typeof(Popup).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Instance);

        private bool _isWheelBeingDragged, _isValueBeingDragged, _isUpdatingFromProperty, _isUpdatingHexValue;
        private double _selectedWheelX, _selectedWheelY, _currentHue, _currentSaturation, _currentValue = 1.0;
        private Color _colorOnOpen;
        private Window _parentWindow;
        private readonly List<ScrollViewer> _subscribedScrollViewers = new List<ScrollViewer>();

        public ColorPicker()
        {
            InitializeComponent();
        }

        private void UpdateColorFromWheel(Point mousePosition)
        {
            double deltaX = mousePosition.X - WheelRadius;
            double deltaY = mousePosition.Y - WheelRadius;
            double distanceFromCenter = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distanceFromCenter > WheelRadius)
            {
                deltaX *= WheelRadius / distanceFromCenter;
                deltaY *= WheelRadius / distanceFromCenter;
            }

            _selectedWheelX = WheelRadius + deltaX;
            _selectedWheelY = WheelRadius + deltaY;
            _currentSaturation = Math.Min(1.0, distanceFromCenter / WheelRadius);

            double angleInDegrees = Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
            _currentHue = angleInDegrees < 0 ? angleInDegrees + 360.0 : angleInDegrees;

            UpdateUIFromHsv();
        }

        private void UpdateValueFromMouse(double mouseYPosition)
        {
            double barHeight = PART_ValueBar.Height;
            double markerHeight = PART_MarkerValue.Height;
            double clampedY = Math.Max(markerHeight / 2, Math.Min(barHeight - markerHeight / 2, mouseYPosition));

            Canvas.SetTop(PART_MarkerValue, clampedY - markerHeight / 2);
            _currentValue = 1.0 - ((clampedY - markerHeight / 2) / (barHeight - markerHeight));

            UpdateUIFromHsv();
        }

        private void UpdateUIFromHsv()
        {
            Color newColor = ConvertHsvToRgbColor(_currentHue, _currentSaturation, _currentValue);

            Canvas.SetLeft(PART_MarkerWheel, _selectedWheelX - PART_MarkerWheel.Width / 2);
            Canvas.SetTop(PART_MarkerWheel, _selectedWheelY - PART_MarkerWheel.Height / 2);

            ColorPreview.Fill = new SolidColorBrush(newColor);
            ColorTextBlock.Text = $"{newColor.R} {newColor.G} {newColor.B}";

            bool prevHexFlag = _isUpdatingHexValue;
            _isUpdatingHexValue = true;
            HexBox.Text = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
            _isUpdatingHexValue = prevHexFlag;

            NotifyColorChanged(newColor);
            UpdateBrightnessBarBackground();
            UpdateBrightnessMarkerPosition();
        }

        private void UpdateHsvFromColor(Color color)
        {
            ConvertRgbToHsv(color, out _currentHue, out _currentSaturation, out _currentValue);

            double hueInRadians = _currentHue * Math.PI / 180.0;
            _selectedWheelX = WheelRadius + (_currentSaturation * WheelRadius * Math.Cos(hueInRadians));
            _selectedWheelY = WheelRadius + (_currentSaturation * WheelRadius * Math.Sin(hueInRadians));

            UpdateUIFromHsv();
        }

        private void ApplyHexColor()
        {
            if (!_isUpdatingHexValue)
            {
                string rawHexInput = HexRegex.Replace(HexBox.Text.Trim().Replace("#", ""), "");

                if (rawHexInput.Length > 0)
                {
                    if (rawHexInput.Length < 6)
                    {
                        char lastCharacter = rawHexInput[rawHexInput.Length - 1];
                        rawHexInput = rawHexInput.PadRight(6, lastCharacter);
                    }

                    if (byte.TryParse(rawHexInput.Substring(0, 2), NumberStyles.HexNumber, null, out byte r) && byte.TryParse(rawHexInput.Substring(2, 2), NumberStyles.HexNumber, null, out byte g) && byte.TryParse(rawHexInput.Substring(4, 2), NumberStyles.HexNumber, null, out byte b))
                    {
                        Color parsedColor = Color.FromRgb(r, g, b);
                        bool prevHexFlag = _isUpdatingHexValue;
                        _isUpdatingHexValue = true;
                        UpdateHsvFromColor(parsedColor);
                        HexBox.Text = $"#{rawHexInput.ToUpper()}";
                        _isUpdatingHexValue = prevHexFlag;
                        NotifyColorChanged(parsedColor);
                    }
                    else
                    {
                        RevertHexText();
                    }
                }
                else
                {
                    RevertHexText();
                }
            }
        }

        private void RevertHexText()
        {
            bool prevHexFlag = _isUpdatingHexValue;
            _isUpdatingHexValue = true;
            HexBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            _isUpdatingHexValue = prevHexFlag;
        }

        private void UpdateBrightnessBarBackground()
        {
            Color pureHueColor = ConvertHsvToRgbColor(_currentHue, _currentSaturation, 1.0);
            PART_ValueBar.Fill = new LinearGradientBrush(pureHueColor, Colors.Black, new Point(0.5, 0), new Point(0.5, 1));
        }

        private void UpdateBrightnessMarkerPosition()
        {
            double barHeight = PART_ValueBar.Height;
            double markerHeight = PART_MarkerValue.Height;
            double markerYPosition = (1.0 - _currentValue) * (barHeight - markerHeight) + markerHeight / 2;

            Canvas.SetTop(PART_MarkerValue, markerYPosition - markerHeight / 2);
            Canvas.SetLeft(PART_MarkerValue, (PART_ValueBar.Width - markerHeight) / 2);
        }

        private void NotifyColorChanged(Color newColor)
        {
            bool prevPropFlag = _isUpdatingFromProperty;
            _isUpdatingFromProperty = true;
            SelectedColor = newColor;
            SelectedColorString = $"{newColor.R} {newColor.G} {newColor.B}";
            _isUpdatingFromProperty = prevPropFlag;
        }

        private void UpdatePopupPosition()
        {
            if (ColorPopup.IsOpen)
            {
                UpdatePositionMethod?.Invoke(ColorPopup, null);
            }
        }

        private void SubscribeToScrollParents()
        {
            UnsubscribeFromScrollParents();

            DependencyObject parent = this;
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent) ?? LogicalTreeHelper.GetParent(parent);

                if (parent is ScrollViewer sv && !_subscribedScrollViewers.Contains(sv))
                {
                    sv.ScrollChanged += ParentScrollViewer_ScrollChanged;
                    _subscribedScrollViewers.Add(sv);
                }
            }
        }

        private void UnsubscribeFromScrollParents()
        {
            foreach (var sv in _subscribedScrollViewers)
            {
                sv.ScrollChanged -= ParentScrollViewer_ScrollChanged;
            }
            _subscribedScrollViewers.Clear();
        }

        private static void ConvertRgbToHsv(Color rgbColor, out double hue, out double saturation, out double value)
        {
            double r = rgbColor.R / 255.0;
            double g = rgbColor.G / 255.0;
            double b = rgbColor.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            value = max;
            saturation = max == 0 ? 0 : delta / max;

            hue = delta switch
            {
                0 => 0,
                _ when max == r => 60.0 * (((g - b) / delta) % 6.0),
                _ when max == g => 60.0 * (((b - r) / delta) + 2.0),
                _ => 60.0 * (((r - g) / delta) + 4.0)
            };

            if (hue < 0)
            {
                hue += 360.0;
            }
        }

        private static Color ConvertHsvToRgbColor(double hue, double saturation, double value)
        {
            if (saturation == 0)
            {
                byte gray = (byte)(value * 255.0);
                return Color.FromRgb(gray, gray, gray);
            }

            hue /= 60.0;
            int sector = (int)Math.Floor(hue);
            double fractional = hue - sector;

            double p = value * (1.0 - saturation);
            double q = value * (1.0 - saturation * fractional);
            double t = value * (1.0 - saturation * (1.0 - fractional));

            var (calcR, calcG, calcB) = sector switch
            {
                0 => (value, t, p),
                1 => (q, value, p),
                2 => (p, value, t),
                3 => (p, q, value),
                4 => (t, p, value),
                _ => (value, p, q)
            };

            return Color.FromRgb((byte)(calcR * 255.0), (byte)(calcG * 255.0), (byte)(calcB * 255.0));
        }

        private void UpdatePositionProxy(object s, EventArgs e) => UpdatePopupPosition();
        private void ParentWindow_MovedOrResized(object sender, EventArgs e) => UpdatePopupPosition();
        private void TglArrow_Checked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = true;
        private void TglArrow_Unchecked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = false;

        private void BtnDefault_Click(object sender, RoutedEventArgs e)
        {
            Color def = DefaultColor;
            SelectedColor = def;
            UpdateHsvFromColor(def);
            NotifyColorChanged(def);
        }

        private void ColorPopup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ColorPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void ColorPopup_Opened(object sender, EventArgs e)
        {
            _colorOnOpen = SelectedColor;
            PopupTransform?.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-5, 0, 0.3, useCubicEase: true));
            PopupBorder?.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.15));
            SubscribeToScrollParents();
        }

        private void ColorPopup_Closed(object sender, EventArgs e)
        {
            UnsubscribeFromScrollParents();

            if (_colorOnOpen != SelectedColor)
            {
                ColorPicked?.Invoke(this, EventArgs.Empty);
            }

            if (Mouse.LeftButton != MouseButtonState.Pressed || !TglArrow.IsMouseOver)
            {
                TglArrow.IsChecked = false;
            }
        }

        private void ParentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((e.VerticalChange != 0 || e.HorizontalChange != 0) && ColorPopup?.IsOpen == true)
            {
                ColorPopup.IsOpen = false;
            }
        }

        private void ColorWheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isWheelBeingDragged = true;
            PART_Wheel.CaptureMouse();
            UpdateColorFromWheel(e.GetPosition(PART_Wheel));
        }

        private void ColorWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isWheelBeingDragged)
            {
                UpdateColorFromWheel(e.GetPosition(PART_Wheel));
            }
        }

        private void ColorWheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isWheelBeingDragged = false;
            PART_Wheel?.ReleaseMouseCapture();
        }

        private void ValueCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isValueBeingDragged = true;
            PART_ValueCanvas.CaptureMouse();
            UpdateValueFromMouse(e.GetPosition(PART_ValueCanvas).Y);
        }

        private void ValueCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isValueBeingDragged)
            {
                UpdateValueFromMouse(e.GetPosition(PART_ValueCanvas).Y);
            }
        }

        private void ValueCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isValueBeingDragged = false;
            PART_ValueCanvas?.ReleaseMouseCapture();
        }

        private void HexBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e?.Key == Key.Enter)
            {
                ApplyHexColor();
                HexBox?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void HexBox_LostFocus(object sender, RoutedEventArgs e) => ApplyHexColor();

        private void HexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingHexValue)
            {
                return;
            }

            string rawHexInput = HexBox.Text.Trim().Replace("#", "");
            string cleanedHexInput = HexRegex.Replace(rawHexInput, "");

            if (rawHexInput != cleanedHexInput)
            {
                bool prevHexFlag = _isUpdatingHexValue;
                _isUpdatingHexValue = true;
                HexBox.Text = $"#{cleanedHexInput}";
                HexBox.CaretIndex = HexBox.Text.Length;
                _isUpdatingHexValue = prevHexFlag;
            }

            if (cleanedHexInput.Length == 6)
            {
                ApplyHexColor();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateHsvFromColor(SelectedColor);
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged += UpdatePositionProxy;
                _parentWindow.SizeChanged += UpdatePositionProxy;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.LocationChanged -= UpdatePositionProxy;
                _parentWindow.SizeChanged -= UpdatePositionProxy;
            }
            UnsubscribeFromScrollParents();
        }
    }
}