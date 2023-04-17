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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Allegro.Core.Encryption;

namespace Allegro.ClassEvents
{
    public class PayableInvoiceCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        string listfintransact = "";
        #endregion
        
        /* PayableInvoices - ValidateFintransact
        PayableInvoices - DEV - Validate Fintransact rows */
        public UiEventResult AfterUpdateData_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(listfintransact))
            {
                listfintransact = listfintransact.Substring(0, listfintransact.Length - 1);
                
                DataSet ds = new DataSet();
                bool validation = false;
                SqlHelper.RetrieveData(ds, new[] { "cst_invoicesintegrity" }, new[] { "SELECT * FROM cst_invoicesintegrity where process = 'Validation Invoice' and processtype in ('MANUAL','BOTH') and transactiontype = 'AP' order by ordervalidation " });
                DataRow[] ModifiedRows = _view.DataSource.Tables["fintransact"].Select(string.Format("fintransact in ({0})",listfintransact));
                
                foreach (DataRow drFintransact in ModifiedRows)
                {
                    
                    
                    validation = true;
                    foreach (DataRow validationrows in ds.Tables["cst_invoicesintegrity"].Rows)
                    {
                        DataSet dsvalidation = new DataSet();
                        
                        string sql = string.Format(validationrows["condition"].ToString(), drFintransact["fintransact"].ToString());
                        
                        SqlHelper.RetrieveData(dsvalidation, new[] { "findetail" }, new[] { sql });
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
                                            
                                            drFintransact["cst_invoicevalidation"] = "Field " + dsvalidation.Tables[0].Columns[i].ToString() + " is empty";
                                        }
                                    }
                                }
                                else
                                {
                                    if (Convert.ToBoolean(validationrows["informativepopup"]))
                                        MessageBox.Show(validationrows["message"].ToString(), "Validation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    drFintransact["cst_invoicevalidation"] = validationrows["message"].ToString();
                                }
                                validation = false;
                                break;
                            }
                            else
                            {
                                System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show(validationrows["message"].ToString(), "Validation Invoices Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
                                
                                if (response == DialogResult.Yes)
                                {
                                    validation = true;
                                }
                                else
                                {
                                    validation = false;
                                    drFintransact["cst_invoicevalidation"] = validationrows["message"].ToString();
                                    
                                    break;
                                }
                            }
                        }
                    }
                    if (validation)
                    {
                        drFintransact["cst_invoicevalidation"] = String.Empty;
                        
                    }
                    
                    
                    
                }
                _view.UpdateData();
                listfintransact = "";
            }
            
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* PayableInvoice GUID Open Document
        PayableInvoice GUID Open Document */
        public UiEventResult BeforeCellActivate_cst_guid_1(object sender, CancelableCellEventArgs e)
        {
            string documentum_user = "";
            string documentum_pwd = "";
            string documentum_first_call = "";
            string documentum_second_call = "";
            
            string guid = e.Cell.Text.ToString();
            if (string.IsNullOrEmpty(guid)) return new UiEventResult(EventStatus.Continue);
            
            string sql = @"select * from interfaceuserid where interfacename like 'DocumentumInterface%'";
            DataTable config = Soap.Invoke<DataTable>("ExtensionsWS.asmx", "ExecuteRetrieveDataTable", new string[] { "sql" }, new object[] { sql });
            if (config.Rows.Count > 0)
            {
                DataRow firstcall = config.Select("interfacename=\'DocumentumInterfaceParentId'").FirstOrDefault();
                DataRow secondcall = config.Select("interfacename=\'DocumentumInterfaceDocumentLink'").FirstOrDefault();
                
                if (firstcall != null)
                {
                    documentum_user = firstcall.Field<string>("login");
                    documentum_pwd = new Allegro.Core.Encryption.Crypt().RC2Decrypt(firstcall.Field<string>("password"));
                    documentum_first_call = firstcall.Field<string>("url");
                }
                
                if (secondcall != null)
                {
                    documentum_second_call = secondcall.Field<string>("url");
                }
            }
            
            if (string.IsNullOrEmpty(documentum_user) || string.IsNullOrEmpty(documentum_pwd) || string.IsNullOrEmpty(documentum_first_call) || string.IsNullOrEmpty(documentum_second_call))
            {
                MessageBox.Show("Unable to open the requested file, system error");
            }
            else
            {
                try
                {
                    System.Net.WebClient client = new System.Net.WebClient { Credentials = new System.Net.NetworkCredential(documentum_user, documentum_pwd) };
                    string response = client.DownloadString(documentum_first_call.Replace("<guid>", guid));
                    JObject json = JObject.Parse(response);
                    string parent_id = json["entries"][0]["content"]["properties"]["parent_id"].ToString();
                    response = client.DownloadString(documentum_second_call.Replace("<parentid>", parent_id));
                    json = JObject.Parse(response);
                    string link = json["links"][1]["href"].ToString();
                    System.Diagnostics.Process.Start(link);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to open the requested file, document does not exists");
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* cst_counterpartynif_drop
        cst_counterpartynif_drop */
        public UiEventResult BeforeDropDown_cst_nif_1(BaseGrid grid, UltraGridCell cell)
        {
            string cp = cell.Row.Cells["counterparty"].GetValue<string>();
            
            string sql = "select nif from cst_counterpartynif where counterparty='" + cp + "'";
            
            return new UiEventResult(EventStatus.Continue, sql);
        }
        /* cst_findetail_activate
        cst_findetail_activate */
        public UiEventResult BeforeRowActivate_findetail_1(object sender, RowEventArgs e)
        {
            string status = e.Row.Cells["finstatus"].Value.ToString();
            string[] fields_findetail = { "cst_concept" };
            
            
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
        /* cst_fintransact_activate
        cst_fintransact_activate */
        public UiEventResult BeforeRowActivate_fintransact_1(object sender, RowEventArgs e)
        {
            string status = e.Row.Cells["finstatus"].Value.ToString();
            string[] fields_fintransact = { "cst_companynif", "cst_nif", "invoicedate", "duedate", "cst_operationdate" };
            
            
            
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
        /* Validate Payable Invoices
        Validate Payable Invoices */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataRow[] ApprovalsRows = _view.DataSource.Tables["fintransact_approval"].Select("", "", DataViewRowState.ModifiedCurrent);
            if (ApprovalsRows.Length > 0)
            {
                foreach (DataRow approvalRow in ApprovalsRows)
                {
                    DataSet dsfintransact = new DataSet();
                    SqlHelper.RetrieveData(dsfintransact, new[] { "fintransact" }, new[] { "SELECT * FROM fintransact where collaboration = '" + approvalRow["collaboration"].ToString() + "'" });
                    
                    if (dsfintransact.Tables[0].Rows.Count > 0)
                    {
                        if (!String.IsNullOrEmpty(dsfintransact.Tables[0].Rows[0]["cst_invoicevalidation"].ToString()))
                        {
                            MessageBox.Show("It is not possible to approve the invoice because it has errors. Check the validation field.", "Approval", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return new UiEventResult(EventStatus.Cancel);
                        }
                    }
                }
            }
            else
            {
                DataSet ds = new DataSet();
                SqlHelper.RetrieveData(ds, new[] { "cst_invoicesintegrity" }, new[] { "SELECT * FROM cst_invoicesintegrity where process = 'Validation Invoice' and processtype in ('MANUAL','BOTH') and transactiontype = 'AP' order by ordervalidation " });
                
                
                DataRow[] ModifiedRows = _view.DataSource.Tables["fintransact"].Select("", "", DataViewRowState.ModifiedCurrent);
                
                foreach (DataRow drFindetail in ModifiedRows)
                {
                    listfintransact +=  drFindetail.Field<string>("fintransact") + ",";
                    
                }
            }
            
            
            DataRow[] DeletedRows = _view.DataSource.Tables["fintransact"].Select("", "", DataViewRowState.Deleted);
            string sql = "update cst_sapinvoiceconciliate set allegroinvoice='', concilationstatus='NOT CONCILIATED', findetails='',invoicediff=0 where allegroinvoice='{0}'";
            string sqlfindetails = "update findetail set cst_conciliatedvendorinv=null  where cst_conciliatedvendorinv='{0}'";
            
            foreach (DataRow row in DeletedRows)
            {
                string findetailssentence = string.Format(sqlfindetails, row["cst_invoicereference", DataRowVersion.Original].ToString());
                Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { findetailssentence });
                
                string sentence = string.Format(sql, row["fintransact", DataRowVersion.Original].ToString());
                Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sentence });
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* SendPayableInvoicesToSap
        SendPayableInvoicesToSap */
        public UiEventResult ToolClick_Send_to_SAP_Before_1()
        {
            try
            {
                List<UltraGridRow> fintransactselection = _view.ViewGrids["fintransact"].GetSelectedRows();
                
                string sendtosaplist = "";
                bool invoicessent = false;
                DataSet fintransactDS = new DataSet();
                string notinvoicessent = "";
                foreach (UltraGridRow actualRow in fintransactselection)
                {
                    string sql = string.Format("select top 1 cst_sapstatus from fintransact where fintransact='{0}'", actualRow.Cells["fintransact"].Value.ToString());
                    string sqlapproval = string.Format("select COUNT(*) as capproval from approval where collaboration='{0}' and approval=1", actualRow.Cells["collaboration"].Value.ToString());
                    
                    string sapstatus = Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sql });
                    int approvals = Soap.Invoke<int>("ExtensionsWS.asmx", "ExecuteScalar", new string[] { "sql" }, new object[] { sqlapproval });
                    
                    
                    
                    if (actualRow.Cells["finstatus"].Value.ToString() == "APPROVED FOR PAYMENT"
                    && (sapstatus != "SENT TO SAP" && sapstatus != "ACCOUNTED") && approvals==2)
                    {
                        
                        sql = string.Format("update fintransact  set cst_sapstatus='SENT TO SAP' where  fintransact='{0}' ", actualRow.Cells["fintransact"].Value.ToString());
                        Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sql });
                        
                        
                        string[] args = new string[2] { "invoice", "type" };
                        
                        object[] parms = new object[2] { actualRow.Cells["fintransact"].Value, "AP" };
                        string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendToSap", args, parms);
                        invoicessent = true;
                    }
                    else
                    {
                        notinvoicessent += actualRow.Cells["fintransact"].Value.ToString() + ",";
                    }
                    
                }
                
                if (invoicessent)
                {
                    MessageBox.Show("Invoices were sent to SAP succesfully. " + (string.IsNullOrEmpty(notinvoicessent) ? "" : notinvoicessent + " invoices weren't sent to SAP because finstatus isn't APPROVED FOR PAYMENT and/or all approvals levels weren't checked."), "Send To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("0 Invoices were sent to SAP. Please select a invoice with Finstatus='APPROVED FOR PAYMENT' and SAP Status = PENDING or ERROR and/or all approvals levels weren't checked.", "Send To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invoices weren't sent to SAP. Please contact with SSA Trading Allegro SGP.", "Send To SAP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

