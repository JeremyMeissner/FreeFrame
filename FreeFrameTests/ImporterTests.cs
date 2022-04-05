using Microsoft.VisualStudio.TestTools.UnitTesting;
using FreeFrame;
using FreeFrame.Components;
using System.Collections.Generic;
using FreeFrame.Components.Shapes;
using System;

namespace FreeFrameTests
{
    [TestClass]
    public class ImporterTests
    {
        [TestMethod]
        public void ImportFromStream_Test_OneElement()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"1000\" height=\"1000\">" +
                "<circle cx=\"600\" cy=\"200\" r=\"100\" fill=\"red\" stroke=\"blue\" stroke-width=\"10\"  />" +
                "<rect width=\"100\" height=\"50\" fill=\"green\" />" +
                "<path d=\"M 20 0 C 0 100, 200 100, 180 0\" stroke=\"black\" fill=\"transparent\"/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(((SVGCircle)result[0]).ToString(), "cx: 600, cy: 200, r: 100");
        }
        [TestMethod]
        public void ImportFromStream_Test_MutlipleElements()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"1000\" height=\"1000\">" +
                "<circle cx=\"600\" cy=\"200\" r=\"100\" fill=\"red\" stroke=\"blue\" stroke-width=\"10\"/>" +
                "<rect width=\"100\" height=\"50\" fill=\"green\" />" +
                "<path d=\"M 20 0 C 0 100, 200 100, 180 0\" stroke=\"black\" fill=\"transparent\"/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual(result.Count, 3);
            Assert.AreEqual(((SVGCircle)result[0]).ToString(), "cx: 600, cy: 200, r: 100");
            Assert.AreEqual(((SVGRectangle)result[1]).ToString(), "width: 100, height: 50");
            Assert.AreEqual(((SVGPath)result[2]).ToString(), "M: 20 0, C: 0 100, 200 100, 180 0");
        }
    }
}