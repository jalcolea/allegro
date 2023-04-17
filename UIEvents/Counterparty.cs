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
    public class CounterpartyCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        DataRow[] addedCounterpartyRows_global = null;
        DataRow[] addedNIFRows_global = null;
        #endregion
        
        /* Counterparty - AfterUpdateData TaxID
        Counterparty - AfterUpdateData TaxID - DEV - Check if the new/updated TaxID value is a valid value with VIES interface */
        public UiEventResult AfterUpdateData_1(object sender, EventArgs e)
        {
            if (addedCounterpartyRows_global != null && addedCounterpartyRows_global.Length > 0)
            {
                foreach (DataRow counterpartyRow in addedCounterpartyRows_global)
                {
                    string strCounterparty = counterpartyRow["counterparty", DataRowVersion.Current].ToString();
                    
                    foreach (DataRow nifRow in addedNIFRows_global)
                    {
                        string strNifCounterparty = nifRow["counterparty", DataRowVersion.Current].ToString();
                        
                        if (strCounterparty == strNifCounterparty)
                        {
                            string counterparty = nifRow["counterparty", DataRowVersion.Current].ToString();
                            string ctcModificado = counterpartyRow["cst_ctc", DataRowVersion.Current].ToString();
                            bool bCreditApproved = false;
                            
                            if (counterpartyRow["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                            {
                                bCreditApproved = true;
                            }
                            
                            string nifOld = "";
                            string nifNew = nifRow["nif", DataRowVersion.Current].ToString();
                            string[] args = new string[5] { "counterparty", "ctcModificado", "nifOld", "nifNew", "creditApproved" };
                            object[] parms = new object[5] { counterparty, ctcModificado, nifOld, nifNew, bCreditApproved };
                            string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_nif", args, parms);
                        }
                    }
                }
            }
            
            DataRow[] addedNIFRows = _view.DataSource.Tables["cst_counterpartynif"].Select("envoystatus is null");
            
            foreach (DataRow nifRow in addedNIFRows)
            {
                string counterparty = nifRow["counterparty", DataRowVersion.Current].ToString();
                string nif = nifRow["nif", DataRowVersion.Current].ToString();
                string[] args = new string[2] { "counterparty", "nif" };
                object[] parms = new object[2] { counterparty, nif };
                string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "actualizarStatusNifNuevos", args, parms);
            }
            
            addedCounterpartyRows_global = null;
            addedNIFRows_global = null;
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Counterparty - AfterUpdateData in CounterpartyNIF
        Counterparty - AfterUpdateData in CounterpartyNIF - DEV - Check if the new/updated TaxID value is a valid value with VIES interface */
        public UiEventResult AfterUpdateData_2(object sender, EventArgs e)
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* Counterparty - BeforeUpdateData TaxID
        Counterparty - BeforeUpdateData TaxID - DEV - Check if the new/updated TaxID value is a valid value with VIES interface */
        public UiEventResult BeforeUpdateData_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            addedCounterpartyRows_global = _view.DataSource.Tables["counterparty"].Select("", "", DataViewRowState.Added);
            addedNIFRows_global = _view.DataSource.Tables["cst_counterpartynif"].Select("", "", DataViewRowState.Added);
            DataRow[] deletedCounterpartyRows = _view.DataSource.Tables["counterparty"].Select("", "", DataViewRowState.Deleted);
            Dictionary<string, string> contrapartesModificadas = new Dictionary<string, string>();
            
            foreach (DataRow CounterpartyRow in deletedCounterpartyRows)
            {
                string counterparty = CounterpartyRow["counterparty", DataRowVersion.Original].ToString();
                string ctcOld = CounterpartyRow["cst_ctc", DataRowVersion.Original].ToString();
                string ctcNew = "";
                bool bCreditApproved = false;
                
                if (CounterpartyRow["cst_counterpartystatus", DataRowVersion.Original].ToString() == "CREDIT APPROVED")
                {
                    bCreditApproved = true;
                }
                
                string[] args = new string[4] { "counterparty", "ctcOld", "ctcNew", "creditApproved" };
                object[] parms = new object[4] { counterparty, ctcOld, ctcNew, bCreditApproved };
                contrapartesModificadas.Add(counterparty, "");
                string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_ctc", args, parms);
            }
            
            DataRow[] newModifiedCounterpartyRows = _view.DataSource.Tables["counterparty"].Select("", "",  DataViewRowState.ModifiedCurrent);
            
            foreach (DataRow counterpartyRow in newModifiedCounterpartyRows)
            {
                if (counterpartyRow["taxid", DataRowVersion.Current] != counterpartyRow["taxid", DataRowVersion.Original] || counterpartyRow["area", DataRowVersion.Current] != counterpartyRow["area", DataRowVersion.Original])
                {
                    string nif = counterpartyRow["taxid", DataRowVersion.Current].ToString();
                    string counterparty = counterpartyRow["counterparty"].ToString();
                    string[] args = new string[2] { "counterparty","nif" };
                    object[] parms = new object[2] { counterparty,nif };
                    string result = Soap.Invoke<string>("cst_VIESInterfaceWS.asmx", "CheckVIES", args, parms);
                }
                
                if (counterpartyRow["cst_counterpartystatus", DataRowVersion.Current] != counterpartyRow["cst_counterpartystatus", DataRowVersion.Original])
                {
                    string counterparty = counterpartyRow["counterparty"].ToString();
                    string statusNew = counterpartyRow["cst_counterpartystatus", DataRowVersion.Current].ToString();
                    
                    if (statusNew.Equals("CREDIT APPROVED"))
                    {
                        string[] args = new string[1] { "counterparty" };
                        object[] parms = new object[1] { counterparty };
                        string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendAndUpdateCTCsToSAP_creditApproved", args, parms);
                    }
                }
                
                if (counterpartyRow["cst_ctc", DataRowVersion.Current] != counterpartyRow["cst_ctc", DataRowVersion.Original])
                {
                    string counterparty = counterpartyRow["counterparty"].ToString();
                    string ctcOld = counterpartyRow["cst_ctc", DataRowVersion.Original].ToString();
                    string ctcNew = counterpartyRow["cst_ctc", DataRowVersion.Current].ToString();
                    bool bCreditApproved = false;
                    
                    if (counterpartyRow["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                    {
                        bCreditApproved = true;
                    }
                    
                    string[] args = new string[4] { "counterparty", "ctcOld", "ctcNew", "creditApproved" };
                    object[] parms = new object[4] { counterparty, ctcOld, ctcNew, bCreditApproved };
                    contrapartesModificadas.Add(counterparty, ctcNew);
                    string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_ctc", args, parms);
                }
                
                if (counterpartyRow["status", DataRowVersion.Current] != counterpartyRow["status", DataRowVersion.Original])
                {
                    string counterparty = counterpartyRow["counterparty"].ToString();
                    string statusNew = counterpartyRow["status", DataRowVersion.Current].ToString();
                    string ctcOld = "";
                    string ctcNew = "";
                    
                    if (statusNew.Equals("ACTIVE"))
                    {
                        ctcNew = counterpartyRow["cst_ctc", DataRowVersion.Current].ToString();
                    }
                    else
                    {
                        ctcOld = counterpartyRow["cst_ctc", DataRowVersion.Original].ToString();
                    }
                    
                    bool bCreditApproved = false;
                    
                    if (counterpartyRow["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                    {
                        bCreditApproved = true;
                    }
                    
                    string[] args = new string[4] { "counterparty", "ctcOld", "ctcNew", "creditApproved" };
                    object[] parms = new object[4] { counterparty, ctcOld, ctcNew , bCreditApproved };
                    contrapartesModificadas.Add(counterparty, ctcNew);
                    string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_ctc", args, parms);
                }
            }
            
            DataRow[] deletedNIFRows = _view.DataSource.Tables["cst_counterpartynif"].Select("", "", DataViewRowState.Deleted);
            
            foreach (DataRow nifRow in deletedNIFRows)
            {
                string counterparty = nifRow["counterparty", DataRowVersion.Original].ToString();
                string ctcModificado = "";
                
                if (contrapartesModificadas.ContainsKey(counterparty))
                {
                    ctcModificado = contrapartesModificadas[counterparty];
                }
                
                bool bCreditApproved = false;
                DataRow[] counterpartyRows = _view.DataSource.Tables["counterparty"].Select("counterparty='" + counterparty + "'");
                
                if (counterpartyRows[0]["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                {
                    bCreditApproved = true;
                }
                
                string nifOld = nifRow["nif", DataRowVersion.Original].ToString();
                string nifNew = "";
                string[] args = new string[5] { "counterparty", "ctcModificado", "nifOld", "nifNew", "creditApproved" };
                object[] parms = new object[5] { counterparty, ctcModificado, nifOld, nifNew, bCreditApproved };
                string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_nif", args, parms);
            }
            
            DataRow[] modifiedNIFRows = _view.DataSource.Tables["cst_counterpartynif"].Select("", "", DataViewRowState.ModifiedCurrent);
            
            foreach (DataRow nifRow in modifiedNIFRows)
            {
                if (nifRow["nif", DataRowVersion.Current] != nifRow["nif", DataRowVersion.Original])
                {
                    string counterparty = nifRow["counterparty"].ToString();
                    string ctcModificado = "";
                    
                    if (contrapartesModificadas.ContainsKey(counterparty))
                    {
                        ctcModificado = contrapartesModificadas[counterparty];
                    }
                    
                    bool bCreditApproved = false;
                    DataRow[] counterpartyRows = _view.DataSource.Tables["counterparty"].Select("counterparty='" + counterparty + "'");
                    
                    if (counterpartyRows[0]["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                    {
                        bCreditApproved = true;
                    }
                    
                    string nifOld = nifRow["nif", DataRowVersion.Original].ToString();
                    string nifNew = nifRow["nif", DataRowVersion.Current].ToString();
                    string[] args = new string[5] { "counterparty", "ctcModificado", "nifOld", "nifNew", "creditApproved" };
                    object[] parms = new object[5] { counterparty, ctcModificado, nifOld, nifNew, bCreditApproved };
                    string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_nif", args, parms);
                }
            }
            
            DataRow[] addedNIFRows = _view.DataSource.Tables["cst_counterpartynif"].Select("", "", DataViewRowState.Added);
            
            foreach (DataRow nifRow in addedNIFRows)
            {
                string counterparty = nifRow["counterparty", DataRowVersion.Current].ToString();
                string ctcModificado = "";
                
                if (contrapartesModificadas.ContainsKey(counterparty))
                {
                    ctcModificado = contrapartesModificadas[counterparty];
                }
                
                bool bCreditApproved = false;
                DataRow[] counterpartyRows = _view.DataSource.Tables["counterparty"].Select("counterparty='" + counterparty + "'");
                
                if (counterpartyRows[0]["cst_counterpartystatus", DataRowVersion.Current].ToString() == "CREDIT APPROVED")
                {
                    bCreditApproved = true;
                }
                
                string nifOld = "";
                string nifNew = nifRow["nif", DataRowVersion.Current].ToString();
                string[] args = new string[5] { "counterparty", "ctcModificado", "nifOld", "nifNew", "creditApproved" };
                object[] parms = new object[5] { counterparty, ctcModificado, nifOld, nifNew, bCreditApproved };
                string result = Soap.Invoke<string>("SAPInterfaceWS.asmx", "SendCounterparties_nif", args, parms);
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        /* Counterparty - BeforeUpdateData CounterpartyNIF
        Counterparty - BeforeUpdateData CounterpartyNIF - DEV - Check if the new/updated NIF value is a valid value with VIES interface */
        public UiEventResult BeforeUpdateData_2(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataRow[] newModifiedCounterpartyRows = _view.DataSource.Tables["cst_counterpartynif"].Select("", "", DataViewRowState.ModifiedCurrent);
            
            
            foreach (DataRow counterpartyRow in newModifiedCounterpartyRows)
            {
                if (counterpartyRow["nif", DataRowVersion.Current] != counterpartyRow["nif", DataRowVersion.Original] )
                {
                    string nif = counterpartyRow["nif", DataRowVersion.Current].ToString();
                    string counterparty = counterpartyRow["counterparty"].ToString();
                    string[] args = new string[2] { "counterparty", "nif" };
                    object[] parms = new object[2] { counterparty, nif };
                    string result = Soap.Invoke<string>("cst_VIESInterfaceWS.asmx", "CheckVIES", args, parms);
                }
            }
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

