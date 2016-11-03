﻿using System;
using System.Collections.Generic;
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

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Дом - состоит из блок-секций в ряд
    /// </summary>
    public class House : ModelBase
    {
        private Document doc = Application.DocumentManager.MdiActiveDocument;

        public House ()
        {
            VisualFront = new VisualFront();
        }   

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
        public List<MapBuilding> Buildings { get; set; }

        public Polyline Contour { get { return contour; } set { DisposeContour(); contour = value;} }
        Polyline contour;

        public VisualFront VisualFront { get; set; }

        public bool IsVisualFront {
            get { return isVisualFront; }
            set { isVisualFront = value; OnIsVisualFrontChanged(); RaisePropertyChanged(); }
        }
        bool isVisualFront;

        public Error Error  { get { return error; } set { error = value; RaisePropertyChanged(); } }
        Error error;

        /// <summary>
        /// Расчет фронтов дома
        /// </summary>
        public void Update ()
        {
            if (Contour == null) return;

            var calcService = FrontGroup.Front.Model.CalcService;
            var frontLines = calcService.CalcFront.CalcHouse(this);
            VisualFront.FrontLines = frontLines;
            VisualFront.VisualUpdate();
        }

        private void OnIsVisualFrontChanged ()
        {
            VisualFront.VisualIsOn = IsVisualFront;
            VisualFront.VisualUpdate();
        }

        /// <summary>
        /// Показать дом на карте
        /// </summary>
        public void Show()
        {
            try
            {                
                doc.Editor.Zoom(GetExtents());
            }
            catch { }
        } 

        /// <summary>
        /// Определение имени дома - по блок-секциям или дефолтное по переданному индексу
        /// </summary>        
        public void DefineName (int countHouse)
        {
            if (!string.IsNullOrEmpty(Name)) return;
            if (Buildings != null)
            {
                //TODO: FIX: определение имени дома по блок-секциям
                Name = Buildings[0].Building.HouseName;
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
            if (Buildings!= null)
            {
                foreach (var item in Buildings)
                {
                    item.Building.HouseName = Name;
                    // TODO: Сохранить имя дома в расшир.данных объекта здания (IBuilding)                                   
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
            if (Buildings != null)
            {
                var ext = new Extents3d();
                foreach (var item in Buildings)
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
            var pls = Buildings.Select(s => s.Contour).ToList();
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
        }

        public void DisposeContour ()
        {
            if (Contour != null && !Contour.IsDisposed)
            {
                Contour.Dispose();
            }
        }
    }
}
