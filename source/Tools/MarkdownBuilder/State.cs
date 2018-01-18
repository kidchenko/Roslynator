﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.Markdown
{
    public enum State
    {
        None,
        RawText,
        InlineCodeText,
        LinkText,
        LinkUrl,
        LinkTitle,
        AngleBrackets,
        Comment
    }
}