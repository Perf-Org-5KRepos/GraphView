﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GraphView
{
    internal class GremlinPropertyOp: GremlinTranslationOperator
    {
        public Dictionary<string, object> Properties { get; set; }

        public GremlinPropertyOp(params object[] properties)
        {
            if (properties.Length % 2 != 0) throw new Exception("The parameter of property should be even");
            if (properties.Length < 2) throw new Exception("The number of parameter of property should be larger than 2");
            Properties = new Dictionary<string, object>();
            for (int i = 0; i < properties.Length; i += 2)
            {
                Properties[properties[i] as string] = properties[i + 1];
            }
        }

        internal override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();

            inputContext.PivotVariable.Property(inputContext, Properties);

            return inputContext;
        }
    }
}