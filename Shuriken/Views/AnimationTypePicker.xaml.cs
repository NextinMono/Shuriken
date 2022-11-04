using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shuriken.Models;
using System.ComponentModel;
using System.Diagnostics;
using Shuriken.Models.Animation;
using Shuriken.Converters;

namespace Shuriken.Views
{
    /// <summary>
    /// SOMEONE SHOULD SERIOUSLY REMAKE THIS, I'M REALLY REALLY DUMB WITH WPF STUFF AND THIS SHOULD BE CLEANED UP
    /// </summary>
    public partial class AnimationTypePicker : Window, INotifyPropertyChanged
    {
        public AnimationTypePicker()
        {
            InitializeComponent();
            AnimType = AnimationType.None;
            foreach (var item in Enum.GetNames(typeof(AnimationType)))
            {
                AnimTypeBox.Items.Add(item);
            }
        }

        public bool SelectionValid { get; set; }

        private void SelectClicked(object sender, EventArgs e)
        {
            DialogResult = true;
        }
        public AnimationType AnimType { get; set; }
        private int typeSelected;
        public int SelectedType 
        {
            get
            {
                return typeSelected;
            }
            set
            {
                var e = Enum.GetValues(typeof(AnimationType)).Cast<AnimationType>().ToList();
                AnimType = (AnimationType)e[value];
                typeSelected = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SpriteListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedType = AnimTypeBox.SelectedIndex;
        }

        private void TexturesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedSpriteID = -1;
            //SelectionValid = false;
        }
    }
}
