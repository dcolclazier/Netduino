using System.Threading;
using NetduinoApplication4.LCD.Abstract.Enums;
using NetduinoApplication4.LCD.Abstract.Interfaces;

namespace NetduinoApplication4.LCD
{
    public class Lcd
    {
        private readonly ILcdTransferProtocol _transferProtocol;

        public Lcd(ILcdTransferProtocol transferProtocol)
        {
            _transferProtocol = transferProtocol;            
        }
        
        public void SetCursorPosition(byte column, byte row)
        {
            _transferProtocol.MoveCursor(column, row);
        }
        public void ClearDisplay()
        {
            _transferProtocol.SendCommand((byte)Command.Clear);
            Thread.Sleep(2);
        }

        public void Write(string text)
        {
            _transferProtocol.SendLine(text);
        }

        public void ShowCursor(bool show)
        {
            _transferProtocol.ShowCursor = show;
        }
        public void BlinkCursor(bool blink)
        {
            _transferProtocol.BlinkCursor = blink;
        }
        public void ActivateDisplay(bool activate)
        {
            _transferProtocol.IsVisible = activate;
        }
        public void Write(string data, int delay, bool newLine)
        {
            _transferProtocol.SendLine(data, delay, newLine);
        }
    }
}