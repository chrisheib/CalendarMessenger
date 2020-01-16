using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var menu = new ContextMenu();
            var mnuExit = new MenuItem("Exit");
            menu.MenuItems.Add(0, mnuExit);

            notifyIcon = new NotifyIcon()
            {
                Icon = new Icon(SystemIcons.Application, 40, 40),
                ContextMenu = menu,
                Text = "Main"
            };
            mnuExit.Click += new EventHandler(MnuExit_Click);

            notifyIcon.Visible = true;

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

        static void Exit()
        {
            notifyIcon.Dispose();
            Application.Exit();
        }


        static void ShowBalloon(NotifyIcon icon, string title, string body)
        {
            if (title != null)
            {
                icon.BalloonTipTitle = title;
            }

            if (body != null)
            {
                icon.BalloonTipText = body;
            }

            icon.ShowBalloonTip(30000);
        }

        static void DoCalendarStuff()
        {

            // Load the calendar file
            IICalendarCollection calendars = iCalendar.LoadFromFile(@"a.ics");

            //
            // Get all events that occur today.
            //
            IList<Occurrence> occurrences = calendars.GetOccurrences(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

            Console.WriteLine("Today's Events:");

            var text = "";
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
                    ShowBalloon(notifyIcon, "Müll!", text);
                }
                Thread.Sleep(30000);
            }
            Exit();
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
    }
}
