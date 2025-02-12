﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMSManagerRebuilt;

public interface IConfigEntry<T> where T : IConfigEntry<T>
{
    int Size { get; }

    bool TryWrite(Span<byte> buffer, out int written);
    abstract bool TryRead(ReadOnlySpan<byte> buffer, out T value);
}
