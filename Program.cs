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
            
            lcd.Write("It is done.",200,true);

            while (true)
            {
                lcd.SetCursorPosition(0,1);   
                
                lcd.Write(Utility.GetMachineTime().Hours.ToString());
                lcd.Write(":");
                lcd.Write(Utility.GetMachineTime().Minutes.ToString());
                lcd.Write(":");
                lcd.Write(Utility.GetMachineTime().Seconds.ToString());
            }


        }

    }
}
