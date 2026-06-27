using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Cardwin.Lua
{
    /// <summary>
    /// Minimal Lua-table value container produced by <see cref="SimpleLuaTableParser"/>.
    /// A table holds both a keyed map (key = value entries) and an ordered array
    /// (bare value entries, e.g. string lists like tags = { "a", "b" }).
    /// Values are: string, double, bool, LuaTable, or null (nil).
    ///
    /// NOTE: This is NOT a Lua VM. It only parses the *data table* syntax used by
    /// BulletRegistry.lua (return { ... }). Behaviour scripts with real Lua logic
    /// (functions / control flow) are NOT executed here — see LuaBulletBehaviors.
    /// Runtime-safe: no UnityEditor APIs, packable.
    /// </summary>
    public class LuaTable
    {
        public readonly Dictionary<string, object> Map = new Dictionary<string, object>();
        public readonly List<object> Array = new List<object>();

        public bool TryGet(string key, out object value) => Map.TryGetValue(key, out value);

        public LuaTable GetTable(string key)
        {
            return Map.TryGetValue(key, out object v) ? v as LuaTable : null;
        }

        public string GetString(string key, string fallback = "")
        {
            if (Map.TryGetValue(key, out object v) && v != null)
                return v as string ?? Convert.ToString(v, CultureInfo.InvariantCulture);
            return fallback;
        }

        public bool GetBool(string key, bool fallback = false)
        {
            if (Map.TryGetValue(key, out object v) && v is bool b)
                return b;
            return fallback;
        }

        public float GetFloat(string key, float fallback = 0f)
        {
            if (Map.TryGetValue(key, out object v) && v != null)
            {
                if (v is double d) return (float)d;
                if (float.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                    return f;
            }
            return fallback;
        }

        public int GetInt(string key, int fallback = 0)
        {
            if (Map.TryGetValue(key, out object v) && v != null)
            {
                if (v is double d) return (int)Math.Round(d);
                if (int.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out int i))
                    return i;
            }
            return fallback;
        }

        public string[] GetStringArray(string key)
        {
            LuaTable t = GetTable(key);
            if (t == null)
                return System.Array.Empty<string>();

            var list = new List<string>(t.Array.Count);
            foreach (object o in t.Array)
            {
                if (o != null)
                    list.Add(o as string ?? Convert.ToString(o, CultureInfo.InvariantCulture));
            }
            return list.ToArray();
        }
    }

    /// <summary>
    /// Recursive-descent parser for the constrained Lua data-table subset used by
    /// the bullet registry: return { key = value, ... } with nested tables, string
    /// arrays, numbers, booleans, nil and -- line comments.
    /// </summary>
    public static class SimpleLuaTableParser
    {
        public static LuaTable Parse(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new FormatException("Empty Lua source.");

            var tokenizer = new Tokenizer(source);
            var tokens = tokenizer.Tokenize();
            var parser = new Parser(tokens);
            return parser.ParseChunk();
        }

        public static bool TryParse(string source, out LuaTable table, out string error)
        {
            table = null;
            error = null;
            try
            {
                table = Parse(source);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        private enum TokenType { LBrace, RBrace, Assign, Comma, String, Number, Name, EOF }

        private struct Token
        {
            public TokenType Type;
            public string Text;
            public double Number;
        }

        private sealed class Tokenizer
        {
            private readonly string _s;
            private int _i;

            public Tokenizer(string s) { _s = s; }

            public List<Token> Tokenize()
            {
                var tokens = new List<Token>();
                while (true)
                {
                    SkipTrivia();
                    if (_i >= _s.Length)
                    {
                        tokens.Add(new Token { Type = TokenType.EOF });
                        return tokens;
                    }

                    char c = _s[_i];
                    switch (c)
                    {
                        case '{': _i++; tokens.Add(new Token { Type = TokenType.LBrace }); break;
                        case '}': _i++; tokens.Add(new Token { Type = TokenType.RBrace }); break;
                        case '=': _i++; tokens.Add(new Token { Type = TokenType.Assign }); break;
                        case ',':
                        case ';': _i++; tokens.Add(new Token { Type = TokenType.Comma }); break;
                        case '"':
                        case '\'': tokens.Add(ReadString(c)); break;
                        default:
                            if (c == '-' && Peek(1) == '-')
                            {
                                SkipLineComment();
                                break;
                            }
                            if (c == '-' || c == '+' || char.IsDigit(c) || (c == '.' && char.IsDigit(Peek(1))))
                            {
                                tokens.Add(ReadNumber());
                                break;
                            }
                            if (IsNameStart(c))
                            {
                                tokens.Add(ReadName());
                                break;
                            }
                            throw new FormatException($"Unexpected character '{c}' at index {_i}.");
                    }
                }
            }

            private char Peek(int offset)
            {
                int j = _i + offset;
                return j < _s.Length ? _s[j] : '\0';
            }

            private void SkipTrivia()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i]))
                    _i++;
            }

            private void SkipLineComment()
            {
                // Supports -- line comments and naive --[[ ]] block comments.
                _i += 2;
                if (_i + 1 < _s.Length && _s[_i] == '[' && _s[_i + 1] == '[')
                {
                    _i += 2;
                    while (_i + 1 < _s.Length && !(_s[_i] == ']' && _s[_i + 1] == ']'))
                        _i++;
                    _i = Math.Min(_s.Length, _i + 2);
                    return;
                }
                while (_i < _s.Length && _s[_i] != '\n')
                    _i++;
            }

            private Token ReadString(char quote)
            {
                _i++; // opening quote
                var sb = new StringBuilder();
                while (_i < _s.Length)
                {
                    char c = _s[_i++];
                    if (c == '\\' && _i < _s.Length)
                    {
                        char n = _s[_i++];
                        switch (n)
                        {
                            case 'n': sb.Append('\n'); break;
                            case 't': sb.Append('\t'); break;
                            case 'r': sb.Append('\r'); break;
                            case '"': sb.Append('"'); break;
                            case '\'': sb.Append('\''); break;
                            case '\\': sb.Append('\\'); break;
                            default: sb.Append(n); break;
                        }
                        continue;
                    }
                    if (c == quote)
                        return new Token { Type = TokenType.String, Text = sb.ToString() };
                    sb.Append(c);
                }
                throw new FormatException("Unterminated string literal.");
            }

            private Token ReadNumber()
            {
                int start = _i;
                if (_s[_i] == '-' || _s[_i] == '+') _i++;
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (char.IsDigit(c) || c == '.' || c == 'e' || c == 'E' || c == 'x' || c == 'X'
                        || ((c == '-' || c == '+') && (_s[_i - 1] == 'e' || _s[_i - 1] == 'E')))
                        _i++;
                    else
                        break;
                }
                string text = _s.Substring(start, _i - start);
                if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    throw new FormatException($"Invalid number '{text}'.");
                return new Token { Type = TokenType.Number, Number = value, Text = text };
            }

            private Token ReadName()
            {
                int start = _i;
                while (_i < _s.Length && IsNamePart(_s[_i]))
                    _i++;
                return new Token { Type = TokenType.Name, Text = _s.Substring(start, _i - start) };
            }

            private static bool IsNameStart(char c) => char.IsLetter(c) || c == '_';
            private static bool IsNamePart(char c) => char.IsLetterOrDigit(c) || c == '_';
        }

        private sealed class Parser
        {
            private readonly List<Token> _tokens;
            private int _i;

            public Parser(List<Token> tokens) { _tokens = tokens; }

            public LuaTable ParseChunk()
            {
                // Optional leading "return".
                if (Current.Type == TokenType.Name && Current.Text == "return")
                    _i++;

                object value = ParseValue();
                if (value is LuaTable table)
                    return table;
                throw new FormatException("Lua chunk root is not a table.");
            }

            private Token Current => _tokens[_i];

            private Token Advance() => _tokens[_i++];

            private void Expect(TokenType type)
            {
                if (Current.Type != type)
                    throw new FormatException($"Expected {type} but got {Current.Type} ('{Current.Text}').");
                _i++;
            }

            private object ParseValue()
            {
                Token t = Current;
                switch (t.Type)
                {
                    case TokenType.LBrace:
                        return ParseTable();
                    case TokenType.String:
                        _i++;
                        return t.Text;
                    case TokenType.Number:
                        _i++;
                        return t.Number;
                    case TokenType.Name:
                        _i++;
                        if (t.Text == "true") return true;
                        if (t.Text == "false") return false;
                        if (t.Text == "nil") return null;
                        return t.Text; // bare identifier value (rare) -> treat as string
                    default:
                        throw new FormatException($"Unexpected token {t.Type} when expecting a value.");
                }
            }

            private LuaTable ParseTable()
            {
                Expect(TokenType.LBrace);
                var table = new LuaTable();

                while (Current.Type != TokenType.RBrace && Current.Type != TokenType.EOF)
                {
                    // key = value  (key is a Name followed by '=')
                    if (Current.Type == TokenType.Name && _tokens[_i + 1].Type == TokenType.Assign)
                    {
                        string key = Advance().Text;
                        Expect(TokenType.Assign);
                        object val = ParseValue();
                        table.Map[key] = val;
                    }
                    else
                    {
                        // array element
                        table.Array.Add(ParseValue());
                    }

                    if (Current.Type == TokenType.Comma)
                        _i++;
                    else
                        break;
                }

                Expect(TokenType.RBrace);
                return table;
            }
        }
    }
}
