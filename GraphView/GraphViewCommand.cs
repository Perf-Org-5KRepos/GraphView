﻿// GraphView
// 
// Copyright (c) 2015 Microsoft Corporation
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
using System;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace GraphView
{
    public class GraphViewCommand : IDisposable
    {
        public GraphViewConnection Connection { get; set; }

        public VertexObjectCache VertexCache { get; }

        public bool InLazyMode { get; set; } = false;
        
        public string CommandText { get; set; }

        public OutputFormat OutputFormat { get; set; }

        private int indexColumnCount;

        public string IndexColumnName => (indexColumnCount++).ToString();

        public GraphViewCommand(GraphViewConnection connection)
        {
            this.Connection = connection;
            this.VertexCache = new VertexObjectCache(this);
        }

        public GraphViewCommand(string commandText)
        {
            this.CommandText = commandText;
        }

        public GraphViewCommand(string commandText, GraphViewConnection connection)
        {
            this.CommandText = commandText;
            this.Connection = connection;
        }

        public IEnumerable<string> Execute()
        {
            if (this.CommandText == null)
            {
                throw new QueryExecutionException("CommandText of GraphViewCommand is not set.");
            }
            return g().EvalGremlinTraversal(this.CommandText);
        }

        public List<string> ExecuteAndGetResults()
        {
            List<string> results = new List<string>();
            foreach (var result in this.Execute())
            {
                results.Add(result);
            }
            return results;
        }

        public void Dispose()
        {
        }

        public GraphTraversal g()
        {
            return new GraphTraversal(this, OutputFormat);
        }
    }
}
