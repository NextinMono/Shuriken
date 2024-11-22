using Shuriken.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for LayerControl.xaml
    /// </summary>
    public partial class LayerControl : UserControl
    {
        private ShurikenUIElement CurrentUIElement { get { return (ShurikenUIElement)DataContext; } }
        public LayerControl()
        {
            InitializeComponent();
        }

        private void SetCastPivot_TL(object sender, RoutedEventArgs e)
        {
            CurrentUIElement.TopLeft.X = 0;
            CurrentUIElement.TopLeft.Y = 0;
            CurrentUIElement.TopRight.X = CurrentUIElement.Size.X / 1280.0f;
            CurrentUIElement.TopRight.Y = 0;

            CurrentUIElement.BottomLeft.X = 0;
            CurrentUIElement.BottomLeft.Y = -(CurrentUIElement.Size.Y / 1280.0f);

            CurrentUIElement.BottomRight.X = CurrentUIElement.Size.X / 1280.0f;
            CurrentUIElement.BottomRight.Y = -(CurrentUIElement.Size.Y / 1280.0f);
        }
    }
}
