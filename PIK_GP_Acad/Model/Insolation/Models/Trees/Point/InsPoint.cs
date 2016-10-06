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

        [ExcludeFromSerialization]
        public string Info { get; set; }

        /// <summary>
        /// Расчет точки - зон освещенности и времени
        /// </summary>
        private void Calc ()
        {
            Illums = Model.CalcService.TreesCalc.CalcPoint(this);
            InsValue = Model.CalcService.CalcTimeAndGetRate(Illums, Building.BuildingType);
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
            var oldBuildingType = Building.BuildingType;

            var insPointVM = new InsPointViewModel(this);
            var uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            if (await uiVisualizerService.ShowDialogAsync(insPointVM) == true)
            {
                // Обновление расчета точки
                Update();

                if (oldBuildingType != Building.BuildingType)
                {
                    // Учет изменения типа здания для всех точек на этом здании
                    var insReq = Model.CalcService.DefineInsRequirement(InsValue.MaxContinuosTime, InsValue.TotalTime, Building.BuildingType);
                    var pointsInBuilding = Model.Tree.GetPointsInBuilding(Building);
                    foreach (var item in pointsInBuilding)
                    {
                        if (item != this)
                        {
                            item.InsValue.Requirement = insReq;
                            item.UpdateVisual();
                        }
                    }
                }

                // Обновление елочек
                Model.Tree.UpdateVisualTree(this);
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
        private string GetInfo()
        {
            var info = new StringBuilder();

            info.Append("Номер: ").Append(Number).Append(", Коорд. - ").Append(Point.ToStringEx()).Append(", Высота точки - ").Append(Height).AppendLine();
            info.Append("Инсоляция: ").Append(InsValue.Requirement.Name).Append(", Макс - ").
                Append(InsValue.MaxContinuosTimeString).Append(", Всего - ").Append(InsValue.TotalTimeString).AppendLine();
            info.Append("Здание: ").Append(Building.BuildinTypeName).Append(", Высота - ").Append(Building.Height).AppendLine();
            info.Append("Окно: ширина - ").Append(Window.Width).Append(", Глубина четверти - ").Append(Window.Quarter).
                Append(", Конструкция - ").Append(Window.Construction.Name).Append(" ").Append(Window.Construction.Depth);

            return info.ToString();
        }

        /// <summary>
        /// Обноаление - расчета и визуализации
        /// </summary>
        public void Update ()
        {
            Calc();
            UpdateVisual();
        }   

        /// <summary>
        /// Обноаление визуализации точки (зон инсоляции и описания точки)
        /// </summary>
        private void UpdateVisual ()
        {
            // Подготовка визуальных объектов
            // Визуализация зон инсоляции точки
            if (VisualIllums == null)            
                VisualIllums = new VisualInsPointIllums();            
            VisualIllums.CreateVisual(this);
            // Визуализация описания точки
            if (VisualPointInfo == null)
                VisualPointInfo = new VisualInsPointInfo(this);
            else
                VisualPointInfo.InsPoint = this;


            // Обноаление визуальных объектов на чертеже, если включены
            if (Model.IsInsActivated)
            {
                // Зоны освещ.
                if (IsVisualIllumsOn)
                    VisualIllums.Update();
                // Описание точки
                VisualPointInfo.IsOn = true;
            }

            Info = GetInfo();
        }
    }
}
