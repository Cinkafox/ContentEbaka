using Robust.LoaderApi;

namespace Content.Datum.Data.FileApis.Interfaces;

public interface IReadWriteFileApi: IFileApi, IWriteFileApi
{
    public bool Has(string path);
}