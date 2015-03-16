using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace CombatController
{
    public class Program
    {
        public const ushort MtrForeAddr = Drv8830Mtr.AddrOpen0;
        public const ushort MtrRoteAddr = Drv8830Mtr.AddrOpen1;

        public static void Main()
        {
            // write your code here
            var led = new OutputPort(Pins.ONBOARD_LED, false);
            led.Write(false);

            Drv8830Mtr.Device = new I2CDevice(new I2CDevice.Configuration(0, 0));

            var mtrFb = new Drv8830Mtr(MtrForeAddr);

            mtrFb.SetSpeed(0);

            var mtrLr = new Drv8830Mtr(MtrRoteAddr);

            mtrLr.SetSpeed(0);

            var server = new WebServerCommandReceiver();

            server.AddAction('s', () =>
            {
                mtrFb.SetSpeed(0);
                mtrLr.SetSpeed(0);
            });

            server.AddAction('f', () =>
            {
                mtrLr.SetSpeed(0);
                mtrFb.SetSpeed(-63);
            });

            server.AddAction('b', () =>
            {
                mtrLr.SetSpeed(0);
                mtrFb.SetSpeed(63);
            });

            server.AddAction('l', () =>
            {
                mtrFb.SetSpeed(0);
                mtrLr.SetSpeed(-63);
            });

            server.AddAction('r', () =>
            {
                mtrFb.SetSpeed(0);
                mtrLr.SetSpeed(63);
            });

            led.Write(true);

            server.WaitForRequest();
        }

    }
}
