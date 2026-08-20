using SetwanDokuFoto.Core.Models;
using SetwanDokuFoto.UI.Controls;
using SetwanDokuFoto.UI.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SetwanDokuFoto.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void PhotoFrame_OnPhotoSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not PhotoFrameControl selected) return;
        foreach (var control in FindVisualChildren<PhotoFrameControl>(this))
            control.SetSelected(ReferenceEquals(control, selected));

        _viewModel.SelectedCell = selected.DataContext as CollageCell;
        _viewModel.StatusMessage = "Frame dipilih — double-click foto untuk crop";
        e.Handled = true;
    }

    private async void PhotoFrame_OnRequestPhoto(object sender, RoutedEventArgs e)
    {
        if (sender is PhotoFrameControl { DataContext: CollageCell cell })
            await _viewModel.ImportPhotoForCellAsync(cell);
        e.Handled = true;
    }

    private void PhotoFrame_OnCropModeChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PhotoFrameControl control) return;
        _viewModel.IsCropActive = control.IsCropActive;
        _viewModel.StatusMessage = control.IsCropActive
            ? "Mode crop aktif — tarik foto, scroll untuk zoom, Enter simpan, Esc batal"
            : "Crop disimpan secara non-destruktif";
        e.Handled = true;
    }

    private PhotoFrameControl? SelectedControl() =>
        FindVisualChildren<PhotoFrameControl>(this).FirstOrDefault(control => control.IsSelected);

    private void Crop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.BeginCrop();
    private void RotateLeft_Click(object sender, RoutedEventArgs e) => SelectedControl()?.Rotate(-15);
    private void RotateRight_Click(object sender, RoutedEventArgs e) => SelectedControl()?.Rotate(15);
    private void ResetCrop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.ResetCrop();
    private void UndoCrop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.UndoCrop();
    private void RedoCrop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.RedoCrop();
    private void CancelCrop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.CancelCrop();
    private void CommitCrop_Click(object sender, RoutedEventArgs e) => SelectedControl()?.CommitCrop();

    private void FlipHorizontal_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedCell?.Photo is { } photo)
            photo.Transform.FlipHorizontal = !photo.Transform.FlipHorizontal;
    }

    private void FlipVertical_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedCell?.Photo is { } photo)
            photo.Transform.FlipVertical = !photo.Transform.FlipVertical;
    }

    private async void ReplacePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedCell is { } cell)
            await _viewModel.ImportPhotoForCellAsync(cell);
    }

    private void RemovePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedCell is not { } cell) return;
        cell.Photo = null;
        _viewModel.StatusMessage = "Foto dihapus dari frame; file asli tidak dihapus";
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var selected = SelectedControl();
        if (e.Key == Key.Escape && selected?.IsCropActive == true)
        {
            selected.CancelCrop();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && selected?.IsCropActive == true)
        {
            selected.CommitCrop();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
        {
            selected?.UndoCrop();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
        {
            selected?.RedoCrop();
            e.Handled = true;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }
}
