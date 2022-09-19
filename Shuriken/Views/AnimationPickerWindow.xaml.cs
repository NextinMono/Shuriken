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

namespace Shuriken.Views
{
    /// <summary>
    /// Interaction logic for AnimationPickerWindow.xaml
    /// </summary>
    public partial class AnimationPickerWindow : Window
    {
        public AnimationPickerWindow()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
            TextureLists.Add("Hide Flag");
            TextureLists.Add("X Position");
            TextureLists.Add("Y Position");
            TextureLists.Add("Rotation");
            TextureLists.Add("X Scale");
            TextureLists.Add("Y Scale");
            TextureLists.Add("SubImage");
            TextureLists.Add("Color");
            TextureLists.Add("Gradient Top Left");
            TextureLists.Add("Gradient Bottom Left");
            TextureLists.Add("Gradient Top Right");
            TextureLists.Add("Gradient Bottom Right");

            if (TextureLists.Count > 0)
                TextureListSelect = 0;
        }

        public bool SelectionValid { get; set; }

        private void SelectClicked(object sender, EventArgs e)
        {
            DialogResult = true;
        }
        public int SelectedTexture { get; private set; }
        public int SelectedSpriteID { get; private set; }
        public int TextureListSelect;
        public ObservableCollection<string> TextureLists { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SpriteListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSpriteID = TypeSelect.SelectedItem == null ? -1 : (int)TypeSelect.SelectedItem;
            SelectionValid = SelectedSpriteID != -1;
        }

        private void TexturesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedSpriteID = -1;
            //SelectionValid = false;
        }
    }
}
