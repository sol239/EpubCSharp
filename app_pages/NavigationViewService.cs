using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace EpubReader.app_pages
{
    public class NavigationViewService
    {
        public static event Action<Visibility> NavigationViewVisibilityChanged;

        public static void SetNavigationViewVisibility(Visibility visibility)
        {
            NavigationViewVisibilityChanged?.Invoke(visibility);
        }
    }
}
