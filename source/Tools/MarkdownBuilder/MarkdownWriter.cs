﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Pihrtsoft.Markdown.Linq;

namespace Pihrtsoft.Markdown
{
    public abstract class MarkdownWriter : IDisposable
    {
        private bool _disposed;

        protected MarkdownWriter(MarkdownWriterSettings settings = null)
        {
            Settings = settings ?? MarkdownWriterSettings.Default;
        }

        public abstract WriteState WriteState { get; }

        public virtual MarkdownWriterSettings Settings { get; }

        public MarkdownFormat Format => Settings.Format;

        internal NewLineHandling NewLineHandling => Settings.NewLineHandling;

        internal string NewLineChars => Settings.NewLineChars;

        public abstract int QuoteLevel { get; }

        public abstract int ListLevel { get; }

        public static MarkdownWriter Create(StringBuilder output, IFormatProvider formatProvider = null, MarkdownWriterSettings settings = null)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            return new MarkdownStringWriter(output, formatProvider ?? CultureInfo.InvariantCulture, settings);
        }

        public static MarkdownWriter Create(TextWriter output, MarkdownWriterSettings settings = null)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            return new MarkdownTextWriter(output, settings);
        }

        public static MarkdownWriter Create(Stream stream, Encoding encoding = null, MarkdownWriterSettings settings = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new MarkdownTextWriter(new StreamWriter(stream, encoding ?? Encoding.UTF8), settings);
        }

        public abstract void WriteStartBold();

        public abstract void WriteEndBold();

        public virtual void WriteBold(string text)
        {
            WriteStartBold();
            WriteString(text);
            WriteEndBold();
        }

        public abstract void WriteStartItalic();

        public abstract void WriteEndItalic();

        public virtual void WriteItalic(string text)
        {
            WriteStartItalic();
            WriteString(text);
            WriteEndItalic();
        }

        public abstract void WriteStartStrikethrough();

        public abstract void WriteEndStrikethrough();

        public virtual void WriteStrikethrough(string text)
        {
            WriteStartStrikethrough();
            WriteString(text);
            WriteEndStrikethrough();
        }

        public abstract void WriteInlineCode(string text);

        public abstract void WriteStartHeading(int level);

        public abstract void WriteEndHeading();

        public virtual void WriteHeading1(string text)
        {
            WriteHeading(1, text);
        }

        public virtual void WriteHeading2(string text)
        {
            WriteHeading(2, text);
        }

        public virtual void WriteHeading3(string text)
        {
            WriteHeading(3, text);
        }

        public virtual void WriteHeading4(string text)
        {
            WriteHeading(4, text);
        }

        public virtual void WriteHeading5(string text)
        {
            WriteHeading(5, text);
        }

        public virtual void WriteHeading6(string text)
        {
            WriteHeading(6, text);
        }

        public virtual void WriteHeading(int level, string text)
        {
            WriteStartHeading(level);
            WriteString(text);
            WriteEndHeading();
        }

        public abstract void WriteStartBulletItem();

        public abstract void WriteEndBulletItem();

        public virtual void WriteBulletItem(string text)
        {
            WriteStartBulletItem();
            WriteString(text);
            WriteEndBulletItem();
        }

        public abstract void WriteStartOrderedItem(int number);

        public abstract void WriteEndOrderedItem();

        public virtual void WriteOrderedItem(int number, string text)
        {
            Error.ThrowOnInvalidItemNumber(number);
            WriteStartOrderedItem(number);
            WriteString(text);
            WriteEndOrderedItem();
        }

        public abstract void WriteStartTaskItem(bool isCompleted = false);

        public abstract void WriteEndTaskItem();

        public virtual void WriteTaskItem(string text, bool isCompleted = false)
        {
            WriteStartTaskItem(isCompleted: isCompleted);
            WriteString(text);
            WriteEndTaskItem();
        }

        public void WriteStartCompletedTaskItem()
        {
            WriteStartTaskItem(isCompleted: true);
        }

        public void WriteCompletedTaskItem(string text)
        {
            WriteTaskItem(text, isCompleted: true);
        }

        public abstract void WriteImage(string text, string url, string title = null);

        public abstract void WriteLink(string text, string url, string title = null);

        public void WriteLinkOrText(string text, string url = null, string title = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                WriteLink(text, url, title);
            }
            else
            {
                WriteString(text);
            }
        }

        public abstract void WriteAutolink(string url);

        public abstract void WriteImageReference(string text, string label);

        public abstract void WriteLinkReference(string text, string label = null);

        public abstract void WriteLabel(string label, string url, string title = null);

        public abstract void WriteIndentedCodeBlock(string text);

        public abstract void WriteFencedCodeBlock(string text, string info = null);

        public abstract void WriteStartBlockQuote();

        public abstract void WriteEndBlockQuote();

        public virtual void WriteBlockQuote(string text)
        {
            WriteStartBlockQuote();
            WriteString(text);
            WriteEndBlockQuote();
        }

        public void WriteHorizontalRule()
        {
            WriteHorizontalRule(Format.HorizontalRuleFormat);
        }

        public void WriteHorizontalRule(HorizontalRuleFormat format)
        {
            WriteHorizontalRule(format.Text, format.Count, format.Separator);
        }

        public abstract void WriteHorizontalRule(string text, int count = HorizontalRuleFormat.DefaultCount, string separator = HorizontalRuleFormat.DefaultSeparator);

        public abstract void WriteStartTable(int columnCount);

        public abstract void WriteStartTable(IReadOnlyList<TableColumnInfo> columns);

        public abstract void WriteEndTable();

        internal void WriteTableRow(MElement content)
        {
            WriteStartTableRow();

            if (content is MContainer container)
            {
                foreach (MElement element in container.Elements())
                    WriteTableCell(element);
            }
            else
            {
                WriteTableCell(content);
            }

            WriteEndTableRow();

            void WriteTableCell(MElement cell)
            {
                WriteStartTableCell();
                Write(cell);
                WriteEndTableCell();
            }
        }

        public abstract void WriteStartTableRow();

        public abstract void WriteEndTableRow();

        public abstract void WriteStartTableCell();

        public abstract void WriteEndTableCell();

        public abstract void WriteTableHeaderSeparator();

        public abstract void WriteCharEntity(char value);

        public abstract void WriteEntityRef(string name);

        public abstract void WriteComment(string text);

        internal void Write(object value)
        {
            if (value == null)
                return;

            if (value is MElement element)
            {
                element.WriteTo(this);
                return;
            }

            if (value is string s)
            {
                WriteString(s);
                return;
            }

            if (value is object[] arr)
            {
                foreach (object item in arr)
                    Write(item);

                return;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                    Write(item);

                return;
            }

            WriteString(value.ToString());
        }

        public abstract void Flush();

        public abstract void WriteString(string text);

        public abstract void WriteRaw(string data);

        public abstract void WriteLine();

        public virtual void WriteValue(bool value)
        {
            WriteString((value) ? "true" : "false");
        }

        public virtual void WriteValue(int value)
        {
            WriteString(value.ToString(null, CultureInfo.InvariantCulture));
        }

        public virtual void WriteValue(long value)
        {
            WriteString(value.ToString(null, CultureInfo.InvariantCulture));
        }

        public virtual void WriteValue(float value)
        {
            WriteString(value.ToString(null, CultureInfo.InvariantCulture));
        }

        public virtual void WriteValue(double value)
        {
            WriteString(value.ToString(null, CultureInfo.InvariantCulture));
        }

        public virtual void WriteValue(decimal value)
        {
            WriteString(value.ToString(null, CultureInfo.InvariantCulture));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing
                    && WriteState != WriteState.Closed)
                {
                    Close();
                }

                _disposed = true;
            }
        }

        public virtual void Close()
        {
            Dispose();
        }
    }
}