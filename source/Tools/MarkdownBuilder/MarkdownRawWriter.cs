﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pihrtsoft.Markdown
{
    internal abstract class MarkdownRawWriter : MarkdownWriter
    {
        private int _lineStartPos;
        private int _emptyLineStartPos = -1;

        private int _headingLevel = -1;

        private IReadOnlyList<TableColumnInfo> _tableColumns;
        private int _tableColumnCount = -1;
        private int _tableRowIndex = -1;
        private int _tableColumnIndex = -1;
        private int _tableCellPos = -1;

        protected State _state;

        private readonly List<State> _states = new List<State>();

        protected MarkdownRawWriter(MarkdownWriterSettings settings = null)
        {
            Settings = settings ?? MarkdownWriterSettings.Default;
        }

        public override WriteState WriteState
        {
            get
            {
                switch (_state)
                {
                    case State.Start:
                        return WriteState.Start;
                    case State.SimpleElement:
                    case State.IndentedCodeBlock:
                    case State.FencedCodeBlock:
                    case State.HorizontalRule:
                    case State.Heading:
                    case State.Bold:
                    case State.Italic:
                    case State.Strikethrough:
                    case State.Table:
                    case State.TableRow:
                    case State.TableCell:
                    case State.BlockQuote:
                    case State.BulletItem:
                    case State.OrderedItem:
                    case State.TaskItem:
                    case State.Document:
                        return WriteState.Content;
                    case State.Closed:
                        return WriteState.Closed;
                    case State.Error:
                        return WriteState.Error;
                    default:
                        throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(_state));
                }
            }
        }

        public override MarkdownWriterSettings Settings { get; }

        protected internal abstract int Length { get; set; }

        protected Func<char, bool> ShouldBeEscaped { get; set; } = MarkdownEscaper.ShouldBeEscaped;

        protected char EscapingChar { get; set; } = '\\';

        private TableColumnInfo CurrentColumn => _tableColumns?[_tableColumnIndex] ?? TableColumnInfo.Default;

        private int ColumnCount => _tableColumns?.Count ?? _tableColumnCount;

        private bool IsLastColumn => _tableColumnIndex == ColumnCount - 1;

        private bool IsFirstColumn => _tableColumnIndex == 0;

        private void Push(State state)
        {
            if (_state == State.Closed)
                throw new InvalidOperationException("Cannot write to a closed writer.");

            if (_state == State.Error)
                throw new InvalidOperationException("Cannot write to a writer in error state.");

            State newState = _stateTable[((int)_state * 15) + (int)state - 1];

            if (newState == State.Error)
                throw new InvalidOperationException($"Cannot move from from state '{_state}' to state '{state}'.");

            _states.Add((_state == State.Start) ? State.Document : _state);
            _state = newState;
        }

        private void Pop(State state)
        {
            int count = _states.Count;

            if (count == 0)
                throw new InvalidOperationException($"Cannot move from from state '{_state}' to state '{state}'.");

            if (_state != state)
                throw new InvalidOperationException($"Cannot move from from state '{_state}' to state '{state}'.");

            _state = _states[count - 1];
            _states.RemoveAt(count - 1);
        }

        private void ThrowIfCannotWriteEnd(State state)
        {
            if (state != _state)
                throw new InvalidOperationException($"Cannot close '{state}' when state is '{_state}'.");
        }

        public override void WriteStartBold()
        {
            try
            {
                Push(State.Bold);
                WriteRaw(Format.BoldDelimiter);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndBold()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.Bold);
                WriteRaw(Format.BoldDelimiter);
                Pop(State.Bold);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteBold(string text)
        {
            try
            {
                base.WriteBold(text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartItalic()
        {
            try
            {
                Push(State.Italic);
                WriteRaw(Format.ItalicDelimiter);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndItalic()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.Italic);
                WriteRaw(Format.ItalicDelimiter);
                Pop(State.Italic);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteItalic(string text)
        {
            try
            {
                base.WriteItalic(text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartStrikethrough()
        {
            try
            {
                Push(State.Strikethrough);
                WriteRaw("~~");
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndStrikethrough()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.Strikethrough);
                WriteRaw("~~");
                Pop(State.Strikethrough);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStrikethrough(string text)
        {
            try
            {
                base.WriteStrikethrough(text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteInlineCode(string text)
        {
            try
            {
                Push(State.SimpleElement);
                WriteRaw("`");

                if (!string.IsNullOrEmpty(text))
                {
                    if (text[0] == '`')
                        WriteRaw(" ");

                    WriteString(text, MarkdownEscaper.ShouldBeEscapedInInlineCode, '`');

                    if (text[text.Length - 1] == '`')
                        WriteRaw(" ");
                }

                WriteRaw("`");
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartHeading(int level)
        {
            try
            {
                Error.ThrowOnInvalidHeadingLevel(level);

                Push(State.Heading);

                _headingLevel = level;

                bool underline = (level == 1 && Format.UnderlineHeading1)
                    || (level == 2 && Format.UnderlineHeading2);

                WriteLine(Format.EmptyLineBeforeHeading);

                if (!underline)
                {
                    WriteRaw(Format.HeadingStart, level);
                    WriteRaw(" ");
                }
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndHeading()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.Heading);

                int level = _headingLevel;
                _headingLevel = -1;

                bool underline = (level == 1 && Format.UnderlineHeading1)
                    || (level == 2 && Format.UnderlineHeading2);

                if (!underline
                    && Format.CloseHeading)
                {
                    WriteRaw(" ");
                    WriteRaw(Format.HeadingStart, level);
                }

                int length = Length - _lineStartPos;

                WriteLineIfNecessary();

                if (underline)
                {
                    WriteRaw((level == 1) ? "=" : "-", length);
                    WriteLine();
                }

                WriteEmptyLineIf(Format.EmptyLineAfterHeading);
                Pop(State.Heading);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteHeading(int level, string text)
        {
            try
            {
                base.WriteHeading(level, text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartBulletItem()
        {
            try
            {
                WriteLineIfNecessary();
                WriteRaw(Format.BulletItemStart);
                Push(State.BulletItem);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndBulletItem()
        {
            try
            {
                Pop(State.BulletItem);
                WriteLineIfNecessary();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteBulletItem(string text)
        {
            try
            {
                base.WriteBulletItem(text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartOrderedItem(int number)
        {
            try
            {
                Error.ThrowOnInvalidItemNumber(number);
                WriteLineIfNecessary();
                WriteValue(number);
                WriteRaw(Format.OrderedItemStart);
                Push(State.OrderedItem);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndOrderedItem()
        {
            try
            {
                Pop(State.OrderedItem);
                WriteLineIfNecessary();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteOrderedItem(int number, string text)
        {
            try
            {
                base.WriteOrderedItem(number, text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartTaskItem(bool isCompleted = false)
        {
            try
            {
                WriteLineIfNecessary();

                if (isCompleted)
                {
                    WriteRaw("- [x] ");
                }
                else
                {
                    WriteRaw("- [ ] ");
                }

                Push(State.TaskItem);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndTaskItem()
        {
            try
            {
                Pop(State.TaskItem);
                WriteLineIfNecessary();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteImage(string text, string url, string title = null)
        {
            try
            {
                if (text == null)
                    throw new ArgumentNullException(nameof(text));

                Error.ThrowOnInvalidUrl(url);

                Push(State.SimpleElement);
                WriteRaw("!");
                WriteLinkCore(text, url, title);
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteLink(string text, string url, string title = null)
        {
            try
            {
                if (text == null)
                    throw new ArgumentNullException(nameof(text));

                Error.ThrowOnInvalidUrl(url);

                Push(State.SimpleElement);
                WriteLinkCore(text, url, title);
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteLinkCore(string text, string url, string title)
        {
            WriteSquareBrackets(text);
            WriteRaw("(");
            WriteString(url, MarkdownEscaper.ShouldBeEscapedInLinkUrl);
            WriteLinkTitle(title);
            WriteRaw(")");
        }

        public override void WriteAutolink(string url)
        {
            try
            {
                Error.ThrowOnInvalidUrl(url);
                Push(State.SimpleElement);
                WriteAngleBrackets(url);
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteImageReference(string text, string label)
        {
            try
            {
                Push(State.SimpleElement);
                WriteRaw("!");
                WriteLinkReferenceCore(text, label);
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteLinkReference(string text, string label = null)
        {
            try
            {
                Push(State.SimpleElement);
                WriteLinkReferenceCore(text, label);
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteLinkReferenceCore(string text, string label = null)
        {
            WriteSquareBrackets(text);
            WriteSquareBrackets(label);
        }

        public override void WriteLabel(string label, string url, string title = null)
        {
            try
            {
                Error.ThrowOnInvalidUrl(url);

                Push(State.SimpleElement);
                WriteLineIfNecessary();
                WriteSquareBrackets(label);
                WriteRaw(": ");
                WriteAngleBrackets(url);
                WriteLinkTitle(title);
                WriteLineIfNecessary();
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteLinkTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                return;

            WriteRaw(" ");
            WriteRaw("\"");
            WriteString(title, MarkdownEscaper.ShouldBeEscapedInLinkTitle);
            WriteRaw("\"");
        }

        private void WriteSquareBrackets(string text)
        {
            WriteRaw("[");
            WriteString(text, MarkdownEscaper.ShouldBeEscapedInLinkText);
            WriteRaw("]");
        }

        private void WriteAngleBrackets(string text)
        {
            WriteRaw("<");
            WriteString(text, MarkdownEscaper.ShouldBeEscapedInAngleBrackets);
            WriteRaw(">");
        }

        public override void WriteIndentedCodeBlock(string text)
        {
            try
            {
                Push(State.IndentedCodeBlock);
                WriteLine(Format.EmptyLineBeforeCodeBlock);
                WriteString(text, _ => false);
                WriteLine();
                WriteEmptyLineIf(Format.EmptyLineAfterCodeBlock);
                Pop(State.IndentedCodeBlock);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteFencedCodeBlock(string text, string info = null)
        {
            try
            {
                Error.ThrowOnInvalidFencedCodeBlockInfo(info);
                Push(State.FencedCodeBlock);

                WriteLine(Format.EmptyLineBeforeCodeBlock);

                WriteRaw(Format.CodeFence);
                WriteRaw(info);
                WriteLine();
                WriteString(text, _ => false);

                WriteLineIfNecessary();

                WriteRaw(Format.CodeFence);

                WriteLine();
                WriteEmptyLineIf(Format.EmptyLineAfterCodeBlock);
                Pop(State.FencedCodeBlock);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartBlockQuote()
        {
            try
            {
                Push(State.BlockQuote);

                WriteLineIfNecessary();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndBlockQuote()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.BlockQuote);
                WriteLineIfNecessary();
                Pop(State.BlockQuote);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteBlockQuote(string text)
        {
            try
            {
                base.WriteBlockQuote(text);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteHorizontalRule(HorizontalRuleStyle style, int count = HorizontalRuleFormat.DefaultCount, string separator = HorizontalRuleFormat.DefaultSeparator)
        {
            try
            {
                Error.ThrowOnInvalidHorizontalRuleCount(count);
                Error.ThrowOnInvalidHorizontalRuleSeparator(separator);

                Push(State.HorizontalRule);

                WriteLineIfNecessary();

                bool isFirst = true;

                string text = GetText();

                for (int i = 0; i < count; i++)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        WriteRaw(separator);
                    }

                    WriteRaw(text);
                }

                WriteLine();
                Pop(State.HorizontalRule);
            }
            catch
            {
                _state = State.Error;
                throw;
            }

            string GetText()
            {
                switch (style)
                {
                    case HorizontalRuleStyle.Hyphen:
                        return "-";
                    case HorizontalRuleStyle.Underscore:
                        return "_";
                    case HorizontalRuleStyle.Asterisk:
                        return "*";
                    default:
                        throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(style));
                }
            }
        }

        public override void WriteStartTable(int columnCount)
        {
            WriteStartTable(null, columnCount);
        }

        public override void WriteStartTable(IReadOnlyList<TableColumnInfo> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            WriteStartTable(columns, columns.Count);
        }

        private void WriteStartTable(IReadOnlyList<TableColumnInfo> columns, int columnCount)
        {
            try
            {
                Push(State.Table);

                WriteLine(Format.EmptyLineBeforeTable);

                _tableColumns = columns;
                _tableColumnCount = columnCount;
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndTable()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.Table);
                _tableRowIndex = -1;
                _tableColumns = null;
                _tableColumnCount = -1;
                WriteEmptyLineIf(Format.EmptyLineAfterTable);
                Pop(State.Table);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartTableRow()
        {
            try
            {
                Push(State.TableRow);
                _tableRowIndex++;
                _tableColumnIndex = -1;
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndTableRow()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.TableRow);

                if (Format.TableOuterDelimiter
                    || (_tableRowIndex == 0 && CurrentColumn.IsWhiteSpace))
                {
                    WriteTableColumnSeparator();
                }

                WriteLine();
                _tableColumnIndex = -1;

                Pop(State.TableRow);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteStartTableCell()
        {
            try
            {
                Push(State.TableCell);

                _tableColumnIndex++;

                if (IsFirstColumn)
                {
                    if (Format.TableOuterDelimiter
                        || IsLastColumn
                        || CurrentColumn.IsWhiteSpace)
                    {
                        WriteTableColumnSeparator();
                    }
                }
                else
                {
                    WriteTableColumnSeparator();
                }

                if (_tableRowIndex == 0)
                {
                    if (Format.TablePadding)
                    {
                        WriteRaw(" ");
                    }
                    else if (Format.FormatTableHeader
                         && CurrentColumn.Alignment == HorizontalAlignment.Center)
                    {
                        WriteRaw(" ");
                    }
                }
                else if (Format.TablePadding)
                {
                    WriteRaw(" ");
                }

                _tableCellPos = Length;
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEndTableCell()
        {
            try
            {
                ThrowIfCannotWriteEnd(State.TableCell);

                if (Format.TableOuterDelimiter
                    || !IsLastColumn)
                {
                    if (_tableRowIndex == 0)
                    {
                        if (Format.FormatTableHeader)
                            WritePadRight(Length - _tableCellPos);
                    }
                    else if (Format.FormatTableContent)
                    {
                        WritePadRight(Length - _tableCellPos);
                    }
                }

                if (_tableRowIndex == 0)
                {
                    if (Format.TablePadding)
                    {
                        if (!CurrentColumn.IsWhiteSpace)
                            WriteRaw(" ");
                    }
                    else if (Format.FormatTableHeader
                         && CurrentColumn.Alignment != HorizontalAlignment.Left)
                    {
                        WriteRaw(" ");
                    }
                }
                else if (Format.TablePadding)
                {
                    if (Length - _tableCellPos > 0)
                        WriteRaw(" ");
                }

                _tableCellPos = -1;
                Pop(State.TableCell);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteTableHeaderSeparator()
        {
            try
            {
                WriteLineIfNecessary();

                WriteStartTableRow();

                int count = ColumnCount;

                for (int i = 0; i < count; i++)
                {
                    _tableColumnIndex = i;

                    if (IsFirstColumn)
                    {
                        if (Format.TableOuterDelimiter
                            || IsLastColumn
                            || CurrentColumn.IsWhiteSpace)
                        {
                            WriteTableColumnSeparator();
                        }
                    }
                    else
                    {
                        WriteTableColumnSeparator();
                    }

                    if (CurrentColumn.Alignment == HorizontalAlignment.Center)
                    {
                        WriteRaw(":");
                    }
                    else if (Format.TablePadding)
                    {
                        WriteRaw(" ");
                    }

                    WriteRaw("---");

                    if (Format.FormatTableHeader)
                        WritePadRight(3, "-");

                    if (CurrentColumn.Alignment != HorizontalAlignment.Left)
                    {
                        WriteRaw(":");
                    }
                    else if (Format.TablePadding)
                    {
                        WriteRaw(" ");
                    }
                }

                WriteEndTableRow();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteTableColumnSeparator()
        {
            WriteRaw("|");
        }

        private void WritePadRight(int width, string padding = " ")
        {
            int totalWidth = Math.Max(CurrentColumn.Width, Math.Max(width, 3));

            WriteRaw(padding, totalWidth - width);
        }

        public override void WriteCharEntity(char value)
        {
            try
            {
                Error.ThrowOnInvalidCharEntity(value);

                Push(State.SimpleElement);
                WriteRaw("&#");

                if (Format.CharEntityFormat == CharEntityFormat.Hexadecimal)
                {
                    WriteRaw("x");
                    WriteRaw(((int)value).ToString("X", CultureInfo.InvariantCulture));
                }
                else if (Format.CharEntityFormat == CharEntityFormat.Decimal)
                {
                    WriteRaw(((int)value).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new InvalidOperationException(ErrorMessages.UnknownEnumValue(Format.CharEntityFormat));
                }

                WriteRaw(";");
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteEntityRef(string name)
        {
            try
            {
                Push(State.SimpleElement);
                WriteRaw("&");
                WriteRaw(name);
                WriteRaw(";");
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteComment(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.IndexOf("--", StringComparison.Ordinal) >= 0)
                        throw new ArgumentException("XML comment text cannot contain '--'.");

                    if (text[text.Length - 1] == '-')
                        throw new ArgumentException("Last character of XML comment text cannot be '-'.");
                }

                Push(State.SimpleElement);
                WriteRaw("<!-- ");
                WriteRaw(text);
                WriteRaw(" -->");
                Pop(State.SimpleElement);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteString(string text)
        {
            if (_state == State.Table
                || _state == State.TableRow)
            {
                throw new InvalidOperationException($"Cannot write text in state '{_state}'.");
            }
            else if (_state == State.Start)
            {
                _state = State.Document;
            }

            if (_lineStartPos == Length)
                WriteIndentation();
        }

        private void WriteString(string text, Func<char, bool> shouldBeEscaped, char escapingChar = '\\')
        {
            try
            {
                ShouldBeEscaped = shouldBeEscaped;
                EscapingChar = escapingChar;
                WriteString(text);
            }
            finally
            {
                EscapingChar = '\\';
                ShouldBeEscaped = MarkdownEscaper.ShouldBeEscaped;
            }
        }

        public override void WriteRaw(string data)
        {
            if (_state == State.Start)
                _state = State.Document;

            if (_lineStartPos == Length)
                WriteIndentation();
        }

        private void WriteRaw(string data, int repeatCount)
        {
            for (int i = 0; i < repeatCount; i++)
                WriteRaw(data);
        }

        protected void WriteIndentation()
        {
            for (int i = 0; i < _states.Count; i++)
                WriteIndentation(_states[i]);

            if (_state == State.IndentedCodeBlock)
            {
                this.WriteIndentation("    ");
            }
            else
            {
                WriteIndentation(_state);
            }

            void WriteIndentation(State state)
            {
                switch (state)
                {
                    case State.BulletItem:
                    case State.OrderedItem:
                    case State.TaskItem:
                        {
                            this.WriteIndentation("  ");
                            break;
                        }
                    case State.BlockQuote:
                        {
                            this.WriteIndentation("> ");
                            break;
                        }
                }
            }
        }

        protected abstract void WriteIndentation(string value);

        protected void OnBeforeWriteLine()
        {
            //TODO: Table

            if (_lineStartPos == Length)
            {
                _emptyLineStartPos = _lineStartPos;
            }
            else
            {
                _emptyLineStartPos = -1;
            }
        }

        protected void OnAfterWriteLine()
        {
            if (_emptyLineStartPos == _lineStartPos)
                _emptyLineStartPos = Length;

            _lineStartPos = Length;
        }

        internal void WriteLineIfNecessary()
        {
            if (_lineStartPos != Length)
                WriteLine();
        }

        private void WriteEmptyLineIf(bool condition)
        {
            if (condition
                && Length > 0)
            {
                WriteLine();
            }
        }

        private void WriteLine(bool addEmptyLine)
        {
            if (_emptyLineStartPos != Length)
            {
                if (_lineStartPos != Length)
                    WriteLine();

                WriteEmptyLineIf(addEmptyLine);
            }
        }

        protected enum State
        {
            Start = 0,
            SimpleElement = 1,
            FencedCodeBlock = 2,
            IndentedCodeBlock = 3,
            HorizontalRule = 4,
            Heading = 5,
            Bold = 6,
            Italic = 7,
            Strikethrough = 8,
            Table = 9,
            TableRow = 10,
            TableCell = 11,
            BulletItem = 12,
            OrderedItem = 13,
            TaskItem = 14,
            BlockQuote = 15,
            Document = 16,
            Closed = 17,
            Error = 18
        }

        private static readonly State[] _stateTable =
        {
            /* State.Start */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote,

            /* State.SimpleElement */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.FencedCodeBlock */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.IndentedCodeBlock */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.HorizontalRule */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.Heading */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.Bold */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.Italic */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.Strikethrough */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.Table */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.TableRow,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.TableRow */
            /* State.SimpleElement     */ State.Error,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Error,
            /* State.Italic            */ State.Error,
            /* State.Strikethrough     */ State.Error,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.TableCell,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.TableCell */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.Error,
            /* State.IndentedCodeBlock */ State.Error,
            /* State.HorizontalRule    */ State.Error,
            /* State.Heading           */ State.Error,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Error,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.Error,
            /* State.OrderedItem       */ State.Error,
            /* State.TaskItem          */ State.Error,
            /* State.BlockQuote        */ State.Error,

            /* State.BulletItem */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote,

            /* State.OrderedItem */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote,

            /* State.TaskItem */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote,

            /* State.BlockQuote */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote,

            /* State.Document */
            /* State.SimpleElement     */ State.SimpleElement,
            /* State.FencedCodeBlock   */ State.FencedCodeBlock,
            /* State.IndentedCodeBlock */ State.IndentedCodeBlock,
            /* State.HorizontalRule    */ State.HorizontalRule,
            /* State.Heading           */ State.Heading,
            /* State.Bold              */ State.Bold,
            /* State.Italic            */ State.Italic,
            /* State.Strikethrough     */ State.Strikethrough,
            /* State.Table             */ State.Table,
            /* State.TableRow          */ State.Error,
            /* State.TableCell         */ State.Error,
            /* State.BulletItem        */ State.BulletItem,
            /* State.OrderedItem       */ State.OrderedItem,
            /* State.TaskItem          */ State.TaskItem,
            /* State.BlockQuote        */ State.BlockQuote
        };
    }
}