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
    public class cst_RisksExportCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Risks Export - Before Retrieve Data
        Risks Export - EVE - Before Retrieve Data */
        public UiEventResult BeforeRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            List<DbCriteria> dbCriteria = new List<DbCriteria>();
            
            string tradeTypesShownParameter = Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "RisksExport", "TradeTypesShown" });
            
            if (!string.IsNullOrEmpty(tradeTypesShownParameter))
            {
                dbCriteria.Add(new DbCriteria("AND", string.Empty, "cstview_risksexport", "tradetype", "in", tradeTypesShownParameter, string.Empty));
            }
            
            string feeTypesShownParameter = Soap.Invoke<string>("cst_ExtensionParameterWS.asmx", "GetKeyValue", new string[] { "extension", "key" }, new object[] { "RisksExport", "FeeTypesShown" });
            
            if (!string.IsNullOrEmpty(feeTypesShownParameter))
            {
                dbCriteria.Add(new DbCriteria("AND", "(", "cstview_risksexport", "feetype", "=", "<null>", string.Empty));
                dbCriteria.Add(new DbCriteria("OR", string.Empty, "cstview_risksexport", "feetype", "in", feeTypesShownParameter, ")"));
            }
            
            //3/1/2021 DZL: Add condition for BegTime
            DateTime dtCurrent = DateTime.Now.Date;
            DateTime dtCalculated = dtCurrent.AddDays(-15);
            
            
            dtCalculated = new DateTime(dtCalculated.Year, dtCalculated.Month, 1);
            dbCriteria.Add(new DbCriteria("AND", "(", "cstview_risksexport", "begtime", ">=", dtCalculated.ToString("yyyy-MM-dd"), ")"));
            
            
            
            
            e.Criteria.AddDbCriteria(dbCriteria.ToArray());
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

