using System.Windows;
using System.Windows.Controls;
using Shuriken.Models;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for Vector2Edit.xaml
    /// </summary>
    public partial class Vector2Control : UserControl
    {
        /// <summary>
        /// Gets or sets the value of the bound Vector2 object
        /// </summary>
        public Vector2 Value
        {
            get => (Vector2)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Vector2), typeof(Vector2Control), new PropertyMetadata(new Vector2()));

        public Vector2Control()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}
