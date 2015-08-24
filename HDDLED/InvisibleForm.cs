//Please note: This application is purely for my own education, to run through coding 
//examples by following tutorials, and to just tinker around with coding.  
//I know it’s bad practice to heavily comment code (code smell), but comments in all of my 
//exercises will largely be left intact as this serves me 2 purposes:
//    I want to retain what my original thought process was at the time
//    I want to be able to look back in 1..5..10 years to see how far I’ve come
//    And I enjoy commenting on things, however redundant this may be . . . 

//Displays an icon in your system tray that display idle or working status of the HDD

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDDLED
{
    public partial class InvisibleForm : Form
    {
        #region Global Variables

        private NotifyIcon _hddNotifyIcon;
        private Icon busyIcon;
        private Icon idleIcon;
        private Thread hddLedWorker;

        #endregion

        #region Main Form Stuffz

        public InvisibleForm()
        {
            InitializeComponent();

            // Load icons from files into objects
            busyIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            // Create nofify icons and assign idle icon to be displayed
            _hddNotifyIcon = new NotifyIcon
            {
                Icon = idleIcon,
                Visible = true
            };

            // Creating menu items and add them to the notification tray icon
            var progNameMenuItem = new MenuItem("HDD LED v1.0 Beta");
            var quitMenuItem = new MenuItem("Quit");
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            _hddNotifyIcon.ContextMenu = contextMenu;

            // Wire up quit button
            quitMenuItem.Click += quitMenuItem_Click;

            // Hide the form.  This is a notification tray application
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Start worker thread that pulls HDD activity
            hddLedWorker = new Thread(new ThreadStart(HddActivtyThread));
            hddLedWorker.Start();
        }// End InvisibleForm

        #endregion

        #region Context Menu Event Handlers

        // Closes the application upon click
        void quitMenuItem_Click(object sender, EventArgs e)
        {
            hddLedWorker.Abort();
            _hddNotifyIcon.Dispose();
            this.Close();
        }

        #endregion

        #region Activity Threads

        // Thread that pulls the HDD for activity and updates the notification icon
        public void HddActivtyThread()
        {
            // WMI class for pulling system data
            // Found from looking in "wbemtest" (Windows Management Instrumental Tester)
            var driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            try
            {
                // Main loop where all the magic happens
                while (true)
                {
                    // Connect to the drive performance instance
                    var driveDataClassCollection = driveDataClass.GetInstances();
                    foreach (var obj in driveDataClassCollection)
                    {
                        // Only process the total instance and ignore all individual instances
                        if (obj["Name"].ToString() == "_Total")
                        {
                            // Pulls in specific data from the WMI tool
                            // Uses the UINT64 unsigned integer
                            if (Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                // Show busy icon
                                _hddNotifyIcon.Icon = busyIcon;
                            }
                            else
                            {
                                // Show idle icon
                                _hddNotifyIcon.Icon = idleIcon;
                            }

                        }
                    }
                    // Sleep for () so the CPU doens't explode trying to iterate this thread as fast as possible
                    // else this will consume 100% of the CPU processing power
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException)
            {
                driveDataClass.Dispose();
                // Thread was aborted
            }// End try
        }// End HddActivityThread

        #endregion
    }
}
