using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MimiJson
{
    public struct JOPair
    {
        private string _Key;
        private JsonValue _Value;
        public string Key
        {
            get
            {
                return _Key;
            }
        }
        public JsonValue Value
        {
            get
            {
                return _Value;
            }
        }

        public JOPair(string key, JsonValue value)
        {
            _Key = key;
            _Value = value;
        }

        public static implicit operator KeyValuePair<string, JsonValue>(JOPair pair)
        {
            return new KeyValuePair<string, JsonValue>(pair.Key, pair.Value);
        }

        public static implicit operator JOPair(KeyValuePair<string, JsonValue> pair)
        {
            return new JOPair(pair.Key, pair.Value);
        }

    }


}
