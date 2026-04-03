using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GTweak.Utilities.Controls;

namespace GTweak.Assets.UserControls
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
          new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FlowPanel),
          new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

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

        public static readonly DependencyProperty ResponsiveModeProperty = DependencyProperty.Register(nameof(ResponsiveMode), typeof(bool), typeof(FlowPanel),
              new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

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

        public bool ResponsiveMode
        {
            get => (bool)GetValue(ResponsiveModeProperty);
            set => SetValue(ResponsiveModeProperty, value);
        }

        private List<FlowItemGroup> _cachedLayout = new List<FlowItemGroup>();
        private readonly List<UIElement> _visibleCache = new List<UIElement>();
        private readonly Stack<FlowItemGroup> _groupPool = new Stack<FlowItemGroup>();
        private readonly Stack<List<FlowItemGroup>> _listPool = new Stack<List<FlowItemGroup>>();
        private double[] _heightCache = new double[32];
        private double _cachedMeasureWidth = -1;

        private double GetChildWidth(UIElement child) => EqualizeItemSize ? ItemWidth : child?.DesiredSize.Width ?? 0;

        private double GetChildHeight(UIElement child) => child?.DesiredSize.Height ?? 0;

        private FlowItemGroup GetGroup()
        {
            if (_groupPool.Count > 0)
            {
                FlowItemGroup group = _groupPool.Pop();
                group.Elements.Clear();
                group.Width = 0;
                group.Height = 0;
                return group;
            }
            return new FlowItemGroup();
        }

        private List<FlowItemGroup> GetGroupList()
        {
            if (_listPool.Count > 0)
            {
                List<FlowItemGroup> list = _listPool.Pop();
                list.Clear();
                return list;
            }
            return new List<FlowItemGroup>();
        }

        private void ReleaseGroups(List<FlowItemGroup> groups)
        {
            if (groups != null)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    _groupPool.Push(groups[i]);
                }
                groups.Clear();
                _listPool.Push(groups);
            }
        }

        private void UpdateVisibleCache()
        {
            _visibleCache.Clear();
            UIElementCollection children = InternalChildren;
            if (children != null)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    UIElement child = children[i];
                    if (child != null && child.Visibility != Visibility.Collapsed)
                    {
                        _visibleCache.Add(child);
                    }
                }
            }
        }

        private double GetResponsiveAlignmentOffset(double outerWidth, double contentWidth)
        {
            double extra = Math.Max(0, outerWidth - contentWidth);

            return ContentAlignment switch
            {
                HorizontalAlignment.Center => extra / 2.0,
                HorizontalAlignment.Right => extra,
                _ => 0
            };
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                UpdateVisibleCache();

                if (_visibleCache.Count == 0)
                {
                    return new Size(0, 0);
                }

                double aw = double.IsInfinity(availableSize.Width) ? 1200 : availableSize.Width;
                double ah = availableSize.Height;

                Size constraint = EqualizeItemSize ? new Size(ItemWidth, double.PositiveInfinity) : new Size(availableSize.Width, double.PositiveInfinity);

                for (int i = 0; i < _visibleCache.Count; i++)
                {
                    _visibleCache[i]?.Measure(constraint);
                }

                if (ResponsiveMode)
                {
                    double totalItemWidth = 0;
                    double totalItemHeight = 0;
                    double maxItemHeight = 0;
                    double maxItemWidth = 0;
                    int count = _visibleCache.Count;

                    for (int i = 0; i < count; i++)
                    {
                        double cw = GetChildWidth(_visibleCache[i]);
                        double ch = GetChildHeight(_visibleCache[i]);

                        totalItemWidth += cw;
                        totalItemHeight += ch;

                        if (cw > maxItemWidth)
                        {
                            maxItemWidth = cw;
                        }

                        if (ch > maxItemHeight)
                        {
                            maxItemHeight = ch;
                        }
                    }

                    double horizontalRequired = totalItemWidth + (count > 1 ? (count - 1) * HorizontalSpacing : 0);
                    double verticalRequired = totalItemHeight + (count > 1 ? (count - 1) * VerticalSpacing : 0);

                    if (horizontalRequired <= aw)
                    {
                        return new Size(aw, maxItemHeight);
                    }

                    return new Size(maxItemWidth, verticalRequired);
                }

                if (_cachedLayout != null)
                {
                    ReleaseGroups(_cachedLayout);
                    _cachedLayout = null;
                }

                _cachedMeasureWidth = aw;
                _cachedLayout = CreateOptimalColumns(aw, ah, _visibleCache);

                double neededHeight = CalculateLayoutHeight(_cachedLayout);
                return new Size(aw, neededHeight);
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                return new Size(0, 0);
            }
        }

        private void ArrangeResponsive(Size finalSize)
        {
            int count = _visibleCache.Count;
            if (count == 0)
            {
                return;
            }

            double totalItemWidth = 0;
            double totalItemHeight = 0;
            double maxItemWidth = 0;

            for (int i = 0; i < count; i++)
            {
                double w = GetChildWidth(_visibleCache[i]);
                double h = GetChildHeight(_visibleCache[i]);

                totalItemWidth += w;
                totalItemHeight += h;

                if (w > maxItemWidth)
                {
                    maxItemWidth = w;
                }
            }

            double horizontalRequired = totalItemWidth + (count > 1 ? (count - 1) * HorizontalSpacing : 0);

            if (horizontalRequired <= finalSize.Width)
            {
                double extraSpace = Math.Max(0, finalSize.Width - totalItemWidth);
                double spacing = count > 1 ? extraSpace / (count - 1) : 0;

                double x = 0;
                if (count == 1)
                {
                    x = GetResponsiveAlignmentOffset(finalSize.Width, totalItemWidth);
                }

                for (int i = 0; i < count; i++)
                {
                    UIElement child = _visibleCache[i];
                    double w = GetChildWidth(child);
                    double h = GetChildHeight(child);

                    child.Arrange(new Rect(x, 0, w, h));
                    x += w + spacing;
                }
            }
            else
            {
                double contentWidth = maxItemWidth;
                double y = 0;

                for (int i = 0; i < count; i++)
                {
                    UIElement child = _visibleCache[i];
                    double w = GetChildWidth(child);
                    double h = GetChildHeight(child);

                    double x = GetResponsiveAlignmentOffset(contentWidth, w);

                    child.Arrange(new Rect(x, y, w, h));
                    y += h + VerticalSpacing;
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                UpdateVisibleCache();

                if (_visibleCache.Count != 0)
                {
                    if (ResponsiveMode)
                    {
                        ArrangeResponsive(finalSize);
                        return finalSize;
                    }

                    List<FlowItemGroup> groupsToArrange = null;
                    bool usedCache = false;

                    if (_cachedLayout != null && Math.Abs(_cachedMeasureWidth - finalSize.Width) < 0.1)
                    {
                        groupsToArrange = _cachedLayout;
                        usedCache = true;
                        _cachedLayout = null;
                    }
                    else
                    {
                        groupsToArrange = CreateOptimalColumns(finalSize.Width, finalSize.Height, _visibleCache);
                    }

                    if (groupsToArrange != null)
                    {
                        ArrangeAligned(groupsToArrange, finalSize.Width);
                        ReleaseGroups(groupsToArrange);
                    }

                    if (!usedCache && _cachedLayout != null)
                    {
                        ReleaseGroups(_cachedLayout);
                        _cachedLayout = null;
                    }
                }

                return finalSize;
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug(ex);
                return finalSize;
            }
        }

        private double CalculateLayoutHeight(List<FlowItemGroup> layout)
        {
            if (layout == null || layout.Count == 0)
            {
                return 0;
            }

            double neededHeight = 0;
            if (Orientation == FlowOrientation.Horizontal)
            {
                for (int i = 0; i < layout.Count; i++)
                {
                    neededHeight += layout[i].Height;
                    if (i > 0)
                    {
                        neededHeight += VerticalSpacing;
                    }
                }
            }
            else
            {
                neededHeight = CalculateMaxColumnHeight(layout);
            }
            return neededHeight;
        }

        private void ArrangeAligned(List<FlowItemGroup> groups, double availableWidth)
        {
            if (groups != null && groups.Count != 0)
            {
                switch (Orientation)
                {
                    case FlowOrientation.Horizontal:
                        ArrangeHorizontal(groups, availableWidth);
                        break;
                    default:
                        ArrangeVertical(groups, availableWidth);
                        break;
                }
            }
        }

        private void ArrangeHorizontal(List<FlowItemGroup> groups, double availableWidth)
        {
            double y = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                FlowItemGroup row = groups[i];
                if (row == null || row.Elements.Count == 0)
                {
                    if (row != null)
                    {
                        y += row.Height + VerticalSpacing;
                    }
                    continue;
                }

                double extraSpace = Math.Max(0, availableWidth - row.Width);
                double x = ContentAlignment switch
                {
                    HorizontalAlignment.Center => extraSpace / 2.0,
                    HorizontalAlignment.Right => extraSpace,
                    _ => 0
                };

                foreach (UIElement child in row.Elements)
                {
                    if (child != null)
                    {
                        double w = GetChildWidth(child);
                        double h = GetChildHeight(child);

                        child.Arrange(new Rect(x, y, w, h));
                        x += w + HorizontalSpacing;
                    }
                }

                y += row.Height + VerticalSpacing;
            }
        }

        private void ArrangeVertical(List<FlowItemGroup> groups, double availableWidth)
        {
            if (groups != null && groups.Count != 0)
            {
                double totalColumnsWidth = 0;
                for (int i = 0; i < groups.Count; i++)
                {
                    totalColumnsWidth += groups[i].Width;
                    if (i < groups.Count - 1)
                    {
                        totalColumnsWidth += HorizontalSpacing;
                    }
                }

                double extraSpace = Math.Max(0, availableWidth - totalColumnsWidth);

                double xV = ContentAlignment switch
                {
                    HorizontalAlignment.Center => extraSpace / 2.0,
                    HorizontalAlignment.Right => extraSpace,
                    _ => 0
                };

                for (int i = 0; i < groups.Count; i++)
                {
                    FlowItemGroup col = groups[i];

                    double shift = 0;
                    if (!EqualizeItemSize && groups.Count > 1)
                    {
                        shift = extraSpace * ((double)i / (groups.Count - 1)) * 0.3;
                    }

                    double columnX = xV + shift;
                    double y = 0;

                    foreach (UIElement child in col.Elements)
                    {
                        double w = GetChildWidth(child);
                        double h = GetChildHeight(child);

                        child.Arrange(new Rect(columnX, y, w, h));
                        y += h + VerticalSpacing;
                    }

                    xV += col.Width + HorizontalSpacing;
                }
            }
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
                if (columns[i] != null && columns[i].Height > max)
                {
                    max = columns[i].Height;
                }
            }
            return max;
        }

        private List<FlowItemGroup> CreateOptimalColumns(double availableWidth, double availableHeight, List<UIElement> visibleChildren)
        {
            if (visibleChildren == null || visibleChildren.Count == 0)
            {
                return GetGroupList();
            }

            switch (Orientation)
            {
                case FlowOrientation.Horizontal:
                    return CreateWrappedRows(availableWidth, visibleChildren);
                case FlowOrientation.Vertical:
                    return CreateWrappedColumns(availableHeight, visibleChildren);
            }

            double basisWidth;
            if (EqualizeItemSize)
            {
                basisWidth = Math.Max(16.0, ItemWidth);
            }
            else
            {
                double sum = 0.0;
                int count = 0;
                for (int i = 0; i < visibleChildren.Count; i++)
                {
                    double w = GetChildWidth(visibleChildren[i]);
                    if (w > 0)
                    {
                        sum += w;
                        count++;
                    }
                }
                basisWidth = Math.Max(16, count > 0 ? sum / count : 100.0);
            }

            int maxPossibleColumns = Math.Max(1, (int)((availableWidth + HorizontalSpacing) / (basisWidth + HorizontalSpacing)));

            for (int columnCount = Math.Min(maxPossibleColumns, visibleChildren.Count); columnCount >= 1; columnCount--)
            {
                List<FlowItemGroup> columns = TryDistributeToColumns(columnCount, availableHeight, visibleChildren);

                if (columns != null && columns.Count > 0 && CalculateTotalWidth(columns) <= availableWidth)
                {
                    return columns;
                }

                if (columns != null)
                {
                    ReleaseGroups(columns);
                }
            }

            return CreateSingleColumn(visibleChildren);
        }

        private List<FlowItemGroup> CreateWrappedRows(double maxWidth, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> rows = GetGroupList();
            if (visibleChildren == null)
            {
                return rows;
            }

            FlowItemGroup current = null;

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                UIElement child = visibleChildren[i];
                if (child != null)
                {
                    double childWidth = GetChildWidth(child);
                    double childHeight = GetChildHeight(child);

                    if (current == null)
                    {
                        current = GetGroup();
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
                        current = GetGroup();
                        current.Elements.Add(child);
                        current.Width = childWidth;
                        current.Height = childHeight;
                        rows.Add(current);
                    }
                }
            }
            return rows;
        }

        private List<FlowItemGroup> CreateWrappedColumns(double maxHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> cols = GetGroupList();
            if (visibleChildren == null)
            {
                return cols;
            }

            FlowItemGroup current = null;

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                UIElement child = visibleChildren[i];
                if (child != null)
                {
                    double childWidth = GetChildWidth(child);
                    double childHeight = GetChildHeight(child);

                    if (current == null)
                    {
                        current = GetGroup();
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
                        current = GetGroup();
                        current.Elements.Add(child);
                        current.Width = childWidth;
                        current.Height = childHeight;
                        cols.Add(current);
                    }
                }
            }
            return cols;
        }

        private List<FlowItemGroup> TryDistributeToColumns(int columnCount, double maxHeight, List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> columns = GetGroupList();
            for (int i = 0; i < columnCount; i++)
            {
                columns.Add(GetGroup());
            }

            if (visibleChildren == null)
            {
                return columns;
            }

            if (UseMasonryLayout)
            {
                if (_heightCache.Length < columnCount)
                {
                    Array.Resize(ref _heightCache, Math.Max(_heightCache.Length * 2, columnCount));
                }

                Array.Clear(_heightCache, 0, columnCount);

                for (int c = 0; c < visibleChildren.Count; c++)
                {
                    UIElement child = visibleChildren[c];
                    if (child != null)
                    {
                        int minIndex = 0;
                        double minH = _heightCache[0];
                        for (int i = 1; i < columnCount; i++)
                        {
                            if (_heightCache[i] < minH)
                            {
                                minH = _heightCache[i];
                                minIndex = i;
                            }
                        }

                        double h = GetChildHeight(child);
                        double newHeight = _heightCache[minIndex] + (_heightCache[minIndex] > 0 ? VerticalSpacing : 0) + h;

                        if (!double.IsInfinity(maxHeight) && newHeight > maxHeight)
                        {
                            ReleaseGroups(columns);
                            return null;
                        }

                        columns[minIndex].Elements.Add(child);
                        columns[minIndex].Width = Math.Max(columns[minIndex].Width, GetChildWidth(child));
                        _heightCache[minIndex] = newHeight;
                    }
                }

                for (int i = 0; i < columnCount; i++)
                {
                    columns[i].Height = _heightCache[i];
                }
            }
            else
            {
                for (int index = 0; index < visibleChildren.Count; index++)
                {
                    UIElement child = visibleChildren[index];
                    if (child != null)
                    {
                        int colIndex = index % columnCount;

                        double addedHeight = (columns[colIndex].Height > 0 ? VerticalSpacing : 0) + GetChildHeight(child);
                        double newHeight = columns[colIndex].Height + addedHeight;

                        if (!double.IsInfinity(maxHeight) && newHeight > maxHeight)
                        {
                            ReleaseGroups(columns);
                            return null;
                        }

                        columns[colIndex].Elements.Add(child);
                        columns[colIndex].Width = Math.Max(columns[colIndex].Width, GetChildWidth(child));
                        columns[colIndex].Height = newHeight;
                    }
                }
            }

            return columns;
        }

        private List<FlowItemGroup> CreateSingleColumn(List<UIElement> visibleChildren)
        {
            List<FlowItemGroup> list = GetGroupList();
            FlowItemGroup col = GetGroup();
            double totalHeight = 0;

            if (visibleChildren != null)
            {
                for (int i = 0; i < visibleChildren.Count; i++)
                {
                    UIElement child = visibleChildren[i];
                    if (child != null)
                    {
                        col.Elements.Add(child);
                        col.Width = Math.Max(col.Width, GetChildWidth(child));
                        totalHeight += GetChildHeight(child);

                        if (i > 0)
                        {
                            totalHeight += VerticalSpacing;
                        }
                    }
                }
            }

            col.Height = totalHeight;
            list.Add(col);
            return list;
        }

        private double CalculateTotalWidth(List<FlowItemGroup> columns)
        {
            if (columns != null && columns.Count != 0)
            {
                double total = 0;
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i] != null)
                    {
                        total += columns[i].Width;
                        if (i > 0)
                        {
                            total += HorizontalSpacing;
                        }
                    }
                }
                return total;
            }

            return 0;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded!, visualRemoved!);
            InvalidateMeasure();
            InvalidateArrange();
        }
    }
}