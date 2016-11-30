﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace PIK_GP_Acad.Insolation.Services
{
    public interface IVisualService : IDisposable
    {
        /// <summary>
        /// Включение/выключение визуализации
        /// </summary>
        bool VisualIsOn { get; set; }

        /// <summary>
        /// Обновление визуализации
        /// </summary>
        void VisualUpdate ();
        List<Entity> CreateVisual ();
        /// <summary>
        /// Удаление визуализации
        /// </summary>
        void VisualsDelete ();
        /// <summary>
        /// Отрисовка визуализации для пользователя
        /// </summary>
        void DrawForUser();        
    }
}
