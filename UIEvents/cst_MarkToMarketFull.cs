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
    public class cst_MarkToMarketFullCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Mark To Market Full - Init View
        Mark To Market Full - EVE - Init View */
        public UiEventResult InitView_1()
        {
            return new UiEventResult(EventStatus.Continue);
        }
        /* Mark To Market Full - Export Excel
        Mark To Market Full - EVE - Logic for toolclick button for export excel file */
        public UiEventResult ToolClick_Export_excel_Before_1()
        {
            if (AppManager.MainForm.ActiveGrid == _view.ViewGrids["position_valuation"] && !AppManager.MainForm.ActiveGrid.IsCardView)
            {
                try
                {
                    new GridExporter(_view.ViewGrids["position_valuation_export"]).ExportExcel(true);
                }
                catch (Exception ex)
                {
                    Exceptions.HandleException(ex);
                    ErrorDialog.Show("Excel", ex);
                }
            }
            
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

