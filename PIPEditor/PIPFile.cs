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

        private SerialPort _arduino;
        private string _filePath;
        private BindingList<PIPEntry> _pipEntries;
        private DataReceived _dataReceived;

        #endregion

        #region public properties

        public delegate void DataReceived(string data);
        public IEnumerable<PIPEntry> PipEntries { get { return this._pipEntries; } }
        public string FilePath { get { return _filePath; } }

        #endregion

        #region constructor

        public PIPFile(string filePath, string comPort, Int32 baudRate, DataReceived dataReceived)
        {
            this._filePath = filePath;
            this._dataReceived = dataReceived;
            this._arduino = new SerialPort(comPort, baudRate, Parity.None, 8);
            this._arduino.DataReceived += this._arduino_DataReceived;
            this._arduino.WriteBufferSize = 4096 * 10;
        }

        ~PIPFile()
        {
            if (this._arduino.IsOpen)
            {
                this._arduino.Close();
            }
        }

        #endregion

        #region public methods

        public void MoveEntryUp(PIPEntry entry)
        {
            if (this._pipEntries.Count > 1)
            {
                var currentIndex = this._pipEntries.IndexOf(entry);

                if (currentIndex > 0)
                {
                    this._pipEntries.Remove(entry);
                    this._pipEntries.Insert(currentIndex - 1, entry);
                }
            }
        }

        public void MoveEntryDown(PIPEntry entry)
        {
            if (this._pipEntries.Count > 1)
            {
                var currentIndex = this._pipEntries.IndexOf(entry);

                if (currentIndex < (this._pipEntries.Count - 1))
                {
                    this._pipEntries.Remove(entry);
                    this._pipEntries.Insert(currentIndex + 1, entry);
                }
            }
        }

        public void ToggleCom()
        {
            if (!this._arduino.IsOpen)
            {
                this._arduino.Open();
            }
            else
            {
                this._arduino.Close();
            }
        }

        public void NewEntry()
        {
            if (this._pipEntries != null)
            {
                this._pipEntries.Add(new PIPEntry() { Data = "New", X = 0, Y = 0, Type = PIPEntry.PipType.TEXT, Size = 1 });
            }
        }

        public void RemoveEntry(PIPEntry entry)
        {
            if (this._pipEntries != null)
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

                        case PIPEntry.PipType.SUBMENUS:
                        case PIPEntry.PipType.SUBSCREENS:
                        case PIPEntry.PipType.IMAGE:
                            break;

                        case PIPEntry.PipType.LINE:
                        case PIPEntry.PipType.FILLRECT:
                        case PIPEntry.PipType.RECT:
                            entry.EndX = pip.ReadInt16();
                            entry.EndY = pip.ReadInt16();
                            entry.Color = pip.ReadInt16();
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
                            pip.Write(Encoding.ASCII.GetBytes(entry.Data));
                            break;

                        case PIPEntry.PipType.SUBMENUS:
                        case PIPEntry.PipType.SUBSCREENS:
                        case PIPEntry.PipType.IMAGE:
                            pip.Write(Encoding.ASCII.GetBytes(entry.Data));
                            break;

                        case PIPEntry.PipType.LINE:
                        case PIPEntry.PipType.FILLRECT:
                        case PIPEntry.PipType.RECT:
                            pip.Write((Int16)entry.EndX);
                            pip.Write((Int16)entry.EndY);
                            pip.Write((Int16)entry.Color);
                            break;
                    }
                }
            }
        }

        public void WriteSerial()
        {
            if (!this._arduino.IsOpen)
            {
                this._arduino.Open();
            }
            
            this.Save();

            this._arduino.Write("u");

            byte[] pipData = File.ReadAllBytes(_filePath);

            this._arduino.Write(pipData, 0, pipData.Length);

            this._arduino.Write(new byte[] { 11 }, 0, 1);
            this._arduino.BaseStream.Flush();
        }

        #endregion

        #region private methods

        private void _arduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string data = serialPort.ReadExisting();

            this._dataReceived(data);
        }

        #endregion
    }
}
