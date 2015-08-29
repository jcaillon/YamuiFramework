﻿using System;
using System.Drawing;
using System.Windows.Forms;
using YamuiFramework.Animations;
using YamuiFramework.Controls;
using YamuiFramework.Native;
using YamuiFramework.Themes;

namespace YamuiFramework.Forms
{
    public sealed class YamuiTaskWindow : YamuiForm
    {
        private static YamuiTaskWindow singletonWindow;
        public static void ShowTaskWindow(IWin32Window parent, string title, Control userControl, int secToClose)
        {
            if (singletonWindow != null)
            {
                singletonWindow.Close();
                singletonWindow.Dispose();
                singletonWindow = null;
            }

            singletonWindow = new YamuiTaskWindow(secToClose, userControl);
            singletonWindow.Text = title;
            singletonWindow.Resizable = false;
            singletonWindow.Movable = true;
            singletonWindow.StartPosition = FormStartPosition.Manual;

            singletonWindow.Show();
        }

        public static bool IsVisible()
        {
            return (singletonWindow != null && singletonWindow.Visible);
        }

        public static void ShowTaskWindow(IWin32Window parent, string text, Control userControl)
        {
            ShowTaskWindow(parent, text, userControl, 0);
        }

        public static void ShowTaskWindow(string text, Control userControl, int secToClose)
        {
            ShowTaskWindow(null, text, userControl, secToClose);
        }

        public static void ShowTaskWindow(string text, Control userControl)
        {
            ShowTaskWindow(null, text, userControl);
        }

        public static void CancelAutoClose()
        {
            if (singletonWindow != null)
                singletonWindow.CancelTimer = true;
        }

        public static void ForceClose()
        {
            if (singletonWindow != null)
            {
                CancelAutoClose();
                singletonWindow.Close();
                singletonWindow.Dispose();
                singletonWindow = null;
            }
        }

        private bool cancelTimer;
        public bool CancelTimer
        {
            get { return cancelTimer; }
            set { cancelTimer = value; }
        }

        private readonly int closeTime;
        private int elapsedTime;
        private int progressWidth;
        private DelayedCall timer;

        private readonly YamuiPanel controlContainer;

        public YamuiTaskWindow()
        {
            controlContainer = new YamuiPanel();
            Controls.Add(controlContainer);
        }

        public YamuiTaskWindow(int duration, Control userControl)
            : this()
        {
            controlContainer.Controls.Add(userControl);
            userControl.Dock = DockStyle.Fill;
            closeTime = duration * 500;

            if (closeTime > 0)
                timer = DelayedCall.Start(UpdateProgress, 5);
        }


        private bool isInitialized;
        protected override void OnActivated(EventArgs e)
        {
            if (!isInitialized)
            {
                MaximizeBox = false;
                MinimizeBox = false;
                Movable = true;

                TopMost = true;

                Size = new Size(400, 200);

                Taskbar myTaskbar = new Taskbar();
                switch (myTaskbar.Position)
                {
                    case TaskbarPosition.Left:
                        Location = new Point(myTaskbar.Bounds.Width + 5, myTaskbar.Bounds.Height - Height - 5);
                        break;
                    case TaskbarPosition.Top:
                        Location = new Point(myTaskbar.Bounds.Width - Width - 5, myTaskbar.Bounds.Height + 5);
                        break;
                    case TaskbarPosition.Right:
                        Location = new Point(myTaskbar.Bounds.X - Width - 5, myTaskbar.Bounds.Height - Height - 5);
                        break;
                    case TaskbarPosition.Bottom:
                        Location = new Point(myTaskbar.Bounds.Width - Width - 5, myTaskbar.Bounds.Y - Height - 5);
                        break;
                    case TaskbarPosition.Unknown:
                    default:
                        Location = new Point(Screen.PrimaryScreen.Bounds.Width - Width - 5, Screen.PrimaryScreen.Bounds.Height - Height - 5);
                        break;
                }

                controlContainer.Location = new Point(0, 60);
                controlContainer.Size = new Size(Width - 40, Height - 80);
                controlContainer.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

                controlContainer.AutoScroll = false;
                controlContainer.HorizontalScrollbar = false;
                controlContainer.VerticalScrollbar = false;
                controlContainer.Refresh();

                isInitialized = true;
            }

            base.OnActivated(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (SolidBrush b = new SolidBrush(ThemeManager.Current.FormColorBackColor))
            {
                e.Graphics.FillRectangle(b, new Rectangle(Width - progressWidth, 0, progressWidth, 5));
            }
        }

        private void UpdateProgress()
        {
            if (elapsedTime == closeTime)
            {
                timer.Dispose();
                timer = null;
                Close();
                return;
            }

            elapsedTime += 5;

            if (cancelTimer)
                elapsedTime = 0;

            double perc = elapsedTime / ((double)closeTime / 100);
            progressWidth = (int)(Width * (perc / 100));
            Invalidate(new Rectangle(0,0,Width,5));

            if (!cancelTimer)
                timer.Reset();
        }
    }
}
