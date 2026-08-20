using SetwanDokuFoto.Core.Models;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SetwanDokuFoto.UI.Controls;

public partial class PhotoFrameControl : UserControl
{
    public static readonly RoutedEvent PhotoSelectedEvent = EventManager.RegisterRoutedEvent(
        nameof(PhotoSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PhotoFrameControl));

    public static readonly RoutedEvent RequestPhotoEvent = EventManager.RegisterRoutedEvent(
        nameof(RequestPhoto), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PhotoFrameControl));

    public static readonly RoutedEvent CropModeChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(CropModeChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PhotoFrameControl));

    private readonly Stack<PhotoTransformSnapshot> _undo = new();
    private readonly Stack<PhotoTransformSnapshot> _redo = new();
    private CollageCell? _cell;
    private PhotoTransform? _observedTransform;
    private BitmapSource? _source;
    private Point _lastPointer;
    private bool _isDragging;
    private bool _isRendering;
    private PhotoTransformSnapshot _gestureStart;
    private PhotoTransformSnapshot? _cropSessionStart;

    public PhotoFrameControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += (_, _) => DetachModel();
    }

    public event RoutedEventHandler PhotoSelected
    {
        add => AddHandler(PhotoSelectedEvent, value);
        remove => RemoveHandler(PhotoSelectedEvent, value);
    }

    public event RoutedEventHandler RequestPhoto
    {
        add => AddHandler(RequestPhotoEvent, value);
        remove => RemoveHandler(RequestPhotoEvent, value);
    }

    public event RoutedEventHandler CropModeChanged
    {
        add => AddHandler(CropModeChangedEvent, value);
        remove => RemoveHandler(CropModeChangedEvent, value);
    }

    public bool IsSelected { get; private set; }
    public bool IsCropActive { get; private set; }

    public void SetSelected(bool value)
    {
        IsSelected = value;
        SelectionBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        if (!value && IsCropActive) CommitCrop();
    }

    public void BeginCrop()
    {
        if (_cell?.Photo is null || IsCropActive) return;
        IsCropActive = true;
        _cropSessionStart = _cell.Photo.Transform.Snapshot();
        OutsideCanvas.Visibility = Visibility.Visible;
        CropOverlay.Visibility = Visibility.Visible;
        CaptionChrome.Visibility = Visibility.Collapsed;
        Panel.SetZIndex(this, 1000);
        Focus();
        RaiseEvent(new RoutedEventArgs(CropModeChangedEvent, this));
    }

    public void CommitCrop()
    {
        if (!IsCropActive) return;
        IsCropActive = false;
        _cropSessionStart = null;
        OutsideCanvas.Visibility = Visibility.Collapsed;
        CropOverlay.Visibility = Visibility.Collapsed;
        CaptionChrome.Visibility = _cell?.ShowCaption == true ? Visibility.Visible : Visibility.Collapsed;
        Panel.SetZIndex(this, 0);
        RaiseEvent(new RoutedEventArgs(CropModeChangedEvent, this));
    }

    public void CancelCrop()
    {
        if (_cell?.Photo is null || !IsCropActive) return;
        if (_cropSessionStart is { } snapshot) _cell.Photo.Transform.Restore(snapshot);
        CommitCrop();
    }

    public void ResetCrop()
    {
        if (_cell?.Photo is null) return;
        RecordUndo(_cell.Photo.Transform.Snapshot());
        _cell.Photo.Transform.Reset();
        RenderPhoto();
    }

    public void Rotate(double degrees)
    {
        if (_cell?.Photo is null) return;
        RecordUndo(_cell.Photo.Transform.Snapshot());
        _cell.Photo.Transform.Rotation += degrees;
        ClampOffsets();
        RenderPhoto();
    }

    public void Zoom(double factor)
    {
        if (_cell?.Photo is null) return;
        ApplyZoom(factor, new Point(ActualWidth / 2, ActualHeight / 2));
    }

    public void UndoCrop()
    {
        if (_cell?.Photo is null || _undo.Count == 0) return;
        _redo.Push(_cell.Photo.Transform.Snapshot());
        _cell.Photo.Transform.Restore(_undo.Pop());
    }

    public void RedoCrop()
    {
        if (_cell?.Photo is null || _redo.Count == 0) return;
        _undo.Push(_cell.Photo.Transform.Snapshot());
        _cell.Photo.Transform.Restore(_redo.Pop());
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        DetachModel();
        _cell = e.NewValue as CollageCell;
        if (_cell is not null) _cell.PropertyChanged += OnCellPropertyChanged;
        AttachTransform();
        LoadPhoto();
    }

    private void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CollageCell.Photo))
        {
            AttachTransform();
            LoadPhoto();
        }
        else if (e.PropertyName == nameof(CollageCell.ShowCaption))
        {
            CaptionChrome.Visibility = _cell?.ShowCaption == true && !IsCropActive
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void AttachTransform()
    {
        if (_observedTransform is not null) _observedTransform.PropertyChanged -= OnTransformChanged;
        _observedTransform = _cell?.Photo?.Transform;
        if (_observedTransform is not null) _observedTransform.PropertyChanged += OnTransformChanged;
    }

    private void DetachModel()
    {
        if (_cell is not null) _cell.PropertyChanged -= OnCellPropertyChanged;
        if (_observedTransform is not null) _observedTransform.PropertyChanged -= OnTransformChanged;
        _observedTransform = null;
        _cell = null;
    }

    private void OnTransformChanged(object? sender, PropertyChangedEventArgs e) => RenderPhoto();

    private void LoadPhoto()
    {
        _source = null;
        var photo = _cell?.Photo;
        try
        {
            if (photo is not null && File.Exists(photo.FilePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 1600;
                bitmap.UriSource = new Uri(photo.FilePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                _source = bitmap;
            }
            else if (photo?.ImageBytes is { Length: > 0 } bytes)
            {
                using var stream = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 1600;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                _source = bitmap;
            }
        }
        catch
        {
            _source = null;
        }

        InsidePhoto.Source = _source;
        OutsidePhoto.Source = _source;
        Placeholder.Visibility = _source is null ? Visibility.Visible : Visibility.Collapsed;
        CaptionChrome.Visibility = _cell?.ShowCaption == true && !IsCropActive
            ? Visibility.Visible
            : Visibility.Collapsed;
        RenderPhoto();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => RenderPhoto();

    private void RenderPhoto()
    {
        if (_isRendering || _source is null || _cell?.Photo is null || ActualWidth <= 0 || ActualHeight <= 0) return;
        _isRendering = true;
        try
        {
            var transform = _cell.Photo.Transform;
            var baseScale = CanvaCropMath.CalculateCoverScale(
                ActualWidth, ActualHeight, _source.PixelWidth, _source.PixelHeight);
            var coveredWidth = _source.PixelWidth * baseScale;
            var coveredHeight = _source.PixelHeight * baseScale;
            var rotationCover = CanvaCropMath.CalculateRotationCoverMultiplier(
                ActualWidth, ActualHeight, coveredWidth, coveredHeight, transform.Rotation);
            var effectiveScale = Math.Max(transform.Scale, rotationCover);
            var displayWidth = coveredWidth * effectiveScale;
            var displayHeight = coveredHeight * effectiveScale;

            ClampOffsets(displayWidth, displayHeight);

            var left = (ActualWidth - displayWidth) / 2 + transform.OffsetX * ActualWidth;
            var top = (ActualHeight - displayHeight) / 2 + transform.OffsetY * ActualHeight;
            var flipX = transform.FlipHorizontal ? -1d : 1d;
            var flipY = transform.FlipVertical ? -1d : 1d;

            ApplyImageLayout(InsidePhoto, left, top, displayWidth, displayHeight, flipX, flipY, transform.Rotation);
            ApplyImageLayout(OutsidePhoto, left, top, displayWidth, displayHeight, flipX, flipY, transform.Rotation);
        }
        finally
        {
            _isRendering = false;
        }
    }

    private static void ApplyImageLayout(
        Image image,
        double left,
        double top,
        double width,
        double height,
        double flipX,
        double flipY,
        double rotation)
    {
        image.Width = width;
        image.Height = height;
        Canvas.SetLeft(image, left);
        Canvas.SetTop(image, top);
        image.RenderTransform = new TransformGroup
        {
            Children = new TransformCollection
            {
                new ScaleTransform(flipX, flipY),
                new RotateTransform(rotation)
            }
        };
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(PhotoSelectedEvent, this));

        if (_source is null)
        {
            RaiseEvent(new RoutedEventArgs(RequestPhotoEvent, this));
            e.Handled = true;
            return;
        }

        if (e.ClickCount == 2)
        {
            BeginCrop();
            e.Handled = true;
            return;
        }

        if (!IsCropActive || _cell?.Photo is null) return;
        _gestureStart = _cell.Photo.Transform.Snapshot();
        _lastPointer = e.GetPosition(this);
        _isDragging = true;
        CaptureMouse();
        e.Handled = true;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _cell?.Photo is null) return;
        var current = e.GetPosition(this);
        var delta = current - _lastPointer;
        _lastPointer = current;
        _cell.Photo.Transform.OffsetX += delta.X / Math.Max(1, ActualWidth);
        _cell.Photo.Transform.OffsetY += delta.Y / Math.Max(1, ActualHeight);
        ClampOffsets();
        e.Handled = true;
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _cell?.Photo is null) return;
        _isDragging = false;
        ReleaseMouseCapture();
        RecordUndo(_gestureStart);
        e.Handled = true;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsCropActive || _cell?.Photo is null) return;
        ApplyZoom(e.Delta > 0 ? 1.1 : 1 / 1.1, e.GetPosition(this));
        e.Handled = true;
    }

    private void ApplyZoom(double factor, Point focus)
    {
        if (_cell?.Photo is null) return;
        var transform = _cell.Photo.Transform;
        var before = transform.Snapshot();
        var oldScale = transform.Scale;
        var newScale = Math.Clamp(oldScale * factor, 1, 8);
        if (Math.Abs(newScale - oldScale) < .0001) return;

        var ratio = newScale / oldScale;
        var focusX = focus.X - ActualWidth / 2;
        var focusY = focus.Y - ActualHeight / 2;
        var offsetX = transform.OffsetX * ActualWidth;
        var offsetY = transform.OffsetY * ActualHeight;

        transform.OffsetX = (focusX - ratio * (focusX - offsetX)) / Math.Max(1, ActualWidth);
        transform.OffsetY = (focusY - ratio * (focusY - offsetY)) / Math.Max(1, ActualHeight);
        transform.Scale = newScale;
        ClampOffsets();
        RecordUndo(before);
    }

    private void ClampOffsets()
    {
        if (_source is null || _cell?.Photo is null || ActualWidth <= 0 || ActualHeight <= 0) return;
        var transform = _cell.Photo.Transform;
        var baseScale = CanvaCropMath.CalculateCoverScale(
            ActualWidth, ActualHeight, _source.PixelWidth, _source.PixelHeight);
        var coveredWidth = _source.PixelWidth * baseScale;
        var coveredHeight = _source.PixelHeight * baseScale;
        var rotationCover = CanvaCropMath.CalculateRotationCoverMultiplier(
            ActualWidth, ActualHeight, coveredWidth, coveredHeight, transform.Rotation);
        var effectiveScale = Math.Max(transform.Scale, rotationCover);
        ClampOffsets(coveredWidth * effectiveScale, coveredHeight * effectiveScale);
    }

    private void ClampOffsets(double displayWidth, double displayHeight)
    {
        if (_cell?.Photo is null) return;
        var transform = _cell.Photo.Transform;
        var desiredX = transform.OffsetX;
        var desiredY = transform.OffsetY;
        if (CoversFrame(desiredX, desiredY, displayWidth, displayHeight, transform.Rotation)) return;

        var low = 0d;
        var high = 1d;
        for (var i = 0; i < 24; i++)
        {
            var mid = (low + high) / 2;
            if (CoversFrame(desiredX * mid, desiredY * mid, displayWidth, displayHeight, transform.Rotation))
                low = mid;
            else
                high = mid;
        }

        transform.OffsetX = desiredX * low;
        transform.OffsetY = desiredY * low;
    }

    private bool CoversFrame(double offsetX, double offsetY, double imageWidth, double imageHeight, double angle)
    {
        var radians = angle * Math.PI / 180d;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var centerX = offsetX * ActualWidth;
        var centerY = offsetY * ActualHeight;
        var halfFrameWidth = ActualWidth / 2;
        var halfFrameHeight = ActualHeight / 2;
        var halfImageWidth = imageWidth / 2 + .01;
        var halfImageHeight = imageHeight / 2 + .01;

        var corners = new[]
        {
            new Point(-halfFrameWidth, -halfFrameHeight),
            new Point(halfFrameWidth, -halfFrameHeight),
            new Point(halfFrameWidth, halfFrameHeight),
            new Point(-halfFrameWidth, halfFrameHeight)
        };

        foreach (var corner in corners)
        {
            var x = corner.X - centerX;
            var y = corner.Y - centerY;
            var localX = x * cos + y * sin;
            var localY = -x * sin + y * cos;
            if (Math.Abs(localX) > halfImageWidth || Math.Abs(localY) > halfImageHeight) return false;
        }

        return true;
    }

    private void RecordUndo(PhotoTransformSnapshot before)
    {
        if (_cell?.Photo is null || before.Equals(_cell.Photo.Transform.Snapshot())) return;
        _undo.Push(before);
        _redo.Clear();
    }
}
