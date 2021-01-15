using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.XlsIO;
using System.IO;
using Syncfusion.Drawing;
using System.Reflection;
using System.Windows;
using Microsoft.AspNetCore.Mvc;

namespace ExcelWorker
{
    public class ExcelWorkerClass
    {
        public static FileStreamResult GetFile(Dictionary<string, int> data)
        {
            if (data == null)
                return null;
            using ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            int i = 1;
            foreach (var item in data)
            {
                worksheet.Range[$"A{i}"].Text = item.Key;
                worksheet.Range[$"B{i++}"].Number= item.Value;
            }
            
            MemoryStream stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(stream, "application/excel");
            fileStreamResult.FileDownloadName = $"{DateTime.Now}.xlsx";
            return fileStreamResult;
        }
    }
}
 