using System;
using System.Reflection;
using System.Threading;
using Json.NETMF;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using NetduinoApplication4.LCD;
using NetduinoApplication4.LCD.Transfer_Protocols;
using SecretLabs.NETMF.Hardware.Netduino;

namespace NetduinoApplication4
{
    public static class Program
    {
        public static void Main()
        {
            var lcd = new Lcd(new GpioLcdTransferProtocol(Pins.GPIO_PIN_D12, //D12 on NetDuino to RS
                                                          Pins.GPIO_PIN_D11, //D11 to Enable
                                                          Pins.GPIO_PIN_D5, //D5 to D4 on Lcd
                                                          Pins.GPIO_PIN_D4, //D4 to D5 on Lcd
                                                          Pins.GPIO_PIN_D3, //D3 to D6 on Lcd
                                                          Pins.GPIO_PIN_D2, //D2 to D7 on Lcd
                                                            2,              //Row Count (only tested with 2)
                                                            16));           //Column Count
            var i2CBus = I2CBus.GetInstance();
            var tempSensor = new IrTempSensor(0x5a,58);
            
            var motionSensor = new MotionSensor(Pins.GPIO_PIN_D7, lcd);
            motionSensor.Activate();
           
            lcd.Write("Alarm Activated.",50,true);

            Thread.Sleep(2000);
            while (true)
            {
                
                var registerData = i2CBus.ReadRegister(tempSensor.Config, 
                    (byte)IrTempSensor.RamRegisters.ObjectTempOne, 1000);
                
                var temp = tempSensor.CalculateTemp(registerData, IrTempSensor.Temp.Fahrenheit);

                lcd.SetCursorPosition(0, 1);
                lcd.Write(temp.ToString().Substring(0,5));

                Thread.Sleep(100);
                
            }
        }
        
    }

    public class MotionSensor
    {
        
        
        private readonly InterruptPort _pir;
        private readonly Lcd _display;
        
        public MotionSensor(Cpu.Pin inputPin, Lcd display)
        {
            _pir = new InterruptPort(inputPin, false, ResistorModes.PullDown, InterruptModes.InterruptEdgeHigh);
            _display = display;
        }

        public void Activate()
        {
            _pir.OnInterrupt += PollAlarm;
        }

        private void PollAlarm(uint data1, uint data2, DateTime time)
        {
            _display.ClearDisplay();
            _display.SetCursorPosition(0, 0);
            _display.Write("Motion Detected!!");
            Thread.Sleep(2500);
            _pir.ClearInterrupt();
            _display.ClearDisplay();
            _display.SetCursorPosition(0, 0);
            _display.Write("All is quiet...");
        }

        
    }
}

