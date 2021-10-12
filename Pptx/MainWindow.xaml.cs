﻿#nullable enable
using System;
using System.Buffers;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using dotnetCampus.OpenXmlUnitConverter;
using OpenMcdf;
using ColorMap = DocumentFormat.OpenXml.Presentation.ColorMap;
using GraphicFrame = DocumentFormat.OpenXml.Presentation.GraphicFrame;
using Path = DocumentFormat.OpenXml.Drawing.Path;
using Rectangle = System.Windows.Shapes.Rectangle;
using SchemeColorValues = DocumentFormat.OpenXml.Drawing.SchemeColorValues;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;
using ShapeProperties = DocumentFormat.OpenXml.Presentation.ShapeProperties;

namespace Pptx
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            //var file = @"F:\temp\foo" + (char) 1+".txt";
            //File.WriteAllText(file, "123");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var file = new FileInfo("Test.pptx");

            using var presentationDocument = PresentationDocument.Open(file.FullName, false);
            var slide = presentationDocument.PresentationPart!.SlideParts.First().Slide;

            var graphicFrame = slide.CommonSlideData!.ShapeTree!.GetFirstChild<GraphicFrame>()!;
            var graphic = graphicFrame.Graphic!;
            var graphicData = graphic.GraphicData!;
            var alternateContent = graphicData.GetFirstChild<AlternateContent>()!;
            var choice = alternateContent.GetFirstChild<AlternateContentChoice>()!;
            var oleObject = choice.GetFirstChild<OleObject>()!;
            Debug.Assert(oleObject.GetFirstChild<OleObjectEmbed>() != null);
            var id = oleObject.Id!;
            var part = slide.SlidePart!.GetPartById(id!);
            Debug.Assert(part.ContentType == "application/vnd.openxmlformats-officedocument.oleObject");

            var allocatedBytesForCurrentThread = GC.GetAllocatedBytesForCurrentThread();
            var s = part.GetStream();

            var byteArrayPool = new ByteArrayPool();
            var tempFolder = @"F:\temp";
            if (!Directory.Exists(tempFolder))
            {
                tempFolder = System.IO.Path.GetTempPath();
            }

            tempFolder = System.IO.Path.Combine(tempFolder, System.IO.Path.GetRandomFileName());

            //CompoundFileUnzipper.Unzip(s, tempFolder, byteArrayPool);

            var fakeStream = new ForwardSeekStream(s,byteArrayPool);
            var cf = new CompoundFile(fakeStream);
            var packageStream = cf.RootStorage.GetStream("Package");
            //var tempFolder = @"F:\temp";
            //if (!Directory.Exists(tempFolder))
            //{
            //    tempFolder = System.IO.Path.GetTempPath();
            //}
            var xlsxFile = System.IO.Path.Combine(tempFolder, System.IO.Path.GetRandomFileName() + ".xlsx");
            using (var fileStream = File.OpenWrite(xlsxFile))
            {
                //fileStream.Write(packageStream.GetData().AsSpan());
                cf.CopyTo(packageStream, fileStream);
            }
            //using (var fileStream = File.OpenWrite(xlsxFile))
            //{
            //    fileStream.Write(packageStream.GetData().AsSpan());
            //}
            //using var spreadsheetDocument = SpreadsheetDocument.Open(xlsxFile, false);
            //var sheets = spreadsheetDocument.WorkbookPart!.Workbook.Sheets;

            var lastAllocatedBytesForCurrentThread = GC.GetAllocatedBytesForCurrentThread();
            Debug.WriteLine(lastAllocatedBytesForCurrentThread - allocatedBytesForCurrentThread);



            //// OpenMcdf.CFException:“Cannot load a non-seekable Stream”
            //var compoundFile = new CompoundFile(part.GetStream(FileMode.Open));

            //var oleFile = System.IO.Path.Combine(tempFolder, System.IO.Path.GetRandomFileName());
            //using (var fileStream = File.OpenWrite(oleFile))
            //{
            //    using var stream = part.GetStream(FileMode.Open);
            //    stream.CopyTo(fileStream);
            //}

            //var compoundFile = new CompoundFile(oleFile);
            //var packageStream = compoundFile.RootStorage.GetStream("Package");
            //var xlsxFile = System.IO.Path.Combine(tempFolder, System.IO.Path.GetRandomFileName() + ".xlsx");
            //using (var fileStream = File.OpenWrite(xlsxFile))
            //{
            //    fileStream.Write(packageStream.GetData().AsSpan());
            //}

            //using var spreadsheetDocument = SpreadsheetDocument.Open(xlsxFile, false);
            //var sheets = spreadsheetDocument.WorkbookPart!.Workbook.Sheets;


        }
    }

}
