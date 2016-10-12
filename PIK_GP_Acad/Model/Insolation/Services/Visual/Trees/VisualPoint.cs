using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using PIK_GP_Acad.Insolation.Models;
using AcadLib;
using System.Drawing;
using static PIK_GP_Acad.Insolation.Services.VisualHelper;

namespace PIK_GP_Acad.Insolation.Services
{
    /// <summary>
    /// Визуализация описания точки на чертеже  - через DrwableOverrule, а не TransientGraphics
    /// Перерисовыватся точка на чертеже - из объекта InsPointDraw (который находит соответствующую точку)
    /// </summary>
    public class VisualPoint : IVisualService
    {
        private bool isOn;

        public InsPoint InsPoint { get; set; }

        public bool VisualIsOn {
            get { return isOn; }
            set {
                isOn = value;
                VisualUpdate();
            }
        }

        public VisualPoint (InsPoint insPoint)
        {
            InsPoint = insPoint;
        }

        public List<Drawable> CreateVisual ()
        {
            List<Drawable> draws = new List<Drawable>();

            // Кружок (положение точки)
            var optCircle = new VisualOption(System.Drawing.Color.Yellow, InsPoint.Point);
            draws.Add(CreateCircle(0.5, optCircle));

            // Подпись
            // Макимальная непрерывная инсоляция            
            var ptText = InsPoint.Point + new Vector3d(0, 0.5, 0);
            var opt = new VisualOption(InsPoint.InsValue.Requirement.Color, ptText);            
            draws.Add(CreateText(InsPoint.InsValue.MaxContinuosTimeString, opt, 0.5, AttachmentPoint.BottomCenter));
            // Тип требования
            opt.Position = ptText + new Vector3d(0,1,0);
            draws.Add(CreateText(InsPoint.InsValue.Requirement.Name, opt, 0.5, AttachmentPoint.BottomCenter));
            // Номер точки
            opt.Position = ptText + new Vector3d(0, 1, 0);
            draws.Add(CreateText(InsPoint.Number.ToString(), opt, 1.5, AttachmentPoint.BottomCenter));

            return draws;
        }

        public void VisualUpdate ()
        {
            // Перерисовать точку
            Autodesk.AutoCAD.ApplicationServices.Application.UpdateScreen();
        }        
    }
}
