
using System;
namespace drz.Abstractions.Services.Message
{
    [Flags]
    public enum MessageIcon : uint
    {
        None = 0x00000000,
        Error = 0x00000010,
        Question = 0x00000020,
        Warning = 0x00000030,
        Information = 0x00000040
    }
}