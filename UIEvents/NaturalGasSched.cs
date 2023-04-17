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
    public class NaturalGasSchedCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* NaturalGasSched - ToolClick_Constraints_Before
        button on natgassched to run constraints */
        public UiEventResult ToolClick_Constraints_Before_1000()
        {
            try
            {
                string msg_ex = string.Empty;
                string msg_res = string.Empty;
                
                //If no constraints selected, check for Group parameter. Else execute for all active.
                msg_ex = "Run Trade Contraints Report for all Active definitions?";
                if (MessageBox.Show(msg_ex, "Trade Constraints", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return new UiEventResult(EventStatus.Cancel);
                
                msg_res = "Full constraint report execution added to grid queue.";
                
                string[] args = new string[2] { "groups_in", "constraints_in" };
                object[] parms = new object[2] { null, null };
                string result = Soap.Invoke<string>("ConstraintsWS.asmx", "StartTaskConstraints", args, parms);
                
                if (result == "ERROR")
                {
                    msg_res = string.Format("Constraint report execution encountered errors.\nPlease review grid log.");
                }
                
                MessageBox.Show(msg_res, "Trade Constraints", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowMessage("ERROR", ex.ToString());
            }
            //return a continue UI event
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

