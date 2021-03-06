﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PIPEditor
{
    public partial class frmMain : Form
    {
        #region private properties

        private PIPFile _pipFile;

        #endregion

        #region constructor

        public frmMain()
        {
            InitializeComponent();

            // populate dropdown
            cbEntryType.DataSource = Enum.GetValues(typeof(PIPEntry.PipType));
        }

        #endregion

        #region events

        private void dlgOpenPip_FileOk(object sender, CancelEventArgs e)
        {
            _pipFile = new PIPFile(dlgOpenPip.FileName, txtComPort.Text, int.Parse(txtBaudRate.Text), dataReceived);
            _pipFile.Load();

            bndPipFile.DataSource = _pipFile;
            renderPip();
        }

        private void dataReceived(string data)
        {
            if (txtComOut.InvokeRequired)
            {
                txtComOut.Invoke(new MethodInvoker(delegate { txtComOut.AppendText(data); }));
            }
        }

        private void lbPipEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            bndPipEntry.DataSource = lbPipEntries.SelectedItem;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dlgOpenPip.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_pipFile != null)
            {
                _pipFile.Save();

                MessageBox.Show("Pip File Saved!");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _pipFile.NewEntry();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            _pipFile.RemoveEntry((PIPEntry)lbPipEntries.SelectedItem);
        }

        private void bndPipFile_DataSourceChanged(object sender, EventArgs e)
        {
            lbPipEntries.DataSource = _pipFile.PipEntries;
        }

        private void bndPipEntry_CurrentItemChanged(object sender, EventArgs e)
        {
            renderPip();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void cmdWriteSerial_Click(object sender, EventArgs e)
        {
            _pipFile.WriteSerial();
            _pipFile.Save();
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (_pipFile != null)
            {
                _pipFile.ToggleCom();
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            this._pipFile.MoveEntryUp((PIPEntry)lbPipEntries.SelectedItem);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            this._pipFile.MoveEntryDown((PIPEntry)lbPipEntries.SelectedItem);
        }

        #endregion

        #region private methods

        private void renderPip()
        {
            // Initialise Pip Screen
            Bitmap pipImage = new Bitmap(picPipScreen.Width, picPipScreen.Height);
            Graphics graphics = Graphics.FromImage(pipImage);

            graphics.FillRectangle(Brushes.Black, new RectangleF(0, 0, pipImage.Width, pipImage.Height));

            foreach (PIPEntry entry in _pipFile.PipEntries)
            {
                switch (entry.Type)
                {
                    case PIPEntry.PipType.SUBSCREENS:
                        renderSubScreens(entry, graphics);
                        break;

                    case PIPEntry.PipType.SUBMENUS:
                        renderSubMenus(entry, graphics);
                        break;

                    case PIPEntry.PipType.TEXT:
                        renderText(entry, graphics);
                        break;

                    case PIPEntry.PipType.IMAGE:
                        renderImage(entry, graphics);
                        break;

                    case PIPEntry.PipType.LINE:
                        renderLine(entry, graphics);
                        break;

                    case PIPEntry.PipType.RECT:
                        renderRect(entry, graphics);
                        break;

                    case PIPEntry.PipType.FILLRECT:
                        renderFillRect(entry, graphics);
                        break;
                }
            }

            picPipScreen.Image = pipImage;
        }

        public static Color convert565ToColour(Int16 color)
        {
            Int32 red = (Int32)(((color >> 0xA) & 0x1F) * 8.225806f);
            Int32 green = (Int32)(((color >> 0x5) & 0x1F) * 8.225806f);
            Int32 blue = (Int32)((color & 0x1F) * 8.225806f);

            if (red < 0)
                red = 0;
            else if (red > 0xFF)
                red = 0xFF;

            if (green < 0)
                green = 0;
            else if (green > 0xFF)
                green = 255;

            if (blue < 0)
                blue = 0;
            else if (blue > 0xFF)
                blue = 0xFF;

            return Color.FromArgb(255, red, green, blue);
        }

        private void renderSubScreens(PIPEntry entry, Graphics graphics)
        {
            using (Font font = new Font("Terminal", 7))
            {
                // Render Text
                var menuTitles = string.Join("  ", entry.Data.Split('|'));

                var brush = new SolidBrush(Color.Green);
                graphics.DrawString(menuTitles, font, brush, new PointF(entry.X, entry.Y));
            }
        }

        private void renderSubMenus(PIPEntry entry, Graphics graphics)
        {
            using (Font font = new Font("Terminal", 7))
            {
                // Render Text
                var menuTitles = entry.Data.Split('|');
                var menuItemHeight = 20;
                var startX = 20;
                var startY = 55;

                foreach (string menuTitle in menuTitles)
                {
                    var brush = new SolidBrush(Color.Green);
                    graphics.DrawString(menuTitle, font, brush, new PointF(startX, startY));

                    startY += menuItemHeight;
                }

            }
        }

        private void renderText(PIPEntry entry, Graphics graphics)
        {
            using (Font font = new Font("Terminal", entry.Size * 7))
            {
                // Render Background
                var backgroundBrush = new SolidBrush(convert565ToColour(entry.BackColor));
                graphics.FillRectangle(backgroundBrush, new RectangleF(new PointF(entry.X, entry.Y), graphics.MeasureString(entry.Data, font)));

                // Render Text
                var brush = new SolidBrush(convert565ToColour(entry.Color));
                graphics.DrawString(entry.Data, font, brush, new PointF(entry.X, entry.Y));
            }
        }

        private void renderImage(PIPEntry entry, Graphics graphics)
        {
            string directory = Path.GetDirectoryName(_pipFile.FilePath);
            string path = string.Format(@"{0}\{1}", directory, entry.Data);

            if (File.Exists(path))
            {
                Image image = Image.FromFile(path);

                graphics.DrawImage(image, new PointF(entry.X, entry.Y));
            }
        }

        private void renderLine(PIPEntry entry, Graphics graphics)
        {
            var pen = new Pen(convert565ToColour(entry.Color));
            graphics.DrawLine(pen, new PointF(entry.X, entry.Y), new PointF(entry.EndX, entry.EndY));
        }

        private void renderRect(PIPEntry entry, Graphics graphics)
        {
            var pen = new Pen(convert565ToColour(entry.Color));
            graphics.DrawRectangle(pen, new Rectangle(entry.X, entry.Y, entry.EndX, entry.EndY));
        }

        private void renderFillRect(PIPEntry entry, Graphics graphics)
        {
            var brush = new SolidBrush(convert565ToColour(entry.Color));
            graphics.FillRectangle(brush, new Rectangle(entry.X, entry.Y, entry.EndX, entry.EndY));
        }

        #endregion
    }
}
