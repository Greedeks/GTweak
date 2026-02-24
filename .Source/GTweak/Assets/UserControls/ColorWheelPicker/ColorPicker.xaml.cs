using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GTweak.Utilities.Animation;
using GTweak.Utilities.Controls;

namespace GTweak.Assets.UserControls.ColorWheelPicker
{
    public partial class ColorPicker
    {
        private const double WheelSize = 150.0;
        private const double WheelRadius = WheelSize / 2.0;

        private bool _isWheelBeingDragged, _isValueBeingDragged, _isInitialized, _isUpdatingFromProperty, _isUpdatingHexValue;
        private double _selectedWheelX, _selectedWheelY, _currentHue, _currentSaturation, _currentValue = 1.0;
        private Color _colorOnOpen;

        internal event EventHandler<Color> ColorPicked;
        internal event EventHandler PickerClosed;

        internal static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        internal static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register(nameof(DefaultColor), typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.White));

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

        internal static readonly DependencyProperty SelectedColorStringProperty =
         DependencyProperty.Register(nameof(SelectedColorString), typeof(string), typeof(ColorPicker),
             new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorStringChanged));

        internal string SelectedColorString
        {
            get => (string)GetValue(SelectedColorStringProperty);
            set => SetValue(SelectedColorStringProperty, value);
        }

        private static void OnSelectedColorStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker picker = (ColorPicker)d;
            string newValue = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(newValue) && !picker._isUpdatingFromProperty)
            {
                try
                {
                    string[] components = newValue.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (components.Length == 3)
                    {
                        Color newColor = Color.FromRgb(byte.Parse(components[0]), byte.Parse(components[1]), byte.Parse(components[2]));

                        picker._isUpdatingFromProperty = true;
                        picker.SelectedColor = newColor;
                        picker.UpdateInternalStateFromColor(newColor);
                        picker._isUpdatingFromProperty = false;
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        public ColorPicker()
        {
            InitializeComponent();
            Loaded += OnControlLoaded;
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
            {
                UpdateInternalStateFromColor(SelectedColor);
                _isInitialized = true;
            }

            Window parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                parentWindow.LocationChanged += delegate { UpdatePopupPosition(); };
                parentWindow.SizeChanged += delegate { UpdatePopupPosition(); };
            }
        }

        private static void OnSelectedColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ColorPicker colorPickerControl = (ColorPicker)dependencyObject;

            if (!colorPickerControl._isUpdatingFromProperty)
            {
                colorPickerControl.UpdateInternalStateFromColor((Color)eventArgs.NewValue);
            }
        }

        private void NotifyColorChanged(Color newColor)
        {
            _isUpdatingFromProperty = true;
            SelectedColor = newColor;
            SelectedColorString = $"{newColor.R} {newColor.G} {newColor.B}";
            _isUpdatingFromProperty = false;
            ColorPicked?.Invoke(this, newColor);
        }

        private void PART_ToggleButton_Checked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = true;

        private void PART_ToggleButton_Unchecked(object sender, RoutedEventArgs e) => ColorPopup.IsOpen = false;

        private void ColorPopup_Opened(object sender, EventArgs e)
        {
            _colorOnOpen = SelectedColor;
            PopupTransform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-5, 0, 0.3, useCubicEase: true));
            PopupBorder.BeginAnimation(UIElement.OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.15));
        }

        private void ColorPopup_Closed(object sender, EventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || !PART_ToggleButton.IsMouseOver)
            {
                PART_ToggleButton.IsChecked = false;
                if (_colorOnOpen != SelectedColor)
                {
                    PickerClosed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void BtnDefault_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = DefaultColor;
            UpdateInternalStateFromColor(DefaultColor);
            NotifyColorChanged(DefaultColor);
        }

        private void ColorWheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isWheelBeingDragged = true;
            PART_Wheel.CaptureMouse();
            CalculateAndSetColorFromWheelPosition(e.GetPosition(PART_Wheel));
        }

        private void ColorWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isWheelBeingDragged)
            {
                CalculateAndSetColorFromWheelPosition(e.GetPosition(PART_Wheel));
            }
        }

        private void ColorWheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isWheelBeingDragged = false;
            PART_Wheel.ReleaseMouseCapture();
        }

        private void ValueCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isValueBeingDragged = true;
            PART_ValueCanvas.CaptureMouse();
            CalculateAndSetValueFromMouseY(e.GetPosition(PART_ValueCanvas).Y);
        }

        private void ValueCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isValueBeingDragged)
            {
                CalculateAndSetValueFromMouseY(e.GetPosition(PART_ValueCanvas).Y);
            }
        }

        private void ValueCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isValueBeingDragged = false;
            PART_ValueCanvas.ReleaseMouseCapture();
        }

        private void HexBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidateAndApplyHexColor();
                PART_HexBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void HexBox_LostFocus(object sender, RoutedEventArgs e) => ValidateAndApplyHexColor();

        private void HexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingHexValue)
            {
                string rawHexInput = PART_HexBox.Text.Trim().Replace("#", "");
                string cleanedHexInput = Regex.Replace(rawHexInput, "[^0-9a-fA-F]", "");

                if (rawHexInput != cleanedHexInput)
                {
                    _isUpdatingHexValue = true;
                    PART_HexBox.Text = $"#{cleanedHexInput}";
                    PART_HexBox.CaretIndex = PART_HexBox.Text.Length;
                    _isUpdatingHexValue = false;
                }

                if (cleanedHexInput.Length == 6)
                {
                    ValidateAndApplyHexColor();
                }
            }
        }

        private void UpdatePopupPosition()
        {
            if (ColorPopup.IsOpen)
            {
                MethodInfo updatePositionMethod = typeof(System.Windows.Controls.Primitives.Popup).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Instance);
                updatePositionMethod?.Invoke(ColorPopup, null);
            }
        }

        private void CalculateAndSetColorFromWheelPosition(Point mousePosition)
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

            if (angleInDegrees < 0)
            {
                _currentHue = angleInDegrees + 360.0;
            }
            else
            {
                _currentHue = angleInDegrees;
            }

            ApplyCurrentHsvToInterface();
        }

        private void CalculateAndSetValueFromMouseY(double mouseYPosition)
        {
            double barHeight = PART_ValueBar.Height;
            double markerHeight = PART_MarkerValue.Height;
            double clampedY = Math.Max(markerHeight / 2, Math.Min(barHeight - markerHeight / 2, mouseYPosition));

            Canvas.SetTop(PART_MarkerValue, clampedY - markerHeight / 2);
            _currentValue = 1.0 - ((clampedY - markerHeight / 2) / (barHeight - markerHeight));

            ApplyCurrentHsvToInterface();
        }

        private void ApplyCurrentHsvToInterface()
        {
            Color newColor = ConvertHsvToRgbColor(_currentHue, _currentSaturation, _currentValue);

            Canvas.SetLeft(PART_MarkerWheel, _selectedWheelX - PART_MarkerWheel.Width / 2);
            Canvas.SetTop(PART_MarkerWheel, _selectedWheelY - PART_MarkerWheel.Height / 2);

            PART_ColorPreview.Fill = new SolidColorBrush(newColor);
            PART_ColorTextBlock.Text = $"{newColor.R} {newColor.G} {newColor.B}";

            if (!_isUpdatingHexValue)
            {
                _isUpdatingHexValue = true;
                PART_HexBox.Text = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
                _isUpdatingHexValue = false;
            }

            NotifyColorChanged(newColor);
            UpdateBrightnessBarBackground();
            UpdateBrightnessMarkerPosition();
        }

        private void UpdateInternalStateFromColor(Color color)
        {
            ConvertRgbToHsv(color, out _currentHue, out _currentSaturation, out _currentValue);

            double hueInRadians = _currentHue * Math.PI / 180.0;
            _selectedWheelX = WheelRadius + (_currentSaturation * WheelRadius * Math.Cos(hueInRadians));
            _selectedWheelY = WheelRadius + (_currentSaturation * WheelRadius * Math.Sin(hueInRadians));

            ApplyCurrentHsvToInterface();
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

        private void ValidateAndApplyHexColor()
        {
            if (!_isUpdatingHexValue)
            {
                string rawHexInput = Regex.Replace(PART_HexBox.Text.Trim().Replace("#", ""), "[^0-9a-fA-F]", "");

                if (rawHexInput.Length != 0)
                {
                    if (rawHexInput.Length < 6)
                    {
                        char lastCharacter = rawHexInput[rawHexInput.Length - 1];
                        rawHexInput = rawHexInput.PadRight(6, lastCharacter);
                    }

                    try
                    {
                        Color parsedColor = Color.FromRgb(Convert.ToByte(rawHexInput.Substring(0, 2), 16), Convert.ToByte(rawHexInput.Substring(2, 2), 16), Convert.ToByte(rawHexInput.Substring(4, 2), 16));

                        _isUpdatingHexValue = true;
                        UpdateInternalStateFromColor(parsedColor);
                        PART_HexBox.Text = $"#{rawHexInput.ToUpper()}";
                        _isUpdatingHexValue = false;
                        NotifyColorChanged(parsedColor);
                    }
                    catch { RevertHexBoxToSelectedColor(); }
                }
                else
                {
                    RevertHexBoxToSelectedColor();
                    return;
                }
            }
        }

        private void RevertHexBoxToSelectedColor()
        {
            _isUpdatingHexValue = true;
            PART_HexBox.Text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
            _isUpdatingHexValue = false;
        }

        private static void ConvertRgbToHsv(Color rgbColor, out double hue, out double saturation, out double value)
        {
            double red = rgbColor.R / 255.0;
            double green = rgbColor.G / 255.0;
            double blue = rgbColor.B / 255.0;

            double maximumColorValue = Math.Max(red, Math.Max(green, blue));
            double minimumColorValue = Math.Min(red, Math.Min(green, blue));
            double colorDifference = maximumColorValue - minimumColorValue;

            value = maximumColorValue;
            saturation = (maximumColorValue == 0) ? 0 : (colorDifference / maximumColorValue);

            hue = colorDifference switch
            {
                0 => 0,
                _ when maximumColorValue == red => 60.0 * (((green - blue) / colorDifference) % 6.0),
                _ when maximumColorValue == green => 60.0 * (((blue - red) / colorDifference) + 2.0),
                _ => 60.0 * (((red - green) / colorDifference) + 4.0)
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
                byte grayScaleValue = (byte)(value * 255.0);
                return Color.FromRgb(grayScaleValue, grayScaleValue, grayScaleValue);
            }

            hue /= 60.0;
            int colorSector = (int)Math.Floor(hue);
            double fractionalSector = hue - colorSector;

            double baseColor = value * (1.0 - saturation);
            double decreasingColor = value * (1.0 - saturation * fractionalSector);
            double increasingColor = value * (1.0 - saturation * (1.0 - fractionalSector));

            (double calculatedRed, double calculatedGreen, double calculatedBlue) = colorSector switch
            {
                0 => (value, increasingColor, baseColor),
                1 => (decreasingColor, value, baseColor),
                2 => (baseColor, value, increasingColor),
                3 => (baseColor, decreasingColor, value),
                4 => (increasingColor, baseColor, value),
                _ => (value, baseColor, decreasingColor)
            };

            return Color.FromRgb((byte)(calculatedRed * 255.0), (byte)(calculatedGreen * 255.0), (byte)(calculatedBlue * 255.0));
        }
    }
}