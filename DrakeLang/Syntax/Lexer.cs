﻿//------------------------------------------------------------------------------
// DrakeLang - Viv's C#-esque sandbox.
// Copyright (C) 2019  Vivian Vea
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//------------------------------------------------------------------------------

using DrakeLang.Text;
using System.Text;

namespace DrakeLang.Syntax
{
    internal sealed class Lexer
    {
        private readonly SourceText _text;

        private int _position;
        private int _start;
        private SyntaxKind _kind;
        private object? _value;

        public Lexer(SourceText text)
        {
            _text = text;
            Diagnostics = new(text);
        }

        #region Private properties

        public DiagnosticsBuilder Diagnostics { get; }

        private char Current => Peek(0);

        private char LookAhead => Peek(1);

        #endregion Private properties

        #region Methods

        /// <summary>
        /// Lexes the next token in the source text.
        /// </summary>
        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            if (TryLookupTokenKind(out SyntaxKind kind))
                _kind = kind;
            else
            {
                switch (Current)
                {
                    case '\'':
                        ReadChar();
                        break;

                    case '"':
                        ReadString();
                        break;

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
                    case '.':
                        ReadNumberToken();
                        break;

                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        ReadWhitespace();
                        break;

                    default:
                        if (char.IsLetter(Current))
                            ReadIdentifierOrKeyword();
                        else if (char.IsWhiteSpace(Current))
                            ReadWhitespace();
                        else
                        {
                            Diagnostics.ReportBadCharacter(_position, Current);
                            Next();
                        }
                        break;
                }
            }

            string? text = _kind.GetText();
            if (text is null)
            {
                int length = _position - _start;
                text = _text.ToString(_start, length);
            }

            return new SyntaxToken(_text, _kind, _start, text, _value);
        }

        #endregion Methods

        #region Private methods

        private bool TryLookupTokenKind(out SyntaxKind syntaxKind)
        {
            switch (Current)
            {
                case '\0':
                    syntaxKind = SyntaxKind.EndOfFileToken;
                    return true;

                case ':':
                    Next();
                    syntaxKind = SyntaxKind.ColonToken;
                    return true;

                case ';':
                    Next();
                    syntaxKind = SyntaxKind.SemicolonToken;
                    return true;

                case '+':
                    Next();
                    if (Current == '+')
                    {
                        Next();
                        syntaxKind = SyntaxKind.PlusPlusToken;
                    }
                    else if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.PlusEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.PlusToken;
                    return true;

                case '-':
                    Next();
                    if (Current == '-')
                    {
                        Next();
                        syntaxKind = SyntaxKind.MinusMinusToken;
                    }
                    else if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.MinusEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.MinusToken;
                    return true;

                case '*':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.StarEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.StarToken;
                    return true;

                case '/':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.SlashEqualsToken;
                    }
                    else if (Current == '/')
                    {
                        do
                        {
                            Next();
                        }
                        while (Current != '\n' && Current != '\r' && Current != '\0');

                        syntaxKind = SyntaxKind.LineCommentToken;
                    }
                    else if (Current == '*')
                    {
                        do
                        {
                            Next();
                        } while ((Peek(-2) != '*' || Peek(-1) != '/') && Current != '\0');

                        syntaxKind = SyntaxKind.MultiLineCommentToken;
                    }
                    else
                        syntaxKind = SyntaxKind.SlashToken;
                    return true;

                case '%':
                    Next();
                    syntaxKind = SyntaxKind.PercentToken;
                    return true;

                case '(':
                    Next();
                    syntaxKind = SyntaxKind.OpenParenthesisToken;
                    return true;

                case ')':
                    Next();
                    syntaxKind = SyntaxKind.CloseParenthesisToken;
                    return true;

                case '{':
                    Next();
                    syntaxKind = SyntaxKind.OpenBraceToken;
                    return true;

                case '}':
                    Next();
                    syntaxKind = SyntaxKind.CloseBraceToken;
                    return true;

                case '[':
                    Next();
                    syntaxKind = SyntaxKind.OpenBracketToken;
                    return true;

                case ']':
                    Next();
                    syntaxKind = SyntaxKind.CloseBracketToken;
                    return true;

                case '.' when !char.IsNumber(LookAhead):
                    Next();
                    syntaxKind = SyntaxKind.DotToken;
                    return true;

                case ',':
                    Next();
                    syntaxKind = SyntaxKind.CommaToken;
                    return true;

                case '_':
                    Next();
                    syntaxKind = SyntaxKind.UnderscoreToken;
                    return true;

                case '~':
                    Next();
                    syntaxKind = SyntaxKind.TildeToken;
                    return true;

                case '^':
                    Next();
                    syntaxKind = SyntaxKind.HatToken;
                    return true;

                case '&':
                    Next();
                    if (Current == '&')
                    {
                        Next();
                        syntaxKind = SyntaxKind.AmpersandAmpersandToken;
                    }
                    else if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.AmpersandEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.AmpersandToken;
                    return true;

                case '|':
                    Next();
                    if (Current == '|')
                    {
                        Next();
                        syntaxKind = SyntaxKind.PipePipeToken;
                    }
                    else if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.PipeEqualsToken;
                    }
                    else if (Current == '>')
                    {
                        Next();
                        syntaxKind = SyntaxKind.PipeGreaterToken;
                    }
                    else
                        syntaxKind = SyntaxKind.PipeToken;
                    return true;

                case '=':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.EqualsEqualsToken;
                    }
                    else if (Current == '>')
                    {
                        Next();
                        syntaxKind = SyntaxKind.EqualsGreaterToken;
                    }
                    else
                        syntaxKind = SyntaxKind.EqualsToken;
                    return true;

                case '!':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.BangEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.BangToken;
                    return true;

                case '<':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.LessEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.LessToken;
                    return true;

                case '>':
                    Next();
                    if (Current == '=')
                    {
                        Next();
                        syntaxKind = SyntaxKind.GreaterEqualsToken;
                    }
                    else
                        syntaxKind = SyntaxKind.GreaterToken;
                    return true;

                default:
                    syntaxKind = default;
                    return false;
            }
        }

        private void ReadChar()
        {
            // Skip quote start.
            _kind = SyntaxKind.CharToken;
            Next();

            if (Current is '\'')
            {
                var span = new TextSpan(_start, 2);
                Diagnostics.ReportEmptyCharacterLiteral(span);

                _value = default(char);
                Next();
                return;
            }

            bool escape = false;
            if (Current is '\\')
            {
                _position++;
                escape = true;
            }

            if (Current is '\n' or '\r' || LookAhead is not '\'')
            {
                var span = new TextSpan(_start, 1);
                Diagnostics.ReportUnterminatedCharacterLiteral(span);
                _value = default(char);

                // don't consume any more characters.
                return;
            }

            if (escape && TryEscapeCharacter(contextIsStr: false, out var escaped))
                _value = escaped;
            else
                _value = Current;

            _position += 2;
        }

        private void ReadString()
        {
            // Skip quote start.
            _position++;

            var sb = new StringBuilder();

            bool done = false;
            bool escape = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        TextSpan span = new TextSpan(_start, 1);
                        Diagnostics.ReportUnterminatedString(span);
                        done = true;
                        break;

                    case '"' when !escape:
                        _position++;
                        done = true;
                        break;

                    case '\\' when !escape:
                        _position++;
                        escape = true;
                        break;

                    default:
                        if (escape)
                        {
                            if (TryEscapeCharacter(contextIsStr: true, out var escaped))
                                sb.Append(escaped);
                            else
                                sb.Append(Current);

                            escape = false;
                        }
                        else
                        {
                            sb.Append(Current);
                            escape = false;
                        }
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
        }

        private void ReadNumberToken()
        {
            // Keep reading digits as long as they're available,
            // as well as a single decimal separator *if* the following char is a digit as well.
            bool isFloat = false;
            while (char.IsDigit(Current) || (!isFloat && Current == '.' && char.IsDigit(LookAhead)))
            {
                Next();
                isFloat |= Peek(-1) == '.';
            }

            if (Current == 'f')
            {
                Next();
                isFloat = true;
            }

            _kind = isFloat ? SyntaxKind.FloatToken : SyntaxKind.IntegerToken;
        }

        private void ReadWhitespace()
        {
            while (char.IsWhiteSpace(Current))
                Next();

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                Next();

            int length = _position - _start;
            string word = _text.ToString(_start, length);

            if (!SyntaxFacts.TryGetKeywordKind(word, out _kind))
            {
                _kind = SyntaxKind.IdentifierToken;
            }
        }

        #endregion Private methods

        #region Helpers

        private char Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        private void Next()
        {
            _position++;
        }

        private bool TryEscapeCharacter(bool contextIsStr, out char escapedChar)
        {
            if (IsEscapable(Current, contextIsStr, out escapedChar))
                return true;

            var span = new TextSpan(_position - 1, 2);
            Diagnostics.ReportUnrecognizedEscapeSequence(span);
            return false;
        }

        #endregion Helpers

        #region Utilities

        private static bool IsEscapable(char c, bool contextIsStr, out char escapedChar)
        {
            switch (c)
            {
                case '\\':
                    escapedChar = '\\';
                    return true;

                case '0':
                    escapedChar = '\0';
                    return true;

                case 'n':
                    escapedChar = '\n';
                    return true;

                case 'r':
                    escapedChar = '\r';
                    return true;

                case 't':
                    escapedChar = '\t';
                    return true;

                case '\'':
                    escapedChar = '\'';
                    return !contextIsStr;

                case '"':
                    escapedChar = '"';
                    return contextIsStr;

                default:
                    escapedChar = default;
                    return false;
            }
        }

        #endregion Utilities
    }
}