using AcadLib;
using AcadLib.Errors;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_GP_Acad.Tests.Insolation.Compare
{
    /// <summary>
    /// Сравнение расчетов инсоляции
    /// </summary>
    public class TestCompareInsolation
    {
        string file1;//@"c:\temp\ГП\test\Compare\test1_correct.dwg";
        string file2;//@"c:\temp\ГП\test\Compare\test1_calc.dwg";

        [CommandMethod(nameof(TestInsCompare1), CommandFlags.Session)]
        public void TestInsCompare1()
        {
            CommandStart.Start(doc =>
            {
                file1 = null;
                file2 = null;
                LoadService.LoadEntityFramework();
                LoadService.LoadMDM();

                // Выбор файлов для сравнения
                SelectFile();

                // Расчет инсоляции - file2
                CalcFile();                                

                var insComparer = new InsCompare(file1, file2);
                insComparer.Compare();
            });
        }

        private void CalcFile()
        {
            file2 = Path.Combine(Path.GetDirectoryName(file1), Path.GetFileNameWithoutExtension(file1) + "_Calc.dwg");
            if (!File.Exists(file2))
            {
                File.Copy(file1, file2);
            }            
            var insCalc = new InsCalc(file2);
            insCalc.Calc();
        }

        private void SelectFile()
        {
            var dlg = new OpenFileDialog("Выбор файла с правильной инсоляцией для проверки", "", "dwg","", OpenFileDialog.OpenFileDialogFlags.NoUrls);
            if (dlg.ShowDialog() !=  System.Windows.Forms.DialogResult.OK)
            {
                throw new CancelByUserException();
            }            
            file1 = dlg.Filename;
            //var files = dlg.GetFilenames();
            //if (file1 != null)
            //{
            //    file2 = files[0];
            //    return;
            //}
            //file1 = files[0];
            //if (files.Length > 1)
            //{
            //    file2 = files[1];
            //}
            //else
            //{
            //    SelectFiles();
            //}                 
        }
    }
}
