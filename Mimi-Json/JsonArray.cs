using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MimiJson
{
    public class JSONArray : IEnumerable<JsonValue>
    {
        private readonly List<JsonValue> values = new List<JsonValue>();

        #region constructors

        public JSONArray()
        {
        }

        public JSONArray(IEnumerable<JsonValue> args):this()
        {
            foreach (var arg in args)
                Add(arg);
        }

        public JSONArray(params JsonValue[] args):this((IEnumerable<JsonValue>)args)
        {
        }

        #endregion

        public void Add(JsonValue value)
        {
            values.Add(value);
        }

        public void Remove(int index)
        {
            if (index >= 0 && index < values.Count)
            {
                values.RemoveAt(index);
            }
            else
            {
                JSONLogger.Error("index out of range: " + index + " (Expected 0 <= index < " + values.Count + ")");
            }
        }

        public void Clear()
        {
            values.Clear();
        }

        public JsonValue GetValue(int index)
        {
            JsonValue value = values[index];
            if (value.Type == JsonValueType.Refer)
                return value.Refer;
            else
                return value;
        }

        public JsonValue this[int index]
        {
            get { return GetValue(index); }
            set { values[index] = value; }
        }

        public int Length
        {
            get { return values.Count; }
        }

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
            var stringBuilder = new StringBuilder();
            string ptab = "";
            for (int i = 0; i < tabs; i++) ptab += JSONObject.Tab;
            string tab = "";
            for (int i = 0; i <= tabs; i++) tab += JSONObject.Tab;
            stringBuilder.Append(formatted ? "[\r\n" : "[");

            foreach (var value in values)
            {
                stringBuilder.Append(tab);
                stringBuilder.Append(value.ToString(formatted ? (tabs + 1) : -1, formatted));
                stringBuilder.Append(formatted ? ",\r\n" : ",");
            }
            if (values.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - (formatted ? 3 : 1), 1);
            }
            stringBuilder.Append(ptab);
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        public IEnumerator<JsonValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public static JSONArray Parse(string jsonString, ref int position)
        {
            if (string.IsNullOrEmpty(jsonString))
                return null;
            
            JSONArray arr;

            Parser.SkipWhitespace(jsonString, ref position);

            if (Parser.CheckArrayStart(jsonString, position))
            {
                arr = new JSONArray();
                position++;
                Parser.SkipWhitespace(jsonString, ref position);
                
                while (!Parser.CheckArrayEnd(jsonString, position))
                {
                    if (position >= jsonString.Length)
                        return arr;
                    
                    JsonValue value = JsonValue.Parse(jsonString, ref position);
                    
                    if (value != null)
                        arr.Add(value);
                    if (Parser.CheckValueSeparator(jsonString, position))
                    {
                        position++;
                        Parser.SkipWhitespace(jsonString, ref position);
                       
                    }
                }
                return arr;
            }

            JSONLogger.Error("Unexpected end of string");
            return null;
        }

        /// <summary>
        /// Concatenate two JSONArrays
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>A new JSONArray that is the result of adding all of the right-hand side array's values to the left-hand side array.</returns>
        public static JSONArray operator +(JSONArray lhs, JSONArray rhs)
        {
            var result = lhs.Clone();
            foreach (var value in rhs.values)
            {
                result.Add(value.Clone());
            }
            return result;
        }

        public JSONArray Clone()
        {
            JSONArray result = new JSONArray();
            foreach (JsonValue v in values)
            {
                result.Add(v.Clone());
            }
            return result;
        }
    }
}
