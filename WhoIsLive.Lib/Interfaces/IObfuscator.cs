namespace WhoIsLive.Lib.Interfaces
{
    public interface IObfuscator
    {
        byte[] Obfuscate(byte[] data);

        byte[] DeObfuscate(byte[] data);
    }
}