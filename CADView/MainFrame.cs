using System;
using System.IO;
using System.Windows.Forms;
using CADCore;
using Engine;

namespace CADView
{
    public partial class MainFrame : Form
    {
        readonly CADTimer _t = new CADTimer();

        private class CADTimer : Timer, ITick
        {
            public event ManagementControl.SystemTickDelegate TickEvent;

            protected override void OnTick(EventArgs e)
            {
                base.OnTick(e);
                if (TickEvent != null)
                    TickEvent(Interval);
            }
        }

        public MainFrame()
        {
            _t.Interval = 1000/30;
            _t.Enabled = true;

            ManagementControl.CreateCADManagement(_t);

            this.FormClosing += (sender, args) => Log._FlushCachedLog();

            InitializeComponent();

            TestOpLine.Click += delegate
            {
                if (ManagementControl.Instance.CurrentCadDocument == null) return;

                ManagementControl.Instance.BeginOperation("Create Line");
            };
            button5.Click += delegate
            {
                if (ManagementControl.Instance.CurrentCadDocument == null) return;

                ManagementControl.Instance.BeginOperation("Create Circle");
            };

            button4.Click +=
                (sender, args) =>
                {
                    if (ManagementControl.Instance.CurrentCadDocument != null)
                    {
                        var block = ManagementControl.Instance.CurrentCadDocument.SaveDocument();
                        block.SetAttribute("version", "Adem X");
                        block.SetAttribute("status", "unzipped");
                        File.WriteAllText(DateTime.Now.ToString().Replace('.', '_').Replace(':', '_') + ".txt",
                            block.DumpToString());
                    }
                };

            LinkCheckBox.CheckedChanged +=
                (sender, args) => ManagementControl.Instance.LinkVertices = LinkCheckBox.Checked;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var page = new TabPage();
            tabControl1.TabPages.Add(page);
            page.Controls.Add(new DocumentControl { Dock = DockStyle.Fill });
            page.Text = ManagementControl.Instance.CurrentCadDocument.DocumentID.ToString();
            page.Tag = ManagementControl.Instance.CurrentCadDocument;
            tabControl1.SelectTab(page);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ManagementControl.Instance.CurrentCadDocument.History.Undo();
            Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ManagementControl.Instance.CurrentCadDocument.History.Redo();
            Refresh();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            ManagementControl.Instance.CurrentCadDocument = (CadDocument)e.TabPage.Tag;
            Refresh();
        }
    }
}
