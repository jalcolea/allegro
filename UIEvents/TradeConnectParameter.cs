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
using System.Xml;

namespace Allegro.ClassEvents
{
    public class TradeConnectParameterCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* Create IMText
        Create IMText */
        public UiEventResult ToolClick_Create_IMText_Before_1()
        {
            SelectCriteria criteria = _view.GetSelectCriteria(true, true);
            
            string[] _string = new string[1] { "Criteria" };
            object[] _object = new object[1] { criteria };
            
            Soap.Invoke<bool>("ExtensionTradeConnectWS.asmx", "CreateIMText", _string, _object); return null;
        }
        
        
    }
}

