using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get { return Resources.MergedDictionaries[0]; }
        }
        public bool darkMode = false;
        ResourceDictionary darkThemeDict;
      
        public void SwitchTheme(bool dark)
        {
            darkMode = dark;
            if(darkMode)
            {
                if(darkThemeDict == null) darkThemeDict = Application.LoadComponent(new Uri("DarkTheme.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                //Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(darkThemeDict);
            }
            else
            {
                if (darkThemeDict == null) darkThemeDict = Application.LoadComponent(new Uri("DarkTheme.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
                //Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Remove(darkThemeDict);
            }
        }
    }
}
