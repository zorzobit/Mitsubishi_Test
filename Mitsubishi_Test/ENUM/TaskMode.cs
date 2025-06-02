namespace Mitsubishi_Test
{
    [Flags]
    public enum TaskMode : byte
    {
        Repeat = 0,
        Cycle = 1,
        Unknown = 0xff,
    }
}
