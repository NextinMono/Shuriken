using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.Models;
using Shuriken.Views;
using System.Runtime.CompilerServices;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Color), typeof(ColorControl), new PropertyMetadata(new Color()));

        public Color Value
        {
            get => (Color)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        
        private void ColorBtnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog window = new System.Windows.Forms.ColorDialog();

            window.AnyColor = true;
            window.AllowFullOpen = true;
            window.SolidColorOnly = false;
            window.Color = System.Drawing.Color.FromArgb(Value.A, Value.R, Value.G, Value.B);
            if(window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Value.R = window.Color.R;
                Value.G = window.Color.G;
                Value.B = window.Color.B;
                //We don't set A since it'll likely be 255 anyway.
            }            
        }

        public ColorControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {

        }
    }
}
