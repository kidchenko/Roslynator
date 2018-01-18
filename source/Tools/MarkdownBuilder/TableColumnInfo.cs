﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Pihrtsoft.Markdown.Linq;

namespace Pihrtsoft.Markdown
{
    [DebuggerDisplay("Alignment = {Alignment} Width = {Width} IsWhiteSpace = {IsWhiteSpace}")]
    public struct TableColumnInfo : IEquatable<TableColumnInfo>
    {
        public TableColumnInfo(Alignment alignment, int width, bool isWhiteSpace)
        {
            Alignment = alignment;
            Width = width;
            IsWhiteSpace = isWhiteSpace;
        }

        public Alignment Alignment { get; }

        public bool IsWhiteSpace { get; }

        public int Width { get; }

        internal TableColumnInfo UpdateWidthIfGreater(int newWidth)
        {
            if (newWidth > Width)
            {
                return new TableColumnInfo(Alignment, newWidth, IsWhiteSpace);
            }
            else
            {
                return this;
            }
        }

        internal static TableColumnInfo Create(MElement element)
        {
            Alignment alignment = (element as MTableColumn)?.Alignment ?? Alignment.Left;

            return new TableColumnInfo(alignment, 0, true);
        }

        internal static TableColumnInfo Create(MElement element, MarkdownStringWriter writer, int index = 0)
        {
            Alignment alignment = (element as MTableColumn)?.Alignment ?? Alignment.Left;

            int length = writer.Length - index;

            return new TableColumnInfo(alignment, length, writer.GetStringBuilder().IsWhiteSpace(index, length));
        }

        public override bool Equals(object obj)
        {
            return (obj is TableColumnInfo other)
                && Equals(other);
        }

        public bool Equals(TableColumnInfo other)
        {
            return Alignment == other.Alignment
                   && IsWhiteSpace == other.IsWhiteSpace
                   && Width == other.Width;
        }

        public override int GetHashCode()
        {
            return Hash.Combine((int)Alignment, Hash.Combine(IsWhiteSpace, Hash.Create(Width)));
        }

        public static bool operator ==(TableColumnInfo info1, TableColumnInfo info2)
        {
            return info1.Equals(info2);
        }

        public static bool operator !=(TableColumnInfo info1, TableColumnInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
