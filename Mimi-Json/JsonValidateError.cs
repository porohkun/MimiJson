
namespace MimiJson
{
    public class JsonValidateError
    {
        public JsonValue Value { get; private set; }
        public JsonValue SchemeValue { get; private set; }
        public string Message { get; private set; }
        public JVErrorType ErrorType { get; private set; }
        public JsonValidateError(JsonValue value, JsonValue schemeValue, JVErrorType type, string message)
        {
            Value = value;
            SchemeValue = schemeValue;
            ErrorType = type;
            Message = message;
        }

    }

    public enum JVErrorType
    {
        InvalidType,
        PropertyMissing,
        InvalidValue
    }
}
