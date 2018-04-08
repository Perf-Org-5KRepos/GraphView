﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphView;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphViewAzureBatchUnitTest.Gremlin.Map
{
    [TestClass]
    public class PathTest : AbstractAzureBatchGremlinTest
    {
        /// <summary>
        /// from org/apache/tinkerpop/gremlin/process/traversal/step/map/PathTest.java
        /// Gremlin: g.V(v1Id).values("name").path();
        /// </summary>
        [TestMethod]
        public void get_g_VX1X_name_path()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.Regular;
                string vertexId1 = this.ConvertToVertexId(graphCommand, "marko");

                graphCommand.OutputFormat = OutputFormat.GraphSON;
                this.job.Traversal = graphCommand.g().V(vertexId1).Values("name").Path();
                List<string> results = this.jobManager.TestQuery(this.job);
                dynamic result = JsonConvert.DeserializeObject<dynamic>(results.FirstOrDefault())[0];

                Assert.AreEqual(vertexId1, (string)result["objects"][0].id);
                Assert.AreEqual("marko", (string)result["objects"][1]);
            }
        }

        /// <summary>
        /// from org/apache/tinkerpop/gremlin/process/traversal/step/map/PathTest.java
        /// Gremlin: g.V(v1Id).out().path().by("age").by("name");
        /// </summary>
        [TestMethod]
        public void get_g_VX1X_out_path_byXageX_byXnameX()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.Regular;
                string vertexId1 = this.ConvertToVertexId(graphCommand, "marko");

                graphCommand.OutputFormat = OutputFormat.GraphSON;
                this.job.Traversal = graphCommand.g().V(vertexId1).Out().Path().By("age").By("name");
                dynamic results = JsonConvert.DeserializeObject<dynamic>(this.jobManager.TestQuery(this.job).FirstOrDefault());

                Assert.AreEqual(3, results.Count);
                List<string> expected = new List<string>();
                foreach (var result in results)
                {
                    expected.Add((string)result["objects"][1]);
                }
                CheckUnOrderedResults(new List<string>() { "lop", "vadas", "josh" }, expected);
            }
        }

        /// <summary>
        /// from org/apache/tinkerpop/gremlin/process/traversal/step/map/PathTest.java
        /// Gremlin: g.V().repeat(__.out()).times(2).path().by().by("name").by("lang");
        /// </summary>
        [TestMethod]
        public void get_g_V_repeatXoutX_timesX2X_path_by_byXnameX_byXlangX()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.GraphSON;
                var vertex = this.getVertexString(graphCommand, "marko");

                this.job.Traversal =
                    graphCommand.g().V().Repeat(GraphTraversal.__().Out()).Times(2).Path().By().By("name").By("lang");
                dynamic results = JsonConvert.DeserializeObject<dynamic>(this.jobManager.TestQuery(this.job).FirstOrDefault());

                Assert.AreEqual(2, results.Count);
                List<string> actualList = new List<string>();
                foreach (var result in results[0]["objects"])
                {
                    actualList.Add(result.ToString());
                }
                CheckPathResults(new List<string> { vertex, "josh", "java" }, actualList);

                actualList.Clear();
                foreach (var result in results[1]["objects"])
                {
                    actualList.Add(result.ToString());
                }
                CheckPathResults(new List<string> { vertex, "josh", "java" }, actualList);
            }
        }

        /// <summary>
        /// from org/apache/tinkerpop/gremlin/process/traversal/step/map/PathTest.java
        /// Gremlin: g.V().out().out().path().by("name").by("age")
        /// </summary>
        [TestMethod]
        public void get_g_V_out_out_path_byXnameX_byXageX()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.GraphSON;

                this.job.Traversal =
                    graphCommand.g().V().Out().Out().Path().By("name").By("age");

                dynamic results = JsonConvert.DeserializeObject<dynamic>(this.jobManager.TestQuery(this.job).FirstOrDefault());

                Assert.AreEqual(2, results.Count);

                int counter = 0;
                foreach (dynamic result in results)
                {
                    List<object> actualList = new List<object>();
                    actualList.Add((string)result["objects"][0]);
                    actualList.Add((int)result["objects"][1]);
                    actualList.Add((string)result["objects"][2]);

                    if (actualList.Last().Equals("ripple"))
                    {
                        CheckPathResults(new List<object> { "marko", 32, "ripple" }, actualList);
                        counter++;
                    }
                    else
                    {
                        CheckPathResults(new List<object> { "marko", 32, "lop" }, actualList);
                        counter++;
                    }
                }

                Assert.AreEqual(2, counter);
            }
        }

        /// <summary>
        /// from org/apache/tinkerpop/gremlin/process/traversal/step/map/PathTest.java
        /// Gremlin: g.V().as("a").has("name", "marko").as("b").has("age", 29).as("c").path()
        /// </summary>
        [TestMethod]
        public void get_g_V_asXaX_hasXname_markoX_asXbX_hasXage_29X_asXcX_path()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.GraphSON;
                var vertex = this.getVertexString(graphCommand, "marko");

                this.job.Traversal =
                    graphCommand.g().V().As("a").Has("name", "marko").As("b").Has("age", 29).As("c").Path();

                dynamic results = JsonConvert.DeserializeObject<dynamic>(this.jobManager.TestQuery(this.job).FirstOrDefault());

                Assert.AreEqual(1, results[0]["objects"].Count);
                Assert.AreEqual(vertex, results[0]["objects"][0].ToString());
            }
        }

        [TestMethod]
        public void PathFromToTest()
        {
            using (GraphViewCommand graphCommand = this.job.Command)
            {
                graphCommand.OutputFormat = OutputFormat.Regular;

                var vertex = this.getVertexString(graphCommand, "marko");
                string vertexId = this.ConvertToVertexId(graphCommand, "marko");

                graphCommand.OutputFormat = OutputFormat.Regular;
                this.job.Traversal =
                    graphCommand.g().V(vertexId).As("a").Out().As("a").In().As("b").Out().As("c").In().As("d").Out()
                        .As("d").CyclicPath().From("a").To("c").SimplePath().From("c").To("d").Path();
                var results = this.jobManager.TestQuery(this.job);
                Assert.AreEqual(13, results.Count);
            }
        }
    }
}