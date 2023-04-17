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
    public class ValuationCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Valuation check archive
        Valuation check archive */
        public UiEventResult BeforeRetrieveData_911(object sender, RetrieveDataEventArgs e)
        {
            _view.DataSource.ExtendedProperties.Remove("Check Archive");
            if (ToolbarHelper.CheckStateButton(_view, "Check Archive"))
            {
                _view.DataSource.ExtendedProperties["Check Archive"] = "Y";
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Add check archive toolbar button
        Add check archive toolbar button */
        public UiEventResult InitView_911()
        {
            ToolbarHelper.AddButton(_view, "Check Archive", true, "query.ico");
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

