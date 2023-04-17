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
    public class ProcessManagerCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Process Manager add archive button
        Process Manager add archive button */
        public UiEventResult InitView_911()
        {
            ToolbarHelper.AddButton(_view, "Archive Selected", false, "Import.ico");
            return new UiEventResult(EventStatus.Continue);
        }
        /* Process Manager archive toolclick
        Process Manager archive toolclick */
        public UiEventResult ToolClick_Archive_Selected_Before_911()
        {
            var rows = _view.ViewGrids["valuation"].Selected.Rows;
            if (rows == null || rows.Count == 0)
            {
                ShowMessage("Selection", "Select one or more rows in the Valuation pane to archive.");
                return new UiEventResult(EventStatus.Cancel);
            }
            var items = new string[rows.Count];
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = rows[i].Cells["valuation"].Value as string;
            }
            ShowMessage("Archive", Soap.Invoke<string>("DbArchiveHelperWS.asmx", "QueueValuationArchive", new[] { "items" }, new object[] { items }));
            return new UiEventResult(EventStatus.Cancel);
        }
        /* Process Manager Calculate Valuation ToolClick
        Process Manager Calculate Valuation ToolClick */
        public UiEventResult ToolClick_Calculate_Valuation_Before_911()
        {
            // INICIO Peticion RITM1178359 - ALERTAS - Pop-up Settlement
            List<string> valmodes_list = new List<string>();
            string msg_date = "You are trying to execute a Settlement Valuation for a day different than Today. This will revaluate positions using OLD prices and deleting manual input of users in findetail.\n\n";
            string msg_criteria = "You are trying to execute a Settlement Valuation without any filtering criteria. This will revaluate the whole portfolio.\n\n";
            string msg_caption = "Settlement validation process";
            string msg_question = "Do you want to continue?";
            
            bool settlement_selected = false;
            string errormsg = "";
            
            DataRowCollection parameter_rows = _view.DtParameter.Rows;
            SelectedRowsCollection valmodes_rows = _view.ViewGrids["valuationmode"].Selected.Rows;
            SelectCriteria criteria = _view.GetSelectCriteria(true, true);
            DateTime valuationtime = Convert.ToDateTime(parameter_rows[0]["valuationtime"]);
            
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new[] { "settlementparameter" }, new[] { "SELECT DISTINCT settlementmode FROM settlementparameter" });
            
            if ((valmodes_rows != null && valmodes_rows.Count > 0) && (ds.Tables.Count >0 && ds.Tables[0].Rows.Count>0))
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++) valmodes_list.Add(ds.Tables[0].Rows[i].Field<string>("settlementmode"));
                
                
                for (int i = 0; i < valmodes_rows.Count; i++)
                {
                    if (valmodes_list.Contains(valmodes_rows[i].Cells["valuationmode"].Value.ToString()))
                    {
                        settlement_selected = true;
                        break;
                    }
                }
            }
            if (settlement_selected)
            {
                if (DateTime.Compare(DateTime.Now.Date, valuationtime.Date)!=0) errormsg = msg_date;
                if (criteria.DbCriteria.Length <= 0) errormsg += msg_criteria;
                if (errormsg.Length > 0)
                {
                    System.Windows.Forms.DialogResult res = MessageBox.Show(errormsg + msg_question, msg_caption,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (res == DialogResult.No) return new UiEventResult(EventStatus.Cancel);
                }
            }
            // FIN Peticion RITM1178359 - ALERTAS - Pop-up Settlement
            
            var rows = _view.ViewGrids["valuation"].Selected.Rows;
            if(rows == null || rows.Count == 0)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            var isValuationArchived = false;
            foreach(var row in rows)
            {
                if(row.Cells["Archive"].Value.Equals("Y"))
                {
                    isValuationArchived = true;
                    ShowMessage("Calculate Valuation", "Valuation '" + row.Cells["Valuation"].Value + "' has been archived. Please retrieve from archive before rerunning the valuation.");
                    break;
                }
            }
            if (isValuationArchived)
            {
                return new UiEventResult(EventStatus.Cancel);
            }
            else
            {
                return new UiEventResult(EventStatus.Continue);
            }
        }
        
        
    }
}

