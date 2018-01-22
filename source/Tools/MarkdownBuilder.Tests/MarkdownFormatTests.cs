﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using static Pihrtsoft.Markdown.Tests.TestHelpers;

namespace Pihrtsoft.Markdown.Tests
{
    public class MarkdownFormatTests
    {
        [Theory]
        [InlineData(EmphasisStyle.Asterisk)]
        [InlineData(EmphasisStyle.Underscore)]
        public void MarkdownFormat_Constructor_BoldStyle(EmphasisStyle boldStyle)
        {
            var format = new MarkdownFormat(boldStyle: boldStyle);

            Assert.Equal(boldStyle, format.BoldStyle);
        }

        [Theory]
        [InlineData(EmphasisStyle.Asterisk)]
        [InlineData(EmphasisStyle.Underscore)]
        public void MarkdownFormat_Constructor_ItalicStyle(EmphasisStyle italicStyle)
        {
            var format = new MarkdownFormat(italicStyle: italicStyle);

            Assert.Equal(italicStyle, format.ItalicStyle);
        }

        [Theory]
        [InlineData(BulletListStyle.Asterisk)]
        [InlineData(BulletListStyle.Minus)]
        [InlineData(BulletListStyle.Plus)]
        public void MarkdownFormat_Constructor_ListItemStyle(BulletListStyle listItemStyle)
        {
            var format = new MarkdownFormat(bulletListStyle: listItemStyle);

            Assert.Equal(listItemStyle, format.BulletListStyle);
        }

        [Fact]
        public void MarkdownFormat_Constructor_HorizontalRule()
        {
            string text = HorizontalRuleText();
            int count = HorizontalRuleCount();
            string space = HorizontalRuleSpace();

            var horizontalRule = new HorizontalRuleFormat(text, count, space);

            var format = new MarkdownFormat();

            Assert.Equal(text, format.HorizontalRuleFormat.Text);
            Assert.Equal(count, format.HorizontalRuleFormat.Count);
            Assert.Equal(space, format.HorizontalRuleFormat.Separator);
        }

        [Theory]
        [InlineData(HeadingStyle.NumberSign)]
        public void MarkdownFormat_Constructor_HeadingStyle(HeadingStyle headingStyle)
        {
            var format = new MarkdownFormat(headingStyle: headingStyle);

            Assert.Equal(headingStyle, format.HeadingStyle);
        }

        [Theory]
        [InlineData(HeadingOptions.None)]
        [InlineData(HeadingOptions.EmptyLineBefore)]
        [InlineData(HeadingOptions.EmptyLineBeforeAndAfter)]
        public void MarkdownFormat_Constructor_HeadingOptions(HeadingOptions headingOptions)
        {
            var format = new MarkdownFormat(headingOptions: headingOptions);

            Assert.Equal(headingOptions, format.HeadingOptions);
        }

        [Theory]
        [InlineData(TableOptions.None)]
        [InlineData(TableOptions.FormatHeader)]
        [InlineData(TableOptions.FormatHeaderAndContent)]
        public void MarkdownFormat_Constructor_TableOptions(TableOptions tableOptions)
        {
            var format = new MarkdownFormat(tableOptions: tableOptions);

            Assert.Equal(tableOptions, format.TableOptions);
        }

        [Theory]
        [InlineData(CodeBlockOptions.None)]
        [InlineData(CodeBlockOptions.EmptyLineBefore)]
        [InlineData(CodeBlockOptions.EmptyLineBeforeAndAfter)]
        public void MarkdownFormat_Constructor_CodeBlockOptions(CodeBlockOptions codeBlockOptions)
        {
            var format = new MarkdownFormat(codeBlockOptions: codeBlockOptions);

            Assert.Equal(codeBlockOptions, format.CodeBlockOptions);
        }

        [Fact]
        public void MarkdownFormat_Equals()
        {
            MarkdownFormat format = CreateMarkdownFormat();

            Assert.True(format.Equals((object)format));
        }

        [Fact]
        public void MarkdownFormat_NotEquals()
        {
            MarkdownFormat format = CreateMarkdownFormat();
            MarkdownFormat format2 = format.Modify();

            Assert.False(format.Equals((object)format2));
        }

        [Fact]
        public void MarkdownFormat_GetHashCode_Equal()
        {
            MarkdownFormat format = CreateMarkdownFormat();

            Assert.Equal(format.GetHashCode(), format.GetHashCode());
        }

        [Fact]
        public void MarkdownFormat_GetHashCode_NotEqual()
        {
            MarkdownFormat format = CreateMarkdownFormat();
            MarkdownFormat format2 = format.Modify();

            Assert.NotEqual(format.GetHashCode(), format2.GetHashCode());
        }

        [Fact]
        public void MarkdownFormat_OperatorEquals()
        {
            MarkdownFormat format = CreateMarkdownFormat();
            MarkdownFormat format2 = format;

            Assert.True(format == format2);
        }

        [Fact]
        public void MarkdownFormat_OperatorNotEquals()
        {
            MarkdownFormat format = CreateMarkdownFormat();
            MarkdownFormat format2 = format.Modify();

            Assert.True(format != format2);
        }
    }
}
