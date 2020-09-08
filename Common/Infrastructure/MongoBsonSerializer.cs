using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Infrastructure
{
    public interface IMongoBsonSerializer<T>  // where T : class
    {
        T Deserialize(string content);

        /// <summary>
        /// Serialize(s) the object of type <c>T</c>.
        /// </summary>
        /// <param name="value">The instance of <c>T</c> to be serialized.</param>
        /// <param name="isISODateTime">
        /// 1. <c>true</c>, the datetime would serialize as iso datetime format which is consistent with mongodb, like "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        /// 2. <c>false</c>, the datetime would serialize as local datetime format, such as: "yyyy/MM/dd HH:mm:ss";
        /// 
        /// Note: 
        /// 1. the default value is false, which means serialize as local datetime format.
        /// 2. in common uses case, you could ommite this parameter for ui display usage, but in backup & restore scenario, 
        /// u must set this parameter as true!! because the mongodb driver only use  the iso datetime format to do serialization,
        /// so for consistence consideration, we must set this parameter to true!!!
        /// </param>
        /// <returns>Th serialized string.</returns>
        string Serializer(T value, bool isISODateTime = false);
    }

    public class MongoBsonSerializer<T> : IMongoBsonSerializer<T> //where T : class
    {
        #region Field

        private JsonWriterSettings m_jsonSerializerSettings;

        #endregion

        #region Constructor

        public MongoBsonSerializer()
        {
            this.m_jsonSerializerSettings = new JsonWriterSettings()
            {
                OutputMode = JsonOutputMode.Strict,
                MaxSerializationDepth = 1000
            };
        }

        #endregion

        #region Public Method


        public T Deserialize(string content)
        {
            if (content == null) throw new ArgumentNullException("content");

            T deserializedObject = BsonSerializer.Deserialize<T>(content);

            return deserializedObject;
        }

        /// <summary>
        /// Serialize(s) the object of type <c>T</c>.
        /// </summary>
        /// <param name="value">The instance of <c>T</c> to be serialized.</param>
        /// <param name="isISODateTime">
        /// 1. <c>true</c>, the datetime would serialize as iso datetime format which is consistent with mongodb, like "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        /// 2. <c>false</c>, the datetime would serialize as local datetime format, such as: "yyyy/MM/dd HH:mm:ss";
        /// 
        /// Note: 
        /// 1. the default value is false, which means serialize as local datetime format.
        /// 2. in common uses case, you could ommite this parameter for ui display usage, but in backup & restore scenario, 
        /// u must set this parameter as true!! because the mongodb driver only use  the iso datetime format to do serialization,
        /// so for consistence consideration, we must set this parameter to true!!!
        /// </param>
        /// <returns>Th serialized string.</returns>
        public string Serializer(T value, bool isISODateTime)
        {
            if (value == null) throw new ArgumentNullException("value");

            using (var streamWriter = new StringWriter())
            {
                using (var bsonWriter = new CustomerJsonWriter(streamWriter, this.m_jsonSerializerSettings, isISODateTime))
                {
                    IBsonSerializer serializer = BsonSerializer.LookupSerializer(typeof(T));
                    if (serializer.ValueType != typeof(T))
                    {
                        var message = string.Format("Serializer type {0} value type does not match document types {1}.",
                                                     serializer.GetType().FullName,
                                                     typeof(T).FullName);
                        throw new ArgumentException(message, "serializer");
                    }

                    Action<BsonSerializationContext.Builder> configurator = null;
                    var context = BsonSerializationContext.CreateRoot(bsonWriter, configurator);
                    BsonSerializationArgs args = new BsonSerializationArgs(typeof(T), false, true); //default(BsonSerializationArgs);
                    serializer.Serialize(context, args, value);
                }
                streamWriter.Flush();

                return streamWriter.ToString();
            }
        }

        #endregion
    }

    #region Nested Object

    public class CustomerJsonWriter : BsonWriter
    {
        #region Field

        // private fields
        private TextWriter _textWriter;
        private JsonWriterSettings _jsonWriterSettings; // same value as in base class just declared as derived class
        private JsonWriterContext _context;

        private const string ISODateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        private const string LocalDateTimeFormat = "yyyy/MM/dd HH:mm:ss";

        private bool isIsoDateTime;

        #endregion

        #region Property

        /// <summary>
        /// Get(s) or set(s) the iso datetime flag which indicate the datetime format.
        /// </summary>
        public bool IsIsoDateTime
        {
            get
            {
                return this.isIsoDateTime;
            }
            set
            {
                this.isIsoDateTime = value;
            }
        }

        #endregion


        #region Constructor

        // constructors
        /// <summary>
        /// Initializes a new instance of the JsonWriter class.
        /// </summary>
        /// <param name="writer">A TextWriter.</param>
        public CustomerJsonWriter(TextWriter writer, bool isIsoDateTime = false)
             : this(writer, JsonWriterSettings.Defaults, isIsoDateTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the JsonWriter class.
        /// </summary>
        /// <param name="writer">A TextWriter.</param>
        /// <param name="settings">Optional JsonWriter settings.</param>
        public CustomerJsonWriter(TextWriter writer, JsonWriterSettings settings, bool isIsoDateTime = false)
            : base(settings)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            _textWriter = writer;
            _jsonWriterSettings = settings; // already frozen by base class
            _context = new JsonWriterContext(null, ContextType.TopLevel, "");
            State = BsonWriterState.Initial;

            this.isIsoDateTime = isIsoDateTime;
        }

        #endregion

        #region Property

        // public properties
        /// <summary>
        /// Gets the base TextWriter.
        /// </summary>
        /// <value>
        /// The base TextWriter.
        /// </value>
        public TextWriter BaseTextWriter
        {
            get { return _textWriter; }
        }

        public override long Position
        {
            get { return 0L; }
        }

        #endregion

        #region Public Method

        // public methods
        /// <summary>
        /// Closes the writer.
        /// </summary>
        public override void Close()
        {
            // Close can be called on Disposed objects
            if (State != BsonWriterState.Closed)
            {
                Flush();
                _context = null;
                State = BsonWriterState.Closed;
            }
        }

        /// <summary>
        /// Flushes any pending data to the output destination.
        /// </summary>
        public override void Flush()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            _textWriter.Flush();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        public override void WriteBinaryData(BsonBinaryData binaryData)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteBinaryData", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var subType = binaryData.SubType;
            var bytes = binaryData.Bytes;
            var guidRepresentation = binaryData.GuidRepresentation;

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$binary\" : \"{0}\", \"$type\" : \"{1}\" }}", Convert.ToBase64String(bytes), ((int)subType).ToString("x2"));
                    break;

                case JsonOutputMode.Shell:
                default:
                    switch (subType)
                    {
                        case BsonBinarySubType.UuidLegacy:
                        case BsonBinarySubType.UuidStandard:
                            _textWriter.Write(GuidToString(subType, bytes, guidRepresentation));
                            break;

                        default:
                            _textWriter.Write("new BinData({0}, \"{1}\")", (int)subType, Convert.ToBase64String(bytes));
                            break;
                    }
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Boolean to the writer.
        /// </summary>
        /// <param name="value">The Boolean value.</param>
        public override void WriteBoolean(bool value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteBoolean", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write(value ? "true" : "false");

            State = GetNextState();
        }

        /// <summary>
        /// Writes BSON binary data to the writer.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public override void WriteBytes(byte[] bytes)
        {
            WriteBinaryData(new BsonBinaryData(bytes, BsonBinarySubType.Binary));
        }

        /// <summary>
        /// Writes a BSON DateTime to the writer.
        /// </summary>
        /// <param name="value">The number of milliseconds since the Unix epoch.</param>
        public override void WriteDateTime(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteDateTime", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            var utcDateTime = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(value);
            if (this.isIsoDateTime)
            {
                _textWriter.Write(string.Format("\"{0}\"", utcDateTime.ToLocalTime().ToString(ISODateTimeFormat)));
            }
            else
            {
                _textWriter.Write(string.Format("\"{0}\"", utcDateTime.ToLocalTime().ToString(LocalDateTimeFormat)));
            }

            State = GetNextState();
        }


        /// <inheritdoc />
        public override void WriteDecimal128(Decimal128 value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteDecimal128", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Shell:
                    _textWriter.Write("NumberDecimal(\"{0}\")", value.ToString());
                    break;

                default:
                    _textWriter.Write("{{ \"$numberDecimal\" : \"{0}\" }}", value.ToString());
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Double to the writer.
        /// </summary>
        /// <param name="value">The Double value.</param>
        public override void WriteDouble(double value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteDouble", BsonWriterState.Value, BsonWriterState.Initial);
            }

            // if string representation looks like an integer add ".0" so that it looks like a double
            var stringRepresentation = MongoDB.Bson.IO.JsonConvert.ToString(value);
            if (Regex.IsMatch(stringRepresentation, @"^[+-]?\d+$"))
            {
                stringRepresentation += ".0";
            }

            WriteNameHelper(Name);
            _textWriter.Write(stringRepresentation);

            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON array to the writer.
        /// </summary>
        public override void WriteEndArray()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value)
            {
                ThrowInvalidState("WriteEndArray", BsonWriterState.Value);
            }

            base.WriteEndArray();
            _textWriter.Write("]");

            _context = _context.ParentContext;
            State = GetNextState();
        }

        /// <summary>
        /// Writes the end of a BSON document to the writer.
        /// </summary>
        public override void WriteEndDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Name)
            {
                ThrowInvalidState("WriteEndDocument", BsonWriterState.Name);
            }

            base.WriteEndDocument();
            if (_jsonWriterSettings.Indent && _context.HasElements)
            {
                _textWriter.Write(_jsonWriterSettings.NewLineChars);
                if (_context.ParentContext != null)
                {
                    _textWriter.Write(_context.ParentContext.Indentation);
                }
                _textWriter.Write("}");
            }
            else
            {
                _textWriter.Write(" }");
            }

            if (_context.ContextType == ContextType.ScopeDocument)
            {
                _context = _context.ParentContext;
                WriteEndDocument();
            }
            else
            {
                _context = _context.ParentContext;
            }

            if (_context == null)
            {
                State = BsonWriterState.Done;
            }
            else
            {
                State = GetNextState();
            }
        }

        /// <summary>
        /// Writes a BSON Int32 to the writer.
        /// </summary>
        /// <param name="value">The Int32 value.</param>
        public override void WriteInt32(int value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteInt32", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Int64 to the writer.
        /// </summary>
        /// <param name="value">The Int64 value.</param>
        public override void WriteInt64(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteInt64", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write(value);
                    break;

                case JsonOutputMode.Shell:
                default:
                    if (value >= int.MinValue && value <= int.MaxValue)
                    {
                        _textWriter.Write("NumberLong({0})", value);
                    }
                    else
                    {
                        _textWriter.Write("NumberLong(\"{0}\")", value);
                    }
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer.
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScript(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteJavaScript", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("{{ \"$code\" : \"{0}\" }}", EscapedString(code));

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON JavaScript to the writer (call WriteStartDocument to start writing the scope).
        /// </summary>
        /// <param name="code">The JavaScript code.</param>
        public override void WriteJavaScriptWithScope(string code)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteJavaScriptWithScope", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteStartDocument();
            WriteName("$code");
            WriteString(code);
            WriteName("$scope");

            State = BsonWriterState.ScopeDocument;
        }

        /// <summary>
        /// Writes a BSON MaxKey to the writer.
        /// </summary>
        public override void WriteMaxKey()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteMaxKey", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$maxKey\" : 1 }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("MaxKey");
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON MinKey to the writer.
        /// </summary>
        public override void WriteMinKey()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteMinKey", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$minKey\" : 1 }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("MinKey");
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON null to the writer.
        /// </summary>
        public override void WriteNull()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteNull", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("null");

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON ObjectId to the writer.
        /// </summary>
        /// <param name="objectId">The ObjectId.</param>
        public override void WriteObjectId(ObjectId objectId)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteObjectId", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var bytes = objectId.ToByteArray();

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    //  _textWriter.Write("{{ \"$oid\" : \"{0}\" }}", BsonUtils.ToHexString(bytes));
                    _textWriter.Write("\"{0}\"", BsonUtils.ToHexString(bytes));
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("ObjectId(\"{0}\")", BsonUtils.ToHexString(bytes));
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON regular expression to the writer.
        /// </summary>
        /// <param name="regex">A BsonRegularExpression.</param>
        public override void WriteRegularExpression(BsonRegularExpression regex)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteRegularExpression", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var pattern = regex.Pattern;
            var options = regex.Options;

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$regex\" : \"{0}\", \"$options\" : \"{1}\" }}", EscapedString(pattern), EscapedString(options));
                    break;

                case JsonOutputMode.Shell:
                default:
                    var escapedPattern = (pattern == "") ? "(?:)" : pattern.Replace("/", @"\/");
                    _textWriter.Write("/{0}/{1}", escapedPattern, options);
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes the start of a BSON array to the writer.
        /// </summary>
        public override void WriteStartArray()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteStartArray", BsonWriterState.Value, BsonWriterState.Initial);
            }

            base.WriteStartArray();
            WriteNameHelper(Name);
            _textWriter.Write("[");

            _context = new JsonWriterContext(_context, ContextType.Array, _jsonWriterSettings.IndentChars);
            State = BsonWriterState.Value;
        }

        /// <summary>
        /// Writes the start of a BSON document to the writer.
        /// </summary>
        public override void WriteStartDocument()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial && State != BsonWriterState.ScopeDocument)
            {
                ThrowInvalidState("WriteStartDocument", BsonWriterState.Value, BsonWriterState.Initial, BsonWriterState.ScopeDocument);
            }

            base.WriteStartDocument();
            if (State == BsonWriterState.Value || State == BsonWriterState.ScopeDocument)
            {
                WriteNameHelper(Name);
            }
            _textWriter.Write("{");

            var contextType = (State == BsonWriterState.ScopeDocument) ? ContextType.ScopeDocument : ContextType.Document;
            _context = new JsonWriterContext(_context, contextType, _jsonWriterSettings.IndentChars);
            State = BsonWriterState.Name;
        }

        /// <summary>
        /// Writes a BSON String to the writer.
        /// </summary>
        /// <param name="value">The String value.</param>
        public override void WriteString(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteString", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            WriteQuotedString(value);

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON Symbol to the writer.
        /// </summary>
        /// <param name="value">The symbol.</param>
        public override void WriteSymbol(string value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteSymbol", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            _textWriter.Write("{{ \"$symbol\" : \"{0}\" }}", EscapedString(value));

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON timestamp to the writer.
        /// </summary>
        /// <param name="value">The combined timestamp/increment value.</param>
        public override void WriteTimestamp(long value)
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteTimestamp", BsonWriterState.Value, BsonWriterState.Initial);
            }

            var secondsSinceEpoch = (int)((value >> 32) & 0xffffffff);
            var increment = (int)(value & 0xffffffff);

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{{ \"$timestamp\" : {{ \"t\" : {0}, \"i\" : {1} }} }}", secondsSinceEpoch, increment);
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("Timestamp({0}, {1})", secondsSinceEpoch, increment);
                    break;
            }

            State = GetNextState();
        }

        /// <summary>
        /// Writes a BSON undefined to the writer.
        /// </summary>
        public override void WriteUndefined()
        {
            if (Disposed) { throw new ObjectDisposedException("JsonWriter"); }
            if (State != BsonWriterState.Value && State != BsonWriterState.Initial)
            {
                ThrowInvalidState("WriteUndefined", BsonWriterState.Value, BsonWriterState.Initial);
            }

            WriteNameHelper(Name);
            switch (_jsonWriterSettings.OutputMode)
            {
                case JsonOutputMode.Strict:
                    _textWriter.Write("{ \"$undefined\" : true }");
                    break;

                case JsonOutputMode.Shell:
                default:
                    _textWriter.Write("undefined");
                    break;
            }

            State = GetNextState();
        }

        #endregion

        #region Private Method

        // protected methods
        /// <summary>
        /// Disposes of any resources used by the writer.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Close();
                }
                catch { } // ignore exceptions
            }
            base.Dispose(disposing);
        }

        // private methods
        private string EscapedString(string value)
        {
            if (value.All(c => !NeedsEscaping(c)))
            {
                return value;
            }

            var sb = new StringBuilder(value.Length);

            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        switch (CharUnicodeInfo.GetUnicodeCategory(c))
                        {
                            case UnicodeCategory.UppercaseLetter:
                            case UnicodeCategory.LowercaseLetter:
                            case UnicodeCategory.TitlecaseLetter:
                            case UnicodeCategory.OtherLetter:
                            case UnicodeCategory.DecimalDigitNumber:
                            case UnicodeCategory.LetterNumber:
                            case UnicodeCategory.OtherNumber:
                            case UnicodeCategory.SpaceSeparator:
                            case UnicodeCategory.ConnectorPunctuation:
                            case UnicodeCategory.DashPunctuation:
                            case UnicodeCategory.OpenPunctuation:
                            case UnicodeCategory.ClosePunctuation:
                            case UnicodeCategory.InitialQuotePunctuation:
                            case UnicodeCategory.FinalQuotePunctuation:
                            case UnicodeCategory.OtherPunctuation:
                            case UnicodeCategory.MathSymbol:
                            case UnicodeCategory.CurrencySymbol:
                            case UnicodeCategory.ModifierSymbol:
                            case UnicodeCategory.OtherSymbol:
                                sb.Append(c);
                                break;
                            default:
                                sb.AppendFormat("\\u{0:x4}", (int)c);
                                break;
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        private BsonWriterState GetNextState()
        {
            if (_context.ContextType == ContextType.Array || _context.ContextType == ContextType.TopLevel)
            {
                return BsonWriterState.Value;
            }
            else
            {
                return BsonWriterState.Name;
            }
        }

        private string GuidToString(BsonBinarySubType subType, byte[] bytes, GuidRepresentation guidRepresentation)
        {
            if (bytes.Length != 16)
            {
                var message = string.Format("Length of binary subtype {0} must be 16, not {1}.", subType, bytes.Length);
                throw new ArgumentException(message);
            }
            if (subType == BsonBinarySubType.UuidLegacy && guidRepresentation == GuidRepresentation.Standard)
            {
                throw new ArgumentException("GuidRepresentation for binary subtype UuidLegacy must not be Standard.");
            }
            if (subType == BsonBinarySubType.UuidStandard && guidRepresentation != GuidRepresentation.Standard)
            {
                var message = string.Format("GuidRepresentation for binary subtype UuidStandard must be Standard, not {0}.", guidRepresentation);
                throw new ArgumentException(message);
            }

            if (guidRepresentation == GuidRepresentation.Unspecified)
            {
                var s = BsonUtils.ToHexString(bytes);
                var parts = new string[]
               {
                    s.Substring(0, 8),
                    s.Substring(8, 4),
                    s.Substring(12, 4),
                    s.Substring(16, 4),
                    s.Substring(20, 12)
               };
                return string.Format("HexData({0}, \"{1}\")", (int)subType, string.Join("-", parts));
            }
            else
            {
                string uuidConstructorName;
                switch (guidRepresentation)
                {
                    case GuidRepresentation.CSharpLegacy: uuidConstructorName = "CSUUID"; break;
                    case GuidRepresentation.JavaLegacy: uuidConstructorName = "JUUID"; break;
                    case GuidRepresentation.PythonLegacy: uuidConstructorName = "PYUUID"; break;
                    case GuidRepresentation.Standard: uuidConstructorName = "UUID"; break;
                    default: throw new BsonInternalException("Unexpected GuidRepresentation");
                }
                var guid = GuidConverter.FromBytes(bytes, guidRepresentation);
                return string.Format("{0}(\"{1}\")", uuidConstructorName, guid.ToString());
            }
        }

        private bool NeedsEscaping(char c)
        {
            switch (c)
            {
                case '"':
                case '\\':
                case '\b':
                case '\f':
                case '\n':
                case '\r':
                case '\t':
                    return true;

                default:
                    switch (CharUnicodeInfo.GetUnicodeCategory(c))
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                        case UnicodeCategory.OtherLetter:
                        case UnicodeCategory.DecimalDigitNumber:
                        case UnicodeCategory.LetterNumber:
                        case UnicodeCategory.OtherNumber:
                        case UnicodeCategory.SpaceSeparator:
                        case UnicodeCategory.ConnectorPunctuation:
                        case UnicodeCategory.DashPunctuation:
                        case UnicodeCategory.OpenPunctuation:
                        case UnicodeCategory.ClosePunctuation:
                        case UnicodeCategory.InitialQuotePunctuation:
                        case UnicodeCategory.FinalQuotePunctuation:
                        case UnicodeCategory.OtherPunctuation:
                        case UnicodeCategory.MathSymbol:
                        case UnicodeCategory.CurrencySymbol:
                        case UnicodeCategory.ModifierSymbol:
                        case UnicodeCategory.OtherSymbol:
                            return false;

                        default:
                            return true;
                    }
            }
        }

        private void WriteNameHelper(string name)
        {
            switch (_context.ContextType)
            {
                case ContextType.Array:
                    // don't write Array element names in Json
                    if (_context.HasElements)
                    {
                        _textWriter.Write(", ");
                    }
                    break;
                case ContextType.Document:
                case ContextType.ScopeDocument:
                    if (_context.HasElements)
                    {
                        _textWriter.Write(",");
                    }
                    if (_jsonWriterSettings.Indent)
                    {
                        _textWriter.Write(_jsonWriterSettings.NewLineChars);
                        _textWriter.Write(_context.Indentation);
                    }
                    else
                    {
                        _textWriter.Write(" ");
                    }
                    WriteQuotedString(name);
                    _textWriter.Write(" : ");
                    break;
                case ContextType.TopLevel:
                    break;
                default:
                    throw new BsonInternalException("Invalid ContextType.");
            }

            _context.HasElements = true;
        }

        private void WriteQuotedString(string value)
        {
            _textWriter.Write("\"");
            _textWriter.Write(EscapedString(value));
            _textWriter.Write("\"");
        }

        #endregion
    }

    /// <summary>
    /// 完全拷贝mongo的JsonWriterContext类，问题在于mongo的该类标记为internal ，外部无法访问
    /// </summary>
    internal class JsonWriterContext
    {
        #region Field

        private JsonWriterContext _parentContext;
        private ContextType _contextType;
        private string _indentation;
        private bool _hasElements = false;

        #endregion

        #region Property

        internal JsonWriterContext ParentContext
        {
            get { return _parentContext; }
        }

        internal ContextType ContextType
        {
            get { return _contextType; }
        }

        internal string Indentation
        {
            get { return _indentation; }
        }

        internal bool HasElements
        {
            get { return _hasElements; }
            set { _hasElements = value; }
        }

        #endregion

        #region Constructor

        internal JsonWriterContext(JsonWriterContext parentContext, ContextType contextType, string indentChars)
        {
            _parentContext = parentContext;
            _contextType = contextType;
            _indentation = (parentContext == null) ? indentChars : parentContext.Indentation + indentChars;
        }

        #endregion
    }

    #endregion
}
