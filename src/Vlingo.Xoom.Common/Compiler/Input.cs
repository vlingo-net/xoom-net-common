// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;

namespace Vlingo.Xoom.Common.Compiler
{
    public class Input
    {
        public Input(
            Type protocol,
            string fullyQualifiedClassName,
            string lookupTypeName,
            string source,
            FileInfo sourceFile,
            DynaClassLoader classLoader,
            DynaType type,
            bool persist)
        {
            Protocol = protocol;
            FullyQualifiedClassName = fullyQualifiedClassName;
            FullyQualifiedNameForTypeLookup = lookupTypeName;
            Source = source;
            SourceFile = sourceFile;
            ClassLoader = classLoader;
            Type = type;
            Persist = persist;
        }

        public Type Protocol { get; }
        public string FullyQualifiedClassName { get; }
        public string FullyQualifiedNameForTypeLookup { get; }
        public string Source { get; }
        public FileInfo SourceFile { get; }
        public DynaClassLoader ClassLoader { get; }
        public DynaType Type { get; }
        public bool Persist { get; }
    }
}
