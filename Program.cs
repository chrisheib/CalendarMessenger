using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DDay.iCal;

namespace CalendarMessenger
{
    static class Program
    {
        static readonly string CRLF = Environment.NewLine;
        static NotifyIcon notifyIcon;
        static Thread thread;
        static string text = "";
        static Icon iconAchtung;
        static Icon iconNormal;
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            PrepareIcons();

            var menu = new ContextMenu();

            var mnuExit = new MenuItem("Exit");
            mnuExit.Click += new EventHandler(MnuExit_Click);
            menu.MenuItems.Add(0, mnuExit);

            notifyIcon = new NotifyIcon()
            {
                Icon = iconNormal,
                ContextMenu = menu,
                Text = "Main",
                Visible = true
            };

            notifyIcon.Click += new EventHandler(NotifyIcon_Click);

            thread = new Thread(delegate ()
                {
                    DoCalendarStuff();
                }
            );

            thread.Start();

            Application.Run();
        }

        static void MnuExit_Click(object sender, EventArgs e)
        {
            thread.Abort();
            Exit();
        }

        static void NotifyIcon_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                ShowBalloon();
            }
        }

        static void Exit()
        {
            notifyIcon.Dispose();
            Application.Exit();
        }

        static void ShowBalloon()
        {
            if (text != "")
            {
                lock (notifyIcon)
                {
                    notifyIcon.BalloonTipTitle = "Müll";
                    notifyIcon.BalloonTipText = text;
                    notifyIcon.ShowBalloonTip(30000);
                }
            }
        }

        static void DoCalendarStuff()
        {
            // Load the calendar file
            IICalendarCollection calendars = iCalendar.LoadFromFile(@"a.ics");

            // Get all events that occur today.
            IList<Occurrence> occurrences = calendars.GetOccurrences(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

            // Iterate through each occurrence and display information about it
            foreach (Occurrence occurrence in occurrences)
            {
                DateTime occurrenceTime = occurrence.Period.StartTime.Local;
                if (occurrence.Source is IRecurringComponent rc)
                {
                    if (CheckMessage(rc.Summary, occurrenceTime))
                    {
                        text += rc.Summary + ": " + occurrenceTime.ToShortDateString() + CRLF;
                    }
                    
                }
            }
            if (text != "")
            {
                lock (notifyIcon)
                {
                    notifyIcon.Icon = iconAchtung;
                    ShowBalloon();
                }
            }
            else
            {
                text = "Kein Müll!";
            }
        }

        static bool CheckMessage(String s, DateTime d)
        {
            var b = false;

            if (s.Contains("Bioabfall"))
            {
                b = true;
            }

            if (s.Contains("Papier") || s.Contains("Gelber Sack"))
            {
                if (d.Month % 2 == 0)
                {
                    b = true;
                }
            }

            return b;
        }

        static void PrepareIcons()
        {
            iconNormal = Properties.Resources.icons8_clock_32;
            iconAchtung = Properties.Resources.icons8_clock_alert_32;
        }
    }
}
