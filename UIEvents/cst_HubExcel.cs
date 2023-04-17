using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Allegro.ClassEvents
{
    #region Interfaces
    
    public interface cst_IHubExcelWorker
    {
        void LoadWorkbook(Stream stream);
        void LoadWorkbook(string path);
        int GetColumnByExcelHeader(string columnName);
        int GetColumnByHeader(string columnName, cst_HubExcelTableContext tableContext);
        string GetHeaderByColumn(int column, cst_HubExcelTableContext tableContext);
        void FillDataTableFromSheet(DataSet dataSet, cst_HubExcelTableContext tableContext);
    }
    
    public interface cst_IHubExcelWorkbook
    {
        int WorksheetsCount();
        
        cst_IHubExcelWorkSheet GetWorkSheet(string key);
        cst_IHubExcelWorkSheet GetWorkSheet(int key);
    }
    
    public interface cst_IHubExcelWorkSheet
    {
        int RowsCount();
        
        cst_IHubExcelRow GetRow(int key);
    }
    
    public interface cst_IHubExcelRow
    {
        int CellsCount();
        
        cst_IHubExcelCell GetCell(int key);
    }
    
    public interface cst_IHubExcelCell
    {
        object GetValue();
    }
    
    public interface cst_IHubExcelManager
    {
        cst_IHubExcelWorkbook LoadWorkbook();
    }
    
    #endregion
    
    #region Common
    
    public class cst_HubExcelBindingColumn
    {
        public DataColumn DataColumn = null;
        public int? ExcelCell = null;
        public object DefaultValue;
        public Func<object> DefValueCustom;
        public Func<object, object> SimplyMapper;
    }
    
    public class cst_HubExcelTableContext
    {
        public string WorksheetName = string.Empty;
        public int? WorksheetIndex = null;
        
        public int HeaderRow;
        public int StartingRow;
        public int StartingColumn;
        public int? RowsToLoadAction = null;
        
        public string TargetTableName = string.Empty;
        public int? TargetTableIndex = null;
        
        public List<cst_HubExcelBindingColumn> ColumnsToFill = new List<cst_HubExcelBindingColumn>();
        public bool CreateIfNotExist { get; set; }
    }
    
    public partial class cst_HubDataTableManager
    {
        private DataSet ds;
        
        public cst_HubDataTableManager(DataSet dsExt)
        {
            ds = dsExt;
        }
        
        public DataTable BuilDataTable(string tableName, IEnumerable<DataColumn> columns)
        {
            DataTable dt = null;
            if (ds.Tables.Contains(tableName) == false)
            {
                dt = ds.Tables.Add(tableName);
                
                foreach (DataColumn column in columns)
                {
                    dt.Columns.Add(column);
                }
            }
            else
            {
                dt = ds.Tables[tableName];
            }
            
            return dt;
        }
    }
    
    public partial class cst_HubExcelWorker : cst_IHubExcelWorker
    {
        private cst_IHubExcelManager excelManager;
        private cst_IHubExcelWorkbook workbook;
        
        public void LoadWorkbook(Stream stream)
        {
            excelManager = new cst_HubExcelManager(stream);
        }
        
        public void LoadWorkbook(string path)
        {
            excelManager = new cst_HubExcelManager(path);
        }
        
        public int GetColumnByExcelHeader(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentException("Column name is not defined");
            
            columnName = columnName.ToUpperInvariant();
            
            int column = 0;
            
            for (int i = 0; i < columnName.Length; i++)
            {
                column *= 26;
                column += (columnName[i] - 'A' + 1);
            }
            
            return column - 1;
        }
        
        public int GetColumnByHeader(string columnName, cst_HubExcelTableContext tableContext)
        {
            if (workbook == null) workbook = excelManager.LoadWorkbook();
            
            cst_IHubExcelWorkSheet workSheet = null;
            
            if (tableContext.WorksheetIndex.HasValue)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetIndex.Value);
            }
            else if (string.IsNullOrEmpty(tableContext.WorksheetName) == false)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetName);
            }
            
            if (workSheet == null)
                throw new ArgumentException("Missing worksheet location data");
            
            try
            {
                cst_IHubExcelRow row = workSheet.GetRow(tableContext.HeaderRow);
                
                int column = tableContext.StartingColumn;
                
                while(column >= 0)
                {
                    try
                    {
                        object value = row.GetCell(column).GetValue();
                        
                        if (value != null && value.Equals(columnName))
                        {
                            return column;
                        }
                        
                        column++;
                    }
                    catch
                    {
                        return -1;
                    }
                }
                
                return -1;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " on excel file at line " + tableContext.HeaderRow, e);
            }
        }
        
        public string GetHeaderByColumn(int column, cst_HubExcelTableContext tableContext)
        {
            if (workbook == null) workbook = excelManager.LoadWorkbook();
            
            cst_IHubExcelWorkSheet workSheet = null;
            
            if (tableContext.WorksheetIndex.HasValue)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetIndex.Value);
            }
            else if (string.IsNullOrEmpty(tableContext.WorksheetName) == false)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetName);
            }
            
            if (workSheet == null)
                throw new ArgumentException("Missing worksheet location data");
            
            try
            {
                cst_IHubExcelRow row = workSheet.GetRow(tableContext.HeaderRow);
                
                object value = row.GetCell(column).GetValue();
                
                if (value != null)
                {
                    return row.GetCell(column).GetValue().ToString();
                }
                
                return string.Empty;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " on excel file at line " + tableContext.HeaderRow, e);
            }
        }
        
        public void FillDataTableFromSheet(DataSet dataSet, cst_HubExcelTableContext tableContext)
        {
            if (workbook == null) workbook = excelManager.LoadWorkbook();
            
            cst_IHubExcelWorkSheet workSheet = null;
            
            if (tableContext.WorksheetIndex.HasValue)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetIndex.Value);
            }
            else if (string.IsNullOrEmpty(tableContext.WorksheetName) == false)
            {
                workSheet = workbook.GetWorkSheet(tableContext.WorksheetName);
            }
            
            if (workSheet == null)
                throw new ArgumentException("Missing worksheet location data");
            
            if (tableContext.TargetTableIndex.HasValue == false && string.IsNullOrEmpty(tableContext.TargetTableName) == true)
                throw new ArgumentException("Missing target Datable information");
            
            DataTable targetTable = null;
            
            if (tableContext.TargetTableIndex.HasValue == true)
            {
                if (dataSet != null && (dataSet.Tables.Count - 1) >= 0 && ((dataSet.Tables.Count - 1) >= tableContext.TargetTableIndex))
                {
                    targetTable = dataSet.Tables[tableContext.TargetTableIndex.Value];
                }
            }
            else if (string.IsNullOrEmpty(tableContext.TargetTableName) == false)
            {
                if (dataSet != null && (dataSet.Tables.Contains(tableContext.TargetTableName)))
                {
                    targetTable = dataSet.Tables[tableContext.TargetTableName];
                }
            }
            
            if (targetTable == null)
            {
                if (tableContext.CreateIfNotExist == true)
                {
                    cst_HubDataTableManager tableMnsg = new cst_HubDataTableManager(dataSet);
                    targetTable = tableMnsg.BuilDataTable(tableContext.TargetTableName, tableContext.ColumnsToFill.Where(col => col.DataColumn != null).Select(col => col.DataColumn));
                }
                else
                {
                    throw new ArgumentException("No datatable was detected on DataSet");
                }
            }
            
            int numberRows = tableContext.RowsToLoadAction.HasValue == true ? tableContext.RowsToLoadAction.Value : workSheet.RowsCount();
            
            for (int i = tableContext.StartingRow; i < numberRows; i++)
            {
                string field = string.Empty;
                object externVal = null;
                
                try
                {
                    cst_IHubExcelRow row = workSheet.GetRow(i);
                    
                    DataRow newRow = targetTable.NewRow();
                    
                    foreach (cst_HubExcelBindingColumn bindingColumn in tableContext.ColumnsToFill.Where(col => col.DataColumn != null))
                    {
                        field = bindingColumn.DataColumn.ColumnName;
                        externVal = null;
                        object value = null;
                        
                        if (bindingColumn.ExcelCell.HasValue == true)
                        {
                            value = row.GetCell(tableContext.StartingColumn + bindingColumn.ExcelCell.Value).GetValue();
                            
                            if (bindingColumn.SimplyMapper != null)
                            {
                                value = bindingColumn.SimplyMapper.Invoke(value);
                            }
                        }
                        else
                        {
                            if (bindingColumn.DefValueCustom != null)
                            {
                                value = bindingColumn.DefValueCustom.Invoke();
                            }
                            else if (bindingColumn.DefaultValue != null)
                            {
                                value = bindingColumn.DefaultValue;
                            }
                            else if (bindingColumn.DataColumn.DefaultValue != null)
                            {
                                value = bindingColumn.DataColumn.DefaultValue;
                            }
                        }
                        
                        externVal = value;
                        
                        object convertedValue = null;
                        
                        if (bindingColumn.DataColumn.DataType == typeof(DateTime))
                        {
                            if (value != null && !value.Equals(string.Empty))
                            {
                                try
                                {
                                    convertedValue = Convert.ToDateTime(value);
                                }
                                catch
                                {
                                    convertedValue = null;
                                }
                            }
                        }
                        else if (bindingColumn.DataColumn.DataType == typeof(decimal))
                        {
                            if (value != null && !value.Equals(string.Empty))
                            {
                                try
                                {
                                    convertedValue = Convert.ToDecimal(value);
                                }
                                catch
                                {
                                    convertedValue = null;
                                }
                            }
                        }
                        else if (bindingColumn.DataColumn.DataType == typeof(bool))
                        {
                            if (value != null && !value.Equals(string.Empty))
                            {
                                try
                                {
                                    convertedValue = Convert.ToBoolean(value);
                                }
                                catch
                                {
                                    convertedValue = null;
                                }
                            }
                        }
                        else
                        {
                            convertedValue = value;
                        }
                        
                        newRow.SetField(targetTable.Columns[bindingColumn.DataColumn.ColumnName], convertedValue);
                    }
                    
                    targetTable.Rows.Add(newRow);
                    
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + " on excel file at line " + (i + tableContext.StartingRow) + " of column field '" + field + (externVal != null ? "' with value '" + externVal + "'" : "'"), e);
                }
            }
        }
    }
    
    #endregion
}
