using Microsoft.VisualStudio.TestTools.UnitTesting;
using FreeFrame;
using FreeFrame.Components;
using System.Collections.Generic;
using FreeFrame.Components.Shapes;
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
                "<svg>" +
                "<circle cx=\"10\" cy=\"10\" r=\"10\" fill=\"#000000FF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("<circle cx=\"10\" cy=\"10\" r=\"10\" fill=\"#000000FF\"/>", ((SVGCircle)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Path()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<path d=\"M 20,0 C 0,100 200,100 180,0 m 50,10 L 10,21 H 10 V 10 s 20,20 10,10 Q 10,10 0,0 t 10,10 A 100 100 60 1 0 20,20 z\" fill=\"000000FF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("<path d=\"M 20,0 C 0,100 200,100 180,0 m 50,10 L 10,21 H 10 V 10 s 20,20 10,10 Q 10,10 0,0 t 10,10 A 100 100 60 1 0 20,20 z\" fill=\"000000FF\"/>", ((SVGPath)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Polygon()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<polygon points=\"50,0 21,90 98,35 2,35 79,90\" fill=\"#000000FF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("<polygon points=\"50,0 21,90 98,35 2,35 79,90\" fill=\"#000000FF\"/>", ((SVGPolygon)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Rectangle()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<rect x=\"10\" y=\"20\" width=\"100\" height=\"50\" rx=\"10\" ry=\"10\" fill=\"#FFFFFFFF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("<rect x=\"10\" y=\"20\" width=\"100\" height=\"50\" rx=\"10\" ry=\"10\" fill=\"#FFFFFFFF\"/>", ((SVGRectangle)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_Line()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<line x1=\"10\" y1=\"20\" x2=\"100\" y2=\"50\" fill=\"#000000FF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual("<line x1=\"10\" y1=\"20\" x2=\"100\" y2=\"50\" fill=\"#000000FF\"/>", ((SVGLine)result[0]).ToString());
        }
        [TestMethod]
        public void ImportFromStream_Test_CountElements()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<circle cx=\"10\" cy=\"10\" r=\"10\" fill=\"#FFFFFFFF\"/>" +
                "<rect x=\"10\" y=\"20\" width=\"100\" height=\"50\" rx=\"10\" ry=\"10\" fill=\"#FFFFFFFF\"/>" +
                "</svg>";
            (List<Shape> result, _, _) = Importer.ImportFromString(svgString);
            // Assert
            Assert.AreEqual(2, result.Count);
        }
        [TestMethod]
        public void ImportFromStream_Test_KnownElement()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<circle cx=\"10\" cy=\"10\" r=\"10\" fill=\"#FFFFFFFF\"/>" +
                "<rect x=\"10\" y=\"20\" width=\"100\" height=\"50\" rx=\"10\" ry=\"10\" fill=\"#FFFFFFFF\"/>" +
                "</svg>";
            (_, _, bool compatibilityFlag) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual(false, compatibilityFlag);
        }
        [TestMethod]
        public void ImportFromStream_Test_UnknownElement()
        {
            // Arrange
            string svgString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<svg>" +
                "<circle cx=\"10\" cy=\"10\" r=\"10\" fill=\"#FFFFFFFF\"/>" +
                "<hello world=\"10\"/>" +
                "</svg>";
            (_, _, bool compatibilityFlag) = Importer.ImportFromString(svgString);

            // Assert
            Assert.AreEqual(true, compatibilityFlag);
        }
    }
}