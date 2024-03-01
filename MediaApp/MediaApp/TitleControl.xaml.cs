using System.Windows;
using System.Windows.Controls;

namespace MediaApp
{
    public partial class TitleControl : UserControl
    {
        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register(
            "TitleText", typeof(string), typeof(TitleControl), new PropertyMetadata(""));

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        public TitleControl()
        {
            InitializeComponent();
        }
    }
}
