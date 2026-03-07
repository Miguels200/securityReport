namespace SecurityReport.Application.Interfaces
{
    public interface IPasswordHasherService
    {
        string Hash(string password);
        int Verify(string hash, string password);
    }
}
