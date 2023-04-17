using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace Allegro.ClassEvents
{
    public class cst_Hub
    {
        public static void FromExcelToDataSet(Stream excelStream, string excelWorksheetName, int excelStartingRow, int excelStartingColumn, List<cst_HubExcelBindingColumn> excelBindingColumns, DataSet dataSet)
        {
            //Set context
            cst_HubExcelTableContext excelTableContext = new cst_HubExcelTableContext();
            excelTableContext.WorksheetName = excelWorksheetName;
            excelTableContext.StartingRow = excelStartingRow;
            excelTableContext.StartingColumn = excelStartingColumn;
            excelTableContext.TargetTableName = excelBindingColumns[0].DataColumn.Table.TableName;
            excelTableContext.ColumnsToFill.AddRange(excelBindingColumns);
            
            //Worker
            cst_IHubExcelWorker excelWorker = new cst_HubExcelWorker();
            excelWorker.LoadWorkbook(excelStream);
            excelWorker.FillDataTableFromSheet(dataSet, excelTableContext);
        }
        
        public static void FromExcelToDataSet(string excelPath, string excelWorksheetName, int excelStartingRow, int excelStartingColumn, List<cst_HubExcelBindingColumn> excelBindingColumns, DataSet dataSet)
        {
            //Set context
            cst_HubExcelTableContext excelTableContext = new cst_HubExcelTableContext();
            excelTableContext.WorksheetName = excelWorksheetName;
            excelTableContext.StartingRow = excelStartingRow;
            excelTableContext.StartingColumn = excelStartingColumn;
            excelTableContext.TargetTableName = excelBindingColumns[0].DataColumn.Table.TableName;
            excelTableContext.ColumnsToFill.AddRange(excelBindingColumns);
            
            //Worker
            cst_IHubExcelWorker excelWorker = new cst_HubExcelWorker();
            excelWorker.LoadWorkbook(excelPath);
            excelWorker.FillDataTableFromSheet(dataSet, excelTableContext);
        }
    }
}
