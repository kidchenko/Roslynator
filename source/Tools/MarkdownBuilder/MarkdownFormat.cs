﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Pihrtsoft.Markdown
{
    public class MarkdownFormat : IEquatable<MarkdownFormat>
    {
        public MarkdownFormat(
            EmphasisStyle boldStyle = EmphasisStyle.Asterisk,
            EmphasisStyle italicStyle = EmphasisStyle.Asterisk,
            BulletListStyle bulletListStyle = BulletListStyle.Asterisk,
            OrderedListStyle orderedListStyle = OrderedListStyle.Dot,
            HeadingStyle headingStyle = HeadingStyle.NumberSign,
            HeadingOptions headingOptions = HeadingOptions.EmptyLineBeforeAndAfter,
            TableOptions tableOptions = TableOptions.FormatHeader | TableOptions.OuterDelimiter | TableOptions.Padding | TableOptions.EmptyLineBeforeAndAfter,
            CodeFenceStyle codeFenceStyle = CodeFenceStyle.Backtick,
            CodeBlockOptions codeBlockOptions = CodeBlockOptions.EmptyLineBeforeAndAfter,
            CharReferenceFormat charReferenceFormat = CharReferenceFormat.Hexadecimal,
            HorizontalRuleFormat? horizontalRuleFormat = null)
        {
            BoldStyle = boldStyle;
            ItalicStyle = italicStyle;
            BulletListStyle = bulletListStyle;
            OrderedListStyle = orderedListStyle;
            HeadingStyle = headingStyle;
            HeadingOptions = headingOptions;
            TableOptions = tableOptions;
            CodeFenceStyle = codeFenceStyle;
            CodeBlockOptions = codeBlockOptions;
            CharReferenceFormat = charReferenceFormat;
            HorizontalRuleFormat = horizontalRuleFormat ?? HorizontalRuleFormat.Default;

            if (BulletListStyle == BulletListStyle.Asterisk)
            {
                BulletItemStart = "* ";
            }
            else if (BulletListStyle == BulletListStyle.Plus)
            {
                BulletItemStart = "+ ";
            }
            else if (BulletListStyle == BulletListStyle.Minus)
            {
                BulletItemStart = "- ";
            }
            else
            {
                throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(BulletListStyle));
            }

            if (BoldStyle == EmphasisStyle.Asterisk)
            {
                BoldDelimiter = "**";
            }
            else if (BoldStyle == EmphasisStyle.Underscore)
            {
                BoldDelimiter = "__";
            }
            else
            {
                throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(BoldStyle));
            }

            if (ItalicStyle == EmphasisStyle.Asterisk)
            {
                ItalicDelimiter = "*";
            }
            else if (ItalicStyle == EmphasisStyle.Underscore)
            {
                ItalicDelimiter = "_";
            }
            else
            {
                throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(ItalicStyle));
            }

            if (OrderedListStyle == OrderedListStyle.Dot)
            {
                OrderedItemStart = ". ";
            }
            else if (OrderedListStyle == OrderedListStyle.Parenthesis)
            {
                OrderedItemStart = ") ";
            }
            else
            {
                throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(OrderedListStyle));
            }
        }

        public static MarkdownFormat Default { get; } = new MarkdownFormat();

        internal static MarkdownFormat Debugging { get; } = new MarkdownFormat(
            Default.BoldStyle,
            Default.ItalicStyle,
            Default.BulletListStyle,
            Default.OrderedListStyle,
            Default.HeadingStyle,
            HeadingOptions.None,
            TableOptions.OuterDelimiter,
            Default.CodeFenceStyle,
            CodeBlockOptions.None,
            Default.CharReferenceFormat,
            Default.HorizontalRuleFormat);

        public EmphasisStyle BoldStyle { get; }

        public EmphasisStyle ItalicStyle { get; }

        public BulletListStyle BulletListStyle { get; }

        public OrderedListStyle OrderedListStyle { get; }

        public HorizontalRuleFormat HorizontalRuleFormat { get; }

        public HeadingStyle HeadingStyle { get; }

        public HeadingOptions HeadingOptions { get; }

        internal bool EmptyLineBeforeHeading => (HeadingOptions & HeadingOptions.EmptyLineBefore) != 0;

        internal bool EmptyLineAfterHeading => (HeadingOptions & HeadingOptions.EmptyLineAfter) != 0;

        public CodeFenceStyle CodeFenceStyle { get; }

        public CodeBlockOptions CodeBlockOptions { get; }

        internal bool EmptyLineBeforeCodeBlock => (CodeBlockOptions & CodeBlockOptions.EmptyLineBefore) != 0;

        internal bool EmptyLineAfterCodeBlock => (CodeBlockOptions & CodeBlockOptions.EmptyLineAfter) != 0;

        public TableOptions TableOptions { get; }

        internal bool TablePadding => (TableOptions & TableOptions.Padding) != 0;

        internal bool TableOuterDelimiter => (TableOptions & TableOptions.OuterDelimiter) != 0;

        internal bool FormatTableHeader => (TableOptions & TableOptions.FormatHeader) != 0;

        internal bool FormatTableContent => (TableOptions & TableOptions.FormatContent) != 0;

        internal bool EmptyLineBeforeTable => (TableOptions & TableOptions.EmptyLineBefore) != 0;

        internal bool EmptyLineAfterTable => (TableOptions & TableOptions.EmptyLineAfter) != 0;

        internal bool UnderlineHeading => (HeadingOptions & HeadingOptions.Underline) != 0;

        internal bool UnderlineHeading1 => (HeadingOptions & HeadingOptions.UnderlineH1) != 0;

        internal bool UnderlineHeading2 => (HeadingOptions & HeadingOptions.UnderlineH2) != 0;

        internal bool CloseHeading => (HeadingOptions & HeadingOptions.Close) != 0;

        public CharReferenceFormat CharReferenceFormat { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as MarkdownFormat);
        }

        public bool Equals(MarkdownFormat other)
        {
            return other != null
                && BoldStyle == other.BoldStyle
                && ItalicStyle == other.ItalicStyle
                && BulletListStyle == other.BulletListStyle
                && OrderedListStyle == other.OrderedListStyle
                && HeadingStyle == other.HeadingStyle
                && HeadingOptions == other.HeadingOptions
                && CodeFenceStyle == other.CodeFenceStyle
                && CodeBlockOptions == other.CodeBlockOptions
                && TableOptions == other.TableOptions
                && CharReferenceFormat == other.CharReferenceFormat
                && HorizontalRuleFormat == other.HorizontalRuleFormat;
        }

        public override int GetHashCode()
        {
            int hashCode = Hash.OffsetBasis;
            hashCode = Hash.Combine((int)BoldStyle, hashCode);
            hashCode = Hash.Combine((int)ItalicStyle, hashCode);
            hashCode = Hash.Combine((int)BulletListStyle, hashCode);
            hashCode = Hash.Combine((int)OrderedListStyle, hashCode);
            hashCode = Hash.Combine((int)HeadingStyle, hashCode);
            hashCode = Hash.Combine((int)HeadingOptions, hashCode);
            hashCode = Hash.Combine((int)CodeFenceStyle, hashCode);
            hashCode = Hash.Combine((int)CodeBlockOptions, hashCode);
            hashCode = Hash.Combine((int)TableOptions, hashCode);
            hashCode = Hash.Combine((int)CharReferenceFormat, hashCode);
            hashCode = Hash.Combine(HorizontalRuleFormat.GetHashCode(), hashCode);
            return hashCode;
        }

        public static bool operator ==(MarkdownFormat format1, MarkdownFormat format2)
        {
            return EqualityComparer<MarkdownFormat>.Default.Equals(format1, format2);
        }

        public static bool operator !=(MarkdownFormat format1, MarkdownFormat format2)
        {
            return !(format1 == format2);
        }

        public MarkdownFormat WithBoldStyle(EmphasisStyle boldStyle)
        {
            return new MarkdownFormat(
                boldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithItalicStyle(EmphasisStyle italicStyle)
        {
            return new MarkdownFormat(
                BoldStyle,
                italicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithBulletListStyle(BulletListStyle bulletListStyle)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                bulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithOrderedListStyle(OrderedListStyle orderedListStyle)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                orderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithHorizontalRuleFormat(HorizontalRuleFormat horizontalRuleFormat)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                horizontalRuleFormat);
        }

        public MarkdownFormat WithHeadingOptions(HeadingStyle headingStyle)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                headingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithHeadingOptions(HeadingOptions headingOptions)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                headingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithTableOptions(TableOptions tableOptions)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                tableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithCodeFenceStyle(CodeFenceStyle codeFenceStyle)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                codeFenceStyle,
                CodeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithCodeBlockOptions(CodeBlockOptions codeBlockOptions)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                codeBlockOptions,
                CharReferenceFormat,
                HorizontalRuleFormat);
        }

        public MarkdownFormat WithCharReferenceFormat(CharReferenceFormat charReferenceFormat)
        {
            return new MarkdownFormat(
                BoldStyle,
                ItalicStyle,
                BulletListStyle,
                OrderedListStyle,
                HeadingStyle,
                HeadingOptions,
                TableOptions,
                CodeFenceStyle,
                CodeBlockOptions,
                charReferenceFormat,
                HorizontalRuleFormat);
        }

        internal string BoldDelimiter { get; }

        internal string ItalicDelimiter { get; }

        internal string BulletItemStart { get; }

        internal string OrderedItemStart { get; }

        internal char HeadingChar
        {
            get { return '#'; }
        }
    }
}
