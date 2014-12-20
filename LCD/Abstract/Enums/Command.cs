namespace NetduinoApplication4.LCD.Abstract.Enums
{
    public enum Command : byte
    {
        Clear = 0x01,
        Home = 0x02,
        EntryModeSet = 0x04,
        DisplayMode = 0x08,
        CursorShift = 0x10,
        LcdFunction = 0x20,
        SetCgRam = 0x40,
        SetDdRam = 0x80
    }
}