using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MimiJson
{
    public class JsonValue : IEnumerable<JsonValue>
    {
        public JsonPos KeyPos { get; internal set; }
        public JsonPos ValPos { get; internal set; }

        public JsonValueType Type { get; private set; }
        public string String { get; set; }
        public double Number { get; set; }
        public JSONObject Obj { get; set; }
        public JSONArray Array { get; set; }
        public bool Boolean { get; set; }
        public JsonValue Refer { get; set; }
        public JsonValue Parent { get; private set; }
        public JsonValue Root { get; private set; }

        #region constructors

        public JsonValue()
        {
            Type = JsonValueType.Null;
        }

        public JsonValue(JsonValueType type)
        {
            Type = type;
            switch (type)
            {
                case JsonValueType.String:
                    String = "";
                    break;
                case JsonValueType.Number:
                    Number = 0;
                    break;
                case JsonValueType.Boolean:
                    Boolean = false;
                    break;
                case JsonValueType.Object:
                    Obj = new JSONObject();
                    break;
                case JsonValueType.Array:
                    Array = new JSONArray();
                    break;
            }
        }

        public JsonValue(string str)
        {
            if (str == null)
                Type = JsonValueType.Null;
            else
            {
                Type = JsonValueType.String;
                String = str;
            }
        }

        public JsonValue(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                Type = JsonValueType.Null;
            else
            {
                Type = JsonValueType.Number;
                Number = number;
            }
        }

        public JsonValue(JSONObject obj)
        {
            if (obj == null)
            {
                Type = JsonValueType.Null;
            }
            else
            {
                Type = JsonValueType.Object;
                Obj = obj;
            }
        }

        public JsonValue(JSONArray array)
        {
            if (array == null)
                Type = JsonValueType.Null;
            else
            {
                Type = JsonValueType.Array;
                Array = array;
            }
        }

        public JsonValue(bool boolean)
        {
            Type = JsonValueType.Boolean;
            Boolean = boolean;
        }

        #endregion

        #region implisit to JSONValue

        public static implicit operator JsonValue(string value)
        {
            return new JsonValue(value);
        }

        public static implicit operator JsonValue(double number)
        {
            return new JsonValue(number);
        }

        public static implicit operator JsonValue(JSONObject obj)
        {
            return new JsonValue(obj);
        }

        public static implicit operator JsonValue(JSONArray array)
        {
            return new JsonValue(array);
        }

        public static implicit operator JsonValue(bool boolean)
        {
            return new JsonValue(boolean);
        }

        #endregion

        #region implisit from JSONValue

        private static InvalidCastException GetImplisitException(JsonValueType tryType, JsonValueType realType)
        {
            return new InvalidCastException(String.Concat("Wrong type '", tryType.ToString(), "'. Real type is '", realType.ToString(), "'."));
        }

        public static implicit operator double(JsonValue value)
        {
            if (value.Type != JsonValueType.Number)
                throw GetImplisitException(JsonValueType.Number, value.Type);
            return value.Number;
        }

        public static implicit operator float(JsonValue value)
        {
            if (value.Type != JsonValueType.Number)
                throw GetImplisitException(JsonValueType.Number, value.Type);
            return (float)value.Number;
        }

        public static implicit operator decimal(JsonValue value)
        {
            if (value.Type != JsonValueType.Number)
                throw GetImplisitException(JsonValueType.Number, value.Type);
            return (decimal)value.Number;
        }

        public static implicit operator int(JsonValue value)
        {
            if (value.Type != JsonValueType.Number)
                throw GetImplisitException(JsonValueType.Number, value.Type);
            return (int)value.Number;
        }

        public static implicit operator long(JsonValue value)
        {
            if (value.Type != JsonValueType.Number)
                throw GetImplisitException(JsonValueType.Number, value.Type);
            return (long)value.Number;
        }

        public static implicit operator string(JsonValue value)
        {
            if (value.Type != JsonValueType.String)
                throw GetImplisitException(JsonValueType.String, value.Type);
            return value.String;
        }

        public static implicit operator JSONObject(JsonValue value)
        {
            if (value.Type != JsonValueType.Object)
                throw GetImplisitException(JsonValueType.Object, value.Type);
            return value.Obj;
        }

        public static implicit operator JSONArray(JsonValue value)
        {
            if (value.Type != JsonValueType.Array)
                throw GetImplisitException(JsonValueType.Array, value.Type);
            return value.Array;
        }

        public static implicit operator bool(JsonValue value)
        {
            if (value.Type != JsonValueType.Boolean)
                throw GetImplisitException(JsonValueType.Boolean, value.Type);
            return value.Boolean;
        }

        #endregion

        #region value getting

        public JsonValue this[int i]
        {
            get
            {
                if (Type != JsonValueType.Array)
                    throw GetImplisitException(JsonValueType.Array, Type);
                return Array[i];
            }
        }

        public JsonValue this[string name]
        {
            get
            {
                if (Type != JsonValueType.Object)
                    throw GetImplisitException(JsonValueType.Object, Type);
                return Obj[name];
            }
        }

        public JsonValue GetReal()
        {
            if (Type == JsonValueType.Refer)
                return Refer;
            else
                return this;
        }

        public JsonValue GetValueByPath(string path)
        {
            return GetValueByPath(path.Split('/'));
        }

        public JsonValue GetValueByPath(string[] path)
        {
            if (path.Length == 0)
                return GetReal();
            if (Type == JsonValueType.Object)
            {
                string child = path[0];
                string[] newpath = new string[path.Length - 1];
                for (int i = 0; i < newpath.Length; i++)
                    newpath[i] = path[i + 1];
                if (child == "#")
                {
                    return Root.GetValueByPath(newpath);
                }
                if (Obj.ContainsKey(child))
                {
                    return Obj[child].GetValueByPath(newpath);
                }
            }

            return null;
        }

        #endregion

        #region parsing

        public static JsonValue Parse(string jsonString)
        {
            int p = 0;
            return Parse(jsonString, ref p);
        }

        public static JsonValue Parse(string jsonString, ref int position)
        {
            if (string.IsNullOrEmpty(jsonString))
                return new JsonValue(JsonValueType.Null);

            JsonValue val = new JsonValue(JsonValueType.Null);

            int startPos = position;
            char c;
            if (position < jsonString.Length)
            {
                c = jsonString[position];

                if (c == '"')
                {
                    val = new JsonValue(Parser.ParseString(jsonString, ref position));
                }
                else if (char.IsDigit(c) || c == '-')
                {
                    val = new JsonValue(Parser.ParseNumber(jsonString, ref position));
                }
                else
                    switch (c)
                    {

                        case '{':
                            val = new JsonValue(JSONObject.Parse(jsonString, ref position));
                            break;

                        case '[':
                            val = new JsonValue(JSONArray.Parse(jsonString, ref position));
                            break;

                        case 'f':
                        case 't':
                            val = new JsonValue(Parser.ParseBoolean(jsonString, ref position));
                            break;

                        case 'n':
                            val = new JsonValue(Parser.ParseNull(jsonString, ref position));
                            break;

                            //default:
                            //    return Fail("beginning of value", startPosition);
                    }
            }
            else
                val = new JsonValue(JsonValueType.Null);
            position++;
            val.KeyPos = new JsonPos(startPos, startPos);
            val.ValPos = new JsonPos(startPos, position);

            Parser.SkipWhitespace(jsonString, ref position);

            val.UpdateParent(val);

            return val;
        }

        internal void UpdateParent(JsonValue parent)
        {
            if (parent == this)
            {
                Root = parent;
            }
            else
            {
                Parent = parent;
                Root = parent.Root;
            }
            switch (Type)
            {
                case JsonValueType.Object:
                    foreach (KeyValuePair<string, JsonValue> pair in Obj)
                    {
                        pair.Value.UpdateParent(this);
                    }
                    break;
                case JsonValueType.Array:
                    foreach (JsonValue value in Array)
                    {
                        value.UpdateParent(this);
                    }
                    break;
            }
        }

        internal void UpdateRefs()
        {
            switch (Type)
            {
                case JsonValueType.Object:
                    {
                        Dictionary<string, JsonValue> refs = new Dictionary<string, JsonValue>();
                        foreach (KeyValuePair<string, JsonValue> pair in Obj)
                        {
                            if (pair.Value.Type == JsonValueType.Object && pair.Value.Obj.ContainsKey("$ref"))
                            {
                                pair.Value.Type = JsonValueType.Refer;
                                pair.Value.Refer = Root.GetValueByPath(pair.Value.Obj["$ref"].String);
                            }
                            else
                                pair.Value.UpdateRefs();
                        }
                        break;
                    }
                case JsonValueType.Array:
                    {
                        foreach (JsonValue value in Array)
                            if (value.Type == JsonValueType.Object && value.Obj.ContainsKey("$ref"))
                            {
                                value.Type = JsonValueType.Refer;
                                value.Refer = Root.GetValueByPath(value.Obj["$ref"].String);
                            }
                            else
                                value.UpdateRefs();
                        break;
                    }
            }
        }

        #endregion

        #region save/load

        public static JsonValue Load(string path)
        {
            return Load(path, Encoding.UTF8);
        }

        public static JsonValue Load(string path, Encoding encoding)
        {
            if (System.IO.File.Exists(path))
            {
                JsonValue result = JsonValue.Parse(System.IO.File.ReadAllText(path, encoding));
                result.UpdateRefs();
                return result;
            }
            return new JsonValue(JsonValueType.Null);
        }

        public void Save(string path, bool unix = true)
        {
            Save(path, Encoding.UTF8, unix);
        }

        public void Save(string path, Encoding encoding, bool unix = true)
        {
            Save(path, this.ToString(), encoding, unix);
        }

        public static void Save(string path, string value, bool unix = true)
        {
            Save(path, value, Encoding.UTF8, unix);
        }

        public static void Save(string path, string value, Encoding encoding, bool unix = true)
        {
            string text = value.Replace('\r', '\n');
            while (text.Contains("\n\n"))
                text = text.Replace("\n\n", "\n");
            System.IO.File.WriteAllText(path, text, encoding);
        }

        #endregion

        public override string ToString()
        {
            return ToString(0, true);
        }

        public string ToString(bool formatted)
        {
            return ToString(0, formatted);
        }

        public string ToString(int tabs, bool formatted)
        {
            switch (Type)
            {
                case JsonValueType.Object:
                    return Obj.ToString(formatted ? tabs : -1, formatted);

                case JsonValueType.Array:
                    return Array.ToString(formatted ? tabs : -1, formatted);

                case JsonValueType.Boolean:
                    return Boolean ? "true" : "false";

                case JsonValueType.Number:
                    return Number.ToString(System.Globalization.CultureInfo.InvariantCulture);

                case JsonValueType.String:
                    return "\"" + String.Replace("\r", @"\r").Replace("\n", @"\n").Replace("\"", @"\""") + "\"";

                case JsonValueType.Refer:
                    return "\"" + String + "\"";

                case JsonValueType.Null:
                    return "null";
            }
            return "null";
        }

        public JsonValue Clone()
        {
            JsonValue result = new JsonValue(Type);
            switch (Type)
            {
                case JsonValueType.String:
                    result.String = String;
                    break;

                case JsonValueType.Boolean:
                    result.Boolean = Boolean;
                    break;

                case JsonValueType.Number:
                    result.Number = Number;
                    break;

                case JsonValueType.Object:
                    result.Obj = Obj.Clone();
                    break;

                case JsonValueType.Array:
                    result.Array = Array.Clone();
                    break;
            }
            return result;
        }

        public IEnumerator<JsonValue> GetEnumerator()
        {
            return ((IEnumerable<JsonValue>)Array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<JsonValue>)Array).GetEnumerator();
        }
    }
}
