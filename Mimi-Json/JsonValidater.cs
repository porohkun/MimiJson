using System.Collections.Generic;

namespace MimiJson
{
    public static class JsonValidater
    {

        public static List<JsonValidateError> Validate(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();
            if (scheme.Type == JsonValueType.Null)
            {
                //errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType, "scheme must be not null"));
                return errors;
            }
            if (value.Type == JsonValueType.Refer && value.Obj["$ref"].String == "#")
                return errors;
            if (scheme.GetReal().Obj.ContainsKey("type"))
                switch (scheme.GetReal().Obj["type"].String)
                {
                    case "object":
                        errors.AddRange(ValidateObject(value.GetReal(), scheme.GetReal()));
                        break;
                    case "array":
                        errors.AddRange(ValidateArray(value.GetReal(), scheme.GetReal()));
                        break;
                    case "string":
                        errors.AddRange(ValidateString(value.GetReal(), scheme.GetReal()));
                        break;
                    case "integer":
                        errors.AddRange(ValidateInteger(value.GetReal(), scheme.GetReal()));
                        break;
                    case "number":
                        errors.AddRange(ValidateNumber(value.GetReal(), scheme.GetReal()));
                        break;
                    case "boolean":
                        errors.AddRange(ValidateBoolean(value.GetReal(), scheme.GetReal()));
                        break;
                    case "null":
                        errors.AddRange(ValidateNull(value.GetReal(), scheme.GetReal()));
                        break;
                    default:
                        errors.Add(new JsonValidateError(value.GetReal(), scheme, JVErrorType.InvalidType,
                            "Ошибка схемы. Неправильный тип значения: \"" + scheme.GetReal().Obj["type"].String + "\"."));
                        break;
                }
            else
            {
                if (scheme.GetReal().Obj.ContainsKey("anyOf"))
                {
                    foreach (JsonValue aval in scheme.GetReal().Obj["anyOf"].Array)
                    {
                        if (Validate(value, aval.GetReal()).Count == 0)
                            return errors;
                    }
                    errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidValue,
                        "Структура должна подходить под одну из представленных схем"));
                }
            }

            return errors;
        }

        public static List<JsonValidateError> ValidateObject(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value==null || value.Type != JsonValueType.Object)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"object\"."));
            }
            if (errors.Count > 0) return errors;

            if (scheme.Obj.ContainsKey("required"))
                foreach (JsonValue req in scheme.Obj["required"].GetReal().Array)
                {
                    JsonValue val = req.GetReal();
                    if (!value.Obj.ContainsKey(val.String))
                    {
                        errors.Add(new JsonValidateError(value, scheme, JVErrorType.PropertyMissing,
                            "Поле \"" + val.String + "\" отсутствует."));
                    }
                }

            bool additional = scheme.Obj.ContainsKey("additionalProperties") &&
                ((scheme.Obj["additionalProperties"].GetReal().Type == JsonValueType.Boolean && scheme.Obj["additionalProperties"].GetReal().Boolean)
                || (scheme.Obj["additionalProperties"].GetReal().Type != JsonValueType.Boolean));
            JsonValue additionalProperties = scheme.Obj.ContainsKey("additionalProperties") ? scheme.Obj["additionalProperties"].GetReal() : null;

            foreach (KeyValuePair<string, JsonValue> prop in value.Obj)
            {
                if (scheme.Obj.ContainsKey("properties"))
                    if (scheme.Obj["properties"].GetReal().Obj.ContainsKey(prop.Key))
                    {
                        errors.AddRange(Validate(prop.Value, scheme.Obj["properties"].GetReal().Obj[prop.Key].GetReal()));
                    }
                    else
                    {
                        if (!additional)
                        {
                            errors.Add(new JsonValidateError(prop.Value, scheme, JVErrorType.PropertyMissing,
                                "Поле \"" + prop.Key + "\" отсутствует в схеме."));
                        }
                    }
                if (additional)
                {
                    if (additionalProperties.Type == JsonValueType.Object)
                    {
                        errors.AddRange(Validate(prop.Value, additionalProperties));
                    }
                }
            }

            return errors;
        }

        public static List<JsonValidateError> ValidateArray(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.Array)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"array\"."));
            }
            if (errors.Count > 0) return errors;

            if (scheme.Obj.ContainsKey("items"))
                foreach (JsonValue item in value.Array)
                {
                    errors.AddRange(Validate(item, scheme.Obj["items"].GetReal()));
                }

            return errors;
        }

        public static List<JsonValidateError> ValidateString(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.String)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"string\"."));
            }
            if (errors.Count > 0) return errors;

            if (scheme.Obj.ContainsKey("enum"))
            {
                bool correct = false;
                for (int i = 0; i < scheme.Obj["enum"].GetReal().Array.Length; i++)
                {
                    if (value.String == scheme.Obj["enum"].GetReal().Array[i].String)
                    {
                        correct = true;
                        i = scheme.Obj["enum"].GetReal().Array.Length;
                    }
                }
                if (!correct)
                    errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                        "Значение должно быть из списка: " + scheme.Obj["enum"].GetReal().Array.ToString() + "."));
            }

            if (scheme.Obj.ContainsKey("minLength"))
            {
                int minLength = (int)scheme.Obj["minLength"].GetReal().Number;
                if (value.String.Length < minLength)
                    errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                        "Длина строки должна быть больше " + minLength + "."));
            }

            if (scheme.Obj.ContainsKey("maxLength"))
            {
                int maxLength = (int)scheme.Obj["maxLength"].GetReal().Number;
                if (value.String.Length > maxLength)
                    errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                        "Длина строки должна быть больше " + maxLength + "."));
            }

            return errors;
        }

        public static List<JsonValidateError> ValidateInteger(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.Number)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"integer\"."));
            }
            if (errors.Count > 0) return errors;

            errors.AddRange(ValidateNumber(value, scheme));

            if (value.Number%1!=0)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Значение должно быть целым числом."));
            }

            return errors;
        }

        public static List<JsonValidateError> ValidateNumber(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.Number)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"number\"."));
            }
            if (errors.Count > 0) return errors;
            
            if (scheme.Obj.ContainsKey("minimum"))
            {
                double min= scheme.Obj["minimum"].GetReal().Number;
                if (scheme.Obj.ContainsKey("exclusiveMinimum")&&scheme.Obj["exclusiveMinimum"].GetReal().Boolean)
                {
                    if (value.Number <= min)
                        errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                            "Значение должно быть больше " + min + "."));
                }
                else
                {
                    if (value.Number < min)
                        errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                            "Значение должно быть больше или равно " + min + "."));
                }
            }

            if (scheme.Obj.ContainsKey("maximum"))
            {
                double max = scheme.Obj["maximum"].GetReal().Number;
                if (scheme.Obj.ContainsKey("exclusiveMaximum") && scheme.Obj["exclusiveMaximum"].GetReal().Boolean)
                {
                    if (value.Number >= max)
                        errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                            "Значение должно быть меньше " + max + "."));
                }
                else
                {
                    if (value.Number > max)
                        errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                            "Значение должно быть меньше или равно " + max + "."));
                }
            }

            return errors;
        }

        public static List<JsonValidateError> ValidateBoolean(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.Boolean)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"boolean\"."));
            }
            if (errors.Count > 0) return errors;

            return errors;
        }

        public static List<JsonValidateError> ValidateNull(JsonValue value, JsonValue scheme)
        {
            List<JsonValidateError> errors = new List<JsonValidateError>();

            if (value.Type != JsonValueType.Null)
            {
                errors.Add(new JsonValidateError(value, scheme, JVErrorType.InvalidType,
                    "Тип значения должен быть \"null\"."));
            }
            if (errors.Count > 0) return errors;

            return errors;
        }

    }
}
