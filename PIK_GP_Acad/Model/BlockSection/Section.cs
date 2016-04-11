using AcadLib.Errors;

namespace PIK_GP_Acad.BlockSection
{
   // Блок-секция
   public class Section
   {
      // Площадь квартир
      public double AreaApart { get; private set; }

      // Общая площадь квартир
      public double AreaApartTotal { get; private set; }

      // Площадь БКФН
      public double AreaBKFN { get; private set; }

      // Имя блок-секции
      public string Name { get; private set; } = string.Empty;

      // Кол этажей
      public int NumberFloor { get; private set; }

      public void SetAreaApart(string textString)
      {
         AreaApart = getArea(textString, Settings.Default.AttrAreaApart);
      }

      public void SetAreaApartTotal(string textString)
      {
         AreaApartTotal = getArea(textString, Settings.Default.AttrAreaApartTotal);
      }

      public void SetAreaBKFN(string textString)
      {
         AreaBKFN = getArea(textString, Settings.Default.AttrAreaBKFN);
      }

      public void SetName(string textString)
      {
         Name = textString.Trim();
      }

      public void SetNumberFloor(string textString)
      {
         NumberFloor = getNum(textString, Settings.Default.AttrNumberFloor);
      }

      private double getArea(string textString, string areaName)
      {
         double val = 0;
         if (!double.TryParse(textString, out val))
         {
            Inspector.AddError($"Не определена площадь в атрибуте {areaName} - значение {textString}", icon: System.Drawing.SystemIcons.Error);
         }
         return val;
      }

      private int getNum(string textString, string attrNumberFloor)
      {
         int val = 0;
         if (!int.TryParse(textString, out val))
         {
            Inspector.AddError($"Не определено кол этажей в атрибуте {attrNumberFloor} - значение {textString}", icon: System.Drawing.SystemIcons.Error);
         }
         return val;
      }
   }
}