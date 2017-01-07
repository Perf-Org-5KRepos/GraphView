﻿using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinFreeVertexVariable : GremlinVertexVariable
    {
        public GremlinFreeVertexVariable()
        {
            VariableName = GenerateTableAlias();
        }

        public override WTableReference ToTableReference()
        {
            return SqlUtil.GetNamedTableReference(this);
        }

        internal override void Both(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            GremlinEdgeVariable bothEdge = new GremlinBoundEdgeVariable(this, new GremlinVariableProperty(this, "BothAdjacencyList"), WEdgeType.BothEdge);
            currentContext.VariableList.Add(bothEdge);
            currentContext.AddLabelPredicateForEdge(bothEdge, edgeLabels);

            GremlinFreeVertexVariable bothVertex = new GremlinFreeVertexVariable();
            currentContext.VariableList.Add(bothVertex);

            // In this case, the both-edge variable is not added to the table-reference list. 
            // Instead, we populate a path this_variable-[bothEdge]->bothVertex in the context
            currentContext.TableReferences.Add(bothVertex);
            currentContext.Paths.Add(new GremlinMatchPath(this, bothEdge, bothVertex));

            currentContext.PivotVariable = bothVertex;
        }

        internal override void In(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            GremlinEdgeVariable inEdge = new GremlinBoundEdgeVariable(this, new GremlinVariableProperty(this, "BothAdjacencyList"));
            currentContext.VariableList.Add(inEdge);
            currentContext.AddLabelPredicateForEdge(inEdge, edgeLabels);

            GremlinFreeVertexVariable inVertex = new GremlinFreeVertexVariable();
            currentContext.VariableList.Add(inVertex);
            currentContext.TableReferences.Add(inVertex);
            currentContext.Paths.Add(new GremlinMatchPath(inVertex, inEdge, this));

            currentContext.PivotVariable = inVertex;
        }

        internal override void InE(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            GremlinEdgeVariable inEdge = new GremlinBoundEdgeVariable(this, new GremlinVariableProperty(this, "BothAdjacencyList"));
            currentContext.VariableList.Add(inEdge);
            currentContext.AddLabelPredicateForEdge(inEdge, edgeLabels);
            currentContext.Paths.Add(new GremlinMatchPath(null, inEdge, this));
            currentContext.PivotVariable = inEdge;
        }

        internal override void Out(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            GremlinEdgeVariable outEdge = new GremlinBoundEdgeVariable(this, new GremlinVariableProperty(this, "BothAdjacencyList"));
            currentContext.VariableList.Add(outEdge);
            currentContext.AddLabelPredicateForEdge(outEdge, edgeLabels);

            GremlinFreeVertexVariable outVertex = new GremlinFreeVertexVariable();
            currentContext.VariableList.Add(outVertex);
            currentContext.TableReferences.Add(outVertex);
            currentContext.Paths.Add(new GremlinMatchPath(this, outEdge, outVertex));

            currentContext.PivotVariable = outVertex;
        }
        internal override void OutE(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            GremlinEdgeVariable outEdgeVar = new GremlinBoundEdgeVariable(this, new GremlinVariableProperty(this, "BothAdjacencyList"));
            currentContext.VariableList.Add(outEdgeVar);
            currentContext.AddLabelPredicateForEdge(outEdgeVar, edgeLabels);
            currentContext.Paths.Add(new GremlinMatchPath(this, outEdgeVar, null));
            currentContext.PivotVariable = outEdgeVar;
        }

        //internal override void Where(GremlinToSqlContext currentContext, Predicate predicate)
        //{
        //    WScalarExpression key = SqlUtil.GetColumnReferenceExpression(VariableName, "id");
        //    WBooleanExpression booleanExpr = SqlUtil.GetBooleanComparisonExpr(key, predicate);
        //    currentContext.AddPredicate(booleanExpr);
        //}
    }
}