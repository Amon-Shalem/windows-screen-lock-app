using System;

namespace CustomScreenLocker
{
    public class LockPreset
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Theme { get; set; } = "ClassicBlack";

        public override string ToString() => Title;
    }
}
