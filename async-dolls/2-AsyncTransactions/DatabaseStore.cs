using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDolls
{
    public class DatabaseStore
    {
        private readonly FileStream fileStore;

        public DatabaseStore(string storePath)
        {
            fileStore = File.OpenWrite(storePath);
        }

        public async Task AppendAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            await stream.CopyToAsync(fileStore, 4096, cancellationToken);
            await fileStore.FlushAsync(cancellationToken);
        }

        public void Close()
        {
            fileStore.Close();
        }
    }
}