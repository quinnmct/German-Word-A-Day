using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace TileTest
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();

            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                bool dailyWord;
                mutex.WaitOne();
                try
                {
                    dailyWord = (bool)IsolatedStorageSettings.ApplicationSettings["trueIfDaily"];
                }
                finally
                {
                    mutex.ReleaseMutex();
                }

                if (dailyWord)
                {
                    toggle.IsChecked = true;
                    toggle.Content = "Every Day (24 Hours)";
                }
                else
                {
                    toggle.IsChecked = false;
                    toggle.Content = "Every Hour (60 Minutes)";
                }
                
            }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            toggle.Content = "Every Day (24 Hours)";
            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove("trueIfDaily");
                    IsolatedStorageSettings.ApplicationSettings.Add("trueIfDaily", true);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            toggle.Content = "Every Hour (60 Minutes)";
            using (Mutex mutex = new Mutex(true, "MyMutex"))
            {
                mutex.WaitOne();
                try
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove("trueIfDaily");
                    IsolatedStorageSettings.ApplicationSettings.Add("trueIfDaily", false);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

        }
    }
}