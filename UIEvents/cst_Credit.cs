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
    public class cst_CreditCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Credit - Before Retrieve Data
        Credit - EVE - Before Retrieve Data */
        public UiEventResult BeforeRetrieveData_1(object sender, RetrieveDataEventArgs e)
        {
            if (e.TableNames.Contains("cstview_creditreport") && !e.TableNames.Contains("cstview_creditreportdetail"))
            {
                e.TableNames.Add("cstview_creditreportdetail");
                
                return new UiEventResult(EventStatus.Continue);
            }
            else
            {
                return new UiEventResult(EventStatus.Cancel);
            }
        }
        /* Credit - Init View
        Credit - EVE - Init View */
        public UiEventResult InitView_1()
        {
            if (_view.DataSource.Tables["cstview_creditreport"].ChildRelations["relation"] == null)
            {
                _view.DataSource.Tables["cstview_creditreport"].ChildRelations.Add(new DataRelation("relation", _view.DataSource.Tables["cstview_creditreport"].Columns["counterparty"], _view.DataSource.Tables["cstview_creditreportdetail"].Columns["counterparty"]));
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

