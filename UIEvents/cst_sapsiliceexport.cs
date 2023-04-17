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
    public class cst_sapsiliceexportCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* SAP Silice Export - Init View
        SAP Silice Export - DEV - Init View */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* SAP Silice Export - Send to SAP button
        SAP Silice Export - DEV - Send to SAP button */
        public UiEventResult ToolClick_Send_to_SAP_Before_1()
        {
            List<UltraGridRow> selection = _view.ActiveGrid.GetSelectedRows();
            string sendtosaplist = "";
            // List<KeyValuePair<String, String>> listaactuals = new List<KeyValuePair<string, string>>();
            
            foreach (UltraGridRow actualRow in selection)
            {
                // KeyValuePair<String, String> keys = new KeyValuePair<string, string>(actualRow.Cells["measure"].Value.ToString(), actualRow.Cells["movimiento"].Value.ToString());
                //listaactuals.Add(keys);
                //MessageBox.Show(listaactuals[0].Value);
                sendtosaplist += actualRow.Cells["measure"].Value + "," + actualRow.Cells["movimiento"].Value + ",";
            }
            
            sendtosaplist = sendtosaplist.TrimEnd(',');
            if (!string.IsNullOrEmpty(sendtosaplist))
            {
                string[] args = new string[1] { "actualslist" };
                object[] parms = new object[1] { sendtosaplist };
                string result = Soap.Invoke<string>("cst_SAPSiliceInterfaceWS.asmx", "SendToSAP", args, parms);
                MessageBox.Show(result, "SAP Silice Export", MessageBoxButtons.OK, result.Contains("Error") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                
                
            }
            else
            {
                MessageBox.Show("No rows were selected.", "SAP Silice Export", MessageBoxButtons.OK,  MessageBoxIcon.Warning );
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

