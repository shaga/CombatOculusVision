using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const double ImageAcceptRate = 0.75;

        private double _imageWidth;
        private double _imageHeight;

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

        private PsNaviController Controller { get; set; }

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
                            break;
                        case EStateCrossKey.Up:
                            ret = CombatCommandSender.SendCommandFore();
                            break;
                        case EStateCrossKey.Down:
                            ret = CombatCommandSender.SendCommandBack();
                            break;
                        case EStateCrossKey.Left:
                            ret = CombatCommandSender.SendCommandLeft();
                            break;
                        case EStateCrossKey.Right:
                            ret = CombatCommandSender.SendCommandRight();
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
            ImageLeft.Device.LensCorrection2 = 400;
            ImageLeft.Device.LensCorrection3 = 400;

            ImageRight.Device.Create(CLEyeCameraDevice.CameraUUID(1));
            ImageRight.Device.Start();
            ImageRight.Device.LensCorrection2 = 400;
            ImageRight.Device.LensCorrection3 = 400;
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
                }
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Controller.StopPollingJoystick();

            var camCount = CLEyeCameraDevice.CameraCount;

            if (camCount == 2)
            {
                ImageLeft.Device.Stop();
                ImageLeft.Device.Destroy();

                ImageRight.Device.Stop();
                ImageRight.Device.Destroy();
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
