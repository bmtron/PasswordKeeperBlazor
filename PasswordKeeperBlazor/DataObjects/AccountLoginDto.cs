namespace PasswordKeeperBlazor.DataObjects;

public class AccountLoginDto
{
    public int LoginId { get; set; }

    public string Account { get; set; }

    public string Username { get; set; }

    public string PasswordEncrypted { get; set; }
    
    public string PasswordDecrypted { get; set; }

    public string Website { get; set; }

    public int FkMasterId { get; set; }
}