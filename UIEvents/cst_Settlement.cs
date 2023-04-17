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
    public class cst_SettlementCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Settlement - After Retrieve Data
        Settlement - EVE - After Retrieve Data */
        public UiEventResult AfterRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            /******************************************************************************
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.1        21/09/2020  SAI              See Country
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            foreach (DataRow row in _view.DataSource.Tables["findetail"].AsEnumerable().Where(dr => !string.IsNullOrEmpty(dr.Field<string>("validation")) && dr.Field<string>("validation").Contains("MtM Accrual must be used to accrue financial positions;")))
            {
                row["validation"] = row.Field<string>("validation").Replace("MtM Accrual must be used to accrue financial positions;", string.Empty);
            }
            
            //SAI 21/09/2020 Add Correct Country
            if (_view.ViewName.Equals("Settlement Validation Global Services"))
            {
                DataSet ds = new DataSet();
                
                SqlHelper.RetrieveData(ds, new[] { "cstview_balanceReport" }, new[] { "SELECT DISTINCT location, country FROM location" });
                
                foreach (DataRow row in _view.DataSource.Tables["findetail"].AsEnumerable())
                {
                    if (!string.IsNullOrEmpty(row["location"].ToString()))
                    {
                        //DZL 12/05/2021 Fix the problem with country = null
                        DataRow drLocation = ds.Tables[0].AsEnumerable().Where(x =>
                        x.Field<string>("location") == row["location"].ToString()).FirstOrDefault();
                        if (drLocation != null)
                        {
                            row["country"] = drLocation.Field<string>("country");
                        }
                    }
                }
            }
            
            
            _view.DataSource.AcceptChanges();
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Disable Due date - Inactive Due date in Settlement Validation
        Disable Due date - EVE - Inactive Due date columns in Settlement Validation Global Services view if row confirmed */
        public UiEventResult BeforeCellActivate_findetail_cst_duedate_1(object sender, CancelableCellEventArgs e)
        {
            if (e.Cell is UltraGridFilterCell)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            else
            {
                if (_view.ActiveGrid.GetSelectedRows() == null || _view.ActiveGrid.GetSelectedRows().Count == 0)
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
                UltraGridRow currentRow = _view.ActiveGrid.GetSelectedRows()[0];
                if (_view.ActiveGrid.GetBindRow(currentRow) == null)
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
                if (((bool)currentRow.Cells["confirmstatus"].Value) == true)
                {
                    e.Cell.Activation = Activation.NoEdit;
                    return new UiEventResult(EventStatus.Cancel);
                }
                else
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
            }
        }
        /* Disable Inv. num.  - Inactive Inv. num. in Settlement Validation
        Disable Invoice number - EVE - Inactive Invoice number columns in Settlement Validation Global Services view if row confirmed */
        public UiEventResult BeforeCellActivate_findetail_cst_invoicenumber_1(object sender, CancelableCellEventArgs e)
        {
            if (e.Cell is UltraGridFilterCell)
            {
                return new UiEventResult(EventStatus.Continue);
            }
            else
            {
                if (_view.ActiveGrid.GetSelectedRows() == null || _view.ActiveGrid.GetSelectedRows().Count == 0)
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
                UltraGridRow currentRow = _view.ActiveGrid.GetSelectedRows()[0];
                if (_view.ActiveGrid.GetBindRow(currentRow) == null)
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
                if (((bool)currentRow.Cells["confirmstatus"].Value) == true)
                {
                    e.Cell.Activation = Activation.NoEdit;
                    return new UiEventResult(EventStatus.Cancel);
                }
                else
                {
                    e.Cell.Activation = Activation.AllowEdit;
                    return new UiEventResult(EventStatus.Continue);
                }
            }
        }
        /* Settlement_Global_Confirm_Status
        Validate Confirm Status in Settlement */
        public UiEventResult BeforeCellUpdate_findetail_confirmstatus_1(object sender, BeforeCellUpdateEventArgs e)
        {
            try
            {
                
                if (_view.ViewGrids["findetail"].ActiveRow.Cells["fintransact"].GetValue<string>() != null)
                {
                    if (_view.ViewGrids["findetail"].ActiveRow.Cells["confirmstatus"].GetValue<Boolean>())
                    {
                        e.Cancel = true;
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
                
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Error Confirm Status Update. Please contact with SSA Trading Allegro SGP." + ex.Message, "Confirm Status Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Settlement - Before Update Data
        Settlement - Before Update Data */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataSet ds = new DataSet();
            String sql = SqlHelper.SqlFormat("SELECT * FROM cst_duedatebosave");
            SqlHelper.RetrieveData(ds, new string[] { "cst_duedatebosave" }, new string[] { sql });
            DataTable dtDueDateBo = ds.Tables["cst_duedatebosave"];
            
            DataRow[] newModifiedFindetailRows = _view.DataSource.Tables["findetail"].Select("", "", DataViewRowState.ModifiedCurrent);
            
            foreach (DataRow drFindetail in newModifiedFindetailRows)
            {
                if (drFindetail["cst_duedatebo", DataRowVersion.Current] != drFindetail["cst_duedatebo", DataRowVersion.Original])
                {
                    bool registroExistente = false;
                    
                    foreach (DataRow drDueDateBo in dtDueDateBo.Rows)
                    {
                        if (drFindetail["trade"].ToString() == drDueDateBo["trade"].ToString()
                            && drFindetail["position"].ToString() == drDueDateBo["position"].ToString()
                            && drFindetail["begtime"].ToString() == drDueDateBo["begtime"].ToString()
                            && drFindetail["fee"].ToString() == drDueDateBo["fee"].ToString()
                            && drFindetail["acctstatus"].ToString() == drDueDateBo["acctstatus"].ToString()
                            && drFindetail["feetype"].ToString() == drDueDateBo["feetype"].ToString()
                            && drFindetail["measure"].ToString() == drDueDateBo["measure"].ToString())
                        {
                            drDueDateBo["cst_duedatebo"] = drFindetail["cst_duedatebo", DataRowVersion.Current];
                            drDueDateBo["revisionname"] = AppManager.UserName;
                            drDueDateBo["revisiondate"] = DateTime.Now;
                            
                            registroExistente = true;
                            break;
                        }
                    }
                    
                    if (!registroExistente)
                    {
                        DataRow dr = dtDueDateBo.NewRow();
                        
                        dr["trade"] = drFindetail["trade"];
                        dr["position"] = drFindetail["position"];
                        dr["begtime"] = drFindetail["begtime"];
                        dr["fee"] = drFindetail["fee"];
                        dr["acctstatus"] = drFindetail["acctstatus"];
                        dr["feetype"] = drFindetail["feetype"];
                        dr["measure"] = drFindetail["measure"];
                        dr["cst_duedatebo"] = drFindetail["cst_duedatebo", DataRowVersion.Current];
                        dr["creationname"] = AppManager.UserName;
                        dr["creationdate"] = DateTime.Now;
                        
                        dtDueDateBo.Rows.Add(dr);
                    }
                }
            }
            
            SqlHelper.UpdateData(ds);
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Settlement - Init View
        Settlement - EVE - Init View */
        public UiEventResult InitView_1()
        {
            if (_view.DataSource.Tables["findetail"].ChildRelations["relationremarks"] == null)
            {
                _view.DataSource.Tables["findetail"].ChildRelations.Add(new DataRelation("relationremarks", new DataColumn[] { _view.DataSource.Tables["findetail"].Columns["trade"], _view.DataSource.Tables["findetail"].Columns["position"] }, new DataColumn[] { _view.DataSource.Tables["cstview_invoicetraderemarks"].Columns["trade"], _view.DataSource.Tables["cstview_invoicetraderemarks"].Columns["position"] }));
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

