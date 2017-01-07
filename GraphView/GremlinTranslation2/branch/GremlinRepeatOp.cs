﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinRepeatOp: GremlinTranslationOperator
    {
        public Predicate TerminationPredicate { get; set; }
        public Predicate EmitPredicate { get; set; }
        public GraphTraversal2 TerminationTraversal { get; set; }
        public GraphTraversal2 EmitTraversal { get; set; }
        public GraphTraversal2 RepeatTraversal { get; set; }
        public int RepeatTimes { get; set; }
        public bool StartFromContext { get; set; }
        public bool EmitContext { get; set; }
        public bool IsEmit { get; set; }

        public GremlinRepeatOp(GraphTraversal2 repeatTraversal)
        {
            RepeatTraversal = repeatTraversal;
        }

        public GremlinRepeatOp()
        {
        }

        internal override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();

            RepeatTraversal.GetStartOp().InheritedVariableFromParent(inputContext);
            GremlinToSqlContext repeatContext = RepeatTraversal.GetEndOp().GetContext();

            RepeatCondition repeatCondition = new RepeatCondition();
            repeatCondition.StartFromContext = StartFromContext;
            repeatCondition.EmitContext = EmitContext;
            if (IsEmit)
            {
                repeatCondition.EmitCondition = SqlUtil.GetTrueBooleanComparisonExpr();
            }
            if (TerminationPredicate != null)
            {
                throw new NotImplementedException();
            }
            if (TerminationTraversal != null)
            {
                TerminationTraversal.GetStartOp().InheritedVariableFromParent(repeatContext);
                repeatCondition.TerminationCondition = TerminationTraversal.GetEndOp().GetContext().ToSqlBoolean();
            }
            if (EmitPredicate != null)
            {
                throw new NotImplementedException();
            }
            if (EmitTraversal != null)
            {
                EmitTraversal.GetStartOp().InheritedVariableFromParent(repeatContext);
                repeatCondition.EmitCondition = EmitTraversal.GetEndOp().GetContext().ToSqlBoolean();
            }

            inputContext.PivotVariable.Repeat(inputContext, repeatContext, repeatCondition);

            return inputContext;
        }

    }

    public class RepeatCondition
    {
        internal bool StartFromContext { get; set; }
        internal bool EmitContext { get; set; }
        internal int RepeatTimes { get; set; }
        internal WBooleanExpression EmitCondition { get; set; }
        internal WBooleanExpression TerminationCondition { get; set; }

        public RepeatCondition()
        {
            RepeatTimes = -1;
            StartFromContext = false;
            EmitContext = false;
        }
    } 
}