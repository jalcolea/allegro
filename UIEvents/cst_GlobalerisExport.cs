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
    public class cst_GlobalerisExportCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Globaleris Export - Before Retrieve Data
        Globaleris Export - EVE - Before Retrieve Data */
        public UiEventResult BeforeRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            _view.DataSource.Tables["cstview_globalerisexport"].SetSelectedRows(new List<DataRow>());
            
            List<DbCriteria> dbCriteria = new List<DbCriteria>();
            
            if (e.Criteria.DateColumn == "creationdate" || e.Criteria.DateColumn == "revisiondate")
            {
                dbCriteria.Add(new DbCriteria("AND", string.Empty, "cst_globalerisexportstatus", e.Criteria.DateColumn, ">=", e.Criteria.BegTime.ToString("s"), string.Empty));
                dbCriteria.Add(new DbCriteria("AND", string.Empty, "cst_globalerisexportstatus", e.Criteria.DateColumn, "<=", e.Criteria.EndTime.ToString("s"), string.Empty));
            }
            
            string positionsShownParameter = _view.Name.ToUpper().Contains("DEMAND")
                ? Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisDemandExport", "PositionsShown" })
                : Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisExport", "PositionsShown" });
            
            if (!string.IsNullOrEmpty(positionsShownParameter))
            {
                dbCriteria.Add(new DbCriteria("AND", string.Empty, "cstview_globalerisexport", "position", "in", positionsShownParameter, string.Empty));
            }
            
            e.Criteria.AddDbCriteria(dbCriteria.ToArray());
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Globaleris Export - Init View
        Globaleris Export - EVE - Init View */
        public UiEventResult InitView_1()
        {
            if (_view.DataSource.Tables["cstview_globalerisexport"].PrimaryKey.Length == 0)
            {
                _view.DataSource.Tables["cstview_globalerisexport"].PrimaryKey = new DataColumn[2] { _view.DataSource.Tables["cstview_globalerisexport"].Columns["trade"], _view.DataSource.Tables["cstview_globalerisexport"].Columns["position"] };
            }
            
            if (_view.DataSource.Tables["cstview_globalerisexport"].ChildRelations["relationtradeposition"] == null)
            {
                _view.DataSource.Tables["cstview_globalerisexport"].ChildRelations.Add(new DataRelation("relationtradeposition", new[] { _view.DataSource.Tables["cstview_globalerisexport"].Columns["trade"], _view.DataSource.Tables["cstview_globalerisexport"].Columns["position"] }, new[] { _view.DataSource.Tables["cst_globalerisexportstatus"].Columns["trade"], _view.DataSource.Tables["cst_globalerisexportstatus"].Columns["position"] }));
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Globaleris Export - Delete Operations In Globaleris
        Globaleris Export - EVE - Delete Operations In Globaleris */
        public UiEventResult ToolClick_Delete_operations_in_Globaleris_After_1()
        {
            List<DataRow> listSelectedRows = _view.DataSource.Tables["cstview_globalerisexport"].GetSelectedRows();
            
            if (listSelectedRows.Count > 0)
            {
                DataTable dtSelectedRows = _view.DataSource.Tables["cstview_globalerisexport"].Clone();
                
                foreach (DataRow drSelectedRow in listSelectedRows)
                {
                    DataRow drNew = dtSelectedRows.NewRow();
                    drNew.ItemArray = drSelectedRow.ItemArray;
                    dtSelectedRows.Rows.Add(drNew);
                }
                
                dtSelectedRows.AcceptChanges();
                
                string allowedOffsetParameter = _view.Name.ToUpper().Contains("DEMAND")
                    ? Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisDemandExport", "AllowedOffsetMonths" })
                    : Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisExport", "AllowedOffsetDays" });
                
                int allowedOffset = string.IsNullOrEmpty(allowedOffsetParameter) ? 0 : Convert.ToInt32(allowedOffsetParameter);
                
                DateTime checkDate = _view.Name.ToUpper().Contains("DEMAND")
                    ? DateTime.Now.Date.AddMonths(allowedOffset).GetFirstDayOfMonth()
                    : DateTime.Now.Date.AddDays(allowedOffset);
                
                if (dtSelectedRows.AsEnumerable().Any(dr => dr.Field<DateTime>("begtime") < checkDate))
                {
                    MessageBox.Show(string.Format("It is not possible to start the process because some of the selected operations have the begtime lower than the date '{0}'.", checkDate.ToString("dd/MM/yyyy")), "Delete operations in Globaleris", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _view.DataSource.Tables["cstview_globalerisexport"].Merge(Soap.Invoke<DataTable>("cst_GlobalerisExportWS.asmx", "DeleteOperationsInGlobaleris", new string[] { "operations" }, new object[] { dtSelectedRows }));
                }
            }
            else
            {
                MessageBox.Show("To start the process it is necessary to select the operations that you want to delete in Globaleris.", "Delete operations in Globaleris", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Globaleris Export - Export Operations To Globaleris
        Globaleris Export - EVE - Export Operations To Globaleris */
        public UiEventResult ToolClick_Export_operations_to_Globaleris_After_1()
        {
            /******************************************************************************
            NAME:  Globaleris Sent Status
            
            PURPOSE/DESCRIPTION/NOTES:    Globaleris Sent Status
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        08/08/2019  (SSA)DZL         Globaleris - Add SENT status for each trades that will be sent to Globaleris
            2.0        01/06/2021  (SSA)DZL         Globaleris - Avoid send positions with Globlalerisstatus=OK
            
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            List<DataRow> listSelectedRows = _view.DataSource.Tables["cstview_globalerisexport"].GetSelectedRows();
            
            if (listSelectedRows.Count > 0)
            {
                DataTable dtSelectedRows = _view.DataSource.Tables["cstview_globalerisexport"].Clone();
                
                //08/08/2019  DZL: Obtain the sent positions.
                
                string trades = String.Join(",", listSelectedRows.AsEnumerable().Select(x => x.Field<string>("trade")).ToArray());
                string sqltrade = "select trade,position,cst_globalerisstatus from position where trade in (" + trades + ")";
                DataTable dtPositions = Soap.Invoke<DataTable>("ExtensionsWS.asmx", "ExecuteRetrieveDataTable", new string[] { "sql" }, new object[] { sqltrade });
                
                foreach (DataRow drSelectedRow in listSelectedRows.Where(x => x.Field<string>("cst_globalerisstatus") != "SENT"
                //01/06/2021  DZL: Positions where cst_globalerisstatus=OK won't be sent.
                && x.Field<string>("cst_globalerisstatus") != "OK"))
                {
                    //08/08/2019  DZL: Positions where cst_globalerisstatus=SENT won't be sent.
                    //01/06/2021  DZL: Positions where cst_globalerisstatus=OK won't be sent.
                    if (!dtPositions.AsEnumerable().Any(x => x.Field<string>("trade") == drSelectedRow.Field<string>("trade") &&
                    (x.Field<string>("cst_globalerisstatus") == "SENT" )))
                    {
                        
                        DataRow drNew = dtSelectedRows.NewRow();
                        drNew.ItemArray = drSelectedRow.ItemArray;
                        dtSelectedRows.Rows.Add(drNew);
                    }
                }
                dtSelectedRows.AcceptChanges();
                
                string allowedOffsetParameter = _view.Name.ToUpper().Contains("DEMAND")
                    ? Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisDemandExport", "AllowedOffsetMonths" })
                    : Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "GlobalerisExport", "AllowedOffsetDays" });
                
                int allowedOffset = string.IsNullOrEmpty(allowedOffsetParameter) ? 0 : Convert.ToInt32(allowedOffsetParameter);
                
                DateTime checkDate = _view.Name.ToUpper().Contains("DEMAND")
                    ? DateTime.Now.Date.AddMonths(allowedOffset).GetFirstDayOfMonth()
                    : DateTime.Now.Date.AddDays(allowedOffset);
                
                
                if (dtSelectedRows.Rows.Count > 0)
                {
                    if (dtSelectedRows.AsEnumerable().Any(dr => dr.Field<DateTime>("begtime") < checkDate))
                    {
                        MessageBox.Show(string.Format("It is not possible to start the process because some of the selected operations have the begtime lower than the date '{0}'.", checkDate.ToString("dd/MM/yyyy")), "Export operations to Globaleris", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        _view.DataSource.Tables["cstview_globalerisexport"].Merge(Soap.Invoke<DataTable>("cst_GlobalerisExportWS.asmx", "ExportOperationsToGlobaleris", new string[] { "operations" }, new object[] { dtSelectedRows }));
                    }
                }
            }
            else
            {
                MessageBox.Show("To start the process it is necessary to select the operations that you want to export to Globaleris.", "Export operations to Globaleris", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

