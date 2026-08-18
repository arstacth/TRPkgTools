using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TRpkgTools
{
    public partial class MainForm : Form
    {
        static readonly Color Panel = Color.FromArgb(32, 32, 36);
        static readonly Color Border = Color.FromArgb(58, 58, 64);
        static readonly Color Muted = Color.FromArgb(150, 150, 158);
        static readonly Color Accent = Color.FromArgb(88, 166, 255);
        static readonly Color Ok = Color.FromArgb(110, 200, 140);
        static readonly Color Err = Color.FromArgb(232, 110, 110);

        readonly List<string> _log = new List<string>();
        Point _dragOffset;
        bool _dragging;
        bool _busy;
        int _barValue;
        int _barMax = 1;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            besidePkgCheck.BringToFront();
            debugCheck.BringToFront();

            MouseDown += OnDragStart;
            MouseMove += OnDragMove;
            MouseUp += OnDragEnd;
            foreach (Control c in new Control[] { titleLabel, subtitleLabel, pathCaptionLabel, statusLabel, progressPanel })
            {
                c.MouseDown += OnDragStart;
                c.MouseMove += OnDragMove;
                c.MouseUp += OnDragEnd;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Border))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void browseButton_Click(object sender, EventArgs e)
        {
            if (_busy)
                return;
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select PKG or filelist";
                dlg.Filter = "PKG / filelist|*.pkg;filelist_*.txt|PKG (*.pkg)|*.pkg|File list (filelist_*.txt)|filelist_*.txt|All files|*.*";
                dlg.CheckFileExists = true;
                string cur = (pathBox.Text ?? "").Trim().Trim('"');
                if (File.Exists(cur))
                {
                    dlg.FileName = Path.GetFileName(cur);
                    string dir = Path.GetDirectoryName(cur);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                        dlg.InitialDirectory = dir;
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    pathBox.Text = dlg.FileName;
            }
        }

        void unpackButton_Click(object sender, EventArgs e)
        {
            Start(true);
        }

        void repackButton_Click(object sender, EventArgs e)
        {
            Start(false);
        }

        void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;
            pathBox.Text = files[0];
        }

        void progressPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Panel);
            int max = _barMax <= 0 ? 1 : _barMax;
            int w = (int)(progressPanel.Width * ((double)_barValue / max));
            if (w > 0)
            {
                using (var b = new SolidBrush(Accent))
                    e.Graphics.FillRectangle(b, 0, 0, Math.Min(w, progressPanel.Width), progressPanel.Height);
            }
        }

        void SetProgress(int value, int max, string file)
        {
            Ui(() =>
            {
                _barMax = max <= 0 ? 1 : max;
                _barValue = value;
                progressPanel.Invalidate();
                if (!string.IsNullOrEmpty(file))
                {
                    _log.Add(file);
                    while (_log.Count > 10)
                        _log.RemoveAt(0);
                    fileLogBox.Text = string.Join("\r\n", _log.ToArray());
                }
            });
        }

        void Start(bool unpack)
        {
            if (_busy)
                return;
            string input = (pathBox.Text ?? "").Trim().Trim('"');
            bool debug = unpack && debugCheck.Checked;
            if (debug)
                input = ResolveTr4(input);
            if (input.Length == 0 || !File.Exists(input))
            {
                SetStatus(debug ? "Select a tr4.pkg." : (unpack ? "Select a .pkg file." : "Select filelist_*.txt."), Err);
                return;
            }

            if (unpack)
            {
                if (debug)
                {
                    if (!string.Equals(Path.GetFileName(input), "tr4.pkg", StringComparison.OrdinalIgnoreCase))
                    {
                        SetStatus("Select a tr4.pkg.", Err);
                        return;
                    }
                }
                else if (!input.EndsWith(".pkg", StringComparison.OrdinalIgnoreCase))
                {
                    SetStatus("Unpack needs a .pkg file.", Err);
                    return;
                }
            }
            else if (PkgPack.PackageNameFromFilelist(input) == null)
            {
                SetStatus("Repack needs filelist_[name].txt.", Err);
                return;
            }

            _busy = true;
            unpackButton.Enabled = false;
            repackButton.Enabled = false;
            browseButton.Enabled = false;
            besidePkgCheck.Enabled = false;
            debugCheck.Enabled = false;
            _log.Clear();
            fileLogBox.Text = "";
            SetStatus("", Muted);
            SetProgress(0, 1, "");
            string outDir = PkgJobs.ExeDir();
            if (unpack && besidePkgCheck.Checked)
            {
                string pkgDir = Path.GetDirectoryName(Path.GetFullPath(input));
                if (!string.IsNullOrEmpty(pkgDir))
                    outDir = pkgDir;
            }
            outDir = Path.GetFullPath(outDir);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string opened = null;
                    string msg;
                    if (!unpack)
                        msg = PkgJobs.Repack(input, PkgJobs.ExeDir(), SetProgress);
                    else if (debug)
                        msg = PkgJobs.UnpackDebug(input, outDir, SetProgress, out opened);
                    else
                        msg = PkgJobs.Unpack(input, outDir, SetProgress);
                    Done(true, msg, opened);
                }
                catch (Exception ex)
                {
                    Done(false, ex.Message, null);
                }
            });
        }

        void Done(bool ok, string message, string openFile)
        {
            Ui(() =>
            {
                _busy = false;
                unpackButton.Enabled = true;
                repackButton.Enabled = true;
                browseButton.Enabled = true;
                besidePkgCheck.Enabled = true;
                debugCheck.Enabled = true;
                if (ok)
                    _barValue = _barMax;
                progressPanel.Invalidate();
                SetStatus(message, ok ? Ok : Err);
                if (ok && !string.IsNullOrEmpty(openFile) && File.Exists(openFile))
                {
                    try
                    {
                        Process.Start("notepad.exe", "\"" + openFile + "\"");
                    }
                    catch
                    {
                    }
                }
            });
        }

        static string ResolveTr4(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            if (Directory.Exists(input))
                return Path.Combine(input, "tr4.pkg");
            if (File.Exists(input)
                && string.Equals(Path.GetFileName(input), "tr4.pkg", StringComparison.OrdinalIgnoreCase))
                return input;
            string dir = Path.GetDirectoryName(input);
            if (!string.IsNullOrEmpty(dir))
                return Path.Combine(dir, "tr4.pkg");
            return Path.Combine(input, "tr4.pkg");
        }

        void SetStatus(string text, Color color)
        {
            statusLabel.Text = text ?? "";
            statusLabel.ForeColor = color;
        }

        void Ui(Action a)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
                BeginInvoke(a);
            else
                a();
        }

        void OnDragStart(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            _dragging = true;
            _dragOffset = e.Location;
            if (sender != this && sender is Control)
            {
                Control c = (Control)sender;
                _dragOffset = new Point(e.X + c.Left, e.Y + c.Top);
            }
        }

        void OnDragMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
                return;
            Point screen = PointToScreen(e.Location);
            if (sender != this && sender is Control)
            {
                Control c = (Control)sender;
                screen = c.PointToScreen(e.Location);
            }
            Location = new Point(screen.X - _dragOffset.X, screen.Y - _dragOffset.Y);
        }

        void OnDragEnd(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }
    }
}
