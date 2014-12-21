using System;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Hardware;
using NetduinoApplication4.LCD.Abstract.Enums;
using NetduinoApplication4.LCD.Abstract.Interfaces;

namespace NetduinoApplication4.LCD.Transfer_Protocols
{
    public class GpioLcdTransferProtocol : ILcdTransferProtocol, IDisposable
    {
        #region Properties
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; UpdateDisplayOptions(); } }
        public bool ShowCursor { get { return _showCursor; } set { _showCursor = value; UpdateDisplayOptions(); } }
        public bool BlinkCursor { get { return _isBlinking; } set { _isBlinking = value; UpdateDisplayOptions(); } }
        #endregion

        #region Private Fields
        private bool _isVisible;
        private bool _showCursor;
        private bool _isBlinking;

        private byte[] _firstHalfAddress;
        private byte[] _secondHalfAddress;
        private byte[] _rowAddress;
   
        private readonly OutputPort _rsPort;
        private readonly OutputPort _enablePort;
        private readonly OutputPort _d4;
        private readonly OutputPort _d5;
        private readonly OutputPort _d6;
        private readonly OutputPort _d7;

        private int _rowCount;
        private int _columnCount;
        private int _currentRow;
        private int _cursorPosition;
        #endregion

        #region Constructors
        public GpioLcdTransferProtocol(Cpu.Pin rsPin, Cpu.Pin enablePin,        
            Cpu.Pin d4, Cpu.Pin d5, Cpu.Pin d6, Cpu.Pin d7, int rowCount, int columnCount)     
        {
            _rsPort = new OutputPort(rsPin,false);
            _enablePort = new OutputPort(enablePin, false);
            _d4 = new OutputPort(d4,false);
            _d5 = new OutputPort(d5, false);
            _d6 = new OutputPort(d6, false);
            _d7 = new OutputPort(d7, false);
            
            Initialize(columnCount, rowCount);
        }
        #endregion

        #region Public Methods
        public void SendCommand(byte command)
        {
            _rsPort.Write(false);
            Write(new [] {command});
            Thread.Sleep(5);
        }
        public void MoveCursor(byte column, byte row)
        {
            //if (_rowCount == 2) row = (byte)(row % 4);
            row = (byte)(row % 2);
            SendCommand((byte)((byte)Command.SetDdRam | (byte)(column + _rowAddress[row]))); //0 based index           
        }     
        public void SendLine(string text)
        {
            if (text.Length > _columnCount)
            {
                SendLines(text);
                return;
            }
            OpenDataChannel(Encoding.UTF8.GetBytes(text));

        }       
        public void SendLine(string text, int delay, bool newLine)
        {
            if (newLine) _cursorPosition = 0;
            foreach (var textChar in text.ToCharArray())
            {
                ResetLines();
                OpenDataChannel(Encoding.UTF8.GetBytes(textChar.ToString()));
                _cursorPosition += 1;
                Thread.Sleep(delay);
            }
        }
        public void Dispose()
        {
            _rsPort.Dispose();
            _enablePort.Dispose();
            _d4.Dispose();
            _d5.Dispose();
            _d6.Dispose();
            _d7.Dispose();

        }
        #endregion

        #region Private Methods
        private void Initialize(int columns, int rowCount)
        {
            _currentRow = 0;
            _columnCount = columns;
            _rowCount = rowCount;

            _isVisible = true;
            _showCursor = false;
            _isBlinking = false;

            _rowAddress = new byte[] { 0x00, 0x40, 0x14, 0x54 };
            _firstHalfAddress = new byte[] { 0x10, 0x20, 0x40, 0x80 };
            _secondHalfAddress = new byte[] { 0x01, 0x02, 0x04, 0x08 };

            Thread.Sleep(250);

            _rsPort.Write(false);
            _enablePort.Write(false); // Enable provides a clock function to syncrhonize data transfer

            Write(0x03, _secondHalfAddress);
            Thread.Sleep(4);
            Write(0x03, _secondHalfAddress);
            Thread.Sleep(4);
            Write(0x03, _secondHalfAddress);
            Thread.Sleep(150);
            Write(0x02, _secondHalfAddress);

            var rowValue = _rowCount == 2 ? LcdFunction.TwoLineDisplay : LcdFunction.OneLineDisplay;
            SendCommand((byte)((byte) Command.LcdFunction | ((byte)LcdFunction.FourBitMode | (byte) rowValue | (byte)LcdFunction.Font5X8)));

            UpdateDisplayOptions();

            SendCommand((byte)Command.Clear);
            Thread.Sleep(2);

            SendCommand(((byte)Command.EntryModeSet | (byte)EntryMode.FromLeft | (byte)EntryMode.ShiftDecrement));

        }
        private void UpdateDisplayOptions()
        {
            var command = (byte)Command.DisplayMode;
            command |= _isVisible ? (byte)DisplayMode.DisplayOn : (byte)DisplayMode.DisplayOff;
            command |= _showCursor ? (byte)DisplayMode.CursorOn : (byte)DisplayMode.CursorOff;
            command |= _isBlinking ? (byte)DisplayMode.CursorBlinkOn : (byte)DisplayMode.CursorBlinkOff;

            SendCommand(command);
        }
        private string[] SplitText(string str)
        {
            if (str.Length > _columnCount * _rowCount) str = str.Substring(0, _columnCount * _rowCount);

            var stringArrayCounter = 0;
            _cursorPosition = 0;

            var charArray = str.ToCharArray();
            var arraySize = (int)System.Math.Ceiling((double)(str.Length + _cursorPosition) / _columnCount);
            var stringArray = new string[arraySize];
            
            for (var i = 0; i < charArray.Length; i++)
            {
                if (_cursorPosition < _columnCount)
                {
                    stringArray[stringArrayCounter] = stringArray[stringArrayCounter] + charArray[i];
                    _cursorPosition += 1;
                }
                else
                {
                    _cursorPosition = 1;
                    stringArrayCounter += 1;
                    stringArray[stringArrayCounter] = stringArray[stringArrayCounter] + charArray[i];
                }
            }
            return stringArray;
        }
        private void SendLines(string text)
        {
            var splitText = SplitText(text);

            foreach (var line in splitText)
            {
                MoveCursor(0, (byte)(_currentRow));
                OpenDataChannel(Encoding.UTF8.GetBytes(line));
                if (_currentRow == _rowCount - 1)
                {
                    Thread.Sleep(500);
                    _currentRow = 0;
                }
                else _currentRow += 1;
            }
        }
        private void Write(byte[] data)
        {
            foreach (var value in data)
            {
                Write(value, _firstHalfAddress); // First half
                Write(value, _secondHalfAddress); // Second half
            }
        }
        private void Write(byte value, byte[] halfAddress)
        {
            _d4.Write((value & halfAddress[0]) > 0);
            _d5.Write((value & halfAddress[1]) > 0);
            _d6.Write((value & halfAddress[2]) > 0);
            _d7.Write((value & halfAddress[3]) > 0);

            _enablePort.Write(true);
            _enablePort.Write(false);
            
        }
        private void ResetLines()
        {
            if (_cursorPosition == 0) return;
            if (_cursorPosition%_columnCount != 0) return;

            if (_currentRow < 1) _currentRow += 1;
            else
            {
                Thread.Sleep(500);
                SendCommand((byte)Command.Clear);
                _currentRow = 0;
            }
            MoveCursor(0, (byte)(_currentRow));
        }
        private void OpenDataChannel(byte[] buffer)
        {
            _rsPort.Write(true);
            Write(buffer);
        }
        #endregion

        
    }
}