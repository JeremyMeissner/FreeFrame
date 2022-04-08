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
        public void ImportFromStream_Test_Circle()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"1000\" height=\"1000\">" +
                "<circle cx=\"600\" cy=\"200\" r=\"100\" fill=\"red\" stroke=\"blue\" stroke-width=\"10\"/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("cx: 600, cy: 200, r: 100", ((SVGCircle)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Path()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"1000\" height=\"1000\">" +
                "<path d=\"M 20,0 C 0,100 200,100 180,0 m 50,10 L 10,21 H 10 V 10 s 20,20 10,10 Q 10,10 0,0 t 10,10 A 100 100 60 1 0 20,20 z\" stroke=\"black\"/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("M 20,0 C 0,100 200,100 180,0 m 50,10 L 10,21 H 10 V 10 s 20,20 10,10 Q 10,10 0,0 t 10,10 A 100 100 60 1 0 20,20 z", ((SVGPath)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Rectangle()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"1000\" height=\"1000\">" +
                "<rect width=\"100\" height=\"50\" fill=\"green\"/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("x: 0, y: 0, width: 100, height: 50, rx: 0, ry: 0", ((SVGRectangle)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_CountElements()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<circle/>" +
                "<rect/>" +
                "<path/>" +
                "</svg>";
            List<Shape> result = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual(3, result.Count);
        }
    }
}