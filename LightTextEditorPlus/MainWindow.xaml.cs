﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LightTextEditorPlus.TextEditorPlus.Render;

namespace LightTextEditorPlus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Foo.Text = ((TextBox)sender).Text;
            Foo.InvalidateArrange();
            Foo.InvalidateVisual();
        }
    }


    class Foo : UIElement
    {
        public string Text { set; get; } = string.Empty;

        protected override void OnRender(DrawingContext drawingContext)
        {
            var fontFamily = new FontFamily("微软雅黑");

            var fontSize = 15;
            var y = 0;
            drawingContext.PushOpacity(0.3);
            foreach (var typeface in fontFamily.GetTypefaces().Skip(1).Take(1))
            {
                double offset = 3;

                var baseLine = fontFamily.GetBaseline(fontSize);

                if (typeface.TryGetGlyphTypeface(out var glyphTypeface))
                {
                    foreach (var c in Text)
                    {
                        if (glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out var glyphIndex))
                        {
                            var width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;
                            width = GlyphExtension.RefineValue(width);

#pragma warning disable 618 // 忽略调用废弃构造函数
                            var glyphRun = new GlyphRun(
#pragma warning restore 618
                                glyphTypeface,
                                0,
                                false,
                                fontSize,
                                new[] { glyphIndex },
                                new Point(offset, baseLine + y),
                                new[] { width },
                                DefaultGlyphOffsetArray,
                                new char[] { c },
                                null,
                                null,
                                null, DefaultXmlLanguage);

                            drawingContext.DrawLine(new Pen(Brushes.Black, 2), new Point(offset, y), new Point(offset + width, y));

                            drawingContext.DrawGlyphRun(Brushes.Coral, glyphRun);

                            var glyphSize = glyphRun.GetSize(fontFamily.LineSpacing);

                            drawingContext.DrawRectangle(null, new Pen(Brushes.Black, 2), new Rect(new Point(offset, y), glyphSize));

                            offset += width;
                        }
                    }
                }

                y += fontSize;
            }
            drawingContext.Pop();
        }

        private static readonly Point[] DefaultGlyphOffsetArray = new Point[] { new Point() };

        private static readonly XmlLanguage DefaultXmlLanguage =
            XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
    }
}
