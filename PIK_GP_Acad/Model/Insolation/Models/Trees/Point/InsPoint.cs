using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using Catel.Services;
using PIK_GP_Acad.Elements.Buildings;
using PIK_GP_Acad.Insolation.Services;
using PIK_GP_Acad.Insolation.UI;
using AcadLib;

namespace PIK_GP_Acad.Insolation.Models
{
    /// <summary>
    /// Расчетная точка
    /// </summary>
    public class InsPoint : ModelBase
    {
        [ExcludeFromSerialization]
        public InsModel Model { get; set; }

        [ExcludeFromSerialization]
        public VisualInsPointIllums VisualIllums { get; private set; }
        [ExcludeFromSerialization]
        public VisualInsPointInfo VisualPointInfo { get; private set; }

        public InsPoint () { }

        public InsPoint (InsModel model)
        {
            this.Model = model;
            Window = new WindowOptions();
            Number = model.Tree.Points.Count + 1;
        }

        protected override void OnInitialized ()
        {
            base.OnInitialized();
            EditPoint = new TaskCommand(OnEditPointExecute);
            DeletePoint = new TaskCommand(OnDeletePointExecute);
        }

        public TaskCommand EditPoint { get; private set; }
        public TaskCommand DeletePoint { get; private set; }


        /// <summary>
        /// Номер точки
        /// </summary>
        public int Number { get; set; }
        public Point3d Point { get; set;}
        [ExcludeFromSerialization]
        public InsBuilding Building { get; set; }
        [ExcludeFromSerialization]
        public List<IIlluminationArea> Illums { get; set; }
        [ExcludeFromSerialization]
        public InsValue InsValue { get; set; }        
        public int Height { get; set; }
        public WindowOptions Window { get; set; }
        public bool IsVisualIllumsOn { get; set; }

        /// <summary>
        /// Начальный угол - ограничивающий освещение точки.
        /// [рад]
        /// </summary>
        public double AngleStartOnPlane { get; set; }
        /// <summary>
        /// Конечный угол в плане (радиан)
        /// </summary>
        public double AngleEndOnPlane { get; set; }

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        public void Calc ()
        {
            Illums = Model.CalcService.TreesCalc.CalcPoint(this);
            InsValue = Model.CalcService.CalcTimeAndGetRate(Illums);

            // Визуализация зон инсоляции точки
            if (VisualIllums == null)
            {
                VisualIllums = new VisualInsPointIllums();
            }            
            VisualIllums.CreateVisual(this);

            // Визуализация описания точки
            if (VisualPointInfo == null)
                VisualPointInfo = new VisualInsPointInfo(this);
            else
                VisualPointInfo.InsPoint = this;
            if (Model.IsInsActivated)
                VisualPointInfo.IsOn = true;
        }        
        
        private void OnIsVisualIllumsOnChanged ()
        {
            // Включение/выключение визуализации инсоляционных зон точки
            if (VisualIllums != null)
            {
                VisualIllums.IsOn = IsVisualIllumsOn;
            }
        }

        private async Task OnEditPointExecute ()
        {
            var insPointVM = new InsPointViewModel(this);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiVisualizerService.ShowDialogAsync(insPointVM) == true)
            {
                // Обновление расета точки
            }
        }

        private async Task OnDeletePointExecute ()
        {
            Model.Tree.DeletePoint(this);
            VisualIllums.IsOn = false;
            VisualPointInfo.IsOn = false;
        }
        
        /// <summary>
        /// Описание точки
        /// </summary>        
        public string Info()
        {
            var info = new StringBuilder();

            info.Append("Номер: ").Append(Number).Append(", коорд. - ").Append(Point).AppendLine();
            info.Append("Инсоляция: ").Append(InsValue.Requirement.Name).Append(", макс - ").
                Append(InsValue.MaxContinuosTime).Append("ч., всего - ").Append(InsValue.TotalTime).AppendLine();
            info.Append("Здание: ").Append(Building.BuildinTypeName).Append(", высота - ").Append(Building.Height).AppendLine();
            info.Append("Окно: ширина - ").Append(Window.Width).Append(", глубина четверти - ").Append(Window.Quarter).
                Append(", конструкция - ").Append(Window.Construction.Name).Append(" ").Append(Window.Construction.Depth);

            return info.ToString();
        }        
    }
}
