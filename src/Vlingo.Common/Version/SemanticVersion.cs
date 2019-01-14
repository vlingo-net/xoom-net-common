// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Common.Version
{
    public class SemanticVersion
    {
        private const int MAJOR_MASK = 0x7fff0000;
        private const int MAJOR_SHIFT = 16;
        private const int MAJOR_MAX = 32767;
        private const int MINOR_MASK = 0x0000ff00;
        private const int MINOR_SHIFT = 8;
        private const int MINOR_MAX = 255;
        private const int PATCH_MASK = 0x000000ff;
        private const int PATCH_MAX = 255;

        private readonly int major;
        private readonly int minor;
        private readonly int patch;

        private SemanticVersion(int major, int minor, int patch)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
        }

        public static SemanticVersion From(int major, int minor, int patch) 
            => new SemanticVersion(major, minor, patch);

        public static SemanticVersion From(string version)
        {
            var parts = version.Split('.');
            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patch = int.Parse(parts[2]);
            return new SemanticVersion(major, minor, patch);
        }

        public static string ToString(int version)
            => $"{(version >> MAJOR_SHIFT)}.{((version & MINOR_MASK) >> MINOR_SHIFT)}.{(version & PATCH_MASK)}";

        public static int ToValue(int major, int minor, int patch)
        {
            if (major < 0 || major > MAJOR_MAX)
            {
                throw new ArgumentException($"Major version must be 0 to {MAJOR_MAX}");
            }
            if (minor < 0 || minor > MINOR_MAX)
            {
                throw new ArgumentException($"Minor version must 0 to {MINOR_MAX}");
            }
            if (patch < 0 || patch > PATCH_MAX)
            {
                throw new ArgumentException($"Patch version must be 0 to {PATCH_MAX}");
            }
            return ((major << MAJOR_SHIFT) & MAJOR_MASK) | ((minor << MINOR_SHIFT) & MINOR_MASK) | (patch & PATCH_MASK);
        }
        public static int ToValue(string version)
        {
            var parts = version.Split('.');
            return ToValue(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        public bool IsCompatibleWith(SemanticVersion previous)
        {
            if (major == previous.major && minor == previous.minor && patch == previous.patch + 1)
            {
                return true;
            }
            if (major == previous.major && minor == previous.minor + 1 && patch == previous.patch)
            {
                return true;
            }
            if (major == previous.major + 1 && minor == previous.minor && patch == previous.patch)
            {
                return true;
            }

            return false;
        }

        public SemanticVersion WithIncrementedMajor() => new SemanticVersion(major + 1, minor, patch);

        public SemanticVersion WithIncrementedMinor() => new SemanticVersion(major, minor + 1, patch);

        public SemanticVersion WithIncrementedPatch() => new SemanticVersion(major, minor, patch + 1);

        public override string ToString() => $"{major}.{minor}.{patch}";

        public int ToValue() => ToValue(major, minor, patch);
    }
}
