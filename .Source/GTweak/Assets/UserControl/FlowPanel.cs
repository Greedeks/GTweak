using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GTweak.Utilities.Animation;

namespace GTweak.Assets.UserControl
{
    internal sealed class FlowPanel : Panel
    {
        private sealed class FlowItemGroup
        {
            internal readonly List<UIElement> Elements = new List<UIElement>();
            internal double Width;
            internal double Height;
        }

        public enum FlowOrientation
        {
            Unset = 0,
            Vertical = 1,
            Horizontal = 2
        }

        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(FlowPanel),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FlowPanel),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ContentAlignmentProperty = DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(FlowPanel),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty UseMasonryLayoutProperty = DependencyProperty.Register(nameof(UseMasonryLayout), typeof(bool), typeof(FlowPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(FlowOrientation), typeof(FlowPanel),
            new FrameworkPropertyMetadata(FlowOrientation.Unset, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty EqualizeItemSizeProperty = DependencyProperty.Register(nameof(EqualizeItemSize), typeof(bool), typeof(FlowPanel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(FlowPanel),
            new FrameworkPropertyMetadata(320.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        public HorizontalAlignment ContentAlignment
        {
            get => (HorizontalAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        public bool UseMasonryLayout
        {
            get => (bool)GetValue(UseMasonryLayoutProperty);
            set => SetValue(UseMasonryLayoutProperty, value);
        }

        public FlowOrientation Orientation
        {
            get => (FlowOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public bool EqualizeItemSize
        {
            get => (bool)GetValue(EqualizeItemSizeProperty);
            set => SetValue(EqualizeItemSizeProperty, value);
        }

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        private readonly Dictionary<UIElement, Point> _lastPos = new Dictionary<UIElement, Point>();

        private static bool Moved(Point a, Point b) => Math.Abs(a.X - b.X) > 0.5 || Math.Abs(a.Y - b.Y) > 0.5;

        private double GetChildWidth(UIElement child) => EqualizeItemSize ? ItemWidth : child.DesiredSize.Width;

        private double GetChildHeight(UIElement child) => child.DesiredSize.Height;

        protected override Size MeasureOverride(Size availableSize)
        {
            List<UIElement> visible = new List<UIElement>(InternalChildren.Count);
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement c = InternalChildren[i];
                if (c != null && c.Visibility != Visibility.Collapsed)
                {
                    visible.Add(c);
                }
            }

            if (visible.Count == 0)
            {
                return new Size(0, 0);
            }

            double aw = double.IsInfinity(availableSize.Width) ? 1200 : availableSize.Width;

            double ah = double.PositiveInfinity;

            if (EqualizeItemSize)
            {
                Size constraint = new Size(ItemWidth, double.PositiveInfinity);
                foreach (UIElement child in visible)
                {
                    child.Measure(constraint);
                }
            }
            else
            {
                Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                foreach (UIElement child in visible)
                {
                    child.Measure(constraint);
                }
            }

            double neededHeight = CalculateNeededHeight(aw, ah, visible);
            return new Size(aw, neededHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            List<UIElement> visible = new List<UIElement>(InternalChildren.Count);
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement c = InternalChildren[i];
                if (c != null && c.Visibility != Visibility.Collapsed)
                {
                    visible.Add(c);
                }
            }

            if (visible.Count == 0)
            {
                return finalSize;
            }

            List<FlowItemGroup> groups = CreateOptimalColumns(finalSize.Width, finalSize.Height, visible);
            ArrangeAligned(groups, finalSize.Width);

            return finalSize;
        }

        private void ArrangeAligned(List<FlowItemGroup> groups, double availableWidth)
        {
            if (groups == null || groups.Count == 0)
            {
                return;
            }

            if (Orientation == FlowOrientation.Horizontal)
            {
                double y = 0;

                for (int i = 0; i < groups.Count; i++)
                {
                    FlowItemGroup row = groups[i];
                    if (row.Elements.Count == 0)
                    {
                        y += row.Height + VerticalSpacing;
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

                    foreach (UIElement child in row.Elements)
                    {
                        double w = GetChildWidth(child);
                        double h = GetChildHeight(child);

                        Point newPos = new Point(x, y);
                        child.Arrange(new Rect(newPos.X, newPos.Y, w, h));

                        if (!_lastPos.TryGetValue(child, out var oldPos) || Moved(oldPos, newPos))
                        {
                            _lastPos[child] = newPos;

                            if (!(child.RenderTransform is TranslateTransform transform))
                            {
                                transform = new TranslateTransform(0, 0);
                                child.RenderTransform = transform;
                            }

                            transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, 0, 0.30, null, false, true));
                            transform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(transform.Y, 0, 0.30, null, false, true));
                        }

                        x += w + HorizontalSpacing;
                    }

                    y += row.Height + VerticalSpacing;
                }

                return;
            }

            double contentWidth = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                contentWidth += groups[i].Width;
                if (i > 0)
                {
                    contentWidth += HorizontalSpacing;
                }
            }

            double extraSpaceV = Math.Max(0, availableWidth - contentWidth);
            double xStartV = ContentAlignment switch
            {
                HorizontalAlignment.Center => extraSpaceV / 2.0,
                HorizontalAlignment.Right => extraSpaceV,
                _ => 0
            };

            double xV = xStartV;

            for (int i = 0; i < groups.Count; i++)
            {
                FlowItemGroup col = groups[i];
                if (col.Elements.Count == 0)
                {
                    xV += col.Width + HorizontalSpacing;
                    continue;
                }

                double shift = 0;
                if (Orientation == FlowOrientation.Unset && groups.Count > 1 && i > 0)
                {
                    shift = extraSpaceV * (i / (double)(groups.Count - 1)) * 0.3;
                }

                double columnX = xV + shift;
                double y = 0;

                foreach (UIElement child in col.Elements)
                {
                    double w = GetChildWidth(child);
                    double h = GetChildHeight(child);

                    Point newPos = new Point(columnX, y);
                    child.Arrange(new Rect(newPos.X, newPos.Y, w, h));

                    if (!_lastPos.TryGetValue(child, out var oldPos) || Moved(oldPos, newPos))
                    {
                        _lastPos[child] = newPos;

                        if (!(child.RenderTransform is TranslateTransform transform))
                        {
                            transform = new TranslateTransform(0, 0);
                            child.RenderTransform = transform;
                        }

                        transform.BeginAnimation(TranslateTransform.XProperty, FactoryAnimation.CreateIn(transform.X, 0, 0.30, null, false, true));
                        transform.BeginAnimation(TranslateTransform.YProperty, FactoryAnimation.CreateIn(transform.Y, 0, 0.30, null, false, true));
                    }

                    y += h + VerticalSpacing;
                }

                xV += col.Width + HorizontalSpacing;
            }
        }

        private double CalculateNeededHeight(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> groups = CreateOptimalColumns(availableWidth, availableHeight, visibleChildren);

            if (Orientation == FlowOrientation.Horizontal)
            {
                double total = 0;
                for (int i = 0; i < groups.Count; i++)
                {
                    total += groups[i].Height;
                    if (i > 0)
                    {
                        total += VerticalSpacing;
                    }
                }
                return total;
            }

            return CalculateMaxColumnHeight(groups);
        }

        private double CalculateMaxColumnHeight(List<FlowItemGroup> columns)
        {
            if (columns == null || columns.Count == 0)
            {
                return 0;
            }

            double max = 0;

            for (int i = 0; i < columns.Count; i++)
            {
                FlowItemGroup col = columns[i];
                double h = 0;

                for (int j = 0; j < col.Elements.Count; j++)
                {
                    UIElement child = col.Elements[j];
                    h += GetChildHeight(child);
                    if (j > 0)
                    {
                        h += VerticalSpacing;
                    }
                }

                if (h > max)
                {
                    max = h;
                }
            }

            return max;
        }

        private List<FlowItemGroup> CreateOptimalColumns(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            if (visibleChildren == null || visibleChildren.Count == 0)
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

            foreach (UIElement child in visibleChildren)
            {
                double childWidth = GetChildWidth(child);
                double childHeight = GetChildHeight(child);

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

            foreach (UIElement child in visibleChildren)
            {
                double childWidth = GetChildWidth(child);
                double childHeight = GetChildHeight(child);

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
            List<FlowItemGroup> columns = new List<FlowItemGroup>(columnCount);
            for (int i = 0; i < columnCount; i++)
            {
                columns.Add(new FlowItemGroup());
            }

            if (UseMasonryLayout)
            {
                var heights = new double[columnCount];

                foreach (UIElement child in visibleChildren)
                {
                    int minIndex = 0;
                    double minH = heights[0];
                    for (int i = 1; i < columnCount; i++)
                    {
                        if (heights[i] < minH)
                        {
                            minH = heights[i];
                            minIndex = i;
                        }
                    }

                    double h = GetChildHeight(child);
                    double newHeight = heights[minIndex] + (heights[minIndex] > 0 ? VerticalSpacing : 0) + h;

                    if (newHeight > maxHeight)
                    {
                        return null;
                    }

                    columns[minIndex].Elements.Add(child);
                    columns[minIndex].Width = Math.Max(columns[minIndex].Width, GetChildWidth(child));
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

                foreach (UIElement child in visibleChildren)
                {
                    int colIndex = index % columnCount;

                    double addedHeight = (columns[colIndex].Height > 0 ? VerticalSpacing : 0) + GetChildHeight(child);
                    double newHeight = columns[colIndex].Height + addedHeight;

                    if (newHeight > maxHeight)
                    {
                        return null;
                    }

                    columns[colIndex].Elements.Add(child);
                    columns[colIndex].Width = Math.Max(columns[colIndex].Width, GetChildWidth(child));
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
                col.Width = Math.Max(col.Width, GetChildWidth(child));

                totalHeight += GetChildHeight(child);
                if (col.Elements.Count > 1)
                {
                    totalHeight += VerticalSpacing;
                }
            }

            col.Height = totalHeight;
            return new List<FlowItemGroup> { col };
        }

        private double CalculateTotalWidth(List<FlowItemGroup> columns)
        {
            if (columns == null || columns.Count == 0)
            {
                return 0;
            }

            double total = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                total += columns[i].Width;
                if (i > 0)
                {
                    total += HorizontalSpacing;
                }
            }
            return total;
        }
    }
}