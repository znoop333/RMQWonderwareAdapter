using System;
using System.Collections.Generic;
using System.Text;

namespace Win32Helper
{
    class AsyncHelpers
    {
        public static Task<bool> RunProcessAsync(string fileName, string arguments)
        {
            // there is no non-generic TaskCompletionSource
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = arguments },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }

        public static async Task<Exception> CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken)
        {
            try
            {
                var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
                var bufferSize = 4096;

                using (var sourceStream =
                      new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))

                using (var destinationStream =
                      new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, fileOptions))

                    await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken)
                                               .ConfigureAwait(continueOnCapturedContext: false);

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

    }
}
