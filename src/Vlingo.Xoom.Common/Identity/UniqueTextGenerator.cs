// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text;

// ReSharper disable InconsistentNaming

namespace Vlingo.Xoom.Common.Identity;

/// <summary>
/// Generates unique text. The longer the generated String the more likely
/// it is to be unique. In tests, a length-10 String is consistently unique
/// to one million instances and greater (actually 100 million but requires
/// several seconds). When using length-7, -8, and -9, the uniqueness fails
/// before one million. Thus, the default length is 10. If you intend to use
/// this generator for short, unique identities, you should plan to ensure
/// uniqueness. Although the likelihood of uniqueness is high, you should
/// still compare against previously generated values, and if non-unique,
/// throw it out and retry generation. The uniqueness has not been tested
/// across simultaneously running machines.
/// </summary>
public class UniqueTextGenerator
{
    private const int DEFAULT_LENGTH = 10;

    private const string DIGITS = "0123456789";
    private const string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string SYMBOLS = "!?$%^&*_-+=:;@~#|,.";

    private readonly Random random;

    public UniqueTextGenerator()
    {
        random = new Random();
    }

    public string Generate() => Generate(DEFAULT_LENGTH);

    public string Generate(int length) => Generate(length, false);

    public string Generate(int length, bool useSymbols)
    {
        var generated = new StringBuilder();
        var maxOptions = useSymbols ? 4 : 3;

        int index;

        for (var i = 0; i < length; ++i)
        {
            var option = random.Next(maxOptions);
            switch (option)
            {
                case 0:
                    index = random.Next(LETTERS.Length);
                    generated.Append(char.ToLowerInvariant(LETTERS[index]));
                    break;
                case 1:
                    index = random.Next(LETTERS.Length);
                    generated.Append(LETTERS[index]);
                    break;
                case 2:
                    index = random.Next(DIGITS.Length);
                    generated.Append(DIGITS[index]);
                    break;
                case 3:
                    index = random.Next(SYMBOLS.Length);
                    generated.Append(SYMBOLS[index]);
                    break;
            }
        }

        return generated.ToString();
    }
}