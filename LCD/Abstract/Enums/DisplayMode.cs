namespace NetduinoApplication4.LCD.Abstract.Enums
{
    public enum DisplayMode : byte
    {
        DisplayOn = 0x04,
        DisplayOff = 0x00,
        CursorOn = 0x02,
        CursorOff = 0x00,
        CursorBlinkOn = 0x01,
        CursorBlinkOff = 0x00
    }
}