﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using AcadLib.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Acad.Insolation;
using PIK_GP_Acad.Insolation.Services;
using AcadLib.XData;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом - состоит из блок-секций в ряд
    /// </summary>
    public class House : ModelBase, IDisposable
    {
        public House (FrontGroup frontGroup)
        {
            FrontGroup = frontGroup;
            Doc = FrontGroup.Front.Model.Doc;
            VisualFront = new VisualFront(Doc);
            //VisualFront.LayerVisual = FrontGroup.Front.Options.FrontLineLayer;
            Sections = new ObservableCollection<MapBuilding>();
        }   

        public Document Doc { get; set; }
        public FrontGroup FrontGroup { get; set; }
        
        /// <summary>
        /// Имя дома - в иделе = имени объекта принятого в проекте
        /// </summary>
        public string Name {
            get { return name; }
            set {
                name = value;
                SaveHouseNameToSection();
                RaisePropertyChanged();
            }
        }
        string name;

        /// <summary>
        /// Блок-секции дома
        /// </summary>
        public ObservableCollection<MapBuilding> Sections { get { return sections; } set { sections = value; RaisePropertyChanged(); } }
        ObservableCollection<MapBuilding> sections;

        public Polyline Contour { get { return contour; } set { DisposeContour(); contour = value;} }
        Polyline contour;

        public VisualFront VisualFront { get; set; }

        public bool IsVisualFront {
            get { return isVisualFront; }
            set { isVisualFront = value; OnIsVisualFrontChanged(); RaisePropertyChanged(); }
        }
        bool isVisualFront;

        public Error Error  { get { return error; } set { error = value; RaisePropertyChanged(); Info = error?.Message; } }
        Error error;

        public string Info {
            get { return info; }
            set { info = value; RaisePropertyChanged(); }
        }               
        string info;

        /// <summary>
        /// Расчет фронтов дома
        /// </summary>
        public void Update ()
        {
            if (Contour == null) return;
            var calcService = FrontGroup.Front.Model.CalcService;
            try
            {
                var frontLines = calcService.CalcFront.CalcHouse(this);
                VisualFront.FrontLines = frontLines;
                VisualFront.VisualUpdate();
            }
            catch(Exception ex)
            {
                AddError(ex.ToString());
            }   
        }

        private void OnIsVisualFrontChanged ()
        {
            VisualFront.VisualIsOn = IsVisualFront;            
        }

        /// <summary>
        /// Показать дом на карте
        /// </summary>
        public void Show()
        {
            try
            {
                using (Doc.LockDocument())
                {
                    Doc.Editor.Zoom(GetExtents());
                }
            }
            catch { }
        } 

        /// <summary>
        /// Определение имени дома - по блок-секциям или дефолтное по переданному индексу
        /// </summary>        
        public void DefineName (int countHouse)
        {
            if (!string.IsNullOrEmpty(Name)) return;
            if (Sections != null)
            {
                //TODO: FIX: определение имени дома по блок-секциям
                Name = Sections[0].Building.HouseName;
            }
            if (string.IsNullOrEmpty(Name))
            {
                Name = "Дом " + countHouse;
            }            
        }

        /// <summary>
        /// Сохранение имени дома в секции
        /// </summary>
        private void SaveHouseNameToSection ()
        {
            if (string.IsNullOrEmpty(Name)) return;
            if (Sections!= null)
            {                
                foreach (var item in Sections)
                {
                    item.Building.HouseName = Name;                    
                }
            }
        }        

        /// <summary>
        /// Добавление ошибки - относящейся к этому дому
        /// </summary>        
        public void AddError (string message)
        {
            if (Error == null)
            {
                var ext = GetExtents();
                Error = new Error("Ошибка.", ext, Matrix3d.Identity, System.Drawing.SystemIcons.Error);
            }
            Error.AdditionToMessage(message);
        }

        private Extents3d GetExtents ()
        {
            if (Contour != null)
            {
                return Contour.GeometricExtents;
            }
            if (Sections != null)
            {
                var ext = new Extents3d();
                foreach (var item in Sections)
                {
                    ext.AddExtents(item.ExtentsInModel);
                }
                return ext;
            }
            return new Extents3d();
        }       

        /// <summary>
        /// Определение полилинии контура дома (объединением полилиний от блок-секций)
        /// </summary>
        public void DefineContour ()
        {
            if (Sections == null) return;
            var pls = Sections.Select(s => s.Contour).ToList();

            // Предварительное соединение полилиний (близкие точки вершин разных полилиний - в среднюю вершину)
            // Сделал при определении домов
                       
            using (var reg = pls.Union(null))
            {
                var ptsRegByLoopType = reg.GetPoints2dByLoopType();
                if (ptsRegByLoopType.Count == 1)
                {
                    Contour = ptsRegByLoopType[0].Key.Cast<Point2d>().ToList().CreatePolyline();
                }
                else
                {
                    // Объединение полилиний
                    var plsLoop = ptsRegByLoopType.Select(s => s.Key.Cast<Point2d>().ToList().CreatePolyline()).ToList();
                    try
                    {
                        var plMerged = plsLoop.Merge(2);
                        Contour = plMerged;
                    }
                    catch (Exception ex)
                    {
                        AddError(ex.Message);
                    }
                }
            }
#if TEST
            EntityHelper.AddEntityToCurrentSpace(Contour);
#endif
        }

        public void DisposeContour ()
        {
            if (Contour != null && !Contour.IsDisposed)
            {
                Contour.Dispose();
            }
        }

        public void ClearVisual ()
        {
            VisualFront?.VisualsDelete();
        }

        public void UpdateVisual ()
        {
            VisualFront?.VisualUpdate();
        }

        public void Dispose ()
        {
            VisualFront?.Dispose();
        }
    }
}
