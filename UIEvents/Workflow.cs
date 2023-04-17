using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinDock;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinGrid;
using Allegro;
using Allegro.UI;
using Allegro.UI.ClassEvents;
using Allegro.UI.Controls;
using Allegro.UI.Dialogs;
using Allegro.UI.Forms;
using Allegro.Core.ClassEvents;
using Allegro.Core.Config;
using Allegro.Core.Criteria;
using Allegro.Core.DataModel;
using Allegro.Core.Extensions;
using Allegro.Core.Logging;
using Allegro.Core.Utils;
using Domain = Allegro.Core.ClassEvents.Domain;

namespace Allegro.ClassEvents
{
    public class WorkflowCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* InitializeEnvStyle
        InitializeEnvStyle - DEV - set stylesheet for non-prod environments */
        public UiEventResult InitView_3()
        {
            string[] devServersName = { "https://detrm", "http://detrm" };
            string[] preServersName = { "https://ppetrm", "http://ppetrm" };
            
            // Declare the RGB variables with the Allegro default RGB colors (216, 228, 242)
                int[] ribbonRGB = new int[3] { 216, 228, 242 };
            int[] tabRGB = new int[3] { 216, 228, 242 };
            
            if (Allegro.UI.AppManager.Url.OriginalString.ToLower().StartsWith("http://localhost"))
            {
                //218, 165, 32
                ribbonRGB = new int[3] { 169, 169, 169 };
                tabRGB = new int[3] { 169, 169, 169 };
            }
            
            //Dev Environment
            foreach (string server in devServersName)
            {
                if (Allegro.UI.AppManager.Url.OriginalString.ToLower().StartsWith(server.ToLower()))
                {
                    //[labrego 06/03/2015] Change the RGB color
                    ribbonRGB = new int[3] { 0, 255, 0 };
                    tabRGB = new int[3] { 0, 255, 0 };
                }
            }
            
            //Pre Environment
            foreach (string server in preServersName)
            {
                if (Allegro.UI.AppManager.Url.OriginalString.ToLower().StartsWith(server.ToLower()))
                {
                    //[labrego 06/03/2015] Change the application style
                    ribbonRGB = new int[3] { 222, 184, 135 };
                    tabRGB = new int[3] { 222, 184, 135 };
                }
            }
            
            
            
            // Set the Main Form's Ribbon Area BackColor to the color set int the RGB config values.
            try
            {
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).RibbonAreaAppearance.BackColor = System.Drawing.Color.FromArgb(ribbonRGB[0], ribbonRGB[1], ribbonRGB[2]);
            }
            catch (Exception) { }
            
            //TabItem area
            try
            {
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).TabSettings.TabItemAppearance.BackColor2 = System.Drawing.Color.FromArgb(tabRGB[0], tabRGB[1], tabRGB[2]);
            }
            catch (Exception) { }
            
            //ClientArea
            try
            {
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).TabSettings.ClientAreaAppearance.BackColor2 = System.Drawing.Color.FromArgb(tabRGB[0], tabRGB[1], tabRGB[2]);
            }
            catch (Exception) { }
            
            
            //Selected items
            try
            {
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).TabSettings.SelectedAppearance.BackColor2 = System.Drawing.Color.FromArgb(tabRGB[0], tabRGB[1], tabRGB[2]);
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).TabSettings.SelectedTabItemAppearance.BackColor2 = System.Drawing.Color.FromArgb(tabRGB[0], tabRGB[1], tabRGB[2]);
                ((Infragistics.Win.UltraWinToolbars.Ribbon)((MainForm)_view.ParentForm).ToolbarsManager.Ribbon).TabSettings.ActiveTabItemAppearance.BackColor2 = System.Drawing.Color.FromArgb(tabRGB[0], tabRGB[1], tabRGB[2]);
            }
            catch (Exception) { }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

