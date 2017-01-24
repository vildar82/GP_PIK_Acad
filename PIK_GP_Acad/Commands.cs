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
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.HorizontalElevation;
using PIK_GP_Acad.Properties;
using AcadLib.Blocks;

[assembly: CommandClass(typeof(PIK_GP_Acad.Commands))]
[assembly: ExtensionApplication(typeof(PIK_GP_Acad.Commands))]

namespace PIK_GP_Acad
{
    public class Commands: IExtensionApplication
    {
        static string fileBlocks = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.LocalSettingsFolder, @"Blocks\ГП\ГП_Блоки.dwg");
        /// <summary>
        /// Ответственные пользователи. Изменение настроек, тестирование, и т.п.
        /// </summary>
        public static readonly List<string> ResponsibleUsers = new List<string>() { "PrudnikovVS", AutoCAD_PIK_Manager.Env.CadManLogin };
        
        public static List<IPaletteCommand> CommandsPalette { get; set; }
        public static string CurDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public const string Group = AutoCAD_PIK_Manager.Commands.Group;
        public const string GroupBS = "БС";
        public const string GroupKP = "Концепция";
        public const string GroupCommon = "Общие";

        private static bool isLoadedEF;

        public void InitCommands()
        {            
            CommandsPalette = new List<IPaletteCommand>()
            {
                // Главная
                new PaletteInsertBlock(Elements.Blocks.Parkings.LineParking.BlockName, fileBlocks,"Блок линии парковки",Resources.GP_LineParking,"Вставка блока линии парковки", "", new List<Property>{new Property ("Длина", 15d)}),
                new PaletteInsertBlock(Elements.Blocks.Parkings.Parking.BlockName,fileBlocks, "Блок парковки",Resources.GP_Parking,"Вставка блока парковки"),
                new PaletteCommand("Спецификация парковок",Resources.GP_ParkingTable,nameof(GP_ParkingCalc),"Выбор блоков парковок и вставка текста машиномест или таблицы всех блоков в Модели."),
                new PaletteCommand("Бергштрих",Resources.GP_Isoline, nameof(GP_Isoline), "Включение одиночных бергштрихов для линий и полилиний."),
                new PaletteCommand("Уровни горизонталей",Resources.GP_HorizontalElevation, nameof(GP_HorizontalElevationStep), "Установка уровней для полилиний горизонталей с заданным шагом."),
                new PaletteInsertBlock("ГП_Дождеприемная решетка",fileBlocks, "Дождеприемная решетка",Resources.GP_RainGrid,"Вставка блока дождеприемной решетки"),
                new PaletteCommand("Линия со стрелками",Resources.GP_LineArrow, nameof(GP_PolylineArrow), "Рисование полилинии с типом линии 'ГП-Стрелка3'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Линия направления движения",Resources.GP_LineDirMove, nameof(GP_PolylineDirMove), "Рисование полилинии с типом линии 'ГП-НапрДвижения'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),                
                new PaletteCommand("Линия с крестиками",Resources.GP_LineCross, nameof(GP_PolylineCross), "Рисование полилинии с типом линии 'ГП-крест'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Прибрежная полоса",Resources.GP_LineRiverside, nameof(GP_PolylineRiverside), "Рисование полилинии с типом линии 'ГП-Прибрежная'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Водоохранная зона",Resources.GP_LineWaterProtectZone, nameof(GP_PolylineWaterProtectZone), "Рисование полилинии с типом линии 'ГП-ВОДООХРАННАЯ'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Шумовое заграждение 1",Resources.GP_LineNoizeBarrier1, nameof(GP_PolylineNoizeBarrier1), "Рисование полилинии с типом линии 'ГП-Шумовое_ограждение_1'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Шумовое заграждение 2",Resources.GP_LineNoizeBarrier2, nameof(GP_PolylineNoizeBarrier2), "Рисование полилинии с типом линии 'ГП-Шумовое_ограждение_2'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("Шумовое заграждение 3",Resources.GP_LineNoizeBarrier2, nameof(GP_PolylineNoizeBarrier3), "Рисование полилинии с типом линии 'ГП-Шумовое_ограждение_3'. Внимание: в типе линии используется форма из файла acadtopo.shx. При передаче файла с таким типом линии вне ПИК, необходимо передавать этот файл."),
                new PaletteCommand("ArcGIS",Resources.ArcGIS, nameof(GP_ArcGIS), "Запуск программы ArcGis"),
                new PaletteCommand("Enla",Resources.enla, nameof(GP_Enla), "Подсчет длин и площадей."),
                new PaletteCommand("Инсоляция",Resources.Sun.ToBitmap(), nameof(GP_InsolationService), "Расчет инсоляции."),
                // БС
                new PaletteCommand("Блоки Блок-Секций", Resources.GP_BlockSectionInsert,nameof(GP_BlockSectionInsert),"Вставка блока Блок-Секции из списка (Разработаны Петровым С.).", GroupBS),
                new PaletteCommand("Спецификация Блок-Секций",Resources.GP_BlockSectionTable, nameof(GP_BlockSectionTable), "Вставка таблицы расчета выбранных блоков Блок-Секций.", GroupBS ),
                new PaletteCommand("Контур Блок-Секций",Resources.GP_BlockSectionContour, nameof(GP_BlockSectionContour), "Создание полилинии контура вокруг блоков Блок-Секций", GroupBS),
                // Концепция
                new PaletteCommand("Блоки Блок-секций",Resources.GP_KP_BlockSectionInsert, nameof(KP_BlockSectionInsert), "Вставка блока блок-секции из списка. Раздел концепции.", GroupKP),
                //new PaletteCommand("Спецификация блок-секций",Resources.GP_KP_BlockSectionTable, nameof(KP_BlockSectionTable), "Расчет ТЭП для неутвержденной стадии.", GroupKP),
                new PaletteCommand("Спецификация блок-секций (новая)",Resources.GP_KP_BlockSectionTableNew, nameof(KP_BlockSectionTableNew), "Расчет ТЭП для неутвержденной стадии (новый).", GroupKP),
                new PaletteCommand("Заливка блок-секций",Resources.GP_KP_BlockSectionFill, nameof(KP_BlockSectionFill), "Заливка контуров блок-секций сплошной штриховкой.", GroupKP),
                new PaletteCommand("Спецификация блок-секций PIK1",Resources.GP_BlockSectionTable, nameof(KP_BlockSectionTableFromGP), "Таблица подсчета блок-секции PIK1.", GroupKP),                
                new PaletteInsertBlock(Elements.Blocks.Social.KindergartenBlock.BlockName, fileBlocks, "Блок ДОО",Resources.KP_DOO, "Вставка блока детского сада (ДОО).", GroupKP),
                new PaletteInsertBlock(Elements.Blocks.Social.SchoolBlock.BlockName, fileBlocks,"Блок СОШ",Resources.KP_School, "Вставка блока школы (СОШ).", GroupKP),
                new PaletteInsertBlock(Elements.Blocks.Parkings.ParkingBuilding.BlockName, fileBlocks, "Блок паркинга",Resources.KP_Parking, "Вставка блока паркинга.", GroupKP),
                new PaletteCommand("Расчет свободной парковки", Resources.KP_KP_AreaParking, nameof(KP_AreaParking), "Расчет машиномест свободной парковки", GroupKP),
                // Общие - штамп
                new PaletteInsertBlock("ГП_Рамка-ПИК", fileBlocks, "Рамка.",Resources.GP_KP_BlockFrame, "Вставка блока рамки.", GroupCommon),
                new PaletteInsertBlock("ГП_Штамп_Форма3_ПИК", fileBlocks,"Штамп. Основной комплект.",Resources.GP_KP_BlockStampForm3, "Вставка блока штампа по форме 3 - Основной комплект.", GroupCommon),
                new PaletteInsertBlock("ГП_Рамка_Буклет",fileBlocks,"Рамка для буклета.",Resources.GP_KP_BlockStampBooklet, "Вставка блока рамки буклета.", GroupCommon)
            };
        }

        [CommandMethod(Group, nameof(PIK_Start), CommandFlags.Modal)]
        public void PIK_Start()
        {
            try
            {
                PaletteSetCommands.Start();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                var horElev = new HorizontalElevationService();
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

        [CommandMethod(Group, nameof(GP_PolylineRiverside), CommandFlags.Modal)]
        public void GP_PolylineRiverside ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Прибрежная", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Прибрежная");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineWaterProtectZone), CommandFlags.Modal)]
        public void GP_PolylineWaterProtectZone ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-ВОДООХРАННАЯ", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-ВОДООХРАННАЯ");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineNoizeBarrier1), CommandFlags.Modal)]
        public void GP_PolylineNoizeBarrier1 ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Шумовое_ограждение_1", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Шумовое_ограждение_1");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineNoizeBarrier2), CommandFlags.Modal)]
        public void GP_PolylineNoizeBarrier2 ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Шумовое_ограждение_2", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Шумовое_ограждение_2");
            });
        }

        [CommandMethod(Group, nameof(GP_PolylineNoizeBarrier3), CommandFlags.Modal)]
        public void GP_PolylineNoizeBarrier3 ()
        {
            CommandStart.Start(doc =>
            {
                Database db = doc.Database;
                db.LoadLineTypePIK("ГП-Шумовое_ограждение_3", "acadtopo.lin");
                Draw.Polyline(lineType: "ГП-Шумовое_ограждение_3");
            });
        }

        [CommandMethod(Group, nameof(GP_ArcGIS), CommandFlags.Modal)]
        public void GP_ArcGIS ()
        {
            CommandStart.Start(doc =>
            {
                ArcGIS.ArcGisService.Start();
            });
        }

        [CommandMethod(Group, nameof(GP_Enla), CommandFlags.Modal)]
        public void GP_Enla ()
        {
            CommandStart.Start(doc =>
            {
                // Загрузка программы подсчета суммы длин и площадей
                var enlaDll = Path.Combine(CurDllDir, @"enla\enla-free.dll");
                if (File.Exists(enlaDll))
                {
                    Assembly.LoadFrom(enlaDll);
                    doc.Editor.Command("enla");
                }                
            });
        }        

        [CommandMethod(Group, nameof(GP_BlockSectionInsert), CommandFlags.Modal)]        
        public void GP_BlockSectionInsert()
        {
            CommandStart.Start(doc =>
            {                
                // Выбор и вставка блока 
                AcadLib.Blocks.Visual.VisualInsertBlock.InsertBlock(fileBlocks, n =>
                        n.StartsWith(BlockSection.SettingsBS.Default.BlockSectionPrefix));
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
        public void GP_BlockSectionContour ()
        {
            CommandStart.Start(doc =>
            {
                BlockSection.BlockSectionContours.CreateContour(doc);
            });
        }

        [CommandMethod(Group, nameof(KP_BlockSectionInsert), CommandFlags.Modal)]
        public void KP_BlockSectionInsert()
        {
            CommandStart.Start(doc =>
            {                
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
                KP.KP_BlockSection.KP_BlockSectionService.CreateTable(false);                
            });            
        }
        [CommandMethod(Group, nameof(KP_BlockSectionTableNew), CommandFlags.Modal)]
        public void KP_BlockSectionTableNew ()
        {
            CommandStart.Start(doc =>
            {
                KP.KP_BlockSection.KP_BlockSectionService.CreateTable(true);
            });
        }
        [CommandMethod(Group, nameof(KP_BlockSectionFill), CommandFlags.Modal)]
        public void KP_BlockSectionFill ()
        {
            CommandStart.Start(doc =>
            {
                KP.KP_BlockSection.KP_BlockSectionService.Fill();
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

        [CommandMethod(Group, nameof(KP_AreaParking), CommandFlags.Modal)]
        public void KP_AreaParking ()
        {
            CommandStart.Start(doc =>
            {
                KP.Parking.Area.AreaParkingService aps = new KP.Parking.Area.AreaParkingService();
                aps.Calc();
            });
        }

        [CommandMethod(Group, nameof(GP_FCS_Balance), CommandFlags.Modal)]
        public static void GP_FCS_Balance ()
        {
            CommandStart.Start(doc =>
            {
                FCS.FCSTable tep = new FCS.FCSTable(doc,
                    new FCS.Balance.BalanceTableService(doc.Database),
                    new FCS.Balance.BalanceClassService());
                tep.Calc();
            });
        }

        [CommandMethod(Group, nameof(GP_InsolationService), CommandFlags.Modal)]
        public void GP_InsolationService ()
        {
            CommandStart.Start(doc =>
            {                
                if (!isLoadedEF)
                {
                    LoadService.LoadEntityFramework();
                    isLoadedEF = true;
                }
                Insolation.Services.InsService.StartInsolationPalette(doc);
            });
        }

        [CommandMethod(Group, nameof(GP_CheckBuildingsIntersects), CommandFlags.Modal)]
        public void GP_CheckBuildingsIntersects()
        {
            CommandStart.Start(doc =>
            {
                Insolation.Models.CheckBuildingIntersect.Check();
            });
        }

        public void Initialize ()
        {
            try
            {
                // Передача списка команд для палитры ПИК в AcadLib.             
                InitCommands();
                PaletteSetCommands.InitPalette(CommandsPalette, nameof(PIK_Start));

                // Загрузка сборки Civil
                string fileCivilDll = Path.Combine(CurDllDir, "PIK_GP_Civil.dll");
                LoadDll(fileCivilDll);
            }
            catch (System.Exception ex)
            {
                Logger.Log.Error(ex, "PIK_GP_ACAD.Initialize().");
            }
        }

        private static void LoadDll (string  file)
        {            
            if (File.Exists(file))
            {
                try
                {
                    Assembly.LoadFrom(file);
                }
                catch { }
            }
        }

        public void Terminate()
        {            
        }
    }        
}