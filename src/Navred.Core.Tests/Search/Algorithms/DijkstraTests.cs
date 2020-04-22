using Navred.Core.Search.Algorithms;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Navred.Core.Tests.Search.Algorithms
{
    public class DijkstraTests
    {
        private Dijkstra Dijkstra => new Dijkstra();

        [Fact]
        public void GraphNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => this.Dijkstra.FindKShortestPaths(null, 0));
        }

        [Fact]
        public void NoConnectionEdges_ReturnsZeroPaths()
        {

        }
    }
}
