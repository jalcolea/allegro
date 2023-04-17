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
    public class SettlementCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Settlement - SAP Account - BeforeUpdate
        Settlement - SAP Account - BeforeUpdate */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_view.ViewName == "Settlement Parameters")
            {
                bool nofields = false;
                bool error = false;
                DataRow[] modifiedSAPAccountRows = _view.DataSource.Tables["cst_sapaccount"].Select("", "", DataViewRowState.ModifiedCurrent);
                DataRow[] addedSAPAccountRows = _view.DataSource.Tables["cst_sapaccount"].Select("", "", DataViewRowState.Added);
                
                foreach (DataRow row in modifiedSAPAccountRows)
                {
                    nofields = true;
                    for (int i = 0; i < row.Table.Columns.Count - 1; i++)
                    {
                        if (row[i] != DBNull.Value)
                        {
                            nofields = false;
                            break;
                        }
                    }
                    if (nofields)
                        break;
                }
                foreach (DataRow row in addedSAPAccountRows)
                {
                    nofields = true;
                    for (int i = 0; i < row.Table.Columns.Count - 1; i++)
                    {
                        if (row[i] != DBNull.Value)
                        {
                            nofields = false;
                            break;
                        }
                    }
                    if (nofields)
                        break;
                }
                if (nofields)
                {
                    MessageBox.Show("No fields were informed.", "SAP Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    error = true;
                }
                else
                {
                    DataSet ds = new DataSet();
                    string sql = String.Empty;
                    foreach (DataRow SAPAccountRow in modifiedSAPAccountRows)
                    {
                        
                        
                        sql = "SELECT * FROM cst_sapaccount where id <> '" + SAPAccountRow["id"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["product"].ToString()))
                            sql += " and product = '" + SAPAccountRow["product"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["tradetype"].ToString()))
                            sql += " and tradetype = '" + SAPAccountRow["tradetype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["positiontype"].ToString()))
                            sql += " and positiontype = '" + SAPAccountRow["positiontype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["transactiontype"].ToString()))
                            sql += " and transactiontype = '" + SAPAccountRow["transactiontype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["counterparty"].ToString()))
                            sql += " and counterparty = '" + SAPAccountRow["counterparty"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["feetype"].ToString()))
                            sql += " and feetype = '" + SAPAccountRow["feetype"].ToString() + "'";
                        sql += "UNION SELECT * FROM cst_sapaccount  where id <> '" + SAPAccountRow["id"].ToString() + "' and (product = '" + SAPAccountRow["product"].ToString() + "' or product is null) and (tradetype = '" + SAPAccountRow["tradetype"].ToString() + "' or tradetype is null) and (positiontype = '" + SAPAccountRow["positiontype"].ToString() + "' or positiontype is null)" +
                        " and (transactiontype = '" + SAPAccountRow["transactiontype"].ToString() + "' or transactiontype is null) and (counterparty = '" + SAPAccountRow["counterparty"].ToString() + "' or counterparty is null) and (feetype = '" + SAPAccountRow["feetype"].ToString() + "' or feetype is null)";
                        SqlHelper.RetrieveData(ds, new[] { "cst_sapaccount" }, new[] { sql });
                        if (ds.Tables["cst_sapaccount"].Rows.Count > 0)
                        {
                            MessageBox.Show("There is a record with similar conditions.", "SAP Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            error = true;
                        }
                    }
                    foreach (DataRow SAPAccountRow in addedSAPAccountRows)
                    {
                        sql = "SELECT * FROM cst_sapaccount where creationdate is not null";
                        if (!String.IsNullOrEmpty(SAPAccountRow["product"].ToString()))
                            sql += " and product = '" + SAPAccountRow["product"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["tradetype"].ToString()))
                            sql += " and tradetype = '" + SAPAccountRow["tradetype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["positiontype"].ToString()))
                            sql += " and positiontype = '" + SAPAccountRow["positiontype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["transactiontype"].ToString()))
                            sql += " and transactiontype = '" + SAPAccountRow["transactiontype"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["counterparty"].ToString()))
                            sql += " and counterparty = '" + SAPAccountRow["counterparty"].ToString() + "'";
                        if (!String.IsNullOrEmpty(SAPAccountRow["feetype"].ToString()))
                            sql += " and feetype = '" + SAPAccountRow["feetype"].ToString() + "'";
                        sql += "UNION SELECT * FROM cst_sapaccount  where (product = '" + SAPAccountRow["product"].ToString() + "' or product is null) and (tradetype = '" + SAPAccountRow["tradetype"].ToString() + "' or tradetype is null) and (positiontype = '" + SAPAccountRow["positiontype"].ToString() + "' or positiontype is null)" +
                        " and (transactiontype = '" + SAPAccountRow["transactiontype"].ToString() + "' or transactiontype is null) and (counterparty = '" + SAPAccountRow["counterparty"].ToString() + "' or counterparty is null) and (feetype = '" + SAPAccountRow["feetype"].ToString() + "' or feetype is null)";
                        SqlHelper.RetrieveData(ds, new[] { "cst_sapaccount" }, new[] { sql });
                        if (ds.Tables["cst_sapaccount"].Rows.Count > 0)
                        {
                            MessageBox.Show("There is a record with similar conditions.", "SAP Account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            error = true;
                        }
                    }
                }
                if (error)
                    return new UiEventResult(EventStatus.Cancel);
                else
                return new UiEventResult(EventStatus.Continue);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

