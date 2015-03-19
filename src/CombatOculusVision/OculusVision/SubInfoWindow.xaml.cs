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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OculusVision.Annotations;

namespace OculusVision
{
    /// <summary>
    /// SubInfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubInfoWindow : Window, INotifyPropertyChanged
    {
        private string _combatStatus;
        private double _stateSize;
        private string _button001Content;
        private string _button002Content;
        public event Action ClosedEvent;

        public event Action Button001Event;
        public event Action Button002Event;

        public string CombatStatus
        {
            get { return _combatStatus; }
            set
            {
                if (value == _combatStatus) return;
                _combatStatus = value;
                OnPropertyChanged();
            }
        }

        public string Button001Content
        {
            get { return _button001Content; }
            set
            {
                if (value == _button001Content) return;
                _button001Content = value;
                OnPropertyChanged();
            }
        }

        public string Button002Content
        {
            get { return _button002Content; }
            set
            {
                if (value == _button002Content) return;
                _button002Content = value;
                OnPropertyChanged();
            }
        }

        public double StateSize
        {
            get { return _stateSize; }
            set
            {
                if (value.Equals(_stateSize)) return;
                _stateSize = value;
                OnPropertyChanged();
            }
        }

        public SubInfoWindow()
        {
            InitializeComponent();

            CombatStatus = "止";
        }

        private void SubInfoWindow_OnClosed(object sender, EventArgs e)
        {
            if (ClosedEvent != null)
            {
                ClosedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubInfoWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StateSize = ActualWidth/6;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button001_OnClick(object sender, RoutedEventArgs e)
        {
            if (Button001Event != null)
            {
                Button001Event();
            }
        }

        private void Button002_OnClick(object sender, RoutedEventArgs e)
        {
            if (Button002Event != null)
            {
                Button002Event();
            }
        }
    }
}
