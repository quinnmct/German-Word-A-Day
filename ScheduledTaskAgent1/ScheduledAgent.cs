//#define DEBUG_AGENT

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System;
using Microsoft.Phone.Scheduler;
using Telerik.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace ScheduledTaskAgent1
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool classInitialized;	 	
        private Random rand = new Random();
        private int wordCount;
        private List<string> loadWordList;
        private List<string> loadDefinitionList;
        private List<string> loadSentenceList;
        private List<int> loadIndices;
        private DateTime lastUpdate;
        private bool updateTile = false;
        private bool dailyChange;


        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!classInitialized)
            {
                classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
             () =>
             {
                    using (Mutex mutex = new Mutex(true, "MyData"))
                    {
                        mutex.WaitOne();
                        try
                        {
                            lastUpdate = (DateTime)IsolatedStorageSettings.ApplicationSettings["lastUpdate"];
                            dailyChange = (bool)IsolatedStorageSettings.ApplicationSettings["trueIfDaily"];
                            //triggers every day if set to daily
                            if (DateTime.Now.Day != lastUpdate.Day && dailyChange)
                            {
                                updateTile = true;
                                loadIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                                loadWordList = (List<string>)IsolatedStorageSettings.ApplicationSettings["wordList"];
                                loadDefinitionList = (List<string>)IsolatedStorageSettings.ApplicationSettings["definitionList"];
                                loadSentenceList = (List<string>)IsolatedStorageSettings.ApplicationSettings["sentenceList"];
                                loadIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                                wordCount = (int)IsolatedStorageSettings.ApplicationSettings["updateCount"];

                                if (loadIndices.Count == loadWordList.Count)
                                {//resets index list if every word has been chosen || will add more words with update!
                                    loadIndices = new List<int>();
                                }


                                loadIndices.Add(wordCount);
                                wordCount = rand.Next(loadWordList.Count);

                                 while(loadIndices.Contains(wordCount)){
                                    wordCount = rand.Next(loadWordList.Count);
                                 }//returns a new count unused before

                                IsolatedStorageSettings.ApplicationSettings.Remove("chosenIndices");
                                IsolatedStorageSettings.ApplicationSettings.Add("chosenIndices", loadIndices);

                                IsolatedStorageSettings.ApplicationSettings.Remove("updateCount");
                                IsolatedStorageSettings.ApplicationSettings.Add("updateCount", wordCount);                            
                                
                                IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdate");
                                IsolatedStorageSettings.ApplicationSettings.Add("lastUpdate", DateTime.Now);
                                IsolatedStorageSettings.ApplicationSettings.Save();
                            }
                            //triggers every hour if set to hourly
                            else if (DateTime.Now.Hour != lastUpdate.Hour && !dailyChange)
                            {
                                updateTile = true;
                                loadIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                                loadWordList = (List<string>)IsolatedStorageSettings.ApplicationSettings["wordList"];
                                loadDefinitionList = (List<string>)IsolatedStorageSettings.ApplicationSettings["definitionList"];
                                loadSentenceList = (List<string>)IsolatedStorageSettings.ApplicationSettings["sentenceList"];
                                loadIndices = (List<int>)IsolatedStorageSettings.ApplicationSettings["chosenIndices"];
                                wordCount = (int)IsolatedStorageSettings.ApplicationSettings["updateCount"];

                                if (loadIndices.Count == loadWordList.Count)
                                {//resets index list if every word has been chosen || will add more words with update!
                                    loadIndices = new List<int>();
                                }

                                loadIndices.Add(wordCount);
                                wordCount = rand.Next(loadWordList.Count);

                                 while(loadIndices.Contains(wordCount)){
                                    wordCount = rand.Next(loadWordList.Count);
                                 }//returns a new count unused before

                                IsolatedStorageSettings.ApplicationSettings.Remove("chosenIndices");
                                IsolatedStorageSettings.ApplicationSettings.Add("chosenIndices", loadIndices);

                                IsolatedStorageSettings.ApplicationSettings.Remove("updateCount");
                                IsolatedStorageSettings.ApplicationSettings.Add("updateCount", wordCount);                            
                                
                                IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdate");
                                IsolatedStorageSettings.ApplicationSettings.Add("lastUpdate", DateTime.Now);
                                IsolatedStorageSettings.ApplicationSettings.Save();
                            }
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }

                    if (updateTile)
                    {
                         var frontGrid = new Grid();
                        frontGrid.Width = 350;
                        frontGrid.Height = 350;
                        
                        TextBlock wordsBlock = new TextBlock()
                        {
                            Text = loadWordList[wordCount],
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
                            FontSize = 44,
                            Margin = new Thickness(8, 5, 5, 5)
                        };

                        TextBlock defsBlock = new TextBlock()
                        {
                            Text = "\n- " + loadDefinitionList[wordCount],
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
                            FontSize = 36,
                            Margin = new Thickness(22, 25, 12, 5)
                        };
                        TextBlock lineBlock = new TextBlock()
                        {
                            Text = "___________________________________",
                            Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
                            FontSize = 60,
                            Margin = new Thickness(0, 260, 0, 0)
                        };
                        frontGrid.Children.Add(wordsBlock);
                        frontGrid.Children.Add(defsBlock);
                        frontGrid.Children.Add(lineBlock);


                        //////////////////////////
                        //grid to hold BACK tile data
                        var backGrid = new Grid();
                        backGrid.Width = 350;
                        backGrid.Height = 350;
                        
                        TextBlock sentencesBlock = new TextBlock()
                        {
                            Text = loadSentenceList[wordCount],
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
                            FontSize = 34,
                            Margin = new Thickness(5, 5, 15, 5)
                        };
                        TextBlock backLineBlock = new TextBlock()
                        {
                            Text = "___________________________________",
                            Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
                            FontSize = 60,
                            Margin = new Thickness(0, 260, 0, 0)
                        };
                        backGrid.Children.Add(sentencesBlock);
                        backGrid.Children.Add(backLineBlock);


                        //////////////////////
                        //initialize radtilehelper extendedData

                        RadFlipTileData flipData = new RadFlipTileData();

                        flipData.Title = "German Word a Day";
                        flipData.VisualElement = frontGrid;
                        flipData.BackVisualElement = backGrid;
                        flipData.IsTransparencySupported = true;
                        foreach (ShellTile tile in ShellTile.ActiveTiles)
                        {
                            string uri = tile.NavigationUri.ToString();
                            // this will be true only for the secondary tiles that we created and not the application's main tile
                            if (uri != "/")
                            {
                                LiveTileHelper.UpdateTile(tile, flipData);
                                break;
                            }
                        }
                   }
             });    
                    
            NotifyComplete();
        }
    }
}