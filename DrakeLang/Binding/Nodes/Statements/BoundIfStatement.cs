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

using System.Collections.Generic;
using System.Collections.Immutable;

namespace DrakeLang.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, ImmutableArray<BoundStatement> thenBody, ImmutableArray<BoundStatement> elseBody)
        {
            Condition = condition;
            ThenBody = thenBody;
            ElseBody = elseBody;
        }

        #region Properties

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

        public BoundExpression Condition { get; }
        public ImmutableArray<BoundStatement> ThenBody { get; }
        public ImmutableArray<BoundStatement> ElseBody { get; }

        #endregion Properties

        public override IEnumerable<BoundNode> GetChildren()
        {
            yield return Condition;
            foreach (var statement in ThenBody)
            {
                yield return statement;
            }

            foreach (var statement in ElseBody)
            {
                yield return statement;
            }
        }
    }
}