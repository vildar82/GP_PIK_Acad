using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Catel.ComponentModel;
using Catel.MVVM.Converters;
using Catel.Reflection;

namespace PIK_GP_Acad.Insolation.UI
{
    public class ObjectToDisplayNameConverter : MarkupExtension, IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value as Type;
            if (type != null)
            {
                DisplayNameAttribute displayAttribute = null;
                if (type.TryGetAttribute(out displayAttribute))
                {
                    return GetDisplayName(displayAttribute);
                }

                return type.Name;
            }

            var memberInfo = value as MemberInfo;
            if (memberInfo != null)
            {
                DisplayNameAttribute displayAttribute = null;
                if (memberInfo.TryGetAttribute(out displayAttribute))
                {
                    return GetDisplayName(displayAttribute);
                }

                return memberInfo.Name;
            }

            // Support enum values
            if (value != null)
            {
                var valueType = value.GetType();
                if (valueType.IsEnumEx())
                {
                    memberInfo = valueType.GetMemberEx(value.ToString(), allowStaticMembers: true).FirstOrDefault();
                    if (memberInfo != null)
                    {
                        DisplayNameAttribute displayAttribute = null;
                        if (memberInfo.TryGetAttribute(out displayAttribute))
                        {
                            return GetDisplayName(displayAttribute);
                        }

                        return memberInfo.Name;
                    }
                }
            }

            return null;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue (IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new ObjectToDisplayNameConverter();
            return _converter;
        }
        private static ObjectToDisplayNameConverter _converter = null;

        protected string GetDisplayName (DisplayNameAttribute attribute)
        {            
            return attribute.DisplayName;
        }
    }
}
