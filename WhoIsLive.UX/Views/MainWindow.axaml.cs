using System;
using Avalonia.Controls;

namespace WhoIsLive.UX.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;

        if (insetsManager != null && OperatingSystem.IsAndroid())
        {
            insetsManager.DisplayEdgeToEdgePreference = true;
            insetsManager.IsSystemBarVisible = false;
        }
        
        InitializeComponent();
    }
}