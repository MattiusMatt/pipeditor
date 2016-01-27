using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace PIPEditor
{
    public class PIPFile
    {
        #region private properties

        private string _filePath;
        private BindingList<PIPEntry> _pipEntries;

        #endregion

        #region public properties

        public IEnumerable<PIPEntry> PipEntries { get { return _pipEntries; } }
        public string FilePath { get { return _filePath; } }

        #endregion

        #region constructor

        public PIPFile (string filePath)
        {
            _filePath = filePath;
        }

        #endregion

        #region public methods

        public void NewEntry()
        {
            if (_pipEntries != null)
            {
                this._pipEntries.Add(new PIPEntry() { Data = "New", X = 0, Y = 0, Type = PIPEntry.PipType.TEXT, Size = 1 });
            }
        }

        public void RemoveEntry(PIPEntry entry)
        {
            if (_pipEntries != null)
            {
                this._pipEntries.Remove(entry);
            }
        }

        public void Load()
        {
            this._pipEntries = new BindingList<PIPEntry>();

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
                            entry.Color = pip.ReadInt16();
                            entry.BackColor = pip.ReadInt16();
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

                    this._pipEntries.Add(entry);
                }
            }
        }

        public void Save()
        {
            using (BinaryWriter pip = new BinaryWriter(File.Create(_filePath)))
            {
                for (int i = 0; i < this._pipEntries.Count; i++)
                {
                    if (i > 0)
                    {
                        // finish previous entry
                        pip.Write((char)13);
                        pip.Write((char)10);
                    }

                    PIPEntry entry = this._pipEntries[i];

                    pip.Write((byte)entry.Type);
                    pip.Write((Int16)entry.X);
                    pip.Write((Int16)entry.Y);

                    switch (entry.Type)
                    {
                        case PIPEntry.PipType.TEXT:
                            pip.Write((byte)entry.Size);
                            pip.Write((Int16)entry.Color);
                            pip.Write((Int16)entry.BackColor);
                            break;

                        case PIPEntry.PipType.IMAGE:
                            break;
                    }

                    pip.Write(Encoding.ASCII.GetBytes(entry.Data));
                }
            }
        }

        public void WriteSerial(string comPort, Int32 baudRate)
        {
            SerialPort arduino = new SerialPort("COM3", baudRate, Parity.None, 8);

            arduino.Open();

            using (BinaryWriter pip = new BinaryWriter(arduino.BaseStream))
            {
                pip.Write('u');
                pip.Flush();

                for (int i = 0; i < _pipEntries.Count; i++)
                {
                    if (i > 0)
                    {
                        // finish previous entry
                        pip.Write((char)13);
                        pip.Write((char)10);
                    }

                    PIPEntry entry = this._pipEntries[i];

                    pip.Write((byte)entry.Type);
                    pip.Write((Int16)entry.X);
                    pip.Write((Int16)entry.Y);

                    switch (entry.Type)
                    {
                        case PIPEntry.PipType.TEXT:
                            pip.Write((byte)entry.Size);
                            pip.Write((Int16)entry.Color);
                            pip.Write((Int16)entry.BackColor);
                            break;

                        case PIPEntry.PipType.IMAGE:
                            break;
                    }

                    pip.Write(Encoding.ASCII.GetBytes(entry.Data));

                    pip.Flush();
                }

                pip.Write((byte)11);
                pip.Flush();
            }

            arduino.Close();
        }

        #endregion
    }
}
