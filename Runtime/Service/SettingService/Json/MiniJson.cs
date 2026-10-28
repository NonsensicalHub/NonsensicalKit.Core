using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NonsensicalKit.Setting.Json
{
    /// <summary>
    /// 轻量 JSON 解析/序列化，兼容 Unity 2018.4+，无第三方依赖。
    /// </summary>
    public static class MiniJson
    {
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return Parser.Parse(json);
        }

        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        private sealed class Parser : IDisposable
        {
            private const string WordBreak = "{}[],:\"";
            private readonly StringReader _reader;

            private Parser(string json)
            {
                _reader = new StringReader(json);
            }

            public static object Parse(string json)
            {
                using (Parser parser = new Parser(json))
                {
                    return parser.ParseValue();
                }
            }

            public void Dispose()
            {
                _reader.Dispose();
            }

            private Dictionary<string, object> ParseObject()
            {
                Dictionary<string, object> table = new Dictionary<string, object>();
                _reader.Read();
                while (true)
                {
                    TOKEN next = NextToken;
                    if (next == TOKEN.NONE)
                    {
                        return null;
                    }

                    if (next == TOKEN.CURLY_CLOSE)
                    {
                        return table;
                    }

                    if (next == TOKEN.COMMA)
                    {
                        continue;
                    }

                    string name = ParseString();
                    if (name == null)
                    {
                        return null;
                    }

                    if (NextToken != TOKEN.COLON)
                    {
                        return null;
                    }

                    _reader.Read();
                    table[name] = ParseValue();
                }
            }

            private List<object> ParseArray()
            {
                List<object> array = new List<object>();
                _reader.Read();
                bool parsing = true;
                while (parsing)
                {
                    TOKEN next = NextToken;
                    switch (next)
                    {
                        case TOKEN.NONE:
                            return null;
                        case TOKEN.SQUARED_CLOSE:
                            parsing = false;
                            break;
                        case TOKEN.COMMA:
                            break;
                        default:
                            object value = ParseByToken(next);
                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseValue()
            {
                return ParseByToken(NextToken);
            }

            private object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.STRING:
                        return ParseString();
                    case TOKEN.NUMBER:
                        return ParseNumber();
                    case TOKEN.CURLY_OPEN:
                        return ParseObject();
                    case TOKEN.SQUARED_OPEN:
                        return ParseArray();
                    case TOKEN.TRUE:
                        return true;
                    case TOKEN.FALSE:
                        return false;
                    case TOKEN.NULL:
                        return null;
                    default:
                        return null;
                }
            }

            private string ParseString()
            {
                StringBuilder s = new StringBuilder();
                _reader.Read();
                bool parsing = true;
                while (parsing)
                {
                    if (_reader.Peek() == -1)
                    {
                        parsing = false;
                        break;
                    }

                    char c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (_reader.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    char[] hex = new char[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }

                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }

                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber()
            {
                string number = NextWord;
                if (number.IndexOf('.') == -1 && number.IndexOf('e') == -1 && number.IndexOf('E') == -1)
                {
                    long parsedInt;
                    if (long.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedInt))
                    {
                        return parsedInt;
                    }
                }

                double parsedDouble;
                if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDouble))
                {
                    return parsedDouble;
                }

                return 0;
            }

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    _reader.Read();
                    if (_reader.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private char PeekChar
            {
                get { return Convert.ToChar(_reader.Peek()); }
            }

            private char NextChar
            {
                get { return Convert.ToChar(_reader.Read()); }
            }

            private string NextWord
            {
                get
                {
                    StringBuilder word = new StringBuilder();
                    while (WordBreak.IndexOf(PeekChar) == -1 && !char.IsWhiteSpace(PeekChar))
                    {
                        word.Append(NextChar);
                        if (_reader.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private TOKEN NextToken
            {
                get
                {
                    EatWhitespace();
                    if (_reader.Peek() == -1)
                    {
                        return TOKEN.NONE;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return TOKEN.CURLY_OPEN;
                        case '}':
                            _reader.Read();
                            return TOKEN.CURLY_CLOSE;
                        case '[':
                            return TOKEN.SQUARED_OPEN;
                        case ']':
                            _reader.Read();
                            return TOKEN.SQUARED_CLOSE;
                        case ',':
                            _reader.Read();
                            return TOKEN.COMMA;
                        case '"':
                            return TOKEN.STRING;
                        case ':':
                            return TOKEN.COLON;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return TOKEN.NUMBER;
                    }

                    switch (NextWord)
                    {
                        case "false":
                            return TOKEN.FALSE;
                        case "true":
                            return TOKEN.TRUE;
                        case "null":
                            return TOKEN.NULL;
                    }

                    return TOKEN.NONE;
                }
            }

            private enum TOKEN
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            }
        }

        private sealed class Serializer
        {
            private readonly StringBuilder _builder = new StringBuilder();

            public static string Serialize(object obj)
            {
                Serializer instance = new Serializer();
                instance.SerializeValue(obj);
                return instance._builder.ToString();
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    _builder.Append("null");
                }
                else if (value is string)
                {
                    SerializeString((string)value);
                }
                else if (value is bool)
                {
                    _builder.Append(((bool)value) ? "true" : "false");
                }
                else if (value is IList)
                {
                    SerializeArray((IList)value);
                }
                else if (value is IDictionary)
                {
                    SerializeObject((IDictionary)value);
                }
                else if (value is char)
                {
                    SerializeString(new string((char)value, 1));
                }
                else
                {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                bool first = true;
                _builder.Append('{');
                foreach (object e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');
                    SerializeValue(obj[e]);
                    first = false;
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList array)
            {
                _builder.Append('[');
                bool first = true;
                for (int i = 0; i < array.Count; i++)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeValue(array[i]);
                    first = false;
                }

                _builder.Append(']');
            }

            private void SerializeString(string str)
            {
                _builder.Append('\"');
                char[] charArray = str.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    char c = charArray[i];
                    switch (c)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            int codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126)
                            {
                                _builder.Append(c);
                            }
                            else
                            {
                                _builder.Append("\\u");
                                _builder.Append(codepoint.ToString("x4"));
                            }

                            break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                if (value is float || value is int || value is uint || value is long || value is double ||
                    value is sbyte || value is byte || value is short || value is ushort || value is ulong ||
                    value is decimal)
                {
                    _builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                }
                else
                {
                    SerializeString(value.ToString());
                }
            }
        }
    }
}
