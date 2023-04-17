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
    public class PriceIndexCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Price value from archive UI
        Price value from archive UI */
        public UiEventResult BeforeRetrieveData_911(object sender, RetrieveDataEventArgs e)
        {
            _view.DataSource.Tables["pricevalue"].ExtendedProperties.Remove("From Archive");
            if (ToolbarHelper.CheckStateButton(_view, "From Archive"))
            {
                _view.DataSource.Tables["pricevalue"].ExtendedProperties["From Archive"] = "pricevalue";
            }
            return new UiEventResult(EventStatus.Continue);
        }
        /* Price index add from archive button
        Price index add from archive button */
        public UiEventResult InitView_911()
        {
            ToolbarHelper.AddButton(_view, "From Archive", true, "Query.ico");
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

