using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GTweak.Modules.Common;

namespace GTweak.Assets.UserControls
{
    public sealed class FlowPanel : Panel
    {
        private const double DefaultFallbackWidth = 320.0;
        private const double MinBasisWidth = 16.0;
        private const double ResponsiveGap = 30.0;
        private const double ColumnSpreadRatio = 0.3;

        private sealed class FlowItemGroup
        {
            internal readonly List<UIElement> Elements = new List<UIElement>();
            internal double Width;
            internal double Height;
        }

        public enum FlowOrientation { Unset, Vertical, Horizontal }

        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(FlowPanel), new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._horizontalSpacing = (double)e.NewValue), ValidateNonNegativeDouble);

        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(FlowPanel), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._verticalSpacing = (double)e.NewValue), ValidateNonNegativeDouble);

        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register(nameof(ContentAlignment), typeof(HorizontalAlignment), typeof(FlowPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._contentAlignment = (HorizontalAlignment)e.NewValue));

        public static readonly DependencyProperty UseMasonryLayoutProperty =
            DependencyProperty.Register(nameof(UseMasonryLayout), typeof(bool), typeof(FlowPanel), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._useMasonryLayout = (bool)e.NewValue));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(FlowOrientation), typeof(FlowPanel), new FrameworkPropertyMetadata(FlowOrientation.Unset, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._orientation = (FlowOrientation)e.NewValue));

        public static readonly DependencyProperty EqualizeItemSizeProperty =
            DependencyProperty.Register(nameof(EqualizeItemSize), typeof(bool), typeof(FlowPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._equalizeItemSize = (bool)e.NewValue));

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(FlowPanel), new FrameworkPropertyMetadata(DefaultFallbackWidth, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, (d, e) => ((FlowPanel)d)._itemWidth = (double)e.NewValue), ValidatePositiveDouble);

        public static readonly DependencyProperty ResponsiveModeProperty =
            DependencyProperty.Register(nameof(ResponsiveMode), typeof(bool), typeof(FlowPanel), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => ((FlowPanel)d)._responsiveMode = (bool)e.NewValue));

        public double HorizontalSpacing
        {
            get => _horizontalSpacing;
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public double VerticalSpacing
        {
            get => _verticalSpacing;
            set => SetValue(VerticalSpacingProperty, value);
        }

        public HorizontalAlignment ContentAlignment
        {
            get => _contentAlignment;
            set => SetValue(ContentAlignmentProperty, value);
        }

        public bool UseMasonryLayout
        {
            get => _useMasonryLayout;
            set => SetValue(UseMasonryLayoutProperty, value);
        }

        public FlowOrientation Orientation
        {
            get => _orientation;
            set => SetValue(OrientationProperty, value);
        }

        public bool EqualizeItemSize
        {
            get => _equalizeItemSize;
            set => SetValue(EqualizeItemSizeProperty, value);
        }

        public double ItemWidth
        {
            get => _itemWidth;
            set => SetValue(ItemWidthProperty, value);
        }

        public bool ResponsiveMode
        {
            get => _responsiveMode;
            set => SetValue(ResponsiveModeProperty, value);
        }

        private double _horizontalSpacing = 20.0;
        private double _verticalSpacing = 10.0;
        private HorizontalAlignment _contentAlignment = HorizontalAlignment.Left;
        private bool _useMasonryLayout = true;
        private FlowOrientation _orientation = FlowOrientation.Unset;
        private bool _equalizeItemSize;
        private double _itemWidth = DefaultFallbackWidth;
        private bool _responsiveMode;

        private List<FlowItemGroup> _cachedLayout;
        private readonly List<UIElement> _visibleCache = new List<UIElement>();
        private readonly Stack<FlowItemGroup> _groupPool = new Stack<FlowItemGroup>();
        private readonly Stack<List<FlowItemGroup>> _listPool = new Stack<List<FlowItemGroup>>();

        private double[] _heightCache = new double[32];
        private double[] _itemWidths = new double[32];
        private double[] _itemHeights = new double[32];
        private double[] _naturalWidths = new double[32];
        private double[] _naturalHeights = new double[32];
        private int _naturalCacheCount;
        private double _naturalCacheMeasuredWidth = -1, _cachedMeasureWidth = -1;

        private static bool ValidateNonNegativeDouble(object value) => value is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d >= 0.0;

        private static bool ValidatePositiveDouble(object value) => value is double d && !double.IsNaN(d) && !double.IsInfinity(d) && d > 0.0;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            _naturalCacheCount = 0;
            _naturalCacheMeasuredWidth = -1;
        }

        private void EnsureItemCaches(int count)
        {
            if (_itemWidths.Length < count)
            {
                int newSize = Math.Max(_itemWidths.Length * 2, count);
                Array.Resize(ref _itemWidths, newSize);
                Array.Resize(ref _itemHeights, newSize);
                Array.Resize(ref _naturalWidths, newSize);
                Array.Resize(ref _naturalHeights, newSize);
            }
        }

        private void SnapshotChildSizes(int count)
        {
            EnsureItemCaches(count);
            bool equalize = _equalizeItemSize;
            double fixedWidth = _itemWidth;

            for (int i = 0; i < count; i++)
            {
                Size desired = _visibleCache[i].DesiredSize;
                _itemWidths[i] = equalize ? fixedWidth : desired.Width;
                _itemHeights[i] = desired.Height;
            }
        }

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
                    FlowItemGroup group = groups[i];
                    group.Elements.Clear();
                    group.Width = 0;
                    group.Height = 0;
                    _groupPool.Push(group);
                }

                groups.Clear();
                _listPool.Push(groups);
            }
        }

        private bool UpdateVisibleCache()
        {
            bool unchanged = true;
            int prevCount = _visibleCache.Count;
            int visIndex = 0;

            UIElementCollection children = InternalChildren;
            if (children != null)
            {
                int childCount = children.Count;
                for (int i = 0; i < childCount; i++)
                {
                    UIElement child = children[i];
                    if (child != null && child.Visibility != Visibility.Collapsed)
                    {
                        if (visIndex < prevCount)
                        {
                            if (!ReferenceEquals(_visibleCache[visIndex], child))
                            {
                                _visibleCache[visIndex] = child;
                                unchanged = false;
                            }
                        }
                        else
                        {
                            _visibleCache.Add(child);
                            unchanged = false;
                        }

                        if (!child.IsMeasureValid)
                        {
                            unchanged = false;
                        }

                        visIndex++;
                    }
                }
            }

            if (visIndex < prevCount)
            {
                _visibleCache.RemoveRange(visIndex, prevCount - visIndex);
                unchanged = false;
            }

            return unchanged;
        }

        private double GetResponsiveAlignmentOffset(double outerWidth, double contentWidth)
        {
            double extra = Math.Max(0, outerWidth - contentWidth);
            return _contentAlignment switch
            {
                HorizontalAlignment.Center => extra / 2.0,
                HorizontalAlignment.Right => extra,
                _ => 0
            };
        }

        private double GetResponsiveHorizontalRequired(double totalItemWidth, int count) => totalItemWidth + (count > 1 ? ResponsiveGap : 0) + (count > 2 ? (count - 2) * _horizontalSpacing : 0);

        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                bool childrenUnchanged = UpdateVisibleCache();
                int count = _visibleCache.Count;
                if (count == 0)
                {
                    if (_cachedLayout != null)
                    {
                        ReleaseGroups(_cachedLayout);
                        _cachedLayout = null;
                    }

                    _naturalCacheCount = 0;
                    _cachedMeasureWidth = 0;
                    return new Size(0, 0);
                }

                double aw = availableSize.Width;
                double ah = availableSize.Height;
                bool isInfiniteWidth = double.IsInfinity(aw);
                double maxWidth = MaxWidth;
                double fallbackWidth = (double.IsInfinity(maxWidth) || double.IsNaN(maxWidth) || maxWidth <= 0) ? DefaultFallbackWidth : maxWidth;
                double finiteAw = (isInfiniteWidth || double.IsNaN(aw) || aw <= 0) ? fallbackWidth : aw;

                if (!_responsiveMode && childrenUnchanged && _cachedLayout != null && _naturalCacheCount == count && finiteAw == _naturalCacheMeasuredWidth)
                {
                    return new Size(_cachedMeasureWidth, CalculateLayoutHeight(_cachedLayout));
                }

                if (_responsiveMode)
                {
                    Size infSize = new Size(finiteAw, double.PositiveInfinity);

                    for (int i = 1; i < count; i++)
                    {
                        _visibleCache[i].Measure(infSize);
                    }

                    if (count > 1)
                    {
                        double lastChildWidth = _visibleCache[count - 1].DesiredSize.Width;
                        double firstMaxWidth = Math.Max(0, finiteAw - lastChildWidth - ResponsiveGap);
                        _visibleCache[0].Measure(new Size(firstMaxWidth, double.PositiveInfinity));
                    }
                    else
                    {
                        _visibleCache[0].Measure(infSize);
                    }

                    if (_cachedLayout != null)
                    {
                        ReleaseGroups(_cachedLayout);
                        _cachedLayout = null;
                    }

                    double totalItemWidth = 0;
                    double maxItemHeight = 0;
                    double totalVerticalHeight = 0;
                    double maxItemWidth = 0;

                    for (int i = 0; i < count; i++)
                    {
                        Size desired = _visibleCache[i].DesiredSize;
                        double cw = desired.Width;
                        double ch = desired.Height;
                        totalItemWidth += cw;
                        totalVerticalHeight += ch;

                        if (ch > maxItemHeight)
                        {
                            maxItemHeight = ch;
                        }

                        if (cw > maxItemWidth)
                        {
                            maxItemWidth = cw;
                        }
                    }

                    double horizontalRequired = GetResponsiveHorizontalRequired(totalItemWidth, count);
                    double lastChildHeight = _visibleCache[count - 1].DesiredSize.Height;
                    double verticalRequired = count > 1 ? Math.Max(totalVerticalHeight - lastChildHeight + (count - 2) * _verticalSpacing, lastChildHeight) : totalVerticalHeight;

                    bool fitsHorizontally = double.IsInfinity(aw) || horizontalRequired <= finiteAw;
                    double resultWidth = double.IsInfinity(aw) ? (fitsHorizontally ? horizontalRequired : maxItemWidth) : finiteAw;
                    double resultHeight = fitsHorizontally ? maxItemHeight : verticalRequired;

                    _cachedMeasureWidth = resultWidth;
                    return new Size(resultWidth, resultHeight);
                }

                EnsureItemCaches(count);

                bool isStretch = _contentAlignment == HorizontalAlignment.Stretch && !_equalizeItemSize && _orientation != FlowOrientation.Horizontal;
                double layoutWidthConstraint = double.IsInfinity(aw) ? double.PositiveInfinity : finiteAw;

                if (isStretch && _orientation == FlowOrientation.Unset && childrenUnchanged && _naturalCacheCount == count && finiteAw <= _naturalCacheMeasuredWidth)
                {
                    int estimatedColumns = EstimateOptimalColumnCount(layoutWidthConstraint, ah, count, _naturalWidths, _naturalHeights);
                    if (estimatedColumns > 1)
                    {
                        double equalWidth = Math.Max(0, (finiteAw - (estimatedColumns - 1) * _horizontalSpacing) / estimatedColumns);
                        Size stretchConstraint = new Size(equalWidth, double.PositiveInfinity);

                        for (int i = 0; i < count; i++)
                        {
                            UIElement child = _visibleCache[i];
                            child.Measure(stretchConstraint);
                            Size desired = child.DesiredSize;
                            _itemWidths[i] = desired.Width;
                            _itemHeights[i] = desired.Height;
                        }

                        if (_cachedLayout != null)
                        {
                            ReleaseGroups(_cachedLayout);
                        }

                        _cachedLayout = TryDistributeToColumns(estimatedColumns, ah, _visibleCache, _itemWidths, _itemHeights);
                        if (_cachedLayout != null)
                        {
                            double fastHeight = CalculateLayoutHeight(_cachedLayout);
                            double fastWidth = double.IsInfinity(aw) ? CalculateLayoutWidth(_cachedLayout) : finiteAw;
                            _cachedMeasureWidth = fastWidth;
                            return new Size(fastWidth, fastHeight);
                        }
                    }
                }

                bool equalize = _equalizeItemSize;
                double fixedWidth = _itemWidth;
                Size initialConstraint = equalize ? new Size(fixedWidth, double.PositiveInfinity) : new Size(finiteAw, double.PositiveInfinity);

                for (int i = 0; i < count; i++)
                {
                    UIElement child = _visibleCache[i];
                    child.Measure(initialConstraint);
                    Size desired = child.DesiredSize;
                    double w = equalize ? fixedWidth : desired.Width;
                    double h = desired.Height;
                    _itemWidths[i] = w;
                    _itemHeights[i] = h;
                    _naturalWidths[i] = w;
                    _naturalHeights[i] = h;
                }

                _naturalCacheCount = count;
                _naturalCacheMeasuredWidth = finiteAw;

                if (_cachedLayout != null)
                {
                    ReleaseGroups(_cachedLayout);
                }

                _cachedLayout = CreateOptimalColumns(layoutWidthConstraint, ah, _visibleCache, _itemWidths, _itemHeights);

                if (isStretch && _cachedLayout != null && _cachedLayout.Count > 1)
                {
                    int targetColumnCount = _cachedLayout.Count;
                    double equalWidth = Math.Max(0, (finiteAw - (targetColumnCount - 1) * _horizontalSpacing) / targetColumnCount);
                    Size stretchConstraint = new Size(equalWidth, double.PositiveInfinity);

                    for (int i = 0; i < count; i++)
                    {
                        UIElement child = _visibleCache[i];
                        child.Measure(stretchConstraint);
                        Size desired = child.DesiredSize;
                        _itemWidths[i] = desired.Width;
                        _itemHeights[i] = desired.Height;
                    }

                    ReleaseGroups(_cachedLayout);
                    if (_orientation == FlowOrientation.Vertical)
                    {
                        _cachedLayout = CreateWrappedColumns(ah, _visibleCache, _itemWidths, _itemHeights);
                    }
                    else
                    {
                        _cachedLayout = TryDistributeToColumns(targetColumnCount, ah, _visibleCache, _itemWidths, _itemHeights) ?? CreateOptimalColumns(layoutWidthConstraint, ah, _visibleCache, _itemWidths, _itemHeights);
                    }
                }

                double finalHeight = CalculateLayoutHeight(_cachedLayout);
                double finalWidth = double.IsInfinity(aw) ? CalculateLayoutWidth(_cachedLayout) : finiteAw;
                _cachedMeasureWidth = finalWidth;
                return new Size(finalWidth, finalHeight);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogDebug(ex);
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

            double width = (double.IsInfinity(finalSize.Width) || double.IsNaN(finalSize.Width) || finalSize.Width <= 0) ? (_cachedMeasureWidth > 0 ? _cachedMeasureWidth : DefaultFallbackWidth) : finalSize.Width;

            double totalWidth = 0;
            for (int i = 0; i < count; i++)
            {
                totalWidth += _visibleCache[i].DesiredSize.Width;
            }

            double horizontalRequired = GetResponsiveHorizontalRequired(totalWidth, count);
            double lastChildWidth = count > 1 ? _visibleCache[count - 1].DesiredSize.Width : 0;
            double hSpacing = _horizontalSpacing;
            double vSpacing = _verticalSpacing;

            if (horizontalRequired <= width || double.IsInfinity(finalSize.Width))
            {
                double x = 0;
                double rightGap = count == 2 ? ResponsiveGap : hSpacing;
                double availableLeftWidth = count > 1 ? Math.Max(0, width - lastChildWidth - rightGap) : width;

                for (int i = 0; i < count; i++)
                {
                    UIElement child = _visibleCache[i];
                    Size desired = child.DesiredSize;
                    double w = desired.Width;
                    double h = desired.Height;
                    double y = Math.Max(0, (finalSize.Height - h) / 2.0);

                    if (i == count - 1 && count > 1)
                    {
                        child.Arrange(new Rect(Math.Max(0, width - w), y, w, h));
                    }
                    else
                    {
                        double arrangeWidth = Math.Min(w, Math.Max(0, availableLeftWidth - x));
                        child.Arrange(new Rect(x, y, arrangeWidth, h));
                        x += arrangeWidth + (i == 0 ? ResponsiveGap : hSpacing);
                    }
                }
            }
            else
            {
                double y = 0;
                for (int i = 0; i < count; i++)
                {
                    UIElement child = _visibleCache[i];
                    Size desired = child.DesiredSize;
                    double w = desired.Width;
                    double h = desired.Height;

                    if (i == count - 1 && count > 1)
                    {
                        double buttonY = Math.Max(0, (_visibleCache[0].DesiredSize.Height - h) / 2.0);
                        child.Arrange(new Rect(Math.Max(0, width - w), buttonY, w, h));
                    }
                    else
                    {
                        double maxWidth = (i == 0 && count > 1) ? Math.Max(0, width - lastChildWidth - ResponsiveGap) : width;
                        double arrangeWidth = Math.Min(w, maxWidth);
                        double xOffset = GetResponsiveAlignmentOffset(maxWidth, arrangeWidth);
                        child.Arrange(new Rect(xOffset, y, arrangeWidth, h));
                        y += h + vSpacing;
                    }
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            try
            {
                int count = _visibleCache.Count;
                if (count == 0)
                {
                    return finalSize;
                }

                if (_responsiveMode)
                {
                    ArrangeResponsive(finalSize);
                    return finalSize;
                }

                if (_cachedLayout == null)
                {
                    SnapshotChildSizes(count);
                    _cachedLayout = CreateOptimalColumns(finalSize.Width, finalSize.Height, _visibleCache, _itemWidths, _itemHeights);
                }

                ArrangeAligned(_cachedLayout, finalSize.Width);

                return finalSize;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogDebug(ex);
                return finalSize;
            }
        }

        private double CalculateLayoutWidth(List<FlowItemGroup> layout)
        {
            if (layout == null || layout.Count == 0)
            {
                return 0;
            }

            if (_orientation == FlowOrientation.Horizontal)
            {
                double maxRowWidth = 0;
                for (int i = 0; i < layout.Count; i++)
                {
                    if (layout[i].Width > maxRowWidth)
                    {
                        maxRowWidth = layout[i].Width;
                    }
                }

                return maxRowWidth;
            }

            return CalculateTotalWidth(layout);
        }

        private double CalculateLayoutHeight(List<FlowItemGroup> layout)
        {
            if (layout != null && layout.Count != 0)
            {
                if (_orientation == FlowOrientation.Horizontal)
                {
                    double neededHeight = 0;
                    double vSpacing = _verticalSpacing;
                    for (int i = 0; i < layout.Count; i++)
                    {
                        neededHeight += layout[i].Height;
                        if (i > 0)
                        {
                            neededHeight += vSpacing;
                        }
                    }

                    return neededHeight;
                }

                return CalculateMaxColumnHeight(layout);
            }

            return 0;
        }

        private void ArrangeAligned(List<FlowItemGroup> groups, double availableWidth)
        {
            if (groups != null && groups.Count != 0)
            {
                if (_orientation == FlowOrientation.Horizontal)
                {
                    ArrangeHorizontal(groups, availableWidth);
                }
                else
                {
                    ArrangeVertical(groups, availableWidth);
                }
            }
        }

        private void ArrangeHorizontal(List<FlowItemGroup> groups, double availableWidth)
        {
            double y = 0;
            double hSpacing = _horizontalSpacing;
            double vSpacing = _verticalSpacing;
            bool equalize = _equalizeItemSize;
            double fixedWidth = _itemWidth;
            HorizontalAlignment alignment = _contentAlignment;

            for (int i = 0; i < groups.Count; i++)
            {
                FlowItemGroup row = groups[i];
                List<UIElement> elements = row.Elements;
                int elemCount = elements.Count;
                if (elemCount == 0)
                {
                    y += row.Height + vSpacing;
                    continue;
                }

                double extraSpace = Math.Max(0, availableWidth - row.Width);
                double x = alignment switch
                {
                    HorizontalAlignment.Center => extraSpace / 2.0,
                    HorizontalAlignment.Right => extraSpace,
                    _ => 0
                };

                for (int j = 0; j < elemCount; j++)
                {
                    UIElement child = elements[j];
                    Size desired = child.DesiredSize;
                    double w = equalize ? fixedWidth : desired.Width;
                    double h = desired.Height;
                    child.Arrange(new Rect(x, y, w, h));
                    x += w + hSpacing;
                }

                y += row.Height + vSpacing;
            }
        }

        private void ArrangeVertical(List<FlowItemGroup> groups, double availableWidth)
        {
            int groupCount = groups.Count;
            double hSpacing = _horizontalSpacing;
            double vSpacing = _verticalSpacing;
            bool equalize = _equalizeItemSize;
            double fixedWidth = _itemWidth;
            HorizontalAlignment alignment = _contentAlignment;

            double totalColumnsWidth = CalculateTotalWidth(groups);
            double extraSpace = Math.Max(0, availableWidth - totalColumnsWidth);

            bool isStretch = alignment == HorizontalAlignment.Stretch && !equalize;
            bool applySpread = !equalize && groupCount > 1 && !isStretch;

            double spreadSpace = applySpread ? extraSpace * ColumnSpreadRatio : 0;
            double alignSpace = extraSpace - spreadSpace;

            double xV = 0;
            if (!isStretch)
            {
                xV = alignment switch
                {
                    HorizontalAlignment.Center => alignSpace / 2.0,
                    HorizontalAlignment.Right => alignSpace,
                    _ => 0
                };
            }

            double equalWidth = isStretch ? Math.Max(0, (availableWidth - (groupCount - 1) * hSpacing) / groupCount) : 0;
            double stepSpread = applySpread ? spreadSpace / (groupCount - 1) : 0;

            for (int i = 0; i < groupCount; i++)
            {
                FlowItemGroup col = groups[i];
                List<UIElement> elements = col.Elements;
                int elemCount = elements.Count;
                double columnX = xV;
                double y = 0;
                double actualColWidth = isStretch ? equalWidth : col.Width;

                for (int j = 0; j < elemCount; j++)
                {
                    UIElement child = elements[j];
                    Size desired = child.DesiredSize;
                    double w = isStretch ? actualColWidth : (equalize ? fixedWidth : desired.Width);
                    double h = desired.Height;

                    child.Arrange(new Rect(columnX, y, w, h));
                    y += h + vSpacing;
                }

                xV += actualColWidth + hSpacing + stepSpread;
            }
        }

        private double CalculateMaxColumnHeight(List<FlowItemGroup> columns)
        {
            double max = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].Height > max)
                {
                    max = columns[i].Height;
                }
            }

            return max;
        }

        private int EstimateOptimalColumnCount(double availableWidth, double availableHeight, int count, double[] widths, double[] heights)
        {
            double sum = 0.0;
            int nonZero = 0;
            for (int i = 0; i < count; i++)
            {
                double w = widths[i];
                if (w > 0)
                {
                    sum += w;
                    nonZero++;
                }
            }

            double hSpacing = _horizontalSpacing;
            double basisWidth = Math.Max(MinBasisWidth, nonZero > 0 ? sum / nonZero : 100.0);
            int maxPossibleColumns = double.IsInfinity(availableWidth) ? count : Math.Max(1, (int)((availableWidth + hSpacing) / (basisWidth + hSpacing)));

            for (int columnCount = Math.Min(maxPossibleColumns, count); columnCount >= 1; columnCount--)
            {
                List<FlowItemGroup> columns = TryDistributeToColumns(columnCount, availableHeight, _visibleCache, widths, heights);
                bool fits = columns != null && columns.Count > 0 && CalculateTotalWidth(columns) <= availableWidth;
                ReleaseGroups(columns);
                if (fits)
                {
                    return columnCount;
                }
            }

            return 1;
        }

        private List<FlowItemGroup> CreateOptimalColumns(double availableWidth, double availableHeight, List<UIElement> visibleChildren, double[] widths, double[] heights)
        {
            int childCount = visibleChildren.Count;
            if (childCount == 0)
            {
                return GetGroupList();
            }

            if (_orientation == FlowOrientation.Horizontal)
            {
                return CreateWrappedRows(availableWidth, visibleChildren, widths, heights);
            }

            if (_orientation == FlowOrientation.Vertical)
            {
                return CreateWrappedColumns(availableHeight, visibleChildren, widths, heights);
            }

            double basisWidth;
            if (_equalizeItemSize)
            {
                basisWidth = Math.Max(MinBasisWidth, _itemWidth);
            }
            else
            {
                double sum = 0.0;
                int nonZero = 0;
                for (int i = 0; i < childCount; i++)
                {
                    double w = widths[i];
                    if (w > 0)
                    {
                        sum += w;
                        nonZero++;
                    }
                }

                basisWidth = Math.Max(MinBasisWidth, nonZero > 0 ? sum / nonZero : 100.0);
            }

            double hSpacing = _horizontalSpacing;
            int maxPossibleColumns = double.IsInfinity(availableWidth) ? childCount : Math.Max(1, (int)((availableWidth + hSpacing) / (basisWidth + hSpacing)));

            for (int columnCount = Math.Min(maxPossibleColumns, childCount); columnCount >= 1; columnCount--)
            {
                List<FlowItemGroup> columns = TryDistributeToColumns(columnCount, availableHeight, visibleChildren, widths, heights);
                if (columns != null && columns.Count > 0 && CalculateTotalWidth(columns) <= availableWidth)
                {
                    return columns;
                }

                ReleaseGroups(columns);
            }

            return CreateSingleColumn(visibleChildren, widths, heights);
        }

        private List<FlowItemGroup> CreateWrappedRows(double maxWidth, List<UIElement> visibleChildren, double[] widths, double[] heights)
        {
            List<FlowItemGroup> rows = GetGroupList();
            FlowItemGroup current = null;
            double hSpacing = _horizontalSpacing;

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                UIElement child = visibleChildren[i];
                double childWidth = widths[i];
                double childHeight = heights[i];

                if (current == null)
                {
                    current = GetGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    rows.Add(current);
                    continue;
                }

                double widthIfAdded = current.Width + (current.Elements.Count > 0 ? hSpacing : 0) + childWidth;

                if (widthIfAdded <= maxWidth || current.Elements.Count == 0)
                {
                    current.Elements.Add(child);
                    current.Width = widthIfAdded;
                    if (childHeight > current.Height)
                    {
                        current.Height = childHeight;
                    }
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

            return rows;
        }

        private List<FlowItemGroup> CreateWrappedColumns(double maxHeight, List<UIElement> visibleChildren, double[] widths, double[] heights)
        {
            List<FlowItemGroup> cols = GetGroupList();
            FlowItemGroup current = null;
            double vSpacing = _verticalSpacing;

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                UIElement child = visibleChildren[i];
                double childWidth = widths[i];
                double childHeight = heights[i];

                if (current == null)
                {
                    current = GetGroup();
                    current.Elements.Add(child);
                    current.Width = childWidth;
                    current.Height = childHeight;
                    cols.Add(current);
                    continue;
                }

                double heightIfAdded = current.Height + (current.Elements.Count > 0 ? vSpacing : 0) + childHeight;

                if (heightIfAdded <= maxHeight || current.Elements.Count == 0)
                {
                    current.Elements.Add(child);
                    current.Height = heightIfAdded;
                    if (childWidth > current.Width)
                    {
                        current.Width = childWidth;
                    }
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

            return cols;
        }

        private List<FlowItemGroup> TryDistributeToColumns(int columnCount, double maxHeight, List<UIElement> visibleChildren, double[] widths, double[] heights)
        {
            List<FlowItemGroup> columns = GetGroupList();
            for (int i = 0; i < columnCount; i++)
            {
                columns.Add(GetGroup());
            }

            double vSpacing = _verticalSpacing;
            bool hasHeightLimit = !double.IsInfinity(maxHeight);
            int childCount = visibleChildren.Count;

            if (_useMasonryLayout)
            {
                if (_heightCache.Length < columnCount)
                {
                    Array.Resize(ref _heightCache, Math.Max(_heightCache.Length * 2, columnCount));
                }

                Array.Clear(_heightCache, 0, columnCount);

                for (int c = 0; c < childCount; c++)
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

                    FlowItemGroup targetCol = columns[minIndex];
                    double newHeight = minH + (targetCol.Elements.Count > 0 ? vSpacing : 0) + heights[c];

                    if (hasHeightLimit && newHeight > maxHeight)
                    {
                        ReleaseGroups(columns);
                        return null;
                    }

                    targetCol.Elements.Add(visibleChildren[c]);
                    if (widths[c] > targetCol.Width)
                    {
                        targetCol.Width = widths[c];
                    }

                    _heightCache[minIndex] = newHeight;
                }

                for (int i = 0; i < columnCount; i++)
                {
                    columns[i].Height = _heightCache[i];
                }
            }
            else
            {
                for (int index = 0; index < childCount; index++)
                {
                    int colIndex = index % columnCount;
                    FlowItemGroup targetCol = columns[colIndex];

                    double newHeight = targetCol.Height + (targetCol.Elements.Count > 0 ? vSpacing : 0) + heights[index];

                    if (hasHeightLimit && newHeight > maxHeight)
                    {
                        ReleaseGroups(columns);
                        return null;
                    }

                    targetCol.Elements.Add(visibleChildren[index]);
                    if (widths[index] > targetCol.Width)
                    {
                        targetCol.Width = widths[index];
                    }

                    targetCol.Height = newHeight;
                }
            }

            return columns;
        }

        private List<FlowItemGroup> CreateSingleColumn(List<UIElement> visibleChildren, double[] widths, double[] heights)
        {
            List<FlowItemGroup> list = GetGroupList();
            FlowItemGroup col = GetGroup();
            double totalHeight = 0;
            double vSpacing = _verticalSpacing;
            int childCount = visibleChildren.Count;

            for (int i = 0; i < childCount; i++)
            {
                col.Elements.Add(visibleChildren[i]);
                if (widths[i] > col.Width)
                {
                    col.Width = widths[i];
                }

                totalHeight += heights[i];
                if (i > 0)
                {
                    totalHeight += vSpacing;
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
                double hSpacing = _horizontalSpacing;
                for (int i = 0; i < columns.Count; i++)
                {
                    total += columns[i].Width;
                    if (i > 0)
                    {
                        total += hSpacing;
                    }
                }

                return total;
            }

            return 0;
        }
    }
}