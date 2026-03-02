using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControls
{
    [ContentProperty("Items")]
    public partial class ExpandableBox : UserControl
    {
        internal static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ExpandableBox), new PropertyMetadata("Select"));

        internal static readonly DependencyProperty MaxDropdownHeightProperty =
            DependencyProperty.Register(nameof(MaxDropdownHeight), typeof(double), typeof(ExpandableBox), new PropertyMetadata(250.0));

        internal string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        internal double MaxDropdownHeight
        {
            get => (double)GetValue(MaxDropdownHeightProperty);
            set => SetValue(MaxDropdownHeightProperty, value);
        }

        public ObservableCollection<object> Items { get; } = new ObservableCollection<object>();

        private static readonly MethodInfo UpdatePositionMethod = typeof(Popup).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Instance);

        private Window _parentWindow;
        private readonly List<ScrollViewer> _subscribedScrollViewers = new List<ScrollViewer>();

        public ExpandableBox()
        {
            InitializeComponent();
        }

        private void UpdatePopupPosition()
        {
            if (DropdownPopup.IsOpen)
            {
                UpdatePositionMethod?.Invoke(DropdownPopup, null);
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

        private void UpdatePositionProxy(object s, EventArgs e) => UpdatePopupPosition();
        private void TglArrow_Checked(object sender, RoutedEventArgs e) => DropdownPopup.IsOpen = true;
        private void TglArrow_Unchecked(object sender, RoutedEventArgs e) => DropdownPopup.IsOpen = false;

        private void Popup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DropdownPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            PopupTransform?.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(-5, 0, 0.3, useCubicEase: true));
            PopupBorder?.BeginAnimation(OpacityProperty, FactoryAnimation.CreateIn(0, 1, 0.15));
            SubscribeToScrollParents();
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            UnsubscribeFromScrollParents();

            if (Mouse.LeftButton != MouseButtonState.Pressed || !TglArrow.IsMouseOver)
            {
                TglArrow.IsChecked = false;
            }
        }

        private void ParentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((e.VerticalChange != 0 || e.HorizontalChange != 0) && DropdownPopup?.IsOpen == true)
            {
                DropdownPopup.IsOpen = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
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