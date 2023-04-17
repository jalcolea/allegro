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
    public class PhysicalLogisticsCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Actual_AfterCellUpdate_Multi_1
        Update Fields
        Targets: actual/cst_albaran,actual/cst_mrn,actual/net,actual/unit,actual/heatvalue,actual/hvunit,actual/cst_densitykg */
        public UiEventResult AfterCellUpdate_multi_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string measuretype = String.Empty;
            string measuretypeUpdate = String.Empty;
            string unitseq = String.Empty;
            string shipment = String.Empty;
            string carrier = String.Empty;
            string viewname = _view.ViewName.Replace("'", "''");
            DataSet ds = new DataSet();
            measuretype = e.Cell.Row.Cells["measuretype"].Value.ToString();
            unitseq = e.Cell.Row.Cells["unitseq"].Value.ToString();
            shipment = e.Cell.Row.Cells["shipment"].Value.ToString();
            
            DataRow[] drShipment = _view.DataSource.Tables["shipment"].Select("shipment = " + shipment);
            if (drShipment.Length > 0)
            {
                carrier = drShipment[0]["carrier"].ToString();
                
            }
            string sql = "SELECT * FROM cst_deliverylegsynchronizer";
            if (measuretype == "RECEIPT")
                SqlHelper.RetrieveData(ds, new[] { "cst_deliverylegsynchronizer" }, new[] { "SELECT * FROM cst_deliverylegsynchronizer WHERE cst_viewname = '" + viewname + "' AND  cst_carrier = '" + carrier + "'" });
            else
            SqlHelper.RetrieveData(ds, new[] { "cst_deliverylegsynchronizer" }, new[] { "SELECT * FROM cst_deliverylegsynchronizer WHERE cst_viewname = '" + viewname + "' AND  cst_carrier = '" + carrier + "' AND cst_mode = 'Bilateral'" });
            
            if (measuretype == "RECEIPT")
                measuretypeUpdate = "DELIVERY";
            else
            measuretypeUpdate = "RECEIPT";
            
            DataRow[] drUnit = _view.DataSource.Tables["physicalmeasure"].Select("measuretype = '" + measuretypeUpdate + "'  and unitseq = " + unitseq + "and shipment = " + shipment);
            
            foreach (DataRow rowdel in ds.Tables["cst_deliverylegsynchronizer"].Rows)
            {
                if (drUnit.Length > 0)
                {
                    if (Convert.ToBoolean(rowdel["cst_albaran"]))
                        drUnit[0]["cst_albaran"] = e.Cell.Row.Cells["cst_albaran"].Value;
                    if (Convert.ToBoolean(rowdel["cst_duamrn"]))
                        drUnit[0]["cst_mrn"] = e.Cell.Row.Cells["cst_mrn"].Value;
                    if (Convert.ToBoolean(rowdel["cst_net"]))
                        drUnit[0]["net"] = e.Cell.Row.Cells["net"].Value;
                    if (Convert.ToBoolean(rowdel["cst_unitquantity"]))
                        drUnit[0]["unit"] = e.Cell.Row.Cells["unit"].Value;
                    if (Convert.ToBoolean(rowdel["cst_heatvalue"]))
                        drUnit[0]["heatvalue"] = e.Cell.Row.Cells["heatvalue"].Value;
                    if (Convert.ToBoolean(rowdel["cst_heatvalueunit"]))
                        drUnit[0]["hvunit"] = e.Cell.Row.Cells["hvunit"].Value;
                    if (Convert.ToBoolean(rowdel["cst_densitykg"]))
                        drUnit[0]["cst_densitykg"] = e.Cell.Row.Cells["cst_densitykg"].Value;
                }
                
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* AfterRetrieveData_ActualsGrid_ActualsPane
        PhysicalLogistics - AfterRetrieveData_ActualsGrid_ActualsPane- DEV - This method is used to set the batchid in the actuals view. When the user has both logistics and transport4 component, the batch id for transport4 tickets are set using this class event. */
        public UiEventResult AfterRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            if (_view.DataSource.Tables.Contains("shipment"))
            {
                object[] shipments = _view.DataSource.Tables["shipment"].GetDistinctValues(
                "shipment", "", "", DataViewRowState.CurrentRows).ToArray();
                
                string sqlShipment = "SELECT estado, shipment FROM cst_shipmentinput";
                SelectCriteria criteriaShipment = new SelectCriteria();
                criteriaShipment.AddDbCriteria(new DbCriteria("AND", "('", "cst_shipmentinput", "shipment", "IN", string.Join("','", shipments), "')"));
                DataSet dsShipment = new DataSet();
                SqlHelper.RetrieveData(dsShipment, new string[] { "cst_shipmentinput" }, new string[] { sqlShipment }, criteriaShipment);
                
                DataTable dt = dsShipment.Tables["cst_shipmentinput"];
                foreach (DataRow dr in dt.Rows)
                {
                    string shipment = dr["shipment"].ToString();
                    DataRow[] drs = _view.DataSource.Tables["shipment"].Select(String.Format("shipment = {0}", shipment));
                    foreach (DataRow drView in drs)
                    {
                        drView["globalerisstate"] = dr["estado"];
                    }
                }
                _view.DataSource.Tables["shipment"].AcceptChanges();
            }
            
            if (DbModel.GetDbTable("transport4ticket") == null)
                return new UiEventResult(EventStatus.Continue);
            ViewGrid viewGrid = sender as ViewGrid;
            if (!_view.DataSource.Tables.Contains("physicalmeasure"))
                return new UiEventResult(EventStatus.Continue);
            if (!_view.DataSource.Tables["physicalmeasure"].Columns.Contains("transport4ticketnumber"))
                return new UiEventResult(EventStatus.Continue);
            string filter = "transport4ticketnumber is not null and batchid is null ";
            System.Collections.ArrayList transport4ticketnumber = _view.DataSource.Tables["physicalmeasure"].GetDistinctValues("transport4ticketnumber", filter, "", DataViewRowState.CurrentRows);
            List<string> list = new List<string>();
            for (int i = 0; i < transport4ticketnumber.Count; i++)
            {
                if (!string.IsNullOrEmpty(transport4ticketnumber[i].ToString()))
                {
                    list.Add(transport4ticketnumber[i].ToString());
                }
            }
            if (transport4ticketnumber.Count <= 0)
                return new UiEventResult(EventStatus.Continue);
            string sql = "SELECT distinct batchnumber, measure FROM transport4ticket";
            SelectCriteria criteria = new SelectCriteria();
            criteria.AddDbCriteriaWithLimit(900, new DbCriteria("AND", "(", "transport4ticket", "custodyticketnumber", "IN", string.Join(",", list), ")"));
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new string[] { "transport4ticket" }, new string[] { sql }, criteria);
            if (!ds.Tables.Contains("transport4ticket"))
                return new UiEventResult(EventStatus.Continue);
            DataView measureDV = new DataView(ds.Tables["transport4ticket"], "", "measure", DataViewRowState.CurrentRows);
            foreach (DataRow row in _view.DataSource.Tables["physicalmeasure"].Select(filter, "", DataViewRowState.CurrentRows))
            {
                DataRowView[] measureRows = measureDV.FindRows(row["measure"]);
                if (measureRows != null && measureRows.Length > 0)
                {
                    DataRowState rowState = row.RowState; row["batchid"] = measureRows[0]["batchnumber"];
                    if (rowState == DataRowState.Unchanged) row.AcceptChanges();
                }
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Transport4Ticket_ActualsView_BeforeCellActivate
        This class event locks  carriermode, trade, position, posdetail. ticket. actualdate, product, unit, location, gross, net, gravity columns in actuals view from being edited. The columns are locked  only for transport4tickets.
        Targets: ticket/carriermode,ticket/trade,ticket/unit,ticket/position,ticket/posdetail,ticket/ticket,ticket/actualdate,ticket/product,ticket/location,ticket/gross,ticket/net,ticket/gravity */
        public UiEventResult BeforeCellActivate_multi_1(object sender, CancelableCellEventArgs e)
        {
            if (DbModel.GetDbTable("transport4ticket") == null) return new UiEventResult(EventStatus.Continue);
            ViewGrid viewGrid = sender as ViewGrid;
            string key = e.Cell.Column.Key;
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            if (viewGrid.ActiveRow.Cells.IndexOf("transport4ticketnumber") >= 0
            && viewGrid.ActiveRow.Cells["transport4ticketnumber"].Value != DBNull.Value
            && viewGrid.ActiveRow.Cells["allocstatus"].Value.ToString().Equals("ALLOC"))
            {
                if (key == "carriermode" || key == "trade" || key == "position" || key == "posdetail" || key == "ticket" ||
                key == "actualdate" || key == "product" || key == "unit" || key == "location" || key == "gross" || key == "net" || key == "gravity")
                    
                e.Cell.Activation = Activation.NoEdit;
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Transport4Ticket_before_updateData_ActualView
        BeforeUpdateData: Prevents measure created by custody ticket from being allocated without trade, position, and posdetail. These measures are created when transport4 component imports tickets, yet not allocated. */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DbModel.GetDbTable("transport4ticket") == null) return new UiEventResult(EventStatus.Continue);
            if (!_view.DataSource.Tables.Contains("physicalmeasure")) return new UiEventResult(EventStatus.Continue);
            string filter = "(trade is null OR position is null OR posdetail is null) AND confirmstatus = " + Expr.Value(true);
            
            System.Collections.ArrayList measures = _view.DataSource.Tables["physicalmeasure"].GetDistinctValues("measure", filter, "", DataViewRowState.ModifiedCurrent);
            if (measures.Count <= 0) return new UiEventResult(EventStatus.Continue);
            
            string sql = "SELECT measure FROM transport4Ticket";
            SelectCriteria criteria = new SelectCriteria();
            criteria.AddDbCriteriaWithLimit(900, new DbCriteria("AND", "(", "transport4ticket", "measure", "IN", string.Join(",", (string[])measures.ToArray(typeof(string))), ")"));
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new string[] { "transport4ticket" }, new string[] { sql }, criteria);
            if (!ds.Tables.Contains("transport4ticket")) return new UiEventResult(EventStatus.Continue);
            
            System.Collections.ArrayList transport4ticketMeasures = ds.Tables["transport4ticket"].GetDistinctValues("measure");
            if (transport4ticketMeasures.Count <= 0) return new UiEventResult(EventStatus.Continue);
            
            string validation = string.Empty;
            foreach (DataRow row in _view.DataSource.Tables["physicalmeasure"].Select(filter, "", DataViewRowState.ModifiedCurrent))
            {
                if (transport4ticketMeasures.Contains(row["measure"]))
                {
                    row["confirmstatus"] = false;
                }
            }
            if (!string.IsNullOrEmpty(validation))
            {
                ErrorDialog.Show("Validation", "Custody ticket cannot be allocated without trade, position or posdetail. ");
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Repsol Shipment - Copy to SapSilice button
        Repsol Shipment - DEV - Copy to SapSilice button */
        public UiEventResult ToolClick_Copy_to_SAPSiliceExport_Before_1()
        {
            if (_view.ViewName.Contains("Repsol's Shipments"))
            {
                string actuallist = "";
                foreach (UltraGridRow actualRow in _view.ViewGrids["actual"].Rows)
                {
                    actuallist += actualRow.Cells["measure"].Value + ",";
                }
                actuallist = actuallist.TrimEnd(',');
                
                if (!string.IsNullOrEmpty(actuallist))
                {
                    string[] args = new string[1] { "actualslist" };
                    object[] parms = new object[1] { actuallist };
                    string result = Soap.Invoke<string>("cst_SAPSiliceInterfaceWS.asmx", "CopyActuals", args, parms);
                    MessageBox.Show(result, "SAP Silice Export", MessageBoxButtons.OK, result.Contains("Error") ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    
                }
                else
                {
                    MessageBox.Show("No rows were selected.", "SAP Silice Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                }
                
                
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Repsol Shipment - Default Actual Quantities
        Repsol Shipment - DEV - Default Actual Quantities */
        public UiEventResult ToolClick_Default_Actual_Quantities_After_1()
        {
            // Load default data.
            Dictionary<string, string> defaultData = new Dictionary<string, string>();
            
            string sql = "SELECT * FROM cst_shipmentinputdefaultdata";
            SelectCriteria criteria = new SelectCriteria();
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new string[] { "cst_shipmentinputdefaultdata" }, new string[] { sql }, criteria);
            
            DataTable dt = ds.Tables["cst_shipmentinputdefaultdata"];
            foreach (DataRow dr in dt.Rows)
            {
                defaultData.Add(dr["target"].ToString(), dr["allegrovalue"].ToString());
            }
            
            
            List<string> shipments = new List<string>();
            foreach (UltraGridRow actualRow in _view.ViewGrids["actual"].Rows)
            {
                string rowShipment = actualRow.Cells["shipment"].Value.ToString();
                shipments.Add(rowShipment);
            }
            
            sql = "SELECT shipment, pcs FROM cst_shipmentinput";
            criteria = new SelectCriteria();
            criteria.AddDbCriteria(new DbCriteria("AND", "('", "cst_shipmentinput", "shipment", "IN", string.Join("','", shipments), "')"));
            ds = new DataSet();
            SqlHelper.RetrieveData(ds, new string[] { "cst_shipmentinput" }, new string[] { sql }, criteria);
            
            DataTable dtGlobalerisData;
            if (!ds.Tables.Contains("cst_shipmentinput"))
            {
                dtGlobalerisData = new DataTable();
            }
            else
            {
                dtGlobalerisData = ds.Tables[0];
            }
            
            foreach (UltraGridRow actualRow in _view.ViewGrids["actual"].Rows)
            {
                actualRow.Cells["cst_accdate"].Value = actualRow.Cells["actualdate"].Value;
                DataRow[] drs = dtGlobalerisData.Select(String.Format("shipment = '{0}'", actualRow.Cells["shipment"].Value.ToString()));
                if (drs.Length > 0)
                {
                    actualRow.Cells["heatvalue"].Value = drs[0]["pcs"];
                    actualRow.Cells["hvunit"].Value = defaultData["HeatValueUnit"];
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

