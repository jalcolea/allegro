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
    public class cst_EPLCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* EPL - Init View
        EPL - EVE - Init View */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* EPL - Load Files
        EPL - EVE - Logic for toolclick button for load files */
        public UiEventResult ToolClick_Load_Files_After_1()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Excel Files|*.xlsx";
                ofd.FilterIndex = 1;
                ofd.Multiselect = true;
                DialogResult dialogResult = ofd.ShowDialog();
                
                if (DialogResult.OK.Equals(dialogResult))
                {
                    System.Text.StringBuilder errors = new System.Text.StringBuilder();
                    
                    foreach (string fileName in ofd.FileNames)
                    {
                        string newFileName = System.IO.Path.GetFileName(fileName);
                        
                        Soap.Invoke("cst_EPLWS.asmx", "LoadFile", new string[] { "filename", "documentData" }, new object[] { newFileName, Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName)) });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Uncontrolled Error", ex.ToString());
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

