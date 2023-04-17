/******************************************************************************
NAME:  TradeImporter

PURPOSE/DESCRIPTION/NOTES:    Trade Importer

REVISIONS:
Ver        Date        Author           Description
---------  ----------  ---------------  ------------------------------------
1.0        01/04/2019  (SSA)DZL         TradeImporter - Initial Version
---------  ----------  ---------------  ------------------------------------
*********************************************************************************/
using Infragistics.Documents.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Allegro.ClassEvents
{
    public class InfragisticsLibWrapper : IExcelLibWrapper
    {
        /// <summary>
        /// Creates a wrapper for Excel lib
        /// </summary>
        /// <param name="filename">Filename to load</param>
        /// <returns>Loaded Excel</returns>
        public IExcelFile Load(FileStream filename)
        {
            var file = Workbook.Load(filename);
            
            return new InfragisticsExcelFile(file);
        }
    }
    
    public class InfragisticsExcelFile : IExcelFile
    {
        private readonly Workbook _workbook;
        public IList<IExcelWorkSheet> WorkSheets { get; private set; }
        
        public InfragisticsExcelFile(Workbook workbook)
        {
            _workbook = workbook;
            WorkSheets = new List<IExcelWorkSheet>();
            foreach (var worksheet in _workbook.Worksheets)
            {
                WorkSheets.Add(new InfragisticsWorkSheet(worksheet));
            }
        }
        
        public void AddEmptyWorkSheet(string worksheetName)
        {
            _workbook.Worksheets.Add(worksheetName);
        }
    }
    
    public class InfragisticsWorkSheet : IExcelWorkSheet
    {
        private readonly Worksheet _worksheet;
        
        public int FirstIndexColumn
        {
            get
            {
                return 0;
            }
        }
        
        public int FirstIndexRow
        {
            get
            {
                return 0;
            }
        }
        
        public int LastIndexColumn
        {
            get
            {
                int index = FirstIndexColumn;
                while (_worksheet.GetCell(GetAddress(index, 0)).Value != null)
                {
                    index++;
                }
                
                return index - 1;
            }
        }
        
        public int LastIndexRow
        {
            get
            {
                int index = FirstIndexRow;
                while (_worksheet.GetCell(GetAddress(0, index)).Value != null)
                {
                    index++;
                }
                
                return index - 1;
            }
        }
        
        public string Name
        {
            get
            {
                return _worksheet.Name;
            }
        }
        
        public InfragisticsWorkSheet(Worksheet worksheet)
        {
            _worksheet = worksheet;
        }
        
        public object GetCellValue(int colIndex, int rowIndex)
        {
            return _worksheet.Rows[rowIndex].GetCellValue(colIndex);
        }
        
        public object GetCellValue(string column, int row)
        {
            return _worksheet.GetCell(column + row.ToString()).Value;
        }
        
        public void SetCellValue(string column, int row, object value)
        {
            _worksheet.GetCell(column + row.ToString()).Value = value;
        }
        
        /// <summary>
        /// Gets Excel addess given a row and a column base 0 indexes
        /// </summary>
        /// <param name="column">Base 0 column index</param>
        /// <param name="row">Base 0 row index</param>
        /// <returns>Excel address</returns>
        private string GetAddress(int column, int row)
        {
            if (column < 26)
            {
                return Convert.ToChar(column + 65).ToString() + (row + 1).ToString();
            }
            else
            {
                var columnAddress = new List<char>();
                int div = column - 26;
                do
                {
                    columnAddress.Insert(0, Convert.ToChar(div % 26 + 65));
                    div /= 26;
                }
                while (div != 0);
                
                return new string(columnAddress.ToArray()) + (row + 1).ToString();
            }
        }
    }
}
