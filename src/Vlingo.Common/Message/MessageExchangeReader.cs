using System;
using System.Numerics;
using Vlingo.Common.Message;

namespace io.vlingo.common.message {
    public class MessageExchangeReader : MessageReader {
        private readonly IMessage message;

        public static MessageExchangeReader from(IMessage message) {
            return new MessageExchangeReader(message);
        }

        //==============================================
        // message header
        //==============================================

        public string id() {
            return message.Id;
        }

        public long idAsLong() {
            return long.Parse(id());
        }

        public string type() {
            return message.Type;
        }

        //==============================================
        // message payload
        //==============================================

        public BigInteger payloadBigDecimalValue(params string[] keys) {
            String value = stringValue(keys);
            return value == null ? 0 : BigInteger.Parse(value);
        }

        public bool payloadBooleanValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? false : bool.Parse(value);
        }

        public DateTime payloadDateValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? DateTime.MinValue : DateTime.Parse(value);
        }

        public double payloadDoubleValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? 0 : double.Parse(value);
        }

        public float payloadFloatValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? 0 : float.Parse(value);
        }

        public int payloadIntegerValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? 0 : int.Parse(value);
        }

        public long payloadLongValue(params string[] keys) {
            string value = stringValue(keys);
            return value == null ? 0 : long.Parse(value);
        }

        public string payloadStringValue(params string[] keys) {
            string value = stringValue(keys);
            return value;
        }

        private MessageExchangeReader(IMessage message) : base(message.Payload<string>()) {
            this.message = message;
        }
    }
}
