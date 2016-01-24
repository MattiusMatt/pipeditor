using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PIPEditor
{
    public class PIPFile
    {
        #region private properties

        private string _filePath;

        #endregion

        #region public properties

        public List<PIPEntry> PipEntries;
        public string FilePath { get { return _filePath; } }

        #endregion

        #region constructor

        public PIPFile (string filePath)
        {
            _filePath = filePath;
        }

        #endregion

        #region public methods

        public void Load()
        {
            this.PipEntries = new List<PIPEntry>();

            using (BinaryReader pip = new BinaryReader(File.OpenRead(_filePath)))
            {
                while (pip.BaseStream.Position < pip.BaseStream.Length)
                {
                    PIPEntry entry = new PIPEntry();

                    entry.Type = (PIPEntry.PipType)pip.ReadByte();
                    entry.X = pip.ReadInt16();
                    entry.Y = pip.ReadInt16();

                    switch (entry.Type)
                    {
                        case PIPEntry.PipType.TEXT:
                            entry.Size = pip.ReadByte();
                            break;

                        case PIPEntry.PipType.IMAGE:
                            break;
                    }

                    var data = new StringBuilder();

                    while (pip.BaseStream.Position < pip.BaseStream.Length)
                    {
                        var nextChar = pip.ReadChar();

                        if (nextChar == (char)13)
                        {
                            pip.ReadChar(); // Om nom nom nom

                            break;
                        }
                        else
                        {
                            data.Append(nextChar);
                        }
                    }

                    entry.Data = data.ToString();

                    this.PipEntries.Add(entry);
                }
            }
        }

        public void Save()
        {
            using (BinaryWriter pip = new BinaryWriter(File.Create(_filePath)))
            {
                for (int i = 0; i < this.PipEntries.Count; i++)
                {
                    if (i > 0)
                    {
                        // finish previous entry
                        pip.Write((char)13);
                        pip.Write((char)10);
                    }

                    PIPEntry entry = this.PipEntries[i];

                    pip.Write((byte)entry.Type);
                    pip.Write((Int16)entry.X);
                    pip.Write((Int16)entry.Y);

                    switch (entry.Type)
                    {
                        case PIPEntry.PipType.TEXT:
                            pip.Write((byte)entry.Size);
                            break;

                        case PIPEntry.PipType.IMAGE:
                            break;
                    }

                    pip.Write(Encoding.ASCII.GetBytes(entry.Data));
                }
            }
        }

        #endregion
    }
}
