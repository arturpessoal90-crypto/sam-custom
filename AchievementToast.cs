/* Modified version of Steam Achievement Manager (zlib license, see LICENSE.txt).
 * This file was added: custom "X has gotten an achievement" popup.
 */

using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SAM.Game
{
    internal sealed class AchievementToast : Form
    {
        private readonly Timer _CloseTimer = new();

        // Keeps the window from stealing focus from whatever you are doing.
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW (no taskbar button)
                return cp;
            }
        }

        public AchievementToast(string title, string subtitle, int durationMs)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(27, 40, 56);
            this.Size = new Size(380, 76);

            var area = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(area.Right - this.Width - 16, area.Bottom - this.Height - 16);

            var trophy = new Label
            {
                Text = "\U0001F3C6",
                Font = new Font("Segoe UI Emoji", 24F),
                ForeColor = Color.Gold,
                AutoSize = false,
                Bounds = new Rectangle(8, 12, 56, 52),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                AutoEllipsis = true,
                Bounds = new Rectangle(68, 10, 304, 24),
            };

            var subLabel = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(150, 170, 190),
                AutoSize = false,
                AutoEllipsis = true,
                Bounds = new Rectangle(68, 36, 304, 32),
            };

            this.Controls.Add(trophy);
            this.Controls.Add(titleLabel);
            this.Controls.Add(subLabel);

            this._CloseTimer.Interval = Math.Max(500, durationMs);
            this._CloseTimer.Tick += (s, e) =>
            {
                this._CloseTimer.Stop();
                this.Close();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this._CloseTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(102, 192, 244), 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 3, this.Height - 3);
            }
        }

        /// <summary>
        /// Shows "&lt;you&gt; has gotten an achievement" for one or more unlocks.
        /// </summary>
        public static void ShowUnlocked(string gameName, System.Collections.Generic.IList<string> names, int durationMs = 3000)
        {
            if (names == null || names.Count == 0)
            {
                return;
            }

            string who = GetPersonaName();
            string title = names.Count == 1
                ? $"{who} has gotten an achievement"
                : $"{who} has gotten {names.Count} achievements";
            string sub = names.Count == 1
                ? $"{names[0]}  -  {gameName}"
                : $"{names[0]} (+{names.Count - 1} more)  -  {gameName}";

            new AchievementToast(title, sub, durationMs).Show();
        }

        private static string _CachedName;

        /// <summary>
        /// Reads your Steam profile name from Steam's loginusers.vdf (the account that logged in last).
        /// Falls back to "Me" if it can't be found.
        /// </summary>
        public static string GetPersonaName()
        {
            if (_CachedName != null)
            {
                return _CachedName;
            }

            string name = "Me";
            try
            {
                string steam = API.Steam.GetInstallPath();
                if (steam != null)
                {
                    string path = Path.Combine(steam, "config", "loginusers.vdf");
                    if (File.Exists(path) == true)
                    {
                        string text = File.ReadAllText(path);
                        foreach (Match block in Regex.Matches(text, "\"\\d{17}\"\\s*\\{([^}]*)\\}"))
                        {
                            string body = block.Groups[1].Value;
                            if (Regex.IsMatch(body, "\"MostRecent\"\\s*\"1\"") == false)
                            {
                                continue;
                            }

                            var m = Regex.Match(body, "\"PersonaName\"\\s*\"([^\"]*)\"");
                            if (m.Success == true && m.Groups[1].Value.Length > 0)
                            {
                                name = m.Groups[1].Value;
                            }
                            break;
                        }
                    }
                }
            }
            catch
            {
                // keep the fallback
            }

            _CachedName = name;
            return name;
        }
    }
}
