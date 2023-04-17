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
    public class ContractExecutionCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Feemethod_beforeCellActive
        Constraint */
        public UiEventResult BeforeCellActivate_feemethod_1(object sender, CancelableCellEventArgs e)
        {
            ViewGrid grid = (ViewGrid)sender;
            if (grid.GetBindRow(e.Cell.Row) == null) return new UiEventResult(EventStatus.Continue);
            string newConstraint = "MONTHPRORATE,FIXEDPRORATE,FIXED,HOUR,DAY,WEEK,MONTH,QUARTER,YEAR,FULLTURN,HALFTURN,PERCENT,DELVOLUME,RECVOLUME,LOSSVOLUME,CONTRACTVOLUME,TIERVOLUME,TIERDIFF,PEAK,UNITDAY,UNITMONTH,MINTOLERANCE,MAXTOLERANCE,UNIT,INCREMENTAL,DECREMENTAL,DELIVERYHOUR,SCHEDULEVOLUME,STORAGE,STORAGEUSE,STORAGEEXCESS,WORLDSCALE,INTEREST,REGRADE,RAILCAR,TRUCK";
            grid.SetColumnStyle(e.Cell.Column, newConstraint);
            return new UiEventResult(EventStatus.Continue);
        }
        
        
    }
}

