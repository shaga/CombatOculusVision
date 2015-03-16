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
        public event Action ClosedEvent;

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
    }
}
