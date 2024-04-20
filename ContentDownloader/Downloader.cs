using System.Buffers.Binary;
using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;

namespace ContentDownloader;

public class Downloader
{
    public RobustBuildInfo Info { get; private set; }
    private HttpClient _http = new();
    private const int ManifestDownloadProtocolVersion = 1;
    private string _path = "./datum/";

    public ManifestReader ManifestReader => new ManifestReader(Info.RobustManifestInfo);

    public Downloader(RobustBuildInfo info)
    {
        Info = info;
        if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
    }
    
    private static void EnsureBuffer(ref byte[] buf, int needsFit)
    {
        if (buf.Length >= needsFit)
            return;

        var newLen = 2 << BitOperations.Log2((uint)needsFit - 1);

        buf = new byte[newLen];
    }

    public bool CheckManifestExist(RobustManifestItem item)
    {
        return File.Exists(_path + item.Hash);
    }

    public async Task<List<RobustManifestItem>> EnsureItems()
    {
        using var manifestReader = ManifestReader;

        List<RobustManifestItem> allItems = [];
        List<RobustManifestItem> items = [];

        while (manifestReader.TryReadItem(out var item))
        {
            if (!CheckManifestExist(item.Value))
                items.Add(item.Value);
            allItems.Add(item.Value);
        }
        
        Console.WriteLine("Download Count:{0}",items.Count);

        await Download(items);
        
        return allItems;
    }

    public async Task Unpack(string path)
    {
        var items = await EnsureItems();
        foreach (var item in items)
        {
            FileInfo fileInfo = new FileInfo(path + item.Path);
            if (!fileInfo.Directory.Exists) fileInfo.Directory.Create();
            
            File.Copy(_path + item.Hash, path + item.Path,true);
        }
    }

    
    public async Task Download(List<RobustManifestItem> toDownload)
    {
        if(toDownload.Count == 0) return;
        
        var requestBody = new byte[toDownload.Count * 4];
        var reqI = 0;
        foreach (var item in toDownload)
        {
            BinaryPrimitives.WriteInt32LittleEndian(requestBody.AsSpan(reqI, 4), item.Id);
            reqI += 4;
        }
        
        var request = new HttpRequestMessage(HttpMethod.Post, Info.RobustManifestInfo.DownloadUri);
        request.Headers.Add(
            "X-Robust-Download-Protocol",
            ManifestDownloadProtocolVersion.ToString(CultureInfo.InvariantCulture));

        request.Content = new ByteArrayContent(requestBody);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        
        var stream = await response.Content.ReadAsStreamAsync();
        var bandwidthStream = new BandwidthStream(stream);
        stream = bandwidthStream;
        if (response.Content.Headers.ContentEncoding.Contains("zstd"))
            stream = new ZStdDecompressStream(stream);

        await using var streamDispose = stream;

        // Read flags header
        var streamHeader = await stream.ReadExactAsync(4,null);
        var streamFlags = (DownloadStreamHeaderFlags)BinaryPrimitives.ReadInt32LittleEndian(streamHeader);
        var preCompressed = (streamFlags & DownloadStreamHeaderFlags.PreCompressed) != 0;

        // compressContext.SetParameter(ZSTD_cParameter.ZSTD_c_nbWorkers, 4);
        // If the stream is pre-compressed we need to decompress the blobs to verify BLAKE2B hash.
        // If it isn't, we need to manually try re-compressing individual files to store them.
        var compressContext = preCompressed ? null : new ZStdCCtx();
        var decompressContext = preCompressed ? new ZStdDCtx() : null;

        // Normal file header:
        // <int32> uncompressed length
        // When preCompressed is set, we add:
        // <int32> compressed length
        var fileHeader = new byte[preCompressed ? 8 : 4];


        try
        {
            // Buffer for storing compressed ZStd data.
            var compressBuffer = new byte[1024];

            // Buffer for storing uncompressed data.
            var readBuffer = new byte[1024];

            var i = 0;
            foreach (var item in toDownload)
            {

                // Read file header.
                await stream.ReadExactAsync(fileHeader, null);

                var length = BinaryPrimitives.ReadInt32LittleEndian(fileHeader.AsSpan(0, 4));

                EnsureBuffer(ref readBuffer, length);
                var data = readBuffer.AsMemory(0, length);

                // Data to write to database.
                var compression = ContentCompressionScheme.None;
                var writeData = data;

                if (preCompressed)
                {
                    // Compressed length from extended header.
                    var compressedLength = BinaryPrimitives.ReadInt32LittleEndian(fileHeader.AsSpan(4, 4));

                    if (compressedLength > 0)
                    {
                        EnsureBuffer(ref compressBuffer, compressedLength);
                        var compressedData = compressBuffer.AsMemory(0, compressedLength);
                        await stream.ReadExactAsync(compressedData, null);

                        // Decompress so that we can verify hash down below.

                        var decompressedLength = decompressContext!.Decompress(data.Span, compressedData.Span);

                        if (decompressedLength != data.Length)
                            throw new Exception($"Compressed blob {i} had incorrect decompressed size!");

                        // Set variables so that the database write down below uses them.
                        compression = ContentCompressionScheme.ZStd;
                        writeData = compressedData;
                    }
                    else
                    {
                        await stream.ReadExactAsync(data, null);
                    }
                }
                else
                {
                    await stream.ReadExactAsync(data, null);
                }

                if (!preCompressed)
                {
                    // File wasn't pre-compressed. We should try to manually compress it to save space in DB.


                    EnsureBuffer(ref compressBuffer, ZStd.CompressBound(data.Length));
                    var compressLength = compressContext!.Compress(compressBuffer, data.Span);

                    // Don't bother saving compressed data if it didn't save enough space.
                    if (compressLength + 10 < length)
                    {
                        // Set variables so that the database write down below uses them.
                        compression = ContentCompressionScheme.ZStd;
                        writeData = compressBuffer.AsMemory(0, compressLength);
                    }
                }
                
                await File.WriteAllBytesAsync(_path + item.Hash,data.ToArray());
                
                Console.WriteLine("path: {0}, hash: {1}", item.Path, item.Hash);
                i += 1;
            }
        }
        finally
        {
            decompressContext?.Dispose();
            compressContext?.Dispose();
        }
    }
}
