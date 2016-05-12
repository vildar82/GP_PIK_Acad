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
        public const string Group = AutoCAD_PIK_Manager.Commands.Group;
        public const string GroupBS = "БС";
        // Комманды
        private const string CommandBlockSectionInsert = "GP_BlockSectionInsert";
        private const string CommandBlockSectionTable = "GP_BlockSectionTable";
        private const string CommandBlockSectionContour = "GP_BlockSectionContour";
        private const string CommandInsertBlockParking = "GP_InsertBlockParking";
        private const string CommandIsoline = "GP_Isoline";
        private const string CommandHorizontalElevationStep = "GP_HorizontalElevationStep";
        private const string CommandPolylineArrow = "GP_PolylineArrow";
        

        [CommandMethod(Group, CommandBlockSectionInsert, CommandFlags.Modal)]
        [PaletteCommand("Блоки Блок-Секций", "Вставка блока Блок-Секции из списка.", GroupBS)]
        public void BlockSectionInsert()
        {
            CommandStart.Start(CommandBlockSectionInsert, doc =>
            {
                // Файл шаблонов
                string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder,
                                                @"Blocks\ГП\ГП_Блоки.dwg");
                // Выбор и вставка блока 
                AcadLib.Blocks.Visual.VisualInsertBlock.InsertBlock(fileBlocks, n =>
                        n.StartsWith(BlockSection.Settings.Default.BlockSectionPrefix));
            });
        }

        /// <summary>
        /// Спецификация блок-секций
        /// </summary>
        [CommandMethod(Group, CommandBlockSectionTable, CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        [PaletteCommand("Спецификация Блок-Секций", "Вставка таблицы расчета выбранных блоков Блок-Секций.", GroupBS)]
        public void BlockSectionTable()
        {
            CommandStart.Start(CommandBlockSectionTable, doc =>
            {
                Inspector.Clear();
                BlockSection.SectionService ss = new BlockSection.SectionService(doc);
                ss.CalcSections();
                Inspector.Show();
            });            
        }

        /// <summary>
        /// Добавление полилиний контуров у блоков Блок-Секций
        /// </summary>
        [CommandMethod(Group, CommandBlockSectionContour, CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        [PaletteCommand("Контур Блок-Секций", "Создание полилинии контура вокруг блоков Блок-Секций", GroupBS)]
        public void BlockSectionContour()
        {
            CommandStart.Start(CommandBlockSectionContour, doc =>
            {
                Inspector.Clear();
                BlockSection.BlockSectionContours.CreateContour(doc);
                Inspector.Show();
            });               
        }        

        [CommandMethod(Group, CommandInsertBlockParking, CommandFlags.Modal)]
        [PaletteCommand("Блок парковки", "Вставка блока парковки")]
        public void InsertBlockParking()
        {
            CommandStart.Start(CommandInsertBlockParking, doc =>
            {
                InsertBlock.Insert("ГП_Линия-Парковки", doc);
            });            
        }        

        [CommandMethod(Group, CommandIsoline, CommandFlags.Modal)]
        [PaletteCommand("Бергштрих", "Включение одиночных бергштрихов для линий и полилиний.")]
        public void Isoline()
        {
            CommandStart.Start(CommandIsoline, doc =>
            {
                Isolines.Isoline.Start();
            });            
        }

        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>
        [PaletteCommand("Уровни горизонталей", "Установка уровней для полилиний горизонталей с заданным шагом.")]
        [CommandMethod(Group, CommandHorizontalElevationStep, CommandFlags.Modal)]
        public void HorizontalElevationStep()
        {
            CommandStart.Start(CommandHorizontalElevationStep, doc =>
            {
                Inspector.Clear();
                HorizontalElevation horElev = new HorizontalElevation();
                horElev.Stepping();
                Inspector.Show();
            });
        }

        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>
        [PaletteCommand("Линия со стрелками", "Рисование полилинии с типом линии 'ГП-Стрелка3'. " +
                        "Внимание: в типе линии используется форма из файла acadtopo.shx. " +
                        "При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл.")]
        [CommandMethod(Group, CommandPolylineArrow, CommandFlags.Modal)]
        public void PolylineArrow()
        {
            CommandStart.Start(CommandPolylineArrow, doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-стрелка3", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-стрелка3");
            });
        }
    }
}