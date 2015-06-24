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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VkMusicPlayer.Model;

namespace VkMusicPlayer.Styles.Controls
{
    /// <summary>
    /// Логика взаимодействия для ImageButton.xaml
    /// </summary>
    public partial class ImageButton : UserControl
    {
        public ImageButton()
        {
            InitializeComponent();

        }

        public event EventHandler Click;

        public ICommand ImageButtonCommand
        {
            get { return (ICommand)GetValue(ImageButtonCommandProperty); }
            set { SetValue(ImageButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ImageButtonCommandProperty =
            DependencyProperty.Register("ImageButtonCommand", typeof(ICommand), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource Normal
        {
            get
            {
                return (ImageSource)GetValue(NormalProperty);
            }
            set { SetValue(NormalProperty, value); }
        }

        public static readonly DependencyProperty NormalProperty =
            DependencyProperty.Register("Normal", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource Hovered
        {
            get
            {
                return (ImageSource)GetValue(HoveredProperty);
            }
            set { SetValue(HoveredProperty, value); }
        }

        public static readonly DependencyProperty HoveredProperty =
            DependencyProperty.Register("Hovered", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource Pressed
        {
            get
            {
                return (ImageSource)GetValue(PressedProperty);
            }
            set { SetValue(PressedProperty, value); }
        }

        public static readonly DependencyProperty PressedProperty =
            DependencyProperty.Register("Pressed", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));
        private void ImgButton_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }
    }
}
