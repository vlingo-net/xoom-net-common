// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
// ReSharper disable InconsistentNaming

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

        private readonly int _major;
        private readonly int _minor;
        private readonly int _patch;

        private SemanticVersion(int major, int minor, int patch)
        {
            _major = major;
            _minor = minor;
            _patch = patch;
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
            if (_major == previous._major && _minor == previous._minor && _patch == previous._patch + 1)
            {
                return true;
            }
            if (_major == previous._major && _minor == previous._minor + 1 && _patch == previous._patch)
            {
                return true;
            }
            if (_major == previous._major + 1 && _minor == previous._minor && _patch == previous._patch)
            {
                return true;
            }

            return false;
        }

        public SemanticVersion WithIncrementedMajor() => new SemanticVersion(_major + 1, _minor, _patch);

        public SemanticVersion WithIncrementedMinor() => new SemanticVersion(_major, _minor + 1, _patch);

        public SemanticVersion WithIncrementedPatch() => new SemanticVersion(_major, _minor, _patch + 1);
        
        public SemanticVersion NextPatch() => WithIncrementedPatch();

        public SemanticVersion NextMinor() => new SemanticVersion(_major, _minor + 1, 0);

        public SemanticVersion NextMajor() => new SemanticVersion(_major + 1, 0, 0);

        public override string ToString() => $"{_major}.{_minor}.{_patch}";

        public int ToValue() => ToValue(_major, _minor, _patch);

        public override bool Equals(object? obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var otherVersion = (SemanticVersion) obj;

            return _major == otherVersion._major && _minor == otherVersion._minor && _patch == otherVersion._patch;
        }

        public override int GetHashCode() => 31 * (_major + _minor + _patch + 1);
    }
}
