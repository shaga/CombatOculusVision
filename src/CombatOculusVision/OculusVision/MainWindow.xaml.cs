using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CLEyeMulticam;
using OculusVision.Annotations;
using OculusVision.Model;
using OpenTK;
using WindowState = System.Windows.WindowState;

namespace OculusVision
{
    [ValueConversion(typeof(double), typeof(Thickness))]
    public abstract class MarginConverter : IValueConverter
    {
        protected enum EType
        {
            Left,
            Right,
        }

        protected abstract EType Type { get; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double marginSize;

            if (value is double)
            {
                marginSize = (double) value;
            }
            else
            {
                marginSize = 0.0;
            }

            marginSize = 0 - marginSize;

            if (Type == EType.Right)
            {
                return new Thickness(marginSize, 0, 0, 0);
            }

            return new Thickness(0, 0, marginSize, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class CamLeftMarginConverter : MarginConverter
    {
        protected override EType Type
        {
            get { return EType.Left; }
        }
    }

    public class CamRightMarginConverter : MarginConverter
    {
        protected override EType Type
        {
            get { return EType.Right; }
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const double ImageAcceptRate = 0.75;

        private double _imageWidth;
        private double _imageHeight;
        private double _marginSize;

        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (value.Equals(_imageWidth)) return;
                _imageWidth = value;
                OnPropertyChanged();
            }
        }

        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                if (value.Equals(_imageHeight)) return;
                _imageHeight = value;
                OnPropertyChanged();
            }
        }

        public double MarginSize
        {
            get { return _marginSize; }
            set
            {
                if (value.Equals(_marginSize)) return;
                _marginSize = value;
                OnPropertyChanged();
            }
        }

        private PsNaviController Controller { get; set; }

        private SubInfoWindow SubWindow { get; set;  }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            InitCams();

            InitRift();

            InitController();
        }

        private void InitController()
        {
            Controller = new PsNaviController();

            Controller.CrossKeyAction += key =>
            {
                var ret = false;

                while (!ret)
                {
                    switch (key)
                    {
                        case EStateCrossKey.None:
                            ret = CombatCommandSender.SendCommandStop();
                            if (SubWindow != null)
                            {
                                SubWindow.CombatStatus = "止";
                            }
                            break;
                        case EStateCrossKey.Up:
                            ret = CombatCommandSender.SendCommandFore();
                            if (SubWindow != null)
                            {
                                SubWindow.CombatStatus = "前";
                            }
                            break;
                        case EStateCrossKey.Down:
                            ret = CombatCommandSender.SendCommandBack();
                            if (SubWindow != null)
                            {
                                SubWindow.CombatStatus = "後";
                            }
                            break;
                        case EStateCrossKey.Left:
                            ret = CombatCommandSender.SendCommandLeft();
                            if (SubWindow != null)
                            {
                                SubWindow.CombatStatus = "左";
                            }
                            break;
                        case EStateCrossKey.Right:
                            ret = CombatCommandSender.SendCommandRight();
                            if (SubWindow != null)
                            {
                                SubWindow.CombatStatus = "右";
                            }
                            break;
                        default:
                            ret = true;
                            break;
                    }
                }
            };

            Controller.Initialize();

            Controller.StartPollingJoystick();
        }

        private void InitCams()
        {
            var camCount = CLEyeCameraDevice.CameraCount;

            if (camCount != 2)
            {
                MessageBox.Show("カメラが接続されていません");
                Close();
                return;
            }

            ImageLeft.Device.Create(CLEyeCameraDevice.CameraUUID(0));
            ImageLeft.Device.Start();
            ImageLeft.Device.LensCorrection1 = 300;
            ImageLeft.Device.LensCorrection2 = 300;
            ImageLeft.Device.LensCorrection3 = 300;

            ImageRight.Device.Create(CLEyeCameraDevice.CameraUUID(1));
            ImageRight.Device.Start();
            ImageRight.Device.LensCorrection1 = 300;
            ImageRight.Device.LensCorrection2 = 300;
            ImageRight.Device.LensCorrection3 = 300;
        }

        private void InitRift()
        {
            using (var rift = new OculusRift())
            {
                if (rift.IsConnected)
                {
                    Left = rift.DesktopX;
                    Top = rift.DesktopY;

                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;

                    SubWindow = new SubInfoWindow();
                    SubWindow.ClosedEvent += () =>
                    {
                        SubWindow = null;
                        Close();
                    };

                    SubWindow.Button001Content = "明るさ+";
                    SubWindow.Button001Event += () =>
                    {
                        if (ImageLeft.Device.LensBrightness < 500)
                        {
                            ImageLeft.Device.LensBrightness += 10;
                        }
                        if (ImageRight.Device.LensBrightness < 500)
                        {
                            ImageRight.Device.LensBrightness += 500;
                        }
                    };
                    SubWindow.Button002Content = "明るさ-";
                    SubWindow.Button002Event += () =>
                    {
                        if (ImageLeft.Device.LensBrightness > 0)
                        {
                            ImageLeft.Device.LensBrightness -= 10;
                        }
                        if (ImageRight.Device.LensBrightness > 0)
                        {
                            ImageRight.Device.LensBrightness -= 0;
                        }
                    };
                    SubWindow.Show();
                }
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (Controller != null)
            {
                Controller.StopPollingJoystick();
            }

            var camCount = CLEyeCameraDevice.CameraCount;

            if (camCount == 2)
            {
                ImageLeft.Device.Stop();
                ImageLeft.Device.Destroy();

                ImageRight.Device.Stop();
                ImageRight.Device.Destroy();
            }

            if (SubWindow != null)
            {
                SubWindow.Close();
            }
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var baseWidth = BaseGrid.ActualWidth/2;
            var baseHeight = BaseGrid.ActualHeight;

            var rate = baseHeight/baseWidth;

            if (rate/ImageAcceptRate < 0.99)
            {
                ImageHeight = baseHeight;
                ImageWidth = baseHeight/ImageAcceptRate;
            }
            else
            {
                ImageWidth = baseWidth;
                ImageHeight = baseWidth*ImageAcceptRate;
            }

            MarginSize = ImageWidth*0.05;
            ImageWidth *= 1.1;
            ImageHeight *= 1.1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
