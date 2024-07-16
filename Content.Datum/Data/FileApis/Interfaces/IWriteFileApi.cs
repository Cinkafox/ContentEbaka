namespace Content.Datum.Data.FileApis.Interfaces;

public interface IWriteFileApi
{
    public bool Save(string path, Stream input);
}