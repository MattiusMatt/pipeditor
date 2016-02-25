using System;

namespace PIPEditor
{
    public class PIPEntry
    {
        #region enums

        public enum PipType
        {
            TEXT = 0,
            IMAGE = 1,
            LINE = 2,
            RECT = 3,
            FILLRECT = 4,
            SUBSCREENS = 5,
            SUBMENUS = 6
        }

        #endregion

        #region public properties

        public PipType Type { get; set; }
        public Int16 X { get; set; }
        public Int16 Y { get; set; }
        public Int16 EndX { get; set; }
        public Int16 EndY { get; set; }
        public int Size { get; set; }
        public Int16 Color { get; set; }
        public Int16 BackColor { get; set; }
        public string Data { get; set; }
        public string Display
        {
            get
            {
                return this.Type == PipType.LINE || this.Type == PipType.RECT || this.Type == PipType.FILLRECT ? this.Type.ToString() : this.Data;
            }
        }

        #endregion
    }
}
