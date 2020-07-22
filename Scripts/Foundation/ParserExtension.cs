namespace UHelper
{
    public static class ParserExtension
    {
        public static float Parse2Float(this string _value){
            if(_value==string.Empty){
                return 0f;
            }
            return float.Parse(_value);
        }
    }
}