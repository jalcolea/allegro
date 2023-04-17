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
    public class cst_TradeLoaderConfCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* TradeImporter - TradeLoaderconf_beforecell
        Trade Importer - DEV - Shows concept available values before cell update */
        public UiEventResult BeforeCellActivate_allegrovalue_1(object sender, CancelableCellEventArgs e)
        {
            /******************************************************************************
            NAME:  TradeImporter
            
            PURPOSE/DESCRIPTION/NOTES:    Trade Importer
            
            REVISIONS:
            Ver        Date        Author           Description
            ---------  ----------  ---------------  ------------------------------------
            1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
            ---------  ----------  ---------------  ------------------------------------
            *********************************************************************************/
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            
            string sql = "select distinct c.dbconstraint from cst_tradeloadertarget t " +
            "inner join viewpane vp on t.datatable = vp.datatable " +
            "inner join viewcolumn vc on vp.viewpane = vc.viewpane and vp.viewname = vc.viewname and t.viewcolumn = vc.viewcolumn " +
            "inner join dbobject c on vc.dbtable = c.dbtable and vc.dbcolumn = c.dbcolumn " +
            "where c.dbconstraint is not null and t.name = {0}";
            
            string sqlTarget = SqlHelper.SqlFormat(sql, e.Cell.Row.Cells["target"].Value);
            
            DataSet dsData = new DataSet();
            SqlHelper.RetrieveData(dsData, new string[] { "target" }, new string[] { sqlTarget });
            
            if (dsData.Tables[0].Rows.Count == 0 || dsData.Tables[0].Rows[0].IsNull("dbconstraint"))
            {
                grid.SetColumnStyle(e.Cell.Column, string.Empty);
                return new UiEventResult(EventStatus.Continue);
            }
            else
            {
                grid.SetColumnStyle(e.Cell.Column, dsData.Tables[0].Rows[0]["dbconstraint"].ToString());
                return new UiEventResult(EventStatus.Continue);
            }
        }
        /* TradeImporter - TradeLoaderconf_IV
        Trade Importer - DEV -  Init view of Indra Trade Importer loader */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

