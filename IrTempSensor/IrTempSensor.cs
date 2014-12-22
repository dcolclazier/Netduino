using Microsoft.SPOT.Hardware;

namespace NetduinoApplication4
{
    public class IrTempSensor
    {
        public enum RamRegisters : byte
        {
            AreaTemperature = 0x06,
            ObjectTempOne = 0x07,
            ObjectTempTwo = 0x08,
            RawIrChannelOne = 0x04,
            RawIrChannelTwo = 0x05
        }
        public enum Temp
        {
            Celcius,
            Kelvin,
            Fahrenheit
        }
        public enum PwmControl
        {
            PwmExtended = 0x00,
            PwmSingle = 0x01,
            PwmEnable = 0x02,
            PwmDisable = 0x00,
            SdaOpenDrain = 0x00,
            SdaPushPull = 0x04,
            ThermalRelaySelected = 0x08,
            PwmSelected = 0x00
        }
        public I2CDevice.Configuration Config { get; private set; }
                
        public IrTempSensor(byte i2CAddress, byte frequency)
        {            
            Config = new I2CDevice.Configuration(i2CAddress,frequency);
        }

        public double CalculateTemp(byte[] registerValue, Temp units)
        {
            double temp = ((registerValue[1] & 0x007F) << 8) + registerValue[0];
            temp = (temp * .02) - 0.01; // 0.02 deg./LSB (MLX90614 resolution)
            var celcius = temp - 273.15;
            var fahrenheit = (celcius * 1.8) + 32;
            switch (units)
            {
                case Temp.Celcius:
                    return celcius;
                case Temp.Kelvin:
                    return temp;
                case Temp.Fahrenheit:
                    return fahrenheit;
                default:
                    return 0;
            }
        }

    }

    
}