using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            _pipFile = new PIPFile(dlgOpenPip.FileName);
            _pipFile.Load();

            bndPipFile.DataSource = _pipFile;
            lbPipEntries.DataSource = _pipFile.PipEntries;
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (_pipFile != null)
            {
                _pipFile.PipEntries.Add(new PIPEntry() { Data = "New", X = 0, Y = 0, Type = PIPEntry.PipType.TEXT, Size = 1 });
                bndPipFile.DataSource = _pipFile;
                lbPipEntries.DataSource = _pipFile.PipEntries;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (_pipFile != null && lbPipEntries.SelectedItem != null)
            {
                _pipFile.PipEntries.Remove((PIPEntry)lbPipEntries.SelectedItem);
            }
        }

        #endregion
    }
}
