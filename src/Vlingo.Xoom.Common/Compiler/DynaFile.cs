// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.IO;

namespace Vlingo.Xoom.Common.Compiler;

public static class DynaFile
{
    public static readonly string GeneratedSources = $"target{Path.DirectorySeparatorChar}generated-sources{Path.DirectorySeparatorChar}";
    public static readonly string GeneratedTestSources = $"target{Path.DirectorySeparatorChar}generated-test-sources{Path.DirectorySeparatorChar}";
    public static readonly string RootOfMainClasses = $"target{Path.DirectorySeparatorChar}classes{Path.DirectorySeparatorChar}";
    public static readonly string RootOfTestClasses = $"target{Path.DirectorySeparatorChar}test-classes{Path.DirectorySeparatorChar}";

    public static FileInfo PersistDynaClass(string pathToClass, byte[] dynaClass)
    {
        var file = new FileInfo(pathToClass);
        // empty text file
        File.WriteAllText(pathToClass, string.Empty);
        
        using var stream = file.OpenWrite();
        stream.Write(dynaClass, 0, dynaClass.Length);

        return file;
    }

    public static FileInfo PersistDynaClassSource(string pathToSource, string dynaClassSource)
    {
        var file = new FileInfo(pathToSource);
        // empty text file
        File.WriteAllText(pathToSource, string.Empty);

        using var stream = new StreamWriter(file.OpenWrite());
        stream.Write(dynaClassSource);

        return file;
    }

    public static string ToFullPath(string fullyQualifiedClassName) => ToPath(fullyQualifiedClassName, true);

    public static string ToNamespacePath(string fullyQualifiedClassName) => ToPath(fullyQualifiedClassName, false);

    private static string ToPath(string fullyQualifiedClassName, bool includeClassName)
    {
        var safeGenericClassName = fullyQualifiedClassName;
        var indexOfGenericStart = safeGenericClassName.IndexOf('<');
        if(indexOfGenericStart >= 0)
        {
            safeGenericClassName = safeGenericClassName.Substring(0, indexOfGenericStart);
        }

        var lastDotIndex = safeGenericClassName.LastIndexOf('.');
        var directoryPath = safeGenericClassName.Substring(0, lastDotIndex).Replace('.', Path.DirectorySeparatorChar);
        if (includeClassName)
        {
            return $"{directoryPath}{Path.DirectorySeparatorChar}{safeGenericClassName.Substring(lastDotIndex + 1)}";
        }
        else
        {
            return directoryPath;
        }
    }
}