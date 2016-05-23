using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
[assembly: ExtensionApplication(typeof(PIK_GP_Acad.Commands))]

namespace PIK_GP_Acad
{
    public class Commands: IExtensionApplication
    {
        public static List<IPaletteCommand> CommandsPalette { get; set; }
        public static string CurDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public const string Group = AutoCAD_PIK_Manager.Commands.Group;
        public const string GroupBS = "БС";
        public const string GroupKP = "Концепция";
        public const string GroupStamp = "Штамп";
        // Комманды
        private const string CommandBlockSectionInsert = "GP_BlockSectionInsert";
        private const string CommandBlockSectionTable = "GP_BlockSectionTable";
        private const string CommandBlockSectionContour = "GP_BlockSectionContour";
        private const string CommandInsertBlockParking = "GP_InsertBlockParking";
        private const string CommandIsoline = "GP_Isoline";
        private const string CommandHorizontalElevationStep = "GP_HorizontalElevationStep";
        private const string CommandPolylineArrow = "GP_PolylineArrow";
        private const string CommandPolylineCross = "GP_PolylineCross";
        private const string Command_KP_BlockSectionTable = "GP_KP_BlockSectionTable";
        private const string Command_KP_BlockSectionInsert = "GP_KP_BlockSectionInsert";
        private const string Command_KP_BlockFrame = "GP_KP_BlockFrame";
        private const string Command_KP_BlockStampForm3 = "GP_KP_BlockStampForm3";
        private const string Command_KP_BlockStampBooklet = "GP_KP_BlockStampBooklet";

        public void InitCommands()
        {
            CommandsPalette = new List<IPaletteCommand>()
            {
                new PaletteCommand("Блоки Блок-Секций", Properties.Resources.GP_BlockSectionInsert,CommandBlockSectionInsert,"Вставка блока Блок-Секции из списка.", GroupBS),
                new PaletteCommand("Спецификация Блок-Секций",Properties.Resources.GP_BlockSectionTable, CommandBlockSectionTable, "Вставка таблицы расчета выбранных блоков Блок-Секций.", GroupBS ),
                new PaletteCommand("Контур Блок-Секций",Properties.Resources.GP_BlockSectionContour, CommandBlockSectionContour, "Создание полилинии контура вокруг блоков Блок-Секций", GroupBS),
                new PaletteCommand("Блок парковки",Properties.Resources.GP_InsertBlockParking,CommandInsertBlockParking,"Вставка блока парковки"),
                new PaletteCommand("Бергштрих",Properties.Resources.GP_Isoline, CommandIsoline, "Включение одиночных бергштрихов для линий и полилиний."),
                new PaletteCommand("Уровни горизонталей",Properties.Resources.GP_HorizontalElevationStep, CommandHorizontalElevationStep, "Установка уровней для полилиний горизонталей с заданным шагом."),
                new PaletteCommand("Линия со стрелками",Properties.Resources.GP_PolylineArrow, CommandPolylineArrow, "Рисование полилинии с типом линии 'ГП-Стрелка3'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Линия с крестиками",Properties.Resources.GP_LineCross, CommandPolylineCross, "Рисование полилинии с типом линии 'ГП-крест'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Вставка блока блок-секции. Раздел Концепция.",Properties.Resources.GP_KP_BlockSectionInsert, Command_KP_BlockSectionInsert, "Вставка блока блок-секции из списка. Раздел концепции.", GroupKP),
                new PaletteCommand("Спецификация блок-секций. Раздел Концепция.",Properties.Resources.GP_KP_BlockSectionTable, Command_KP_BlockSectionTable, "Таблица подсчета блок-секции концепции.", GroupKP),
                new PaletteCommand("Рамка.",Properties.Resources.GP_KP_BlockFrame, Command_KP_BlockFrame, "Вставка блока рамки.", GroupStamp),
                new PaletteCommand("Штамп. Форма 3. Основной комплект.",Properties.Resources.GP_KP_BlockStampForm3, Command_KP_BlockStampForm3, "Вставка блока штампа по форме 3 - Основной комплект.", GroupStamp),
                new PaletteCommand("Рамка для буклета.",Properties.Resources.GP_KP_BlockStampBooklet, Command_KP_BlockStampBooklet, "Вставка блока рамки буклета.", GroupStamp),
            };
        }

        [CommandMethod(Group, "PIK_Start", CommandFlags.Modal)]
        public void PaletteStart()
        {
            CommandStart.Start(doc =>
            {
                PaletteSetCommands.Start();
            });
        }

        [CommandMethod(Group, CommandBlockSectionInsert, CommandFlags.Modal)]        
        public void BlockSectionInsert()
        {
            CommandStart.Start(doc =>
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
        public void BlockSectionTable()
        {
            CommandStart.Start(doc =>
            {                
                BlockSection.SectionService ss = new BlockSection.SectionService(doc);
                ss.CalcSections();                
            });            
        }

        /// <summary>
        /// Добавление полилиний контуров у блоков Блок-Секций
        /// </summary>
        [CommandMethod(Group, CommandBlockSectionContour, CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]        
        public void BlockSectionContour()
        {
            CommandStart.Start(doc =>
            {                
                BlockSection.BlockSectionContours.CreateContour(doc);                
            });               
        }        

        [CommandMethod(Group, CommandInsertBlockParking, CommandFlags.Modal)]        
        public void InsertBlockParking()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Линия-Парковки", doc);
            });            
        }        

        [CommandMethod(Group, CommandIsoline, CommandFlags.Modal)]        
        public void Isoline()
        {
            CommandStart.Start(doc =>
            {
                Isolines.Isoline.Start();
            });            
        }

        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>        
        [CommandMethod(Group, CommandHorizontalElevationStep, CommandFlags.Modal)]
        public void HorizontalElevationStep()
        {
            CommandStart.Start(doc =>
            {                
                HorizontalElevation horElev = new HorizontalElevation();
                horElev.Stepping();                
            });
        }

        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>        
        [CommandMethod(Group, CommandPolylineArrow, CommandFlags.Modal)]
        public void PolylineArrow()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-стрелка3", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-стрелка3");
            });
        }

        /// <summary>
        /// Концепция (КП). Подсчет таблицы блок-секций
        /// </summary>        
        [CommandMethod(Group, Command_KP_BlockSectionTable, CommandFlags.Modal)]
        public void KP_BlockSectionTable()
        {
            CommandStart.Start(doc =>
            {                
                KP.KP_BlockSection.KP_BlockSectionService.CreateTable();                
            });
        }

        [CommandMethod(Group, Command_KP_BlockSectionInsert, CommandFlags.Modal)]
        public void KP_BlockSectionInsert()
        {
            CommandStart.Start(doc =>
            {
                // Файл шаблонов
                string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder,
                                                @"Blocks\ГП\ГП_Блоки.dwg");
                // Выбор и вставка блока 
                AcadLib.Blocks.Visual.VisualInsertBlock.InsertBlock(fileBlocks, n =>
                    KP.KP_BlockSection.KP_BlockSectionService.IsBlockSection(n));
            });
        }

        [CommandMethod(Group, Command_KP_BlockFrame, CommandFlags.Modal)]
        public void KP_BlockFrame()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Рамка-ПИК", doc);
            });
        }

        [CommandMethod(Group, Command_KP_BlockStampForm3, CommandFlags.Modal)]
        public void KP_BlockStampForm3()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Штамп_Форма3_ПИК", doc);
            });
        }

        [CommandMethod(Group, Command_KP_BlockStampBooklet, CommandFlags.Modal)]
        public void KP_BlockStampBooklet()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Рамка_Буклет", doc);
            });
        }

        public void Initialize()
        {
            // Передача списка команд для палитры ПИК в AcadLib.  
            InitCommands();          
            PaletteSetCommands.InitPalette(CommandsPalette);
        }

        public void Terminate()
        {            
        }
    }
}