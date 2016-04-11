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
    public class Commands : IExtensionApplication
    {
        public static string CurDllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static PaletteSetCommands _palette;
        private readonly Guid PaletteGuid = new Guid("4770C55E-0793-46C3-AEA4-CF0902320E71");

        [CommandMethod("PIK", "PIK-Start", CommandFlags.Modal)]
        public void Pik_Palette()
        {
            if (_palette == null)
            {
                var comms = LoadCommands();
                _palette = PaletteSetCommands.Create("ПИК-ГП", comms, PaletteGuid);
            }
            _palette.Visible = true;
        }


        /// <summary>
        /// Изменение уровней горизонталей
        /// </summary>
        [CommandMethod("PIK", "GP-HorizontalElevationStep", CommandFlags.Modal)]
        public void HorizontalElevationStep()
        {
            Logger.Log.Info("Start command GP-HorizontalElevationStep");
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            try
            {
                Inspector.Clear();
                HorizontalElevation horElev = new HorizontalElevation();
                horElev.Stepping();

                if (Inspector.HasErrors)
                {
                    Inspector.Show();
                }
            }
            catch (System.Exception ex)
            {
                if (!ex.Message.Contains("Отменено пользователем"))
                {
                    Logger.Log.Error(ex, "GP-HorizontalElevationStep");
                }
                doc.Editor.WriteMessage(ex.Message);
            }
        }

        /// <summary>
        /// Создание блоков монтажных планов (создаются блоки с именем вида АКР_Монтажка_2).
        /// </summary>
        [CommandMethod("PIK", "GP-BlockSectionTable", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void BlockSectionTable()
        {
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
        [CommandMethod("PIK", "GP-BlockSectionContour", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void BlockSectionContour()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
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

        [CommandMethod("PIK", "GP-BlockSectionInsert", CommandFlags.Modal)]
        public void BlockSectionInsert()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
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

        [CommandMethod("PIK", "GP-InsertBlockParking", CommandFlags.Modal)]
        public void InsertBlockParking()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            try
            {                
                InsertBlock.Insert("ГП_Линия-Парковки", doc);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(AcadLib.General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP-InsertBlockParking. {doc.Name}");
                }
            }
        }

        [CommandMethod("PIK", "GP-InsertBlockPikLogo", CommandFlags.Modal)]
        public void InsertBlockPikLogo()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            try
            {
                InsertBlock.Insert("PIK_Logo", doc);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(AcadLib.General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP-InsertBlockParking. {doc.Name}");
                }
            }
        }

        [CommandMethod("PIK", "GP-Isoline", CommandFlags.Modal)]
        public void GpIsoline()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Logger.Log.Info("Start Command: GP-Isoline");
            try
            {
                Isolines.Isoline.Start();
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage(ex.Message);
                if (!ex.Message.Contains(AcadLib.General.CanceledByUser))
                {
                    Logger.Log.Error(ex, $"Command: GP-Isoline. {doc.Name}");
                }
            }
        }

        public List<IPaletteCommand> LoadCommands()
        {
            List<IPaletteCommand> comms = new List<IPaletteCommand>()
            {
             new PaletteCommand("Вставка блока Парковки", ImageSourceForImageControl(Properties.Resources.GP_BlockParking), InsertBlockParking, ""),
             new PaletteCommand("Вставка блока Блок-Секции", ImageSourceForImageControl(Properties.Resources.GB_BlockSectionInsert), BlockSectionInsert, ""),
             new PaletteCommand("Спецификация Блок-Секций", ImageSourceForImageControl(Properties.Resources.GP_BlockSectionTable), BlockSectionTable, "Подсчет выбранных на чертеже блоков секций"),
             new PaletteCommand("Уровни горизонталей", ImageSourceForImageControl(Properties.Resources.GP_HorizontalElevation), HorizontalElevationStep, "Изменение уровня горизонталей"),
             new PaletteCommand("Бергштрихи одиночные", ImageSourceForImageControl(Properties.Resources.GP_Isoline), GpIsoline, "Одиночные бергштрихи в полилиниях и отрезках.")
            };
            return comms;
        }

        public ImageSource ImageSourceForImageControl(Bitmap image)
        {            
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    image.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
        }

        public void Initialize()
        {
            //// Добавление иконки в трей
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //if (doc == null) return;
            //Editor ed = doc.Editor;
            //Database db = doc.Database;

            //Pane pane = new Pane();
            //pane.Icon = Properties.Resources.pik_logo;
            //pane.MouseDown += PikTray_MouseDown;
            //Application.StatusBar.Panes.Add(pane);
        }

        private void PikTray_MouseDown(object sender, StatusBarMouseDownEventArgs e)
        {            
            Pik_Palette();
        }

        public void Terminate()
        {            
        }
    }
}