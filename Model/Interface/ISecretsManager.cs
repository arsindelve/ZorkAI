namespace Model.Interface;

public interface ISecretsManager
{
    Task<string> GetSecret(string secretName);
}