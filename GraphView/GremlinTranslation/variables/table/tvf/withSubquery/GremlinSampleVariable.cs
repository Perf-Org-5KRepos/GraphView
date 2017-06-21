﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinSampleVariable : GremlinTableVariable
    {
        public GremlinKeyword.Scope Scope { get; set; }
        public int AmountToSample { get; set; }
        public GremlinToSqlContext ProbabilityContext { get; set; }

        public GremlinSampleVariable(GremlinKeyword.Scope scope, int amountToSample, GremlinToSqlContext probabilityContext)
            : base(GremlinVariableType.Table)
        {
            Scope = scope;
            AmountToSample = amountToSample;
            ProbabilityContext = probabilityContext;
        }

        internal override List<GremlinVariable> FetchAllVars()
        {
            List<GremlinVariable> variableList = new List<GremlinVariable>() { this };
            variableList.AddRange(ProbabilityContext.FetchAllVars());
            return variableList;
        }

        internal override List<GremlinTableVariable> FetchAllTableVars()
        {
            List<GremlinTableVariable> variableList = new List<GremlinTableVariable> { this };
            variableList.AddRange(ProbabilityContext.FetchAllTableVars());
            return variableList;
        }

        public override WTableReference ToTableReference()
        {
            List<WScalarExpression> parameters = new List<WScalarExpression>();
            parameters.Add(SqlUtil.GetValueExpr(AmountToSample));
            if (ProbabilityContext != null)
            {
                parameters.Add(SqlUtil.GetScalarSubquery(ProbabilityContext.ToSelectQueryBlock()));
            }
            var tableRef = Scope == GremlinKeyword.Scope.Global
                ? SqlUtil.GetFunctionTableReference(GremlinKeyword.func.SampleGlobal, parameters, GetVariableName())
                : SqlUtil.GetFunctionTableReference(GremlinKeyword.func.SampleLocal, parameters, GetVariableName());
            return SqlUtil.GetCrossApplyTableReference(tableRef);
        }
    }
}
