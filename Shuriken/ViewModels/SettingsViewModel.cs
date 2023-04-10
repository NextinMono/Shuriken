using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shuriken.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public string Theme
        {
            get
            {
                return Properties.Settings.Default.DarkThemeEnabled ? "Dark" : "Light";
            }
            set
            {
                Properties.Settings.Default.DarkThemeEnabled = value == "Dark";
                var app = (App)Application.Current;
                app.SwitchTheme(value == "Dark");
                Properties.Settings.Default.Save();
                return;
            }
        }

        public SettingsViewModel()
        {
            DisplayName = "Settings";
            IconCode = "\xf013";
        }
    }
}
