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
    public class CreditAnalysisCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* CreditAnalysis - AfterRetrieve
        CreditAnalysis - AfterRetrieve */
        public UiEventResult AfterRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            string valuationtime = string.Empty;
            if (_view.DtParameter.Rows.Count > 0)
            {
                if (!_view.DtParameter.Rows[0].IsNull("valuationtime"))
                {
                    valuationtime = _view.DtParameter.Rows[0]["valuationtime"].ToString();
                }
            }
            
            
            
            foreach (DataRow row in _view.DataSource.Tables["cst_specificcollateral"].Rows)
            {
                if (row["valuationtime"].ToString() != valuationtime && (!String.IsNullOrEmpty(row["valuationtime"].ToString())))
                    row.Delete();
            }
            //_view.DataSource.Tables["cst_specificcollateral"].Rows.Clear();
            //DataSet dscollateral = new DataSet();
            //SqlHelper.RetrieveData(dscollateral, new[] { "cst_specificcollateral" }, new[] { "Select * From cst_specificcollateral where valuationtime = '2022-06-01 00:00:00.000'" });
            
            //foreach (DataRow row in dscollateral.Tables[0].Rows)
                //{
                //    DataRow newrow = new DataRow();
                //    newrow = row;
                //    _view.DataSource.Tables["cst_specificcollateral"].Rows.Add(newrow);
            //}
            _view.UpdateData();
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

