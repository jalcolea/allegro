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
using System.Linq;

namespace Allegro.ClassEvents
{
    public class CreditpartyCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        decimal prev_collateral = 0;
        //List<string> modifications = new List<string>();
        #endregion
        
        /* cst_creditparty_after_update
        cst_creditparty_after_update */
        public UiEventResult AfterUpdateData_1(object sender, EventArgs e)
        {
            if (prev_collateral > 0)
            {
                DataSet ds = new DataSet();
                SqlHelper.RetrieveData(ds, new[] { "tmp" }, new[] { string.Format("SELECT surrogate from collateral where surrogate >'{0}'", prev_collateral) });
                
                if (ds.Tables.Contains("tmp") && ds.Tables["tmp"].Rows.Count > 0)
                {
                    object[] param = new object[1] { ds.Tables["tmp"].AsEnumerable().Select(x => x.Field<decimal>("surrogate").ToString()).ToArray() };
                    Soap.Invoke<string>("cst_collateral_servicesWS.asmx", "UpdateCollateralContracts",
                    new string[] { "idColaterals" }, param);
                }
                
            }
            
            //            if (modifications.Count > 0)
                //            {
                //                Soap.Invoke<string>("cst_collateral_servicesWS.asmx", "ValidateCollaterals",
                //                new string[] { "idColaterals" }, new object[] { modifications.ToArray() });
            //            }
            prev_collateral = 0;
            //            modifications.Clear();
            return new UiEventResult(EventStatus.Continue);
        }
        /* cst_collateral_drop_specifictrade
        cst_collateral_drop_specifictrade */
        public UiEventResult BeforeDropDown_cst_specifictrade_1(BaseGrid grid, UltraGridCell cell)
        {
            //Los trades mostrados son los referidos a los existentes entre la compa??a y contraparte,
            //cuyos endtime est? comprendido entre las fechas del colateral(Letter Date y Expiration Date).
            DateTime letterdate = cell.Row.Cells["letterdate"].GetValue<DateTime>();
            DateTime expirationdate = cell.Row.Cells["expirationdate"].GetValue<DateTime>();
            string sql = "select distinct t.trade from trade t join position p on t.trade = p.trade where t.status = 'ACTIVE' and t.company = '" +
            cell.Row.Cells["company"].GetValue<string>() + "' and p.counterparty = '" + cell.Row.Cells["creditparty"].GetValue<string>() + "'";
            
            if (letterdate != DateTime.MinValue) sql += " and t.endtime >= CONVERT (DATETIME,'" + letterdate.ToShortDateString() + "',103)";
            if (expirationdate != DateTime.MinValue) sql += "and t.endtime <= CONVERT (DATETIME,'" + expirationdate.ToShortDateString() + "',103)";
            
            return new UiEventResult(EventStatus.Continue,sql);
        }
        /* cst_creditparty_befor_update
        Credit Party-before update data */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataRow[] mod_rows = _view.DataSource.Tables["collateral"].Select("", "", DataViewRowState.Added | DataViewRowState.ModifiedOriginal);
            DataRow[] mod_rows_contracts = _view.DataSource.Tables["collateralcontract"].Select("", "", DataViewRowState.Added | DataViewRowState.ModifiedOriginal);
            
            
            //            modifications.AddRange(mod_rows.Select(x =>x.Field<int>("surrogate").ToString()));
            //            modifications.AddRange(mod_rows_contracts.Select(x =>x.Field<int>("idcollateral").ToString()));
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new[] { "tmp" }, new[] { "SELECT max (surrogate) as surrogate from collateral" });
            
            if (ds.Tables.Contains("tmp") && ds.Tables["tmp"].Rows.Count > 0)
                prev_collateral = ds.Tables["tmp"].Rows[0].Field <decimal>("surrogate");
            
            foreach (DataRow row in mod_rows)
            {
                if (!String.IsNullOrEmpty(row.Field<string>("cst_specifictrade")))
                {
                    row.SetField<bool>("cst_specific", true);
                    row.SetField<decimal>("valuationpct", 1);
                }
                
                //else row.SetField<bool>("cst_specific", false);
            }
            
            foreach (UltraGridRow gridrow in _view.ViewGrids["collateral"].Rows)
            {
                string validation = "";
                string id = gridrow.Cells["surrogate"].Value.ToString();
                if (String.IsNullOrEmpty(gridrow.Cells["guarantor"].Value.ToString()))
                    validation += "Guarantor Code not set properly, ";
                DataRow [] tmp = _view.DataSource.Tables["collateralcontract"].Select("idcollateral= " + id + " and apply=1");
                if (tmp.Count()==0) validation += "Apply contracts not set properly";
                gridrow.Cells["validation"].Value = validation;
                
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Credit Party - Init View
        Credit Party - EVE - Init View */
        public UiEventResult InitView_1()
        {
            if (_view.DataSource.Tables["creditparty"].ChildRelations["relation"] == null)
            {
                _view.DataSource.Tables["creditparty"].ChildRelations.Add(
                new DataRelation("relation",
                _view.DataSource.Tables["creditparty"].Columns["creditparty"],
                _view.DataSource.Tables["counterparty"].Columns["counterparty"]));
            }
            
            try
            {
                if (_view.DataSource.Tables["collateral"].ChildRelations["relation_collateral"] == null)
                {
                    _view.DataSource.Tables["collateral"].ChildRelations.Add(
                    new DataRelation("relation_collateral",
                    _view.DataSource.Tables["collateral"].Columns["surrogate"],
                    _view.DataSource.Tables["collateralContract"].Columns["idcollateral"]));
                }
            }
            catch (Exception)
            {
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

