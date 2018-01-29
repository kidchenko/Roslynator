// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Pihrtsoft.Markdown.Linq;

namespace Pihrtsoft.Markdown
{
    internal class MarkdownStringWriter : MarkdownRawWriter, ITableAnalyzer
    {
        private readonly StringBuilder _sb;
        private readonly IFormatProvider _formatProvider;
        private bool _isOpen;

        public MarkdownStringWriter(MarkdownWriterSettings settings = null)
            : this(new StringBuilder(), settings)
        {
        }

        public MarkdownStringWriter(StringBuilder sb, MarkdownWriterSettings settings = null)
            : this(sb, CultureInfo.CurrentCulture, settings)
        {
        }

        public MarkdownStringWriter(IFormatProvider formatProvider, MarkdownWriterSettings settings = null)
            : this(new StringBuilder(), formatProvider, settings)
        {
        }

        public MarkdownStringWriter(StringBuilder sb, IFormatProvider formatProvider, MarkdownWriterSettings settings = null)
            : base(settings)
        {
            _sb = sb ?? throw new ArgumentNullException(nameof(sb));
            _formatProvider = formatProvider;
            _isOpen = true;
        }

        protected internal virtual StringBuilder GetStringBuilder()
        {
            return _sb;
        }

        public virtual IFormatProvider FormatProvider
        {
            get { return _formatProvider ?? CultureInfo.CurrentCulture; }
        }

        protected internal override int Length
        {
            get { return _sb.Length; }
            set { _sb.Length = value; }
        }

        public override void WriteString(string text)
        {
            try
            {
                base.WriteString(text);

                ThrowIfClosed();

                if (string.IsNullOrEmpty(text))
                    return;

                int length = text.Length;

                int prev = 0;

                int i = 0;
                while (i < length)
                {
                    char ch = text[i];

                    if (ch == 10)
                    {
                        OnBeforeWriteLine();

                        if (NewLineHandling == NewLineHandling.Replace)
                        {
                            WriteRaw(text, prev, i - prev);
                            WriteNewLine();
                        }
                        else if (NewLineHandling == NewLineHandling.None)
                        {
                            WriteRaw(text, prev, i + 1 - prev);
                        }

                        OnAfterWriteLine();
                        prev = ++i;
                    }
                    else if (ch == 13)
                    {
                        OnBeforeWriteLine();

                        if (i < length - 1
                            && text[i + 1] == 10)
                        {
                            if (NewLineHandling == NewLineHandling.Replace)
                            {
                                WriteRaw(text, prev, i - prev);
                                WriteNewLine();
                            }
                            else if (NewLineHandling == NewLineHandling.None)
                            {
                                WriteRaw(text, prev, i + 2 - prev);
                            }

                            i++;
                        }
                        else if (NewLineHandling == NewLineHandling.Replace)
                        {
                            WriteRaw(text, prev, i - prev);
                            WriteNewLine();
                        }
                        else if (NewLineHandling == NewLineHandling.None)
                        {
                            WriteRaw(text, prev, i + 1 - prev);
                        }

                        OnAfterWriteLine();
                        prev = ++i;
                    }
                    else if (ShouldBeEscaped(ch))
                    {
                        WriteRaw(text, prev, i - prev);
                        WriteChar(EscapingChar);
                        WriteChar(ch);
                        prev = ++i;
                    }
                    else
                    {
                        i++;
                    }
                }

                WriteRaw(text, prev, text.Length - prev);
            }
            catch
            {
                _state = State.Error;
                throw;
            }

            void WriteRaw(string value, int startIndex, int count)
            {
                ThrowIfClosed();
                _sb.Append(value, startIndex, count);
            }

            void WriteChar(char ch)
            {
                ThrowIfClosed();
                _sb.Append(ch);
            }

            void WriteNewLine()
            {
                ThrowIfClosed();
                _sb.Append(NewLineChars);
            }
        }

        public override void WriteRaw(string data)
        {
            try
            {
                base.WriteRaw(data);
                ThrowIfClosed();
                _sb.Append(data);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteLine()
        {
            try
            {
                OnBeforeWriteLine();
                ThrowIfClosed();
                _sb.Append(NewLineChars);
                OnAfterWriteLine();
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        public override void WriteValue(int value)
        {
            WriteString(value.ToString(FormatProvider));
        }

        public override void WriteValue(long value)
        {
            WriteString(value.ToString(FormatProvider));
        }

        public override void WriteValue(float value)
        {
            WriteString(value.ToString(FormatProvider));
        }

        public override void WriteValue(double value)
        {
            WriteString(value.ToString(FormatProvider));
        }

        public override void WriteValue(decimal value)
        {
            WriteString(value.ToString(FormatProvider));
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            if (Settings.CloseOutput)
                _isOpen = false;
        }

        private void ThrowIfClosed()
        {
            if (!_isOpen)
                throw new ObjectDisposedException(null, "Cannot write to a closed writer.");
        }

        public IReadOnlyList<TableColumnInfo> AnalyzeTable(IEnumerable<MElement> rows)
        {
            return TableAnalyzer.Analyze(rows, Settings, FormatProvider)?.AsReadOnly();
        }
    }
}