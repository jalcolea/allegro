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
    public class ReceivableInvoiceCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        string listfintransact = "";
        #endregion
        
        /* ReceivableInvoices - AfterCellUpdateDataNIF
        Receivable Invoices - DEV - AfterCellUpdateDataNIF */
        public UiEventResult AfterCellUpdate_fintransact_cst_nif_1(object sender, CellEventArgs e)
        {
            if (((ViewGrid)sender).GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            listfintransact += "'" + e.Cell.Row.Cells["fintransact"].Value + "',";
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ReceivableInvoices - AfterCellUpdateInvAddress
        Receivable Invoices - DEV - AfterCellUpdateInvAddress - Update Address field after cell update */
        public UiEventResult AfterCellUpdate_fintransact_invaddress_1(object sender, CellEventArgs e)
        {
            try
            {
                string sql = string.Format("select ISNULL(address,'')  + ' ' + ISNULL(zip,'') + ' ' + ISNULL(city,'') + ' ' + ISNULL(country,'') from address where counterparty='{0}' and addresscode='{1}'", e.Cell.Row.Cells["counterparty"].Value, e.Cell.Row.Cells["invaddress"].Value);
                string address = Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
                e.Cell.Row.Cells["cst_address"].Value = address;
            }
            catch(Exception ex)
            {
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* cst_ReceivableInvoice_afterretrieve
        cst_ReceivableInvoice_afterretrieve */
        public UiEventResult AfterRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            string[] fields_fintransact = { "cst_companynif", "cst_nif", "invoicedate", "duedate", "cst_operationdate" };
            string[] fields_findetail = { "cst_concept" };
            
            String status = "";
            
            ViewGrid panemaster = _view.ViewGrids["fintransact"];
            ViewGrid panedetail = _view.ViewGrids["findetail"];
            
            //foreach (UltraGridRow row in panemaster.Rows)
                //{
                //    status = row.Cells["finstatus"].Value.ToString();
                //    Activation active = (status == "NEW" ? Activation.AllowEdit : Activation.NoEdit);
                //    foreach (string item in fields_fintransact)
                    //    {
                    //        row.Cells[item].Activation = active;
                    //        row.Cells[item].Row.Activation = active;
                    //        //          row.Cells[item].Column.CellActivation = active;
                //    }
            //}
            
            //foreach (UltraGridRow row in panedetail.Rows)
                //{
                //    status = row.Cells["finstatus"].Value.ToString();
                //    Activation active = status == "NEW" ? Activation.AllowEdit : Activation.ActivateOnly;
                //    foreach (string item in fields_findetail)
                    //    {
                    //        row.Cells[item].Activation = active;
                    //        row.Cells[item].Row.Activation = active;
                    //        row.Cells[item].Column.CellActivation = active;
                //    }
            //}
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ReceivableInvoices - AfterUpdateData
        Receivable Invoices - DEV - Launch Tax Indicator process when the NIF changes */
        public UiEventResult AfterUpdateData_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(listfintransact))
            {
                listfintransact = listfintransact.Substring(0, listfintransact.Length - 1);
                
                string[] arrayfintransact = listfintransact.Split(',');
                
                
                string sqlfintransact = "select distinct position from findetail where fintransact in (" + listfintransact + ") and position is not null";
                DataTable dtPositions = Soap.Invoke<DataTable>("ExtensionsWS.asmx", "ExecuteRetrieveDataTable", new string[] { "sql" }, new object[] { sqlfintransact });
                
                List<string> findetailPositionList = dtPositions.AsEnumerable().Select(x => x.Field<string>("position")).Distinct().ToList();
                string findetailPositionListStr = "'" + findetailPositionList.Aggregate((buffer, next) => buffer + "','" + next) + "'";
                
                string[] args = new string[1] { "positions" };
                object[] parms = new object[1] { findetailPositionListStr };
                
                
                foreach (string fintransact in arrayfintransact)
                {
                    string sql = string.Format("update findetail set cst_nif=(select top 1 cst_nif from fintransact where fintransact={0}) where  fintransact={0} ", fintransact);
                    Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sql });
                }
                
                //Launch
                string result = Soap.Invoke<string>("cst_TaxIndicatorCalculateWS.asmx", "TaxIndicatorCalculate", args, parms);
                
                
                
                listfintransact = "";
                
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Receivable Invoices - DropdownCounterpartyNIF
        Receivable Invoices - DEV - Before Dropdown Counterparty NIF */
        public UiEventResult BeforeDropDown_cst_nif_1(BaseGrid grid, UltraGridCell cell)
        {
            string sql = "select distinct nif from cst_counterpartynif where counterparty ='" + cell.Row.Cells["counterparty"].GetValue<string>() + "'";
            return new UiEventResult(EventStatus.Continue, sql);
        }
        /* ReceivableInvoices - BeforeCellActivateFindetail
        Receivable Invoices - DEV - Before Cell Activate NIF Findetail */
        public UiEventResult BeforeRowActivate_findetail_1(object sender, RowEventArgs e)
        {
            string status = e.Row.Cells["finstatus"].Value.ToString();
            string[] fields_findetail = { "cst_concept", "cst_taxindicator", "cst_taxremark" };
            
            
            foreach (UltraGridCell cell in e.Row.Cells)
            {
                MetaDataDS.viewcolumnRow columnRow = _view.MetaData.viewcolumn.FindByviewnameviewpaneviewcolumn("Receivable Invoices", "findetail", cell.Column.Key);
                if (columnRow == null) continue;
                
                Activation act = (status == "NEW" && fields_findetail.Contains(columnRow.dbcolumn) ? Activation.AllowEdit : Activation.NoEdit);
                
                cell.Activation = act;
                cell.Column.CellActivation = act;
            }
            
            
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ReceivableInvoices - BeforeCellActivateEditFields
        Receivable Invoices - DEV - Before Cell Activate NIF Fintransact */
        public UiEventResult BeforeRowActivate_fintransact_1(object sender, RowEventArgs e)
        {
            string status = e.Row.Cells["finstatus"].Value.ToString();
            string[] fields_fintransact = {  "cst_nif", "invoicedate", "duedate", "cst_originalinvoice", "cst_rectificationreason", "cst_invoicenotes", "invaddress" };
            
            
            
            foreach (UltraGridCell cell in e.Row.Cells)
            {
                MetaDataDS.viewcolumnRow columnRow = _view.MetaData.viewcolumn.FindByviewnameviewpaneviewcolumn("Receivable Invoices", "fintransact", cell.Column.Key);
                if (columnRow == null) continue;
                
                Activation act = (status == "NEW" && fields_fintransact.Contains(columnRow.dbcolumn) ? Activation.AllowEdit : Activation.NoEdit);
                
                cell.Activation = act;
                cell.Column.CellActivation = act;
            }
            
            
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Receivable_Invoices_BeforeRowsDeleted_1
        Validate invoice delete */
        public UiEventResult BeforeRowsDeleted_fintransact_1(object sender, BeforeRowsDeletedEventArgs e)
        {
            List<DataRow> fintransactRowsToDelete = new List<DataRow>();
            for (int i = 0; i < e.Rows.Length; i++) fintransactRowsToDelete.Add(((ViewGrid)sender).GetBindRow(e.Rows[i]));
            foreach (DataRow fintransactRow in fintransactRowsToDelete)
            {
                if (fintransactRow.RowState == DataRowState.Added) continue;
                
                if (fintransactRow["finstatus"].ToString()!="NEW")
                {
                    ErrorDialog.Show("Error", "Invoice sent to SAP - No deletion possible.");
                    e.Cancel = true;
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* ReceivableInvoices - Set Invoice Date Today
        Receivable Invoices - DEV - Set Invoice Date Today before Create Invoices */
        public UiEventResult ToolClick_Create_Invoices_Before_2()
        {
            DataRowCollection parameter_rows = _view.DtParameter.Rows;
            parameter_rows[0]["Invoicedate"] = DateTime.Now.Date;
            parameter_rows[0]["acctdate"] = DateTime.Now.Date;
            return new UiEventResult(EventStatus.Continue);
        }
        /* ValidateCreateInvoices
        Create Invoices Before */
        public UiEventResult ToolClick_Create_Invoices_Before_3()
        {
            try
            {
                
                
                SelectCriteria criteria = _view.GetSelectCriteria(true, true);
                bool counterpartySelected = false;
                
                if (criteria.DbCriteria.Length > 0)
                {
                    for (int i = 0; i < criteria.DbCriteria.Length; i++)
                    {
                        if (criteria.DbCriteria[i].DbColumn == "counterparty" && !String.IsNullOrEmpty(criteria.DbCriteria[i].DbValue))
                        {
                            counterpartySelected = true;
                            break;
                        }
                    }
                    //    counterpartySelected = true;
                }
                else
                {
                    counterpartySelected = true;
                }
                if (counterpartySelected)
                {
                    DataSet dsvalidation = new DataSet();
                    DataSet dsfindetail = new DataSet();
                    String sql = String.Empty;
                    SqlHelper.RetrieveData(dsfindetail, new[] { "findetail" }, new[] { "Select top(1)* from findetail" });
                    DateTime columnDate;
                    if (criteria.DbCriteria.Length > 0)
                    {
                        sql = "Select distinct counterparty,contract,paymentterms,currency from findetail where ";
                        for (int i = 0; i < criteria.DbCriteria.Length; i++)
                        {
                            if (i != 0)
                            {
                                
                                if (criteria.DbCriteria[i].DbTable != "findetail" && !String.IsNullOrEmpty(criteria.DbCriteria[i].DbTable))
                                {
                                    MessageBox.Show("The only1 table allowed in the filters is the findetail table.", "Create Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return new UiEventResult(EventStatus.Cancel);
                                }
                                
                                if (!dsfindetail.Tables[0].Columns.Contains(criteria.DbCriteria[i].DbColumn))
                                {
                                    MessageBox.Show("The only table allowed in the filters is the findetail table.", "Create Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return new UiEventResult(EventStatus.Cancel);
                                }
                            }
                            if (i != 0)
                                sql += " and ";
                            sql += criteria.DbCriteria[i].DbColumn;
                            sql += "=";
                            
                            if (DateTime.TryParse(criteria.DbCriteria[i].DbValue, out columnDate))
                            {
                                sql += "CONVERT(DATETIME, '" + criteria.DbCriteria[i].DbValue.ToString("yyyy-MM-dd") + "')";
                            }
                            else
                            {
                                sql += "'" + criteria.DbCriteria[i].DbValue + "'";
                            }
                            
                            
                        }
                        
                        
                        SqlHelper.RetrieveData(dsvalidation, new[] { "findetail" }, new[] { sql });
                        if (dsvalidation.Tables[0].Rows.Count > 1)
                        {
                            System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show("There are different combinations of COUNTERPARTY - CONTRACT - PAYMENTTERMS - CURRENCY, more than one invoice will be generated. Do you want to continue with the invoice generation process?", "Create Invoices Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
                            
                            if (response == DialogResult.Yes)
                            {
                                return new UiEventResult(EventStatus.Continue);
                            }
                            else
                            {
                                return new UiEventResult(EventStatus.Cancel);
                            }
                            
                        }
                    }
                }
                else {
                    MessageBox.Show("To create an invoice it is necessary to select a counterparty.", "Create Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return new UiEventResult(EventStatus.Cancel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Validation Create Invoices. Please contact with SSA Trading Allegro SGP.", "Create Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return new UiEventResult(EventStatus.Cancel);
            }
            return new UiEventResult(EventStatus.Continue);
            
            // return new UiEventResult(EventStatus.Cancel);
        }
        /* ValidationSendToSAPInvoices
        Send Invoices Validation */
        public UiEventResult ToolClick_Send_to_SAP_Before_1()
        {
            try
            {
                DataSet ds = new DataSet();
                SqlHelper.RetrieveData(ds, new[] { "cst_invoicesintegrity" }, new[] { "SELECT * FROM cst_invoicesintegrity where process = 'Sending Invoices to SAP' and processtype in ('MANUAL','BOTH') and transactiontype = 'AR' order by ordervalidation " });
                
                Boolean validation = true;
                
                List<DataRow> listSelectedRows = _view.DataSource.Tables["fintransact"].GetSelectedRows();
                foreach (DataRow selectrows in listSelectedRows)
                {
                    validation = true;
                    foreach (DataRow validationrows in ds.Tables["cst_invoicesintegrity"].Rows)
                    {
                        DataSet dsvalidation = new DataSet();
                        string sql = string.Format(validationrows["condition"].ToString(), selectrows["fintransact"].ToString());
                        SqlHelper.RetrieveData(dsvalidation, new[] { "fintransact" }, new[] { sql });
                        if (dsvalidation.Tables[0].Rows.Count > 0)
                        {
                            if (!Convert.ToBoolean(validationrows["userconfirmation"]))
                            {
                                if (validationrows["invoicevalidation"].ToString() == "Mandatory fields are empty")
                                {
                                    for (int i = 0; i < dsvalidation.Tables[0].Columns.Count; i++)
                                    {
                                        if (String.IsNullOrEmpty(dsvalidation.Tables[0].Rows[0][i].ToString()))
                                        {
                                            
                                            if (Convert.ToBoolean(validationrows["informativepopup"]))
                                                MessageBox.Show("Field " + dsvalidation.Tables[0].Columns[i].ToString() + " is empty", "Send Invoices To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            
                                            selectrows["cst_invoicevalidation"] = "Field " + dsvalidation.Tables[0].Columns[i].ToString() + " is empty";
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    
                                    if (Convert.ToBoolean(validationrows["informativepopup"]))
                                        MessageBox.Show(validationrows["message"].ToString(), "Send Invoices To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    selectrows["cst_invoicevalidation"] = validationrows["message"].ToString();
                                }
                                validation = false;
                                
                                selectrows["cst_sapready"] = false;
                                _view.UpdateData();
                                
                                break;
                            }
                            else
                            {
                                System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show(validationrows["message"].ToString(), "Send Invoices To SAP Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
                                
                                if (response == DialogResult.Yes)
                                {
                                    validation = true;
                                }
                                else
                                {
                                    validation = false;
                                    selectrows["cst_sapready"] = false;
                                    selectrows["cst_invoicevalidation"] = validationrows["message"].ToString();
                                    _view.UpdateData();
                                    
                                    break;
                                }
                            }
                        }
                    }
                    if (validation)
                    {
                        selectrows["cst_sapready"] = true;
                        selectrows["cst_invoicevalidation"] = String.Empty;
                        _view.UpdateData();
                    }
                    
                    
                    if (validation)
                    {
                        List<UltraGridRow> fintransactselection = _view.ViewGrids["fintransact"].GetSelectedRows();
                        //if (_view.DataSource.HasChanges())
                            //{
                            //    ShowMessage("Warning", "There are unsaved changes.\nPlease save or discard the changes before proceeding.");
                            //    return new UiEventResult(EventStatus.Cancel);
                        //}
                        bool invoicessent = false;
                        
                        DataSet fintransactDS = new DataSet();
                        foreach (UltraGridRow actualRow in fintransactselection)
                        {
                            if (String.IsNullOrEmpty(actualRow.Cells["cst_rectificationreason"].Value.ToString()))
                                if ((actualRow.Cells["cst_sapstatus"].Value.ToString() == "PENDING") || (actualRow.Cells["cst_sapstatus"].Value.ToString().Contains("ERROR")))
                            {
                                string sql = string.Format("select top 1 cst_sapstatus from fintransact where fintransact='{0}'", actualRow.Cells["fintransact"].Value.ToString());
                                string sapstatus = Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
                                
                                if (sapstatus != "SENT TO SAP")
                                {
                                    sql = string.Format("update fintransact  set cst_sapstatus='SENT TO SAP' where  fintransact='{0}' ", actualRow.Cells["fintransact"].Value.ToString());
                                    Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sql });
                                    
                                    
                                    actualRow.Cells["finstatus"].Value = "APPROVED";
                                    _view.UpdateData();
                                    string[] args = new string[2] { "invoice", "type" };
                                    // sendtosaplist += actualRow.Cells["fintransact"].Value + ",";
                                    object[] parms = new object[2] { actualRow.Cells["fintransact"].Value, "AR" };
                                    string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendToSap", args, parms);
                                    invoicessent = true;
                                }
                            }
                        }
                        foreach (UltraGridRow actualRow in fintransactselection)
                        {
                            if (!String.IsNullOrEmpty(actualRow.Cells["cst_rectificationreason"].Value.ToString()))
                                if ((actualRow.Cells["cst_sapstatus"].Value.ToString() == "PENDING") || (actualRow.Cells["cst_sapstatus"].Value.ToString().Contains("ERROR")))
                            {
                                string sql = string.Format("select top 1 cst_sapstatus from fintransact where fintransact='{0}'", actualRow.Cells["fintransact"].Value.ToString());
                                string sapstatus = Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
                                
                                if (sapstatus != "SENT TO SAP")
                                {
                                    sql = string.Format("update fintransact  set cst_sapstatus='SENT TO SAP' where  fintransact='{0}' ", actualRow.Cells["fintransact"].Value.ToString());
                                    Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sql });
                                    
                                    actualRow.Cells["finstatus"].Value = "APPROVED";
                                    _view.UpdateData();
                                    string[] args = new string[2] { "invoice", "type" };
                                    //sendtosaplist += actualRow.Cells["fintransact"].Value + ",";
                                    object[] parms = new object[2] { actualRow.Cells["fintransact"].Value, "AR" };
                                    string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendToSap", args, parms);
                                    invoicessent = true;
                                }
                            }
                        }
                        if (invoicessent)
                        {
                            MessageBox.Show("Invoices were sent to SAP succesfully.", "Send To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("0 Invoices were sent to SAP. Please select a invoice with SAP Status = PENDING or ERROR", "Send To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        }
                        return new UiEventResult(EventStatus.Continue);
                    }
                    
                    
                }
                return new UiEventResult(EventStatus.Continue);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Error Validation Create Invoices. Please contact with SSA Trading Allegro SGP." + ex.Message, "Create Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return new UiEventResult(EventStatus.Cancel);
            }
            // return new UiEventResult(EventStatus.Cancel);
        }
        /* ReceivableInvoice - Void Invoice
        ReceivableInvoice - Void Invoice - DEV - Call UpdateInvoice process after a void invoice is created. */
        public UiEventResult ToolClick_Void_Invoice_After_2()
        {
            List<string> transacts = new List<string>();
            // string transacts =
            SelectedRowsCollection selectedRows = _view.ViewGrids["fintransact"].Selected.Rows;
            foreach (UltraGridRow row in selectedRows)
            {
                string invoice = row.Cells["invoice"].Value.ToString();
                if (!string.IsNullOrEmpty(invoice))
                {
                    char last = invoice[invoice.Length - 1];
                    char next = ' ';
                    if (((int)last) <= 57) next = 'A';
                    else next = last++;
                    transacts.Add(invoice + next.ToString());
                }
            }
            if (transacts.Count>0)
            {
                bool res = Soap.Invoke<bool>("cst_updateinvoice_serviceWS.asmx", "UpdateInvoice", new string[] { "transacts" }, new object[] { transacts.ToArray()});
            }
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

