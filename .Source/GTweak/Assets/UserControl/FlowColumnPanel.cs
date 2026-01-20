using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControl
{
    internal sealed class FlowColumnPanel : Panel
    {
        private class ColumnInfo
        {
            internal List<UIElement> Elements { get; set; } = new List<UIElement>();
            internal double Width { get; set; }
        }

        internal static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(FlowColumnPanel),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FlowColumnPanel),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(FlowColumnPanel),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty UseMasonryLayoutProperty = DependencyProperty.Register(nameof(UseMasonryLayout), typeof(bool), typeof(FlowColumnPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        internal double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        internal double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        internal HorizontalAlignment ContentAlignment
        {
            get => (HorizontalAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        internal bool UseMasonryLayout
        {
            get => (bool)GetValue(UseMasonryLayoutProperty);
            set => SetValue(UseMasonryLayoutProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var visibleChildren = InternalChildren.Cast<UIElement>()
                .Where(c => c != null && c.Visibility != Visibility.Collapsed)
                .ToList();

            if (visibleChildren.Count == 0)
            {
                return new Size(0, 0);
            }

            double availableWidth = double.IsInfinity(availableSize.Width) ? 1200 : availableSize.Width;
            double availableHeight = double.IsInfinity(availableSize.Height) ? 800 : availableSize.Height;

            foreach (UIElement child in visibleChildren)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            double neededHeight = CalculateNeededHeight(availableWidth, double.PositiveInfinity, visibleChildren);


            return new Size(availableWidth, neededHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = InternalChildren.Cast<UIElement>()
                .Where(c => c != null && c.Visibility != Visibility.Collapsed)
                .ToList();

            if (visibleChildren?.Count == 0)
            {
                return new Size(0, 0);
            }

            var columns = CreateOptimalColumns(finalSize.Width, finalSize.Height, visibleChildren);
            ArrangeAligned(columns ?? new List<ColumnInfo>(), finalSize.Width);

            return finalSize;
        }

        private void ArrangeAligned(List<ColumnInfo> columns, double availableWidth)
        {
            if (columns?.Count == 0)
            {
                return;
            }

            double contentWidth = (columns?.Sum(c => c.Width) ?? 0) + Math.Max(0, (columns?.Count ?? 0) - 1) * HorizontalSpacing;
            double extraSpace = Math.Max(0, availableWidth - contentWidth);
            double xStart = ContentAlignment switch
            {
                HorizontalAlignment.Center => (availableWidth - contentWidth) / 2.0,
                HorizontalAlignment.Right => availableWidth - contentWidth,
                _ => 0
            };

            double x = xStart;

            for (int i = 0; i < columns?.Count; i++)
            {
                var column = columns[i];
                if (column?.Elements == null || column?.Elements.Count == 0)
                {
                    x += column?.Width ?? 0.0;
                    x += HorizontalSpacing;
                    continue;
                }

                double shift = (columns != null && i > 0 && columns.Count > 1) ? (extraSpace * (i / (double)(columns.Count - 1)) * 0.3) : 0;
                double columnX = x + shift;
                double y = 0;

                foreach (var child in column?.Elements)
                {
                    if (child.Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }

                    if (!(child.RenderTransform is TranslateTransform transform))
                    {
                        transform = new TranslateTransform(0, 0);
                        child.RenderTransform = transform;
                    }

                    child.Arrange(new Rect(columnX, y, child.DesiredSize.Width, child.DesiredSize.Height));

                    transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, 0, 0.30, null, false, true));
                    transform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(transform.Y, 0, 0.30, null, false, true));

                    y += child.DesiredSize.Height + VerticalSpacing;
                }

                x += column.Width + HorizontalSpacing;
            }
        }


        private double CalculateNeededHeight(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            var columns = CreateOptimalColumns(availableWidth, availableHeight, visibleChildren);
            return CalculateMaxColumnHeight(columns);
        }

        private double CalculateMaxColumnHeight(List<ColumnInfo> columns)
        {
            if (columns?.Count == 0)
            {
                return 0;
            }

            return columns.Max(column =>
            {
                if (column?.Elements == null)
                {
                    return 0;
                }

                double h = 0;
                foreach (var child in column?.Elements)
                {
                    if (child.Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }

                    h += child.DesiredSize.Height + (h > 0 ? VerticalSpacing : 0);
                }
                return h;
            });
        }

        private List<ColumnInfo> CreateOptimalColumns(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            if (visibleChildren?.Count == 0)
            {
                return new List<ColumnInfo>();
            }

            int maxPossibleColumns = Math.Max(1, (int)(availableWidth / 100));

            for (int columnCount = maxPossibleColumns; columnCount >= 1; columnCount--)
            {
                var columns = TryDistributeToColumns(columnCount, availableHeight, visibleChildren);
                if (columns != null && CalculateTotalWidth(columns) <= availableWidth)
                {
                    return columns;
                }
            }

            return CreateSingleColumn(visibleChildren);
        }

        private List<ColumnInfo> TryDistributeToColumns(int columnCount, double maxHeight, List<UIElement> visibleChildren)
        {
            var columns = Enumerable.Range(0, columnCount).Select(_ => new ColumnInfo()).ToList();

            if (UseMasonryLayout)
            {
                double[] heights = new double[columnCount];

                foreach (UIElement child in visibleChildren)
                {
                    int minIndex = Array.IndexOf(heights, heights.Min());
                    double h = child.DesiredSize.Height;
                    double newHeight = heights[minIndex] + (heights[minIndex] > 0 ? VerticalSpacing : 0) + h;

                    if (newHeight > maxHeight)
                    {
                        return null;
                    }

                    columns[minIndex].Elements.Add(child);
                    columns[minIndex].Width = Math.Max(columns[minIndex].Width, child.DesiredSize.Width);
                    heights[minIndex] = newHeight;
                }
            }
            else
            {
                int index = 0;
                foreach (UIElement child in visibleChildren)
                {
                    int colIndex = index % columnCount;
                    columns[colIndex].Elements.Add(child);
                    columns[colIndex].Width = Math.Max(columns[colIndex].Width, child.DesiredSize.Width);
                    index++;
                }
            }

            return columns;
        }


        private List<ColumnInfo> CreateSingleColumn(List<UIElement> visibleChildren)
        {
            var col = new ColumnInfo();
            foreach (UIElement child in visibleChildren)
            {
                col.Elements.Add(child);
                col.Width = Math.Max(col.Width, child.DesiredSize.Width);
            }
            return new List<ColumnInfo> { col };
        }

        private double CalculateTotalWidth(List<ColumnInfo> columns) => (columns?.Sum(c => c.Width) ?? 0) + Math.Max(0, (columns?.Count ?? 0) - 1) * HorizontalSpacing;
    }
}
