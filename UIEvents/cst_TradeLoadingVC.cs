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
    public class cst_TradeLoading  : ViewClass
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Class Variables
        public cst_TradeLoading(ViewForm view):base(view){}
        #endregion
        
        /* TradeImporter - TradeLoader_ic
        Trade Importer - DEV - Init class Trade Loading */
        public override void InitializeClass()
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
            base.InitializeClass();
        }
        
        
    }
}

