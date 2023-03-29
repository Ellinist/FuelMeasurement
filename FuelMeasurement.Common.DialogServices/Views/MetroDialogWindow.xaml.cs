using MahApps.Metro.Controls;
using Prism.Services.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FuelMeasurement.Common.DialogModule.Views
{
    /// <summary>
    /// Interaction logic for CustomNotificationWindow.xaml
    /// </summary>
    public partial class MetroDialogWindow : MetroWindow, IDialogWindow
    {
        public MetroDialogWindow()
        {
            InitializeComponent();
            
        }
        
        public IDialogResult Result { get; set; }

        /// <summary>
        /// когда родительское окно распахнуто его актуальные размеры почему то больше на 16. Такая фишка.
        /// </summary>
        const int ADDED_PIXEL_BY_SYSTEM = 16;
        /// <summary>
        /// Насколько затухает родительское окно диалога
        /// </summary>
        const double OPACITY_FADE_VALUE = 0.5;

        private double _parentWindowDefaultOpacity = 1;

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            var parentWindow = this.Owner;
            if (parentWindow != null)
            {

                if (parentWindow.WindowState != WindowState.Maximized)
                {

                    this.SetValue(WidthProperty, parentWindow.ActualWidth);
                    this.SetCurrentValue(WidthProperty, parentWindow.ActualWidth);
                    this.Left = parentWindow.Left;
                }
                else
                {
                    this.SetValue(WidthProperty, parentWindow.ActualWidth - ADDED_PIXEL_BY_SYSTEM);
                    this.SetCurrentValue(WidthProperty, parentWindow.ActualWidth - ADDED_PIXEL_BY_SYSTEM);
                    var rect = GetWindowRectangle(parentWindow);
                    this.Left = rect.Left + ADDED_PIXEL_BY_SYSTEM / 2;
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Make sure RECT is actually OUR defined struct, not the windows rect.
        public static RECT GetWindowRectangle(Window window)
        {
            RECT rect;
            GetWindowRect(new WindowInteropHelper(window).Handle, out rect);

            return rect;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            var parentWindow = this.Owner;
            if (parentWindow != null)
            {
                parentWindow.Opacity = _parentWindowDefaultOpacity;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = this.Owner;
            if (parentWindow != null)
            {
                //это нельзя перенести в контент рендеред,он при закрытии тоже вызывается и все портит
                //а все из контет рендеред нельзя сюда потому что не работает
                //СЛавься Майкрософт бля
                _parentWindowDefaultOpacity = parentWindow.Opacity;
                parentWindow.Opacity = OPACITY_FADE_VALUE;
            }
        }
    }
}
