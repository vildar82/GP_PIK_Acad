using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AcadLib;
using AcadLib.Errors;
using AcadLib.PaletteCommands;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using PIK_GP_Acad.Model.HorizontalElevation;

[assembly: CommandClass(typeof(PIK_GP_Acad.Commands))]

namespace PIK_GP_Acad
{
    public class Commands
    {
        public static string CurDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>
        [PaletteCommand("Уровни горизонталей", "Установка уровней для полилиний горизонталей с заданным шагом.")]
        [CommandMethod("PIK", "GP_HorizontalElevationStep", CommandFlags.Modal)]
        public void HorizontalElevationStep()
        {
            CommandStart.Start("GP_HorizontalElevationStep", doc=>
            {
                Inspector.Clear();
                HorizontalElevation horElev = new HorizontalElevation();
                horElev.Stepping();                
                Inspector.Show();                
            });

            //Logger.Log.StartCommand("GP_HorizontalElevationStep");
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //if (doc == null) return;
            //try
            //{
            //    Inspector.Clear();
            //    HorizontalElevation horElev = new HorizontalElevation();
            //    horElev.Stepping();

            //    if (Inspector.HasErrors)
            //    {
            //        Inspector.Show();
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    if (!ex.Message.Contains(General.CanceledByUser))
            //    {
            //        Logger.Log.Error(ex, "GP_HorizontalElevationStep");
            //    }
            //    doc.Editor.WriteMessage(ex.Message);
            //}
        }

        /// <summary>
        /// Спецификация блок-секций
        /// </summary>
        [CommandMethod("PIK", "GP_BlockSectionTable", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        [PaletteCommand("Спецификация Блок-Секций", "Вставка таблицы расчета выбранных блоков Блок-Секций.")]
        public void BlockSectionTable()
        {
            Logger.Log.StartCommand("GP_BlockSectionTable");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            try
            {
                BlockSection.SectionService ss = new BlockSection.SectionService(doc);
                ss.CalcSections();
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage("\n" + ex.Message);
            }
        }

        /// <summary>
        /// Добавление полилиний контуров у блоков Блок-Секций
        /// </summary>
        [CommandMethod("PIK", "GP_BlockSectionContour", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        [PaletteCommand("Контур Блок-Секций", "Создание полилинии контура вокруг блоков Блок-Секций")]
        public void BlockSectionContour()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.StartCommand("GP_BlockSectionContour");
            try
            {
                Inspector.Clear();
                BlockSection.BlockSectionContours.CreateContour(doc);
                Inspector.Show();
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage("\n" + ex.Message);
            }
        }

        [CommandMethod("PIK", "GP_BlockSectionInsert", CommandFlags.Modal)]
        [PaletteCommand("Блоки Блок-Секций", "Вставка блока Блок-Секции из списка.")]
        public void BlockSectionInsert()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.StartCommand("GP_BlockSectionInsert");
            try
            {
                // Файл шаблонов
                string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Blocks\ГП\ГП_Блоки.dwg");                
                // Выбор и вставка блока 
                AcadLib.Blocks.Visual.VisualInsertBlock.InsertBlock(fileBlocks, n => n.StartsWith(BlockSection.Settings.Default.BlockSectionPrefix));
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains("Отменено пользователем"))
                {
                    Logger.Log.Error(ex, $"Command: GP-BlockSectionInsert. {doc.Name}");
                }
            }
        }

        [CommandMethod("PIK", "GP_InsertBlockParking", CommandFlags.Modal)]
        [PaletteCommand("Блок парковки", "Вставка блока парковки")]
        public void InsertBlockParking()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.StartCommand("GP_InsertBlockParking");
            try
            {                
                InsertBlock.Insert("ГП_Линия-Парковки", doc);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP-InsertBlockParking. {doc.Name}");
                }
            }
        }

        [CommandMethod("PIK", "GP_InsertBlockPikLogo", CommandFlags.Modal)]
        [PaletteCommand("Блок логотипа","Вставка блока логотипа ПИК")]
        public void InsertBlockPikLogo()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.StartCommand("GP_InsertBlockPikLogo");
            try
            {
                InsertBlock.Insert("PIK_Logo", doc);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP_InsertBlockPikLogo. {doc.Name}");
                }
            }
        }

        [CommandMethod("PIK", "GP_Isoline", CommandFlags.Modal)]
        [PaletteCommand("Бергштрих", "Включение одиночных бергштрихов для линий и полилиний.")]
        public void GpIsoline()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.Info("Start Command: GP_Isoline");
            try
            {
                Isolines.Isoline.Start();
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP_Isoline. {doc.Name}");
                }
            }
        }                        
    }
}