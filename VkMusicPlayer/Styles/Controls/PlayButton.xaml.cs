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

namespace VkMusicPlayer.Styles.Controls
{
    /// <summary>
    /// Логика взаимодействия для PlayButton.xaml
    /// </summary>
    public partial class PlayButton : UserControl
    {
        public PlayButton()
        {
            InitializeComponent();

        }

        public event EventHandler Click;

        public ICommand ImageButtonCommand
        {
            get { return (ICommand)GetValue(PlayButtonCommandProperty); }
            set { SetValue(PlayButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty PlayButtonCommandProperty =
            DependencyProperty.Register("PlayButtonCommand", typeof(ICommand), typeof(PlayButton), new UIPropertyMetadata(null));

        public ImageSource PauseNormal
        {
            get { return (ImageSource)GetValue(PauseNormalProperty); }
            set { SetValue(PauseNormalProperty, value); }
        }

        public static readonly DependencyProperty PauseNormalProperty =
            DependencyProperty.Register("PauseNormal", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource PauseHovered
        {
            get { return (ImageSource)GetValue(PauseHoveredProperty); }
            set { SetValue(PauseHoveredProperty, value); }
        }

        public static readonly DependencyProperty PauseHoveredProperty =
            DependencyProperty.Register("PauseHovered", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));


        public ImageSource PausePressed
        {
            get { return (ImageSource)GetValue(PausePressedProperty); }
            set { SetValue(PausePressedProperty, value); }
        }

        public static readonly DependencyProperty PausePressedProperty =
            DependencyProperty.Register("PausePressed", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));



        public ImageSource Normal
        {
            get
            {
                if (IsPlaying)
                    return (ImageSource) GetValue(NormalProperty);
                else
                    return (ImageSource) GetValue(PauseNormalProperty);
            }
            set { SetValue(NormalProperty, value); }
        }

        public static readonly DependencyProperty NormalProperty =
            DependencyProperty.Register("Normal", typeof(ImageSource), typeof(PlayButton), new UIPropertyMetadata(null));


        public ImageSource Hovered
        {
            get
            {
                if(IsPlaying)
                    return (ImageSource)GetValue(HoveredProperty);
                else
                    return (ImageSource)GetValue(PauseHoveredProperty);
            }
            set { SetValue(HoveredProperty, value); }
        }

        public static readonly DependencyProperty HoveredProperty =
            DependencyProperty.Register("Hovered", typeof(ImageSource), typeof(PlayButton), new UIPropertyMetadata(null));


        public ImageSource Pressed
        {
            get
            {
                if (IsPlaying)
                    return (ImageSource) GetValue(PressedProperty);
                else
                    return (ImageSource) GetValue(PausePressedProperty);
            }
            set { SetValue(PressedProperty, value); }
        }

        public static readonly DependencyProperty PressedProperty =
            DependencyProperty.Register("Pressed", typeof(ImageSource), typeof(PlayButton), new UIPropertyMetadata(null));


        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof(bool), typeof(PlayButton), new UIPropertyMetadata(null));

        private void PlButton_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }
    }
}
