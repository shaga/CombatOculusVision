using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CombatController
{
    public class Drv8830Mtr
    {
        public const ushort Addr00 = 0x60;
        public const ushort Addr0Open = 0x61;
        public const ushort Addr01 = 0x62;
        public const ushort AddrOpen0 = 0x63;
        public const ushort AddrOpenOpen = 0x64;
        public const ushort AddrOpen1 = 0x65;
        public const ushort Addr10 = 0x66;
        public const ushort Addr1Open = 0x67;
        public const ushort Addr11 = 0x68;

        private const byte DirStop = 0x00;
        private const byte DirPosi = 0x01;
        private const byte DirNega = 0x02;
        private const byte DirBrake = 0x03;

        private const int I2CClock = 100;
        private const int I2CTimeout = 100;

        public static I2CDevice Device { get; set; }

        private I2CDevice.Configuration Configuration { get; set; }

        private I2CDevice.I2CTransaction[] ReadTransactions { get; set; }
        private I2CDevice.I2CTransaction[] WriteTransactions { get; set; }

        public Drv8830Mtr(ushort address)
        {
            Configuration = new I2CDevice.Configuration(address, I2CClock);

            if (Device == null)
            {
                Device = new I2CDevice(Configuration);
            }

            ReadTransactions = new I2CDevice.I2CTransaction[2];
            WriteTransactions = new I2CDevice.I2CTransaction[1];
        }

        public int GetSpeed()
        {
            SetConfig();

            int speed;
            var buffer = new byte[1];

            ReadTransactions[0] = I2CDevice.CreateWriteTransaction(new byte[] {0x00});
            ReadTransactions[1] = I2CDevice.CreateReadTransaction(buffer);

            Device.Execute(ReadTransactions, I2CTimeout);

            if ((buffer[0] & 0x02) == 0x02)
            {
                speed = -1*(buffer[0] >> 2);
            }
            else
            {
                speed = (buffer[0] >> 2);
            }

            return speed;
        }

        public void SetSpeed(int speed)
        {
            SetConfig();

            byte dir;

            if (speed > 0)
            {
                dir = DirPosi;
            }
            else if (speed < 0)
            {
                dir = DirNega;
                speed = -1*speed;
            }
            else
            {
                dir = DirStop;
            }

            var power = (byte) ((speed << 2) | dir);

            WriteTransactions[0] = I2CDevice.CreateWriteTransaction(new byte[] {0x00, power});

            Device.Execute(WriteTransactions, I2CTimeout);
        }

        public void Brake()
        {
            SetConfig();

            WriteTransactions[0] = I2CDevice.CreateWriteTransaction(new byte[] {0x00, DirBrake});

            Device.Execute(WriteTransactions, I2CTimeout);
        }

        private void SetConfig()
        {
            if (Device.Config.Address == Configuration.Address)
            {
                return;
            }

            Device.Config = Configuration;
        }
    }
}
