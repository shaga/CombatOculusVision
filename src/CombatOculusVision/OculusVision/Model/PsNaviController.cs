using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D10;
using SlimDX.DirectInput;
using Debug = System.Diagnostics.Debug;

namespace OculusVision.Model
{
    public enum EStateCrossKey
    {
        None = 0,
        Up = 6,
        Right,
        Down,
        Left,
    }

    public class PsNaviController
    {
        private const string PrefixMotioninJoy = "MotioninJoy";
        private const int PollingSleepLength = 100;

        private EStateCrossKey StateCrossKey { get; set; }

        private bool[] IsPressedCrossKey = new bool[4];

        private DirectInput DirectInput { get; set; }

        private Guid GamepadGuid { get; set; }

        private Joystick Joystick { get; set; }

        private bool IsRunning { get; set; }

        public event Action<EStateCrossKey> CrossKeyAction;

        public PsNaviController()
        {
            GamepadGuid = Guid.Empty;
            Joystick = null;
        }

        public void Initialize()
        {
            DirectInput = new DirectInput();

            var devices = DirectInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

            if (devices == null || !devices.Any())
            {
                return;
            }

            var instance = devices.FirstOrDefault(di => di.InstanceName.StartsWith(PrefixMotioninJoy));

            if (instance == null)
            {
                return;
            }

            GamepadGuid = instance.InstanceGuid;

            InitJoystick();
        }

        private void InitJoystick()
        {
            if (GamepadGuid == Guid.Empty)
            {
                return;
            }

            Joystick = new Joystick(DirectInput, GamepadGuid);

            foreach (var objectInstance in Joystick.GetObjects(ObjectDeviceType.Axis))
            {
                Joystick.GetObjectPropertiesById((int) objectInstance.ObjectType).SetRange(-100, 100);
            }

            Joystick.Properties.AxisMode = DeviceAxisMode.Absolute;

            Joystick.Acquire();
        }

        public void StopPollingJoystick()
        {
            IsRunning = false;
        }

        public void StartPollingJoystick()
        {
            IsRunning = true;

            Task.Run(async () =>
            {
                while (IsRunning)
                {
                    PollJoystick();
                    await Task.Delay(PollingSleepLength);
                }
            });
        }

        private void PollJoystick()
        {
            var state = new JoystickState();

            if (Joystick.Poll().IsFailure)
            {
                Debug.WriteLine("Joystick polling is failed.");

                return;
            }

            if (Joystick.GetCurrentState(ref state).IsFailure)
            {
                Debug.WriteLine("Failed to get joystick state");
                return;
            }

            var next = EStateCrossKey.None;

            var btnPressed = state.GetButtons();

            if (btnPressed.Skip((int)EStateCrossKey.Up).Take(4).Any(s => s))
            {
                next =
                    Enum.GetValues(typeof(EStateCrossKey))
                        .OfType<EStateCrossKey>()
                        .Where(v => v != EStateCrossKey.None)
                        .FirstOrDefault(v => StateCrossKey != v && !IsPressedCrossKey[(int)v - (int)EStateCrossKey.Up] && btnPressed[(int)v]);

                if (next == EStateCrossKey.None) next = StateCrossKey;
            }

            if (next != StateCrossKey)
            {
                Debug.WriteLine("CrossKey:" + next);

                if (CrossKeyAction != null) CrossKeyAction(next);

                StateCrossKey = next;
            }

            foreach (var value in Enum.GetValues(typeof(EStateCrossKey)).OfType<EStateCrossKey>().Where(v => v != EStateCrossKey.None))
            {
                IsPressedCrossKey[(int)value - (int)EStateCrossKey.Up] = btnPressed[(int)value];
            }
        }
    }
}
