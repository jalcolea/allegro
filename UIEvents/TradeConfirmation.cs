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
    public class TradeConfirmationCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        string listpositions="";
        #endregion
        
        /* TradeConfirmation - AfterCellUpdateDataCounterpartyNIF
        TradeConfirmation - DEV - AfterCellUpdateData CounterpartyNIF */
        public UiEventResult AfterCellUpdate_tradeconfirmation_cst_counterpartynif_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            
            listpositions += "'" + e.Cell.Row.Cells["position"].Value + "',";
            
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeConfirmation_Tradestatus_AfterCellUpdate_1
        Refresh status */
        public UiEventResult AfterCellUpdate_tradeconfirmation_tradestatus_1(object sender, CellEventArgs e)
        {
            
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string status = Soap.Invoke<string>("TradeExecution/TradeExecutionWS.asmx", "RetrieveParameters", new string[] { "table", "key", "keyValue", "column" }, new object[] { "tradestatus", "tradestatus", e.Cell.Row.Cells["tradestatus"].GetValue<string>(), "status" });
            if (!string.IsNullOrEmpty(status)) e.Cell.Row.Cells["status"].Value = status;
            
            return new UiEventResult(EventStatus.Continue);
            
        }
        /* TradeConfirmation_afterretrieve
        TradeConfirmation_afterretrieve */
        public UiEventResult AfterRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            ViewGrid pane = _view.ViewGrids["tradeconfirmation"];
            for (int cont = 0; cont < pane.Rows.Count; cont++)
            {
                pane.Rows[cont].Cells["cst_signaturestatus"].Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_signaturestatus"].Row.Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_signaturestatus"].Column.CellActivation= Activation.AllowEdit;
                
                pane.Rows[cont].Cells["cst_companynif"].Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_companynif"].Row.Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_companynif"].Column.CellActivation = Activation.AllowEdit;
                
                pane.Rows[cont].Cells["cst_counterpartynif"].Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_counterpartynif"].Row.Activation = Activation.AllowEdit;
                pane.Rows[cont].Cells["cst_counterpartynif"].Column.CellActivation = Activation.AllowEdit;
                
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* TradeConfirmation - AfterUpdateData TaxIndicator
        TradeConfirmation - AfterUpdateData TaxIndicator - DEV - Launch TaxIndicator process for trades where cst_counterpartynif were changed */
        public UiEventResult AfterUpdateData_2(object sender, EventArgs e)
        {
            ViewGrid pane = _view.ViewGrids["tradeconfirmation"];
            
            
            
            if (!string.IsNullOrEmpty(listpositions))
            {
                if (listpositions.Contains(","))
                {
                    listpositions = listpositions.Substring(0, listpositions.Length - 1);
                }
                string[] args = new string[1] { "positions" };
                object[] parms = new object[1] { listpositions };
                string[] arraypositions = listpositions.Split(',');
                
                foreach (string pos in arraypositions)
                {
                    string sql = string.Format("update findetail set cst_nif=(select top 1 cst_counterpartynif from position where position={0}) where  position={0} and fee is null", pos);
                    Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
                }
                
                //Launch
                string result = Soap.Invoke<string>("cst_TaxIndicatorCalculateWS.asmx", "TaxIndicatorCalculate", args, parms);
                
                
                
                listpositions = "";
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* cst_TradeConfirmations_drop_cst_companynif
        cst_drop_cst_companynif */
        public UiEventResult BeforeDropDown_cst_companynif_1(BaseGrid grid, UltraGridCell cell)
        {
            string sql = "select distinct nif from cst_counterpartynif where counterparty ='" + cell.Row.Cells["company"].GetValue<string>() + "'";
            return new UiEventResult(EventStatus.Continue,sql);
        }
        /* cst_TradeConfirmations_drop_cst_counterpartynif
        cst_TradeConfirmations_drop_cst_counterpartynif */
        public UiEventResult BeforeDropDown_cst_counterpartynif_1(BaseGrid grid, UltraGridCell cell)
        {
            string sql = "select distinct nif from cst_counterpartynif where counterparty ='" + cell.Row.Cells["counterparty"].GetValue<string>() + "'";
            return new UiEventResult(EventStatus.Continue,sql);
        }
        /* cst_TradeConfirmation_beforeupdate
        cst_TradeConfirmation_beforeupdate */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool counterparty_nif_changed = false;
            bool company_nif_changed = false;
            ViewGrid pane = _view.ViewGrids["tradeconfirmation"];
            string sql = "update position set cst_companynif='{0}', cst_counterpartynif='{1}' where position='{2}'";
            string sqlfd = "update findetail set cst_companynif='{0}', cst_nif='{1}' where position = '{2}' and fee is null";
            string sqlfd_fee = "update findetail set cst_companynif='{0}', " +
            "cst_nif=(select cst_invoicingvatnumber from address where addresscode='MAIN' and counterparty='{1}') " +
            "where position = '{2}' and fee is not null and counterparty <> '{1}'";
            
            string sqlfd_fee_pos = "update findetail set cst_companynif='{0}', " +
            "cst_nif=(select cst_counterpartynif from position where position = '{2}') " +
            "where position = '{2}' and fee is not null and counterparty = '{1}'";
            
            //string sqlfd_fee2 = ""; si coincide la contraparte con la posicion, la de la posicion, si no el nif el main
            string sentence = "";
            string fee = "";
            string position = "";
            string companynif = "";
            string counterpartynif = "";
            string counterparty = "";
            
            for (int cont = 0; cont<pane.Rows.Count; cont++)
            {
                counterparty_nif_changed = false;
                company_nif_changed = false;
                
                
                companynif = pane.Rows[cont].Cells["cst_companynif"].Value.ToString();
                counterpartynif = pane.Rows[cont].Cells["cst_counterpartynif"].Value.ToString();
                counterparty = pane.Rows[cont].Cells["counterparty"].Value.ToString();
                
                position = pane.Rows[cont].Cells["position"].Value.ToString();
                if (pane.Rows[cont].Cells["cst_companynif"].OriginalValue.ToString()!= companynif) company_nif_changed=true;
                if (pane.Rows[cont].Cells["cst_counterpartynif"].OriginalValue.ToString() != counterpartynif) counterparty_nif_changed = true;
                
                if (counterparty_nif_changed || company_nif_changed)
                {
                    sentence = String.Format(sql, companynif, counterpartynif,position);
                    Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sentence});
                    
                    sentence = String.Format(sqlfd, companynif, counterpartynif, position);
                    Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sentence });
                    
                    sentence = String.Format(sqlfd_fee, companynif, counterparty, position);
                    Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sentence });
                    
                    sentence = String.Format(sqlfd_fee_pos, companynif, counterparty, position);
                    Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sentence });
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

