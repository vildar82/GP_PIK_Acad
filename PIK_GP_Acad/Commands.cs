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
using PIK_GP_Acad.Properties;

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

        public void InitCommands()
        {            
            CommandsPalette = new List<IPaletteCommand>()
            {
                // Главная
                new PaletteCommand("Блок линии парковки",Resources.GP_LineParking,nameof(GP_InsertBlockLineParking),"Вставка блока линии парковки"),
                new PaletteCommand("Блок парковки",Resources.GP_Parking,nameof(GP_InsertBlockParking),"Вставка блока парковки"),
                new PaletteCommand("Спецификация линейных парковок",Resources.GP_ParkingTable,nameof(GP_ParkingCalc),"Выбор блоков парковок (2 вида блока) и вставка текста машиномест или таблицы всех блоков в Модели."),
                new PaletteCommand("Бергштрих",Resources.GP_Isoline, nameof(GP_Isoline), "Включение одиночных бергштрихов для линий и полилиний."),
                new PaletteCommand("Уровни горизонталей",Resources.GP_HorizontalElevation, nameof(GP_HorizontalElevationStep), "Установка уровней для полилиний горизонталей с заданным шагом."),
                new PaletteCommand("Линия со стрелками",Resources.GP_LineArrow, nameof(GP_PolylineArrow), "Рисование полилинии с типом линии 'ГП-Стрелка3'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Линия направления движения",Resources.GP_LineDirMove, nameof(GP_PolylineDirMove), "Рисование полилинии с типом линии 'ГП-НапрДвижения'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),                
                new PaletteCommand("Линия с крестиками",Resources.GP_LineCross, nameof(GP_PolylineCross), "Рисование полилинии с типом линии 'ГП-крест'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("ArcGIS",Resources.ArcGIS, nameof(GP_ArcGIS), "Запуск программы ArcGis"),
                // БС
                new PaletteCommand("Блоки Блок-Секций", Resources.GP_BlockSectionInsert,nameof(GP_BlockSectionInsert),"Вставка блока Блок-Секции из списка.", GroupBS),
                new PaletteCommand("Спецификация Блок-Секций",Resources.GP_BlockSectionTable, nameof(GP_BlockSectionTable), "Вставка таблицы расчета выбранных блоков Блок-Секций.", GroupBS ),
                new PaletteCommand("Контур Блок-Секций",Resources.GP_BlockSectionContour, nameof(GP_BlockSectionContour), "Создание полилинии контура вокруг блоков Блок-Секций", GroupBS),
                // Концепция
                new PaletteCommand("Блок блок-секции",Resources.GP_KP_BlockSectionInsert, nameof(KP_BlockSectionInsert), "Вставка блока блок-секции из списка. Раздел концепции.", GroupKP),
                new PaletteCommand("Спецификация блок-секций",Resources.GP_KP_BlockSectionTable, nameof(KP_BlockSectionTable), "Таблица подсчета блок-секции концепции.", GroupKP),
                new PaletteCommand("Спецификация блок-секций PIK1",Resources.GP_BlockSectionTable, nameof(KP_BlockSectionTableFromGP), "Таблица подсчета блок-секции PIK1.", GroupKP),                
                new PaletteCommand("Блок ДОО",Resources.KP_DOO, nameof(KP_BlockDOOInsert), "Вставка блока детского сада (ДОО).", GroupKP),
                new PaletteCommand("Блок СОШ",Resources.KP_School, nameof(KP_BlockSchoolInsert), "Вставка блока школы (СОШ).", GroupKP),
                new PaletteCommand("Расчет свободной парковки", Resources.KP_KP_AreaParking, nameof(KP_AreaParking), "Расчет машиномест свободной парковки", GroupKP),
                // Штамп
                new PaletteCommand("Рамка.",Resources.GP_KP_BlockFrame, nameof(GP_BlockFrame), "Вставка блока рамки.", GroupStamp),
                new PaletteCommand("Штамп. Основной комплект.",Resources.GP_KP_BlockStampForm3, nameof(GP_BlockStampForm3), "Вставка блока штампа по форме 3 - Основной комплект.", GroupStamp),
                new PaletteCommand("Рамка для буклета.",Resources.GP_KP_BlockStampBooklet, nameof(GP_BlockStampBooklet), "Вставка блока рамки буклета.", GroupStamp)
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


        //
        // Главная
        //

        [CommandMethod(Group, nameof(GP_InsertBlockLineParking), CommandFlags.Modal)]
        public void GP_InsertBlockLineParking()
        {
            CommandStart.Start(doc =>
            {
                List<AcadLib.Blocks.Property> props = new List<AcadLib.Blocks.Property>
                {
                    new AcadLib.Blocks.Property ("Длина", 15d)
                };
                InsertBlock.Insert(Parkings.LineParking.LineParkingBlockName, doc, props);
            });
        }

        [CommandMethod(Group, nameof(GP_InsertBlockParking), CommandFlags.Modal)]
        public void GP_InsertBlockParking()
        {
            CommandStart.Start(doc =>
            {                
                InsertBlock.Insert(Parkings.Parking.ParkingBlockName, doc);
            });
        }

        [CommandMethod(Group, nameof(GP_ParkingCalc), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void GP_ParkingCalc()
        {
            CommandStart.Start(doc =>
            {
                Parkings.ParkingService ps = new Parkings.ParkingService();
                ps.CalcAndTable();
            });
        }

        [CommandMethod(Group, nameof(GP_Isoline), CommandFlags.Modal)]
        public void GP_Isoline()
        {
            CommandStart.Start(doc =>
            {
                Isolines.Isoline.Start();
            });
        }

        [CommandMethod(Group, nameof(GP_HorizontalElevationStep), CommandFlags.Modal)]
        public void GP_HorizontalElevationStep()
        {
            CommandStart.Start(doc =>
            {
                HorizontalElevation horElev = new HorizontalElevation();
                horElev.Stepping();
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineArrow), CommandFlags.Modal)]
        public void GP_PolylineArrow()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Стрелка", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Стрелка");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineDirMove), CommandFlags.Modal)]
        public void GP_PolylineDirMove()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-НапрДвижения", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-НапрДвижения");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineCross), CommandFlags.Modal)]
        public void GP_PolylineCross()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Крест", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Крест");
            });
        }

        [CommandMethod(Group, nameof(GP_ArcGIS), CommandFlags.Modal)]
        public void GP_ArcGIS()
        {
            CommandStart.Start(doc =>
            {
                ArcGIS.ArcGisService.Start();
            });
        }


        //
        // БС
        //

        [CommandMethod(Group, nameof(GP_BlockSectionInsert), CommandFlags.Modal)]        
        public void GP_BlockSectionInsert()
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
        
        [CommandMethod(Group, nameof(GP_BlockSectionTable), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]        
        public void GP_BlockSectionTable()
        {
            CommandStart.Start(doc =>
            {                
                BlockSection.SectionService ss = new BlockSection.SectionService(doc);
                ss.CalcSections();                
            });            
        }
        
        [CommandMethod(Group, nameof(GP_BlockSectionContour), CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]        
        public void GP_BlockSectionContour()
        {
            CommandStart.Start(doc =>
            {                
                BlockSection.BlockSectionContours.CreateContour(doc);                
            });               
        }


        //
        // Концепция
        //

        [CommandMethod(Group, nameof(KP_BlockSectionInsert), CommandFlags.Modal)]
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

        [CommandMethod(Group, nameof(KP_BlockSectionTable), CommandFlags.Modal)]
        public void KP_BlockSectionTable()
        {
            CommandStart.Start(doc =>
            {                
                KP.KP_BlockSection.KP_BlockSectionService.CreateTable();                
            });
        }

        [CommandMethod(Group, nameof(KP_BlockSectionTableFromGP), CommandFlags.Modal)]
        public void KP_BlockSectionTableFromGP()
        {
            CommandStart.Start(doc =>
            {
                BlockSection.SectionService ss = new BlockSection.SectionService(doc);
                ss.CalcSectionsForKP();
            });
        }

        [CommandMethod(Group, nameof(KP_BlockDOOInsert), CommandFlags.Modal)]
        public void KP_BlockDOOInsert()
        {
            CommandStart.Start(doc =>
            {                
                InsertBlock.Insert("КП_ДОО", doc);
            });
        }

        [CommandMethod(Group, nameof(KP_BlockSchoolInsert), CommandFlags.Modal)]
        public void KP_BlockSchoolInsert()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("КП_СОШ", doc);
            });
        }

        [CommandMethod(Group, nameof(KP_AreaParking), CommandFlags.Modal)]
        public void KP_AreaParking()
        {
            CommandStart.Start(doc =>
            {
                KP.Parking.AreaParkingService aps = new KP.Parking.AreaParkingService();
                aps.Calc();
            });
        }


        //
        // Штамп
        //

        [CommandMethod(Group, nameof(GP_BlockFrame), CommandFlags.Modal)]
        public void GP_BlockFrame()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Рамка-ПИК", doc);
            });
        }        

        [CommandMethod(Group, nameof(GP_BlockStampForm3), CommandFlags.Modal)]
        public void GP_BlockStampForm3()
        {
            CommandStart.Start(doc =>
            {
                InsertBlock.Insert("ГП_Штамп_Форма3_ПИК", doc);
            });
        }

        [CommandMethod(Group, nameof(GP_BlockStampBooklet), CommandFlags.Modal)]
        public void GP_BlockStampBooklet()
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

            // Загрузка сборки Civil
            string fileCivilDll = Path.Combine(CurDllDir, "PIK_GP_Civil.dll");
            if (File.Exists(fileCivilDll))
            {
                try
                {
                    Assembly.LoadFrom(fileCivilDll);
                }
                catch { }
            }
        }
        
        public void Terminate()
        {
            
        }
    }        
}