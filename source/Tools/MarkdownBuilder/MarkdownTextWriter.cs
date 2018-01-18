﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Pihrtsoft.Markdown.Linq;

namespace Pihrtsoft.Markdown
{
    internal class MarkdownTextWriter : MarkdownWriter
    {
        private const int BufferSize = 1024 * 6;
        private const int BufferOverflow = 32;

        private TextWriter _writer;
        private bool _skipWrite;

        private readonly char[] _bufChars;
        private int _bufPos;
        private readonly int _bufLen = BufferSize;

        public MarkdownTextWriter(TextWriter writer, MarkdownWriterSettings settings)
            : base(settings)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));

            _bufChars = new char[_bufLen + BufferOverflow];
        }

        protected internal override int Length { get; set; }

        public override unsafe MarkdownWriter WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return this;

            OnBeforeWrite();
            WriteStringUnsafe(value);
            return this;
        }

        private unsafe void WriteStringUnsafe(string value)
        {
            fixed (char* pSrcStart = value)
            {
                WriteStringUnsafe(pSrcStart, pSrcStart + value.Length);
            }
        }

        private unsafe void WriteStringUnsafe(char* pSrcStart, char* pSrcEnd)
        {
            fixed (char* pDstStart = _bufChars)
            {
                char* pDst = pDstStart + _bufPos;
                char* pSrc = pSrcStart;

                int ch = 0;
                while (true)
                {
                    char* pDstEnd = pDst + (pSrcEnd - pSrc);

                    if (pDstEnd > pDstStart + _bufLen)
                        pDstEnd = pDstStart + _bufLen;

                    while (pDst < pDstEnd
                        && !ShouldBeEscaped((char)(ch = *pSrc))) //TODO: \r\n
                    {
                        pSrc++;
                        *pDst = (char)ch;
                        pDst++;
                        Length++;
                    }

                    if (pSrc >= pSrcEnd)
                        break;

                    if (pDst >= pDstEnd)
                    {
                        _bufPos = (int)(pDst - pDstStart);
                        FlushBuffer();
                        pDst = pDstStart;
                        continue;
                    }

                    switch (ch)
                    {
                        case (char)10:
                            {
                                switch (NewLineHandling)
                                {
                                    case NewLineHandling.Replace:
                                        {
                                            pDst = WriteNewLine(pDst);
                                            break;
                                        }
                                    case NewLineHandling.None:
                                        {
                                            *pDst = (char)ch;
                                            pDst++;
                                            Length++;
                                            break;
                                        }
                                }

                                OnAfterWriteLine();

                                if (pSrc < pSrcEnd)
                                {
                                    WriteIndentation();
                                    _startOfLine = true;
                                }

                                break;
                            }
                        case (char)13:
                            {
                                switch (NewLineHandling)
                                {
                                    case NewLineHandling.Replace:
                                        {
                                            if (pSrc < pSrcEnd
                                                && pSrc[1] == '\n')
                                            {
                                                pSrc++;
                                            }

                                            pDst = WriteNewLine(pDst);
                                            break;
                                        }
                                    case NewLineHandling.None:
                                        {
                                            OnBeforeWrite();
                                            *pDst = (char)ch;
                                            pDst++;
                                            Length++;
                                            break;
                                        }
                                }

                                OnAfterWriteLine();

                                if (pSrc < pSrcEnd)
                                {
                                    WriteIndentation();
                                    _startOfLine = true;
                                }

                                break;
                            }
                        default:
                            {
                                *pDst = EscapingChar;
                                pDst++;
                                Length++;
                                *pDst = (char)ch;
                                pDst++;
                                Length++;
                                break;
                            }
                    }

                    pSrc++;
                }

                _bufPos = (int)(pDst - pDstStart);
            }
        }

        public override MarkdownWriter WriteRaw(string value)
        {
            if (string.IsNullOrEmpty(value))
                return this;

            OnBeforeWrite();
            WriteRawUnsafe(value);
            return this;
        }

        private unsafe void WriteRawUnsafe(string s)
        {
            fixed (char* pSrcStart = s)
            {
                WriteRawUnsafe(pSrcStart, pSrcStart + s.Length);
            }
        }

        private unsafe void WriteRawUnsafe(char* pSrcStart, char* pSrcEnd)
        {
            fixed (char* pDstStart = _bufChars)
            {
                char* pDst = pDstStart + _bufPos;
                char* pSrc = pSrcStart;

                while (true)
                {
                    char* pDstEnd = pDst + (pSrcEnd - pSrc);

                    if (pDstEnd > pDstStart + _bufLen)
                        pDstEnd = pDstStart + _bufLen;

                    while (pDst < pDstEnd)
                    {
                        *pDst = *pSrc;
                        pSrc++;
                        pDst++;
                        Length++;
                    }

                    if (pSrc >= pSrcEnd)
                        break;

                    if (pDst >= pDstEnd)
                    {
                        _bufPos = (int)(pDst - pDstStart);
                        FlushBuffer();
                        pDst = pDstStart;
                        continue;
                    }
                }

                _bufPos = (int)(pDst - pDstStart);
            }
        }

        private unsafe char* WriteNewLine(char* pDst)
        {
            fixed (char* pDstStart = _bufChars)
            {
                _bufPos = (int)(pDst - pDstStart);
                WriteRawUnsafe(Settings.NewLineChars);
                return pDstStart + _bufPos;
            }
        }

        public override MarkdownWriter WriteLine()
        {
            WriteRawUnsafe(Settings.NewLineChars);
            OnAfterWriteLine();
            return this;
        }

        public override void WriteValue(int value)
        {
            _writer.Write(value);
        }

        public override void WriteValue(long value)
        {
            _writer.Write(value);
        }

        public override void WriteValue(float value)
        {
            _writer.Write(value);
        }

        public override void WriteValue(double value)
        {
            _writer.Write(value);
        }

        public override void WriteValue(decimal value)
        {
            _writer.Write(value);
        }

        public override void Flush()
        {
            FlushBuffer();

            _writer?.Flush();
        }

        protected virtual void FlushBuffer()
        {
            try
            {
                if (!_skipWrite)
                    _writer.Write(_bufChars, 0, _bufPos);
            }
            catch
            {
                _skipWrite = true;
                throw;
            }
            finally
            {
                _bufPos = 0;
            }
        }

        public override void Close()
        {
            try
            {
                FlushBuffer();
            }
            finally
            {
                _skipWrite = true;

                if (_writer != null)
                {
                    try
                    {
                        _writer.Flush();
                    }
                    finally
                    {
                        try
                        {
                            if (Settings.CloseOutput)
                                _writer.Dispose();
                        }
                        finally
                        {
                            _writer = null;
                        }
                    }
                }
            }
        }

        protected internal override IReadOnlyList<TableColumnInfo> AnalyzeTable(IEnumerable<MElement> rows)
        {
            return TableAnalyzer.Analyze(rows, Settings, _writer.FormatProvider)?.AsReadOnly();
        }
    }
}