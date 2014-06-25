﻿using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

using UnityPlayer;

#if WINDOWS_PHONE
using System.Linq;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
#elif NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
#endif

#if NETFX_CORE
namespace UnityProject.Win
#else
namespace UnityProject.WinPhone
#endif
{
    /**
     * This is a partial class containing code that can be shared between all 
     * Unity porting projects in Marker Metro.
     * 
     * 
     * (Dont forget that the namespace must match)
     */
    public sealed partial class MainPage
    {
#if WINDOWS_PHONE
        private Timer timer;
#endif

        /**
         * Exhibits information about memory usage in the game screen. WP8 only.
         * 
         */
        private bool DisplayMemoryInfo = false;


        /**
         * Call this on MainPage.xaml.cs.
         */
        private void Initialize()
        {
            // wire up the configuration file handler:
            DeviceInformation.DoGetEnvironment = GetEnvironment;

            if (DisplayMemoryInfo)
                BeginRecording();
#if NETFX_CORE
            Window.Current.VisibilityChanged += (s, e) =>
            {
                if (!e.Visible)
                    FireTilesUpdate();
            };
#endif
        }

        private DeviceInformation.Environment GetEnvironment()
        {
#if QA
            return DeviceInformation.Environment.QA;
#elif DEBUG
            return DeviceInformation.Environment.Dev;
#else
            return DeviceInformation.Environment.Production;
#endif
        }

        /**
         * Add this to your DrawingSurfaceBackgroundGrid block in MaingPage.xaml:
         *  <TextBlock x:Name="TextBoxMemoryStats" Text="0 MB" IsHitTestVisible="False" Visibility="Collapsed"/>
         */
        private void BeginRecording()
        {
            // start a timer to report memory conditions every 3 seconds 
#if WINDOWS_PHONE
            TextBlockMemoryStats.Visibility = System.Windows.Visibility.Visible;

            timer = new Timer(state =>
            {
                string report = "";
                
                report +=
                   "Current: " + (DeviceStatus.ApplicationCurrentMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Peak: " + (DeviceStatus.ApplicationPeakMemoryUsage / 1000000).ToString() + "MB\n" +
                   "Memory Limit: " + (DeviceStatus.ApplicationMemoryUsageLimit / 1000000).ToString() + "MB\n\n" +
                   "Device Total Memory: " + (DeviceStatus.DeviceTotalMemory / 1000000).ToString() + "MB\n" +
                   "Working Limit: " + Convert.ToInt32((Convert.ToDouble(DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit")) / 1000000)).ToString() + "MB";

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    TextBlockMemoryStats.Text = report;
                    //Debug.WriteLine(report);
                });

            },
                null,
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(3));
#endif
        }

#if NETFX_CORE
        private void FireTilesUpdate()
        {
            // For examples of all possible tile template types go to http://msdn.microsoft.com/library/windows/apps/windows.ui.notifications.tiletemplatetype
            var squareTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text03); // This template requires a SquareTile image in solution Assets folder!
            var wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01); // This template requires a Wide310x150Logo image in solution Assets folder!

            AppCallbacks.Instance.InvokeOnAppThread(() =>
            {
                try
                {
                    var wideTexts = wideTile.GetElementsByTagName("text");
                    var squareTexts = squareTile.GetElementsByTagName("text");
                    UpdateLiveTiles(wideTexts, squareTexts);

                    AppCallbacks.Instance.InvokeOnUIThread(() =>
                    {
                        var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                        var squareTileNotification = new TileNotification(squareTile);
                        var wideTileNotification = new TileNotification(wideTile);

                        updater.Update(squareTileNotification);
                        updater.Update(wideTileNotification);
                    }, false);

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }, false);
        }
#elif WINDOWS_PHONE
        internal static void FireTilesUpdate()
        {
            UnityApp.BeginInvoke(() =>
            {
                try
                {
                    ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault();
                    if (oTile != null)
                    {
                        FlipTileData tileData = UpdateLiveTiles();
                        if(tileData != null) oTile.Update(tileData);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
        }
#endif

#if NETFX_CORE
        /**
         * Updates the Live Tiles.
         * This method offers access to the texts to the wide and medium tiles, but you can tailor
         * it to your project's specific needs.
         * 
         * To use it, just write the game code bits here to update the texts.
         * This method already runs in the game thread, no need to call InvokeOnAppThread.
         */
        private void UpdateLiveTiles(XmlNodeList wideTexts, XmlNodeList squareTexts) {
            /* implement this method! */
        } 
#elif WINDOWS_PHONE
        /**
         * Updates the Live Tiles.
         * 
         * To use it, you must create and return a FlipTileData instance filled with the tile contents.
         * 
         * Attention:
         * You have to add a call to FireTilesUpdate on the beggining of the methods Application_Deactivated and 
         * Application_Closing on App.xaml.cs, like this:
         * YourNamespace.MainPage.FireTilesUpdate();
         */
        private static FlipTileData UpdateLiveTiles()
        {
            return null;
        }
#endif
    }
}
