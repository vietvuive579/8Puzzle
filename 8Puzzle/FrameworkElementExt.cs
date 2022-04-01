using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace _8Puzzle
{
    public static class FrameworkElementExt
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            Panel parent = element.Parent as Panel;
            if (parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
              .Where(x => x != element)
              .Select(x => Panel.GetZIndex(x))
              .Max();
            Panel.SetZIndex(element, maxZ + 1);
        }
    }
}
