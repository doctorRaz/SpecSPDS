using System;

namespace dRz.Abstractions.Services.Message
{
    [Flags]
    public enum MessageButtons : uint
    {
        Ok = 0x00000000,
        OkCancel = 0x00000001,
        AbortRetryIgnore = 0x00000002,
        YesNoCancel = 0x00000003,
        YesNo = 0x00000004,
        RetryCancel = 0x00000005
    }
}
