using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControl
{
    internal sealed class FlowPanel : Panel
    {
        private class FlowItemGroup
        {
            internal List<UIElement> Elements { get; set; } = new List<UIElement>();
            internal double Width { get; set; }
            internal double Height { get; set; }
        }

        internal enum FlowOrientation
        {
            Unset = 0,
            Vertical = 1,
            Horizontal = 2
        }

        internal static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(FlowPanel),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FlowPanel),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(FlowPanel),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        internal static readonly DependencyProperty UseMasonryLayoutProperty = DependencyProperty.Register(nameof(UseMasonryLayout), typeof(bool), typeof(FlowPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        internal static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(FlowOrientation), typeof(FlowPanel),
            new FrameworkPropertyMetadata(FlowOrientation.Unset, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

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

        internal FlowOrientation Orientation
        {
            get => (FlowOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            List<UIElement> visibleChildren = InternalChildren.Cast<UIElement>().Where(c => c != null && c.Visibility != Visibility.Collapsed).ToList();

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

            double neededHeight = CalculateNeededHeight(availableWidth, availableHeight, visibleChildren);

            return new Size(availableWidth, neededHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            List<UIElement> visibleChildren = InternalChildren.Cast<UIElement>().Where(c => c != null && c.Visibility != Visibility.Collapsed).ToList();

            if (visibleChildren?.Count == 0)
            {
                return new Size(0, 0);
            }

            List<FlowItemGroup> columns = CreateOptimalColumns(finalSize.Width, finalSize.Height, visibleChildren);
            ArrangeAligned(columns ?? new List<FlowItemGroup>(), finalSize.Width);

            return finalSize;
        }

        private void ArrangeAligned(List<FlowItemGroup> columns, double availableWidth)
        {
            if (columns?.Count == 0)
            {
                return;
            }

            if (Orientation == FlowOrientation.Horizontal)
            {
                double y = 0;
                for (int i = 0; i < columns.Count; i++)
                {
                    var row = columns[i];
                    if (row?.Elements == null || row.Elements.Count == 0)
                    {
                        y += row?.Height ?? 0;
                        y += VerticalSpacing;
                        continue;
                    }

                    double extraSpace = Math.Max(0, availableWidth - row.Width);
                    double xStart = ContentAlignment switch
                    {
                        HorizontalAlignment.Center => extraSpace / 2.0,
                        HorizontalAlignment.Right => extraSpace,
                        _ => 0
                    };

                    double x = xStart;
                    foreach (var child in row.Elements)
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

                        child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));

                        transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, 0, 0.30, null, false, true));
                        transform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(transform.Y, 0, 0.30, null, false, true));

                        x += child.DesiredSize.Width + HorizontalSpacing;
                    }

                    y += row.Height + VerticalSpacing;
                }

                return;
            }

            double contentWidth = (columns?.Sum(c => c.Width) ?? 0) + Math.Max(0, (columns?.Count ?? 0) - 1) * HorizontalSpacing;
            double extraSpaceV = Math.Max(0, availableWidth - contentWidth);
            double xStartV = ContentAlignment switch
            {
                HorizontalAlignment.Center => (availableWidth - contentWidth) / 2.0,
                HorizontalAlignment.Right => availableWidth - contentWidth,
                _ => 0
            };

            double xV = xStartV;

            for (int i = 0; i < columns.Count; i++)
            {
                FlowItemGroup column = columns[i];
                if (column?.Elements == null || column.Elements.Count == 0)
                {
                    xV += column?.Width ?? 0;
                    xV += HorizontalSpacing;
                    continue;
                }

                double shift = (Orientation == FlowOrientation.Unset && columns != null && i > 0 && columns.Count > 1) ? (extraSpaceV * (i / (double)(columns.Count - 1)) * 0.3) : 0;
                double columnX = xV + shift;
                double y = 0;

                foreach (var child in column.Elements)
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

                xV += column.Width + HorizontalSpacing;
            }
        }

        private double CalculateNeededHeight(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> columns = CreateOptimalColumns(availableWidth, availableHeight, visibleChildren);

            if (Orientation == FlowOrientation.Horizontal)
            {
                return (columns?.Sum(r => r.Height) ?? 0) + Math.Max(0, (columns?.Count ?? 0) - 1) * VerticalSpacing;
            }

            return CalculateMaxColumnHeight(columns);
        }

        private double CalculateMaxColumnHeight(List<FlowItemGroup> columns)
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
                foreach (var child in column.Elements)
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

        private List<FlowItemGroup> CreateOptimalColumns(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            if (visibleChildren?.Count == 0)
            {
                return new List<FlowItemGroup>();
            }

            if (Orientation == FlowOrientation.Horizontal)
            {
                return CreateWrappedRows(availableWidth, visibleChildren);
            }

            if (Orientation == FlowOrientation.Vertical)
            {
                return CreateWrappedColumns(availableHeight, visibleChildren);
            }

            int maxPossibleColumns = Math.Max(1, (int)(availableWidth / 100));
            for (int columnCount = maxPossibleColumns; columnCount >= 1; columnCount--)
            {
                List<FlowItemGroup> columns = TryDistributeToColumns(columnCount, availableHeight, visibleChildren);
                if (columns != null && CalculateTotalWidth(columns) <= availableWidth)
                {
                    return columns;
                }
            }

            return CreateSingleColumn(visibleChildren);
        }

        private List<FlowItemGroup> CreateWrappedRows(double maxWidth, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> rows = new List<FlowItemGroup>();
            FlowItemGroup current = null;

            foreach (var child in visibleChildren)
            {
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;

                if (current == null)
                {
                    current = new FlowItemGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    rows.Add(current);
                    continue;
                }

                double widthIfAdded = current.Width + (current.Width > 0 ? HorizontalSpacing : 0) + childWidth;

                if (widthIfAdded <= maxWidth || current.Elements.Count == 0)
                {
                    current.Elements.Add(child);
                    current.Width = widthIfAdded;
                    current.Height = Math.Max(current.Height, childHeight);
                }
                else
                {
                    current = new FlowItemGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    rows.Add(current);
                }
            }

            return rows;
        }

        private List<FlowItemGroup> CreateWrappedColumns(double maxHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> cols = new List<FlowItemGroup>();
            FlowItemGroup current = null;

            foreach (var child in visibleChildren)
            {
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;

                if (current == null)
                {
                    current = new FlowItemGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    cols.Add(current);
                    continue;
                }

                double heightIfAdded = current.Height + (current.Height > 0 ? VerticalSpacing : 0) + childHeight;

                if (heightIfAdded <= maxHeight || current.Elements.Count == 0)
                {
                    current.Elements.Add(child);
                    current.Height = heightIfAdded;
                    current.Width = Math.Max(current.Width, childWidth);
                }
                else
                {
                    current = new FlowItemGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    cols.Add(current);
                }
            }

            return cols;
        }

        private List<FlowItemGroup> TryDistributeToColumns(int columnCount, double maxHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> columns = Enumerable.Range(0, columnCount).Select(_ => new FlowItemGroup()).ToList();

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

                for (int i = 0; i < columnCount; i++)
                {
                    columns[i].Height = heights[i];
                }
            }
            else
            {
                int index = 0;
                for (int i = 0; i < columnCount; i++)
                {
                    columns[i].Width = 0;
                    columns[i].Height = 0;
                }

                foreach (UIElement child in visibleChildren)
                {
                    int colIndex = index % columnCount;

                    double addedHeight = (columns[colIndex].Height > 0 ? VerticalSpacing : 0) + child.DesiredSize.Height;
                    double newHeight = columns[colIndex].Height + addedHeight;
                    if (newHeight > maxHeight)
                    {
                        return null;
                    }

                    columns[colIndex].Elements.Add(child);
                    columns[colIndex].Width = Math.Max(columns[colIndex].Width, child.DesiredSize.Width);
                    columns[colIndex].Height = newHeight;

                    index++;
                }
            }

            return columns;
        }

        private List<FlowItemGroup> CreateSingleColumn(List<UIElement> visibleChildren)
        {
            FlowItemGroup col = new FlowItemGroup();
            double totalHeight = 0;
            foreach (UIElement child in visibleChildren)
            {
                col.Elements.Add(child);
                col.Width = Math.Max(col.Width, child.DesiredSize.Width);
                totalHeight += child.DesiredSize.Height + (totalHeight > 0 ? VerticalSpacing : 0);
            }
            col.Height = totalHeight;
            return new List<FlowItemGroup> { col };
        }

        private double CalculateTotalWidth(List<FlowItemGroup> columns) => (columns?.Sum(c => c.Width) ?? 0) + Math.Max(0, (columns?.Count ?? 0) - 1) * HorizontalSpacing;
    }
}