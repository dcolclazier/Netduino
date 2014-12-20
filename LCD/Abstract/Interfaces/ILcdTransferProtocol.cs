namespace NetduinoApplication4.LCD.Abstract.Interfaces
{
    public interface ILcdTransferProtocol
    {
        void SendCommand(byte command);
        void SendLine(string text);
        void SendLine(string text, int delay, bool newLine);
        void MoveCursor(byte column, byte row);
        bool IsVisible { set; }
        bool ShowCursor { set; }
        bool BlinkCursor { set; }
        
    }
}