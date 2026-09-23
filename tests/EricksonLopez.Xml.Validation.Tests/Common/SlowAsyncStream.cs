// Copyright © Erickson Lopez. MIT License.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Xml.Validation.Tests.Common;

/// <summary>
/// A memory stream decorator that deterministically triggers cancellation upon the first async read,
/// guaranteeing reliable, non-flaky testing of cooperative cancellation loops.
/// </summary>
public sealed class SlowAsyncStream : MemoryStream
{
    private readonly CancellationTokenSource _cts;

    public SlowAsyncStream(byte[] buffer, CancellationTokenSource cts) : base(buffer)
    {
        _cts = cts;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        _cts.Cancel();
        return await base.ReadAsync(buffer, cancellationToken);
    }
}
