﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.Markdown.Linq
{
    public class MItalic : MInline
    {
        public MItalic()
        {
        }

        public MItalic(object content)
            : base(content)
        {
        }

        public MItalic(params object[] content)
            : base(content)
        {
        }

        public MItalic(MItalic other)
            : base(other)
        {
        }

        public override MarkdownKind Kind => MarkdownKind.Italic;

        public override void WriteTo(MarkdownWriter writer)
        {
            writer.WriteStartItalic();
            WriteContentTo(writer);
            writer.WriteEndItalic();
        }

        internal override MElement Clone()
        {
            return new MItalic(this);
        }

        internal override void ValidateElement(MElement element)
        {
            if (element.Kind == MarkdownKind.Italic)
                Error.InvalidContent(this, element);

            base.ValidateElement(element);
        }
    }
}
