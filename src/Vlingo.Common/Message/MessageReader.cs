using Newtonsoft.Json.Linq;
using System;
using System.Numerics;

namespace io.vlingo.common.message {
    public abstract class MessageReader {
        private JObject representation;

        public MessageReader(string jsonRepresentation) {
            initialize(jsonRepresentation);
        }

        public MessageReader(JObject jsonRepresentation) {
            this.representation = jsonRepresentation;
        }

        public JArray arrayConverter(params string[] keys) {
            JArray array = null;

            JToken element = navigateTo(representationer(), keys);

            if (element != null) {
                array = (JArray)element;
            }

            return array;
        }

        public BigInteger bigDecimalValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? 0 : BigInteger.Parse(value);
        }

        public bool booleanValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? false : bool.Parse(value);
        }

        public DateTime dateValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? DateTime.MinValue : DateTime.Parse(value);
        }

        public double doubleValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? double.MinValue : double.Parse(value);
        }

        public float floatValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? 0 : float.Parse(value);
        }

        public int integervalue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? 0 : int.Parse(value);
        }

        public long longValue(params string[] keys) {
            string value = stringValue(keys);

            return value == null ? 0 : Int64.Parse(value);
        }

        public string stringValue(params string[] keys) {
            return stringValue(representationer(), keys);
        }

        public String[] stringArrayValue(params string[] keys) {
            JArray array = arrayConverter(keys);

            if (array != null) {
                int size = array.Count;
                string[] stringArray = new String[size];
                int idx = 0;
                foreach (var item in array) {
                    var itemProperties = item.Children<JProperty>();
                    stringArray[idx] = itemProperties[idx].Value<string>();
                    idx++;
                }
                return stringArray;
            }
            return new String[0];
        }

        //==============================================
        // internal implementation
        //==============================================

        protected JToken elementFrom(JObject jsonObject, string key) {
            JToken element = jsonObject.GetValue(key);

            if (element == null) {
                element = jsonObject.GetValue(string.Concat("@", key));
            }

            return element;
        }

        protected JToken navigateTo(JObject startingJsonObject, params string[] keys) {
            if (keys.Length == 0) {
                throw new ArgumentException("Must specify one or more keys.");
            }

            int keyIndex = 1;

            JToken element = elementFrom(startingJsonObject, keys[0]);

            if (element.Type != JTokenType.Null/* && element.Type != JTokenType.??? -!element.isJsonPrimitive()-*/ && element.Type != JTokenType.Array) {
                JObject obj = element.ToObject<JObject>();

                for (; element != null /*&& !element.isJsonPrimitive()*/ && keyIndex < keys.Length; ++keyIndex) {

                    element = elementFrom(obj, keys[keyIndex]);

                    if (/*!element.isJsonPrimitive()*/true) {

                        element = elementFrom(obj, keys[keyIndex]);

                        if (element.Type == JTokenType.Null) {
                            element = null;
                        }
                        else {
                            obj = element.ToObject<JObject>();
                        }
                    }
                }
            }

            if (element != null) {
                if (element.Type != JTokenType.Null) {
                    if (keyIndex != keys.Length) {
                        throw new ArgumentException("Last name must reference a simple value.");
                    }
                }
                else {
                    element = null;
                }
            }

            return element;
        }

        protected JObject parse(string jsonRepresentation) {
            try {
                JObject jsonObject = JObject.Parse(jsonRepresentation);
                return jsonObject;
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                throw new SystemException(e.Message, e.InnerException);
            }
        }

        protected JObject representationer() {
            return representation;
        }

        protected String stringValue(JObject startingJsonObject, params string[] keys) {
            String value = null;

            JToken element = navigateTo(startingJsonObject, keys);

            if (element != null) {
                value = (string)element;
            }

            return value;
        }

        private void initialize(string jsonRepresentation) {
            this.representation = parse(jsonRepresentation);
        }
    }
}
