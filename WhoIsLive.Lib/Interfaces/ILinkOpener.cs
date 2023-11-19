namespace WhoIsLive.Lib.Interfaces
{
    public interface ILinkOpener
    {
        public string DisplayName { get; }

        public string Directory { get; set; }

        bool IsInstalled();

        void Open(string url);

        void Open(Uri uri);
    }
}