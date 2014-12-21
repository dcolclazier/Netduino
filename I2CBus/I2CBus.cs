using System;
using Microsoft.SPOT.Hardware;

namespace NetduinoApplication4
{
    public class I2CBus : IDisposable
    {
        private static I2CBus _instance;
        private readonly I2CDevice _slave;
        public static I2CBus GetInstance()
        {
            return _instance ?? (_instance = new I2CBus());           
        }
        private I2CBus()
        {
            _slave = new I2CDevice(new I2CDevice.Configuration(0,0));
        }
        public void Dispose()
        {
            _slave.Dispose();
        }

        private void Transact(I2CDevice.Configuration config, I2CDevice.I2CTransaction[] transaction , int timeout)
        {
            _slave.Config = config;
            lock (_slave)
            {
                var transferred = _slave.Execute(transaction, timeout);
                if(transferred == 0)
                    throw new Exception("Could not write to the device.");
            }
        }       
        public byte[] ReadRegister(I2CDevice.Configuration config, byte register, int timeout)
        {            
            var registerValue = new byte[] { 0, 0 };

            var xActions = new I2CDevice.I2CTransaction[2];
            xActions[0] = I2CDevice.CreateWriteTransaction(new[] {register});
            xActions[1] = I2CDevice.CreateReadTransaction(registerValue);

            Transact(config, xActions, timeout);
            return registerValue;
        }      
    }
}