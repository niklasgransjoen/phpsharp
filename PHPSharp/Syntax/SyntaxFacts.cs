﻿//------------------------------------------------------------------------------
// PHP Sharp. Because PHP isn't good enough.
// Copyright (C) 2019  Niklas Gransjøen
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

namespace PHPSharp.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                    return 6;

                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5;

                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;

                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                    return 3;

                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;

                case SyntaxKind.PipePipeToken:
                    return 1;

                default:
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string word)
        {
            switch (word)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;

                case "false":
                    return SyntaxKind.FalseKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";

                case SyntaxKind.MinusToken:
                    return "-";

                case SyntaxKind.StarToken:
                    return "*";

                case SyntaxKind.SlashToken:
                    return "/";

                case SyntaxKind.BangToken:
                    return "!";

                case SyntaxKind.EqualsToken:
                    return "=";

                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";

                case SyntaxKind.PipePipeToken:
                    return "||";

                case SyntaxKind.EqualsEqualsToken:
                    return "==";

                case SyntaxKind.BangEqualsToken:
                    return "!=";

                case SyntaxKind.OpenParenthesisToken:
                    return "(";

                case SyntaxKind.CloseParenthesisToken:
                    return ")";

                case SyntaxKind.TrueKeyword:
                    return "true";

                case SyntaxKind.FalseKeyword:
                    return "false";

                default:
                    return null;
            }
        }
    }
}