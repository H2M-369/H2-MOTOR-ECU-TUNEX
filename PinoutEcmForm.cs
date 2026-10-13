using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace _3D_Chart_Graph
{
    /// <summary>Viewer referensi pinout ECM Honda. Branding halaman sumber disembunyikan di viewer aplikasi.</summary>
    internal sealed class PinoutEcmForm : Form
    {
        private const string PinoutUrl = "https://autotroniccommunity.github.io/pinouECMHonda/";
        private readonly WebBrowser browser;
        private readonly Label status;

        public PinoutEcmForm()
        {
            Text = "EcuTuneX | Pinout ECM Honda";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(850, 580);
            Size = new Size(1180, 760);
            BackColor = Color.FromArgb(12, 12, 15);
            ForeColor = Color.White;
            ShowIcon = false;

            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(22, 22, 27), Padding = new Padding(12, 8, 12, 8) };
            var title = new Label { Text = "PINOUT ECM HONDA", Dock = DockStyle.Left, Width = 260, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold), ForeColor = Color.FromArgb(255, 75, 85) };
            var open = new Button { Text = "Buka di Browser", Dock = DockStyle.Right, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(35, 35, 42), ForeColor = Color.White };
            open.FlatAppearance.BorderColor = Color.FromArgb(214, 28, 46);
            open.Click += delegate { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(PinoutUrl) { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show("Tidak dapat membuka browser: " + ex.Message, "Pinout ECM", MessageBoxButtons.OK, MessageBoxIcon.Warning); } };
            header.Controls.Add(open);
            header.Controls.Add(title);

            status = new Label { Dock = DockStyle.Bottom, Height = 25, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), BackColor = Color.FromArgb(22, 22, 27), ForeColor = Color.Gainsboro, Text = "Memuat referensi pinout..." };
            browser = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = true, WebBrowserShortcutsEnabled = true, AllowWebBrowserDrop = false };
            browser.DocumentCompleted += Browser_DocumentCompleted;
            Controls.Add(browser);
            Controls.Add(status);
            Controls.Add(header);
            Shown += delegate { browser.Navigate(PinoutUrl); };
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (browser.Url == null || e.Url == null || !string.Equals(browser.Url.Host, e.Url.Host, StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                if (browser.Document != null && browser.Document.Body != null)
                {
                    // Hilangkan hanya teks nama sekolah pada halaman yang ditampilkan di viewer ini.
                    string html = browser.Document.Body.InnerHtml ?? string.Empty;
                    html = Regex.Replace(html, "SMK\\s+GAJAH\\s+MADA\\s+BANYUWANGI", string.Empty, RegexOptions.IgnoreCase);
                    browser.Document.Body.InnerHtml = html;
                    status.Text = "Referensi pinout ECM Honda | Periksa tipe konektor dan nomor part sebelum wiring.";
                }
            }
            catch
            {
                status.Text = "Referensi terbuka; beberapa elemen mungkin tidak dapat disesuaikan oleh engine browser lama.";
            }
        }
    }
}
