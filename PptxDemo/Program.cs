﻿using System;
using System.Diagnostics;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using dotnetCampus.OpenXmlUnitConverter;
using GroupShape = DocumentFormat.OpenXml.Presentation.GroupShape;
using Shape = DocumentFormat.OpenXml.Presentation.Shape;

using var presentationDocument =
    DocumentFormat.OpenXml.Packaging.PresentationDocument.Open("Test.pptx", false);
var presentationPart = presentationDocument.PresentationPart;
var slidePart = presentationPart!.SlideParts.First();
var slide = slidePart.Slide;
GetShape(slide);
var timing = slide.Timing;
// 第一级里面默认只有一项
var commonTimeNode = timing?.TimeNodeList?.ParallelTimeNode?.CommonTimeNode;
if (commonTimeNode?.NodeType?.Value == TimeNodeValues.TmingRoot)
{
    // 这是符合约定
    // nodeType="tmRoot"
}

if (commonTimeNode?.ChildTimeNodeList == null) return;
// 理论上只有一项，而且一定是 SequenceTimeNode 类型
var sequenceTimeNode = commonTimeNode.ChildTimeNodeList.GetFirstChild<SequenceTimeNode>();
var mainSequenceTimeNode = sequenceTimeNode.CommonTimeNode;
if (mainSequenceTimeNode?.NodeType?.Value == TimeNodeValues.MainSequence)
{
    // [TimeLine 对象 (PowerPoint) | Microsoft Docs](https://docs.microsoft.com/zh-cn/office/vba/api/PowerPoint.TimeLine )
    //  MainSequence 主动画序列
    var mainParallelTimeNode = mainSequenceTimeNode.ChildTimeNodeList;

    foreach (var openXmlElement in mainParallelTimeNode)
    {
        // 并行关系的
        if (openXmlElement is ParallelTimeNode parallelTimeNode)
        {
            var timeNode = parallelTimeNode.CommonTimeNode.ChildTimeNodeList
                .GetFirstChild<ParallelTimeNode>().CommonTimeNode.ChildTimeNodeList
                .GetFirstChild<ParallelTimeNode>().CommonTimeNode;

            switch (timeNode.PresetClass.Value)
            {
                case TimeNodePresetClassValues.Entrance:
                    // 进入动画
                    break;
                case TimeNodePresetClassValues.Exit:
                    // 退出动画
                    break;
                case TimeNodePresetClassValues.Emphasis:
                    // 强调动画
                    break;
                case TimeNodePresetClassValues.Path:
                    // 路由动画
                    break;
                case TimeNodePresetClassValues.Verb:
                    break;
                case TimeNodePresetClassValues.MediaCall:
                    // 播放动画
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

static void GetShape(Slide slide)
{
    foreach (var openXmlElement in slide.CommonSlideData.ShapeTree)
    {
        if (openXmlElement is Shape shape)
        {
            ReadShape(shape);
        }
        else if (openXmlElement is GroupShape groupShape)
        {
            ReadGroupShape(groupShape);
        }
    }
}

static void ReadGroupShape(GroupShape groupShape)
{
    foreach (var openXmlElement in groupShape.ChildElements)
    {
        if (openXmlElement is Shape shape)
        {
            ReadShape(shape);
        }
        else if (openXmlElement is GroupShape group)
        {
            ReadGroupShape(group);
        }
    }
}

static void ReadShape(Shape shape)
{
    ReadFill(shape);

    ReadLineWidth(shape);
}

static void ReadFill(Shape shape)
{
    // 更多读取画刷颜色请看 [dotnet OpenXML 获取颜色方法](https://blog.lindexi.com/post/Office-%E4%BD%BF%E7%94%A8-OpenXML-SDK-%E8%A7%A3%E6%9E%90%E6%96%87%E6%A1%A3%E5%8D%9A%E5%AE%A2%E7%9B%AE%E5%BD%95.html )

    var shapeProperties = shape.ShapeProperties;
    if (shapeProperties == null)
    {
        return;
    }

    var groupFill = shapeProperties.GetFirstChild<GroupFill>();
    if (groupFill != null)
    {
        // 如果是组合的颜色画刷，那需要去获取组合的
        var groupShape = shape.Parent as GroupShape;
        var solidFill = groupShape?.GroupShapeProperties?.GetFirstChild<SolidFill>();

        if (solidFill is null)
        {
            // 继续获取组合的组合
            while (groupShape != null)
            {
                groupShape = groupShape.Parent as GroupShape;
                solidFill = groupShape?.GroupShapeProperties?.GetFirstChild<SolidFill>();

                if (solidFill != null)
                {
                    break;
                }
            }
        }

        if (solidFill is null)
        {
            Console.WriteLine($"没有颜色");
        }
        else
        {
            Debug.Assert(solidFill.RgbColorModelHex?.Val != null, "solidFill.RgbColorModelHex.Val != null");
            Console.WriteLine(solidFill.RgbColorModelHex.Val.Value);
        }
    }
    else
    {
        var solidFill = shapeProperties.GetFirstChild<SolidFill>();

        Debug.Assert(solidFill?.RgbColorModelHex?.Val != null, "solidFill.RgbColorModelHex.Val != null");
        Console.WriteLine(solidFill.RgbColorModelHex.Val.Value);
    }
}

static void ReadLineWidth(Shape shape)
{
    // 读取线条宽度的方法
    var outline = shape.ShapeProperties?.GetFirstChild<Outline>();
    if (outline != null)
    {
        var lineWidth = outline.Width;
        var emu = new Emu(lineWidth);
        var pixel = emu.ToPixel();
        Console.WriteLine($"线条宽度 {pixel.Value}");
    }
    else
    {
        // 这形状没有定义轮廓
    }
}
