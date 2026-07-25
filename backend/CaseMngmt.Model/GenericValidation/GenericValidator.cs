using System.ComponentModel;

namespace CaseMngmt.Models.GenericValidation
{
    public class GenericValidator
    {
        public bool IsValid(Type type, string value, int? maxlength)
        {
            try
            {
                if (value == null)
                {
                    return true;
                }
                object? parseValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
                if (parseValue != null)
                {
                    if (maxlength != null)
                    {
                        return value.Length <= maxlength;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
