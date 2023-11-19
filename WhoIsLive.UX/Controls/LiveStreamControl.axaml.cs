using System;
using Avalonia.Controls;
using WhoIsLive.UX.ViewModels.Controls;

namespace WhoIsLive.UX.Controls;

#nullable enable

public partial class LiveStreamControl : UserControl
{
    public LiveStreamControl()
    {
        InitializeComponent();
    }

    protected override void OnDataContextBeginUpdate()
    {
        if (DataContext is LiveStreamViewModel viewModel)
            viewModel.CopyStreamURLToClipboardRequested -= OnCopyStreamURLToClipboardRequested;

        base.OnDataContextBeginUpdate();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is LiveStreamViewModel viewModel)
            viewModel.CopyStreamURLToClipboardRequested += OnCopyStreamURLToClipboardRequested;

        base.OnDataContextChanged(e);
    }

    private void OnCopyStreamURLToClipboardRequested(object? sender, string url)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null && topLevel.Clipboard != null)
        {
            topLevel.Clipboard.SetTextAsync(url);
        }
    }
}