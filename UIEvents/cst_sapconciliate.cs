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
    public class cst_sapconciliateCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* SAP Conciliate GUID Open Document
        SAP Conciliate GUID Open Document. */
        public UiEventResult BeforeCellActivate_guid_1(object sender, CancelableCellEventArgs e)
        {
            string documentum_user="";
            string documentum_pwd = "";
            string documentum_first_call = "";
            string documentum_second_call = "";
            
            string guid = e.Cell.Text.ToString();
            if (string.IsNullOrEmpty(guid)) return new UiEventResult(EventStatus.Continue);
            
            string sql = @"select * from interfaceuserid where interfacename like 'DocumentumInterface%'";
            DataTable config = Soap.Invoke<DataTable>("ExtensionsWS.asmx", "ExecuteRetrieveDataTable", new string[] { "sql" }, new object[] { sql });
            if (config.Rows.Count>0)
            {
                DataRow firstcall =  config.Select("interfacename=\'DocumentumInterfaceParentId'").FirstOrDefault();
                DataRow secondcall = config.Select("interfacename=\'DocumentumInterfaceDocumentLink'").FirstOrDefault();
                
                if (firstcall != null )
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
                    response = client.DownloadString(documentum_second_call.Replace("<parentid>",parent_id));
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
        /* SAP Conciliate Invoices - Init View
        SAP Conciliate Invoices - DEV - Init View */
        public UiEventResult InitView_1()
        {
            try
            {
                if (_view.DataSource.Tables["cst_sapinvoiceconciliate"].ChildRelations["relation_findetail"] == null)
                {
                    _view.DataSource.Tables["cst_sapinvoiceconciliate"].ChildRelations.Add(
                    new DataRelation("relation_findetail",
                    _view.DataSource.Tables["cst_sapinvoiceconciliate"].Columns["ctc"],
                    _view.DataSource.Tables["cstview_sapconfindetail"].Columns["ctc"], false));
                }
            }
            catch (Exception)
            {
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* SAP Conciliate Invoices - Calculate Invoice Diff
        SAP Conciliate Invoices - DEV - Calculate Invoice Diff */
        public UiEventResult ToolClick_Calculate_Invoice_Diff_Before_1()
        {
            SelectedRowsCollection selectedRows = _view.ViewGrids["constraintdefinition"].Selected.Rows;
            
            foreach (UltraGridRow row in selectedRows)
            {
                if (row.Cells["concilationstatus"].Value == "PENDING")
                {
                    if (row.Cells["findetails"].Value != DBNull.Value)
                    {
                        string findetails = row.Cells["findetails"].Value.ToString();
                        DateTime invoicedate = DateTime.Parse(row.Cells["invoicedate"].Value.ToString());
                        
                        string[] args = new string[2] { "invoices", "invoicedate" };
                        object[] parms = new object[2] { findetails, invoicedate };
                        string result = Soap.Invoke<string>("cst_SAPConciliateInterfaceWS.asmx", "CreateManualInvoice", args, parms);
                        MessageBox.Show("Invoice was created for Vendor Invoice: " + row.Cells["findetails"].Value.ToString(), "SAP Concilation");
                    }
                }
                else
                {
                    continue;
                }
                
            }
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* SAP Conciliate Invoices - Create Manual Invoice
        SAP Conciliate Invoices - DEV - Create Manual Invoice */
        public UiEventResult ToolClick_Create_Manual_Invoice_Before_1()
        {
            #region Import Findetails
            SelectedRowsCollection SAPConcilationSelectedRows = _view.ViewGrids["SAP_Concilation"].Selected.Rows;
            
            string selectedfindetail = "";
            string selectedposition = "";
            
            
            
            foreach (UltraGridRow SAPConcilationRow in SAPConcilationSelectedRows)
            {
                if (SAPConcilationRow.Cells["concilationstatus"].Value.ToString() == "PENDING" || SAPConcilationRow.Cells["concilationstatus"].Value.ToString() == "NOT CONCILIATED")
                {
                    
                    string SAPConcilationCTC = SAPConcilationRow.Cells["ctc"].Value.ToString();
                    selectedfindetail = "";
                    selectedposition = "";
                    
                    SelectedRowsCollection FinDetailSelectedRows = _view.ViewGrids["Findetail"].Selected.Rows;
                    
                    if (FinDetailSelectedRows.Count <= 20)
                    {
                        foreach (UltraGridRow FindetailRow in FinDetailSelectedRows)
                        {
                            string findetail = FindetailRow.Cells["findetail"].Value.ToString();
                            string position = FindetailRow.Cells["position"].Value.ToString();
                            string fintransact = FindetailRow.Cells["fintransact"].Value.ToString();
                            if (string.IsNullOrEmpty(fintransact))
                            {
                                if (FindetailRow.Cells["ctc"].Value.ToString() == SAPConcilationCTC)
                                {
                                    selectedfindetail += findetail + ",";
                                    selectedposition += position + ",";
                                }
                                else
                                {
                                    MessageBox.Show(findetail + " CTC is " + FindetailRow.Cells["ctc"].Value.ToString() + ", it's must be " + SAPConcilationCTC, "Conciliation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    
                                    return new UiEventResult(EventStatus.Cancel);
                                }
                            }
                            else
                            {
                                MessageBox.Show(findetail + " was assigned to another invoice. Please select another one where fintransact field is empty.", "Conciliation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return new UiEventResult(EventStatus.Cancel);
                            }
                            
                        }
                        
                        if (!string.IsNullOrEmpty(selectedfindetail))
                        {
                            selectedfindetail = selectedfindetail.Substring(0, selectedfindetail.Length - 1);
                            SAPConcilationRow.Cells["findetails"].Value = selectedfindetail;
                            
                            selectedposition = selectedposition.Substring(0, selectedposition.Length - 1);
                            
                        }
                    }
                    else if (FinDetailSelectedRows.Count == 0)
                    {
                        MessageBox.Show("Please select at least 1 Findetail row in 'Settlement Validation Positions' pane.", "Conciliation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                    else
                    {
                        MessageBox.Show("The maximum of Findetails rows for manual concilation is 20. Please use the Automatic Concilation process.", "Conciliation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return new UiEventResult(EventStatus.Cancel);
                    }
                }
            }
            
            _view.UpdateData();
            
            #endregion
            #region Validation
            DataSet ds = new DataSet();
            SqlHelper.RetrieveData(ds, new[] { "cst_invoicesintegrity" }, new[] { "SELECT * FROM cst_invoicesintegrity where process = 'Conciliation Invoice' and processtype in ('MANUAL','BOTH') and transactiontype = 'AP' order by ordervalidation" });
            
            Boolean validation = true;
            SelectedRowsCollection selectedRowsConci = _view.ViewGrids["SAP_Concilation"].Selected.Rows;
            foreach (UltraGridRow selectrows in selectedRowsConci)
            {
                
                if (selectrows.Cells["concilationstatus"].Value.ToString() == "PENDING" || selectrows.Cells["concilationstatus"].Value.ToString() == "NOT CONCILIATED")
                {
                    validation = true;
                    foreach (DataRow validationrows in ds.Tables["cst_invoicesintegrity"].Rows)
                    {
                        DataSet dsvalidation = new DataSet();
                        
                        string sql = string.Format(validationrows["condition"].ToString(), selectrows.Cells["vendorinvoice"].Value.ToString());
                        
                        sql = sql.Replace("findetaillist", selectedfindetail);
                        
                        SqlHelper.RetrieveData(dsvalidation, new[] { "findetail" }, new[] { sql });
                        if (dsvalidation.Tables[0].Rows.Count > 0 && dsvalidation.Tables[0].Rows[0][0] != DBNull.Value)
                        {
                            
                            if (!Convert.ToBoolean(validationrows["userconfirmation"]))
                            {
                                
                                if (Convert.ToBoolean(validationrows["informativepopup"]))
                                    MessageBox.Show(validationrows["message"].ToString(), "Conciliation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                                selectrows.Cells["cst_invoicevalidation"].Value = validationrows["message"].ToString();
                                if ((validationrows["invoicevalidation"].ToString() == "Amount different and exceeded discrepancy limit") || (validationrows["invoicevalidation"].ToString() == "Amount different and exceeded discrepancy limit - Credit Notes"))
                                    selectrows.Cells["concilationstatus"].Value = "NOT CONCILIATED";
                                
                                validation = false;
                                break;
                            }
                            else
                            {
                                
                                System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show(validationrows["message"].ToString(), "Conciliation Invoices Warning", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);
                                
                                if (response == DialogResult.Yes)
                                {
                                    validation = true;
                                    if (validationrows["invoicevalidation"].ToString() == "Counterparty NIF")
                                    {
                                        string sqlupdateniffindetails = string.Format("update findetail set cst_nif='{0}' where findetail in ({1})", selectrows.Cells["cst_nifallegro"].Value.ToString(), selectedfindetail);
                                        Soap.Invoke<string>("ExtensionsWS.asmx", "ExecuteNonQuery", new string[] { "sql" }, new object[] { sqlupdateniffindetails });
                                        //Launch Tax Indicator process for the new NIF value.
                                        string[] args = new string[1] { "positions" };
                                        object[] parms = new object[1] { selectedposition };
                                        
                                        Soap.Invoke<string>("cst_TaxIndicatorCalculateWS.asmx", "TaxIndicatorCalculate", args, parms);
                                        
                                    }
                                }
                                else
                                {
                                    validation = false;
                                    
                                    selectrows.Cells["cst_invoicevalidation"].Value = validationrows["message"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                }
                if (validation)
                {
                    selectrows.Cells["cst_invoicevalidation"].Value = String.Empty;
                    // _view.RetrieveData();
                    selectrows.Update();
                }
                else
                {
                    
                    
                    selectrows.Update();
                    _view.UpdateData();
                    return new UiEventResult(EventStatus.Continue);
                }
            }
            #endregion
            #region Create Invoices
            
            SelectedRowsCollection selectedRows = _view.ViewGrids["SAP_Concilation"].Selected.Rows;
            
            foreach (UltraGridRow row in selectedRows)
            {
                if (row.Cells["concilationstatus"].Value.ToString() == "PENDING" || row.Cells["concilationstatus"].Value.ToString() == "NOT CONCILIATED")
                {
                    if (row.Cells["findetails"].Value != DBNull.Value)
                    {
                        string findetails = row.Cells["findetails"].Value.ToString();
                        string vendorinvoice = row.Cells["vendorinvoice"].Value.ToString();
                        
                        DateTime invoicedate = DateTime.Parse(row.Cells["invoicedate"].Value.ToString());
                        
                        string[] args = new string[3] { "vendorinvoice", "invoices", "invoicedate" };
                        object[] parms = new object[3] { vendorinvoice, findetails, invoicedate };
                        string result = Soap.Invoke<string>("cst_SAPConciliateInterfaceWS.asmx", "CreateManualInvoice", args, parms);
                        if (result == "OK")
                        {
                            MessageBox.Show("Invoice was created for Vendor Invoice: " + row.Cells["vendorinvoice"].Value.ToString(), "SAP Concilation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Invoice can't be created for Vendor Invoice: " + row.Cells["vendorinvoice"].Value.ToString() + ". The sum for amounts for credit/debit is different than the total for the selected findetails.", "SAP Concilation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            
                            _view.UpdateData();
                            return new UiEventResult(EventStatus.Continue);
                        }
                    }
                }
                else
                {
                    continue;
                }
                
            }
            
            #endregion
            
            #region Validation Create
            ds.Clear();
            SqlHelper.RetrieveData(ds, new[] { "cst_invoicesintegrity" }, new[] { "SELECT * FROM cst_invoicesintegrity where process = 'Validation Invoice' and processtype in ('MANUAL','BOTH') and transactiontype = 'AP' order by ordervalidation " });
            
            foreach (UltraGridRow SAPConcilationRow in SAPConcilationSelectedRows)
            {
                
                validation = true;
                DataSet dsfintransact = new DataSet();
                DataSet dsconciliated = new DataSet();
                foreach (DataRow validationrows in ds.Tables["cst_invoicesintegrity"].Rows)
                {
                    DataSet dsvalidation = new DataSet();
                    SqlHelper.RetrieveData(dsconciliated, new[] { "cst_sapinvoiceconciliate" }, new[] { "SELECT * FROM cst_sapinvoiceconciliate where vendorinvoice = '" + SAPConcilationRow.Cells["vendorinvoice"].Value.ToString() + "'" });
                    
                    string sql = string.Format(validationrows["condition"].ToString(), dsconciliated.Tables[0].Rows[0]["allegroinvoice"].ToString());
                    SqlHelper.RetrieveData(dsfintransact, new[] { "fintransact" }, new[] { "SELECT * FROM fintransact where fintransact = '" + dsconciliated.Tables[0].Rows[0]["allegroinvoice"].ToString() + "'" });
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
                                        foreach (DataRow posRow in dsfintransact.Tables["fintransact"].Rows)
                                        {
                                            posRow["cst_invoicevalidation"] = "Field " + dsvalidation.Tables[0].Columns[i].ToString() + " is empty";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Convert.ToBoolean(validationrows["informativepopup"]))
                                    MessageBox.Show(validationrows["message"].ToString(), "Validation Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                                foreach (DataRow posRow in dsfintransact.Tables["fintransact"].Rows)
                                {
                                    posRow["cst_invoicevalidation"] = validationrows["message"].ToString();
                                }
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
                                foreach (DataRow posRow in dsfintransact.Tables["fintransact"].Rows)
                                {
                                    posRow["cst_invoicevalidation"] = validationrows["message"].ToString();
                                }
                                
                                
                                break;
                            }
                        }
                    }
                }
                if (validation)
                {
                    foreach (DataRow posRow in dsfintransact.Tables["fintransact"].Rows)
                    {
                        posRow["cst_invoicevalidation"] = String.Empty;
                    }
                    
                }
                
                SqlHelper.UpdateData(dsfintransact);
            }
            
            _view.UpdateData();
            #endregion Validation Create
            
            _view.RetrieveData();
            return new UiEventResult(EventStatus.Continue);
        }
        /* SAP Conciliate Invoices - Import findetails
        SAP Conciliate Invoices - DEV - Import findetails */
        public UiEventResult ToolClick_Import_findetails_Before_1()
        {
            SelectedRowsCollection SAPConcilationSelectedRows = _view.ViewGrids["SAP_Concilation"].Selected.Rows;
            
            foreach (UltraGridRow SAPConcilationRow in SAPConcilationSelectedRows)
            {
                string SAPConcilationCTC = SAPConcilationRow.Cells["ctc"].Value.ToString();
                string selectedfindetail = "";
                
                SelectedRowsCollection FinDetailSelectedRows = _view.ViewGrids["Findetail"].Selected.Rows;
                
                foreach (UltraGridRow FindetailRow in FinDetailSelectedRows)
                {
                    string findetail = FindetailRow.Cells["findetail"].Value.ToString();
                    if (FindetailRow.Cells["ctc"].Value.ToString() == SAPConcilationCTC)
                    {
                        selectedfindetail += findetail + ",";
                    }
                    else
                    {
                        MessageBox.Show(findetail + " CTC is " + FindetailRow.Cells["ctc"].Value.ToString() + ", it's must be " + SAPConcilationCTC);
                    }
                    
                }
                
                if (!string.IsNullOrEmpty(selectedfindetail))
                {
                    selectedfindetail = selectedfindetail.Substring(0, selectedfindetail.Length - 1);
                    SAPConcilationRow.Cells["findetails"].Value = selectedfindetail;
                }
                
            }
            
            _view.UpdateData();
            
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

