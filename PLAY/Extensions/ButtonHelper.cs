using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PLAY.Extensions
{
    public class ButtonHelper
    {
        public static bool? GetDialogResult(DependencyObject obj)
        {
            return (bool?) obj.GetValue(DialogResultProperty);
        }

        public static void SetDialogResult(DependencyObject obj, bool? value)
        {
            obj.SetValue(DialogResultProperty, value);
        }

        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof (bool?), typeof (ButtonHelper),
                new UIPropertyMetadata
                {
                    PropertyChangedCallback = (obj, e) =>
                    {
                        Button button = obj as Button;
                        if (button == null)
                            throw new InvalidOperationException("DialogResult is working, but not other controls.");
                        button.Click += (sender, e2) =>
                        {
                            Window.GetWindow(button).DialogResult = GetDialogResult(button);
                        };
                    }
                });
    }
}
