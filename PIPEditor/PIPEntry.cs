using System;

namespace PIPEditor
{
    public class PIPEntry
    {
        #region enums

        public enum PipType
        {
            TEXT = 0,
            IMAGE = 1
        }

        #endregion

        #region public properties

        public PipType Type { get; set; }
        public Int16 X { get; set; }
        public Int16 Y { get; set; }
        public int Size { get; set; }
        public Int16 Color { get; set; }
        public Int16 BackColor { get; set; }
        public string Data { get; set; }

        #endregion
    }
}
