﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView.GremlinTranslation
{
    internal class GremlinTailOp: GremlinTranslationOperator
    {
        public int Limit { get; set; }

        public GremlinTailOp()
        {
            Limit = 1;
        }

        public GremlinTailOp(int limit)
        {
            Limit = limit;
        }

        public override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();
            //WScalarExpression valueExpr = GremlinUtil.GetValueExpr(Limit.ToString());
            //inputContext.SetCurrProjection(GremlinUtil.GetFunctionCall("tail", valueExpr));

            //GremlinToSqlContext newContext = new GremlinToSqlContext();
            //GremlinDerivedVariable newDerivedVariable = new GremlinDerivedVariable(inputContext.ToSelectQueryBlock());
            //newContext.AddNewVariable(newDerivedVariable);
            //newContext.SetDefaultProjection(newDerivedVariable);
            //newContext.SetCurrVariable(newDerivedVariable);

            if (inputContext.CurrVariable is GremlinEdgeVariable)
            {
                var sinkNode = inputContext.GetSinkNode(inputContext.CurrVariable);
                sinkNode.Low = 0 - Limit;
                sinkNode.High = 0;
            }
            else
            {
                inputContext.CurrVariable.Low = 0 - Limit;
                inputContext.CurrVariable.High = 0;
            }

            return inputContext;
        }

    }
}