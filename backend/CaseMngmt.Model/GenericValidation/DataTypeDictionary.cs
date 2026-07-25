namespace CaseMngmt.Models.GenericValidation
{
    public static class DataTypeDictionary
    {
        public static readonly Dictionary<string, Type> DataTypeAlias = new() {
            { "byte", typeof(byte) },
            { "short" , typeof(short) },
            { "int" , typeof(int) },
            { "long" , typeof(long) },
            { "float" , typeof(float) },
            { "double" , typeof(double) },
            { "decimal" , typeof(decimal) },

            // Template screen
            { "numeric" , typeof(int) },
            { "bignumeric" , typeof(float) },
            { "date" , typeof(DateTime) },
            { "datetime" , typeof(DateTime) },
            { "currency" , typeof(float) },
            { "string" , typeof(string) },
            { "list" , typeof(string) },
            { "textarea" , typeof(string) }
        };

        public static readonly List<string> ImageTypes = new List<string> {
            ".jpeg",
            ".jpg",
            ".png",
        };
    }
}
