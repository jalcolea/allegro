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
using Allegro.ESSConnect.Schema;
using  Allegro.Power.Schema;
using  System.Xml;

namespace Allegro.ClassEvents
{
    public class PowerSchedCE  : UiClassEvent
    {
        private static IAllegroLogger log = AllegroLoggerManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        /* CreateEssMessage
        This event triggers the ESSConnect method to create the ESS messages based on view criteria or the highlighted rows in Power Scheduling view. The criteria is built only based on the company and controlarea. */
        public UiEventResult ToolClick_Create_Message_Before_1()
        {
            List<string> controlareaList = new List<string>();
            List<string> companyList = new List<string>();
            foreach (UltraGridRow scheduleRow in this._view.ViewGrids["powerschedule"].Selected.Rows)
            {
                if (scheduleRow.Cells["controlarea"].Value == DBNull.Value) continue;
                if (!controlareaList.Contains(scheduleRow.Cells["controlarea"].Value.ToString()))
                    controlareaList.Add(scheduleRow.Cells["controlarea"].Value.ToString());
                if (scheduleRow.Cells["company"].Value == DBNull.Value) continue;
                if (!companyList.Contains(scheduleRow.Cells["company"].Value.ToString()))
                    companyList.Add(scheduleRow.Cells["company"].Value.ToString());
                
            }
            SelectCriteria viewCriteria = this._view.GetSelectCriteria(true, true);
            SelectCriteria criteria = new SelectCriteria();
            criteria.BegTime = viewCriteria.BegTime;
            criteria.EndTime = viewCriteria.EndTime;
            criteria.DateColumn = viewCriteria.DateColumn;
            bool containsIndex = false;
            EssMessageDS essMessageDS = new EssMessageDS();
            essMessageDS.EnforceConstraints = false;
            if (this._view.ViewGrids["powerschedule"].Selected.Rows.Count == 0)
            {
                foreach (DbCriteria dbCriteria in viewCriteria.DbCriteria)
                {
                    if (dbCriteria.DbColumn.Equals("controlarea"))
                    {
                        criteria.AddDbCriteria(dbCriteria);
                        containsIndex = true;
                        
                    }
                    else if (dbCriteria.DbColumn.Equals("company"))
                    {
                        criteria.AddDbCriteria(dbCriteria);
                        containsIndex = true;
                    }
                    else
                    criteria.AddDbCriteria(dbCriteria);
                    
                }
            }
            if (!containsIndex && this._view.ViewGrids["powerschedule"].Selected.Rows.Count == 0)
            {
                if (System.Windows.Forms.MessageBox.Show("System will create ESS Messages for all Control areas defined in the ESS Config table", "WARNING", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return new UiEventResult(EventStatus.Cancel);
            }
            
            string[] controlarea = controlareaList.ToArray();
            string[] company = companyList.ToArray();
            if (controlarea.Length > 0 || company.Length > 0)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("(");
                for (int i = 0; i < controlarea.Length; i++)
                {
                    if (i == controlarea.Length - 1)
                        builder.AppendFormat("{0} )", Expr.Value(controlarea[i]));
                    else
                    builder.AppendFormat("{0}, ", Expr.Value(controlarea[i]));
                }
                if (builder.Length > 1)
                    criteria.AddDbCriteria(new DbCriteria("AND", "(", "powerposition", "controlarea", "IN", builder.ToString(), ")"));
                builder.Remove(0, builder.Length);
                builder.Append("(");
                for (int i = 0; i < company.Length; i++)
                {
                    if (i == company.Length - 1)
                        builder.AppendFormat("{0} )", Expr.Value(company[i]));
                    else
                    builder.AppendFormat("{0}, ", Expr.Value(company[i]));
                }
                if(builder.Length > 1)
                    criteria.AddDbCriteria(new DbCriteria("AND", "(", "position", "company", "IN", builder.ToString(), ")"));
            }
            string[] arg_name = new string[2] { "essMessageDS", "criteria" };
            object[] arg_value = new object[2] { essMessageDS, criteria };
            XmlNode node = Soap.Invoke("ESSConnect/EssMessageWS.asmx", "CreateESSMessage", arg_name, arg_value);
            DataSet dsTemp = new DataSet();
            dsTemp.EnforceConstraints = false;
            XmlHelper.LoadXml(node, dsTemp);
            FormHelper.ShowValidation(dsTemp);
            return new UiEventResult(EventStatus.Cancel);
        }
        
        
    }
}

