using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightBulbApp
{
    public static class ShellHelper
    {
        public static async Task<int> Bash(this string cmd, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return -1;

            var source = new TaskCompletionSource<int>();
            var escapedArgs = cmd.Replace("\"", "\\\"");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.Exited += (sender, args) =>
            {
                logger.LogWarning(process.StandardError.ReadToEnd());
                logger.LogInformation(process.StandardOutput.ReadToEnd());
                if (process.ExitCode == 0)
                {
                    source.SetResult(0);
                }
                else
                {
                    source.SetException(new ShellException($"Command `{cmd}` failed with exit code `{process.ExitCode}`"));
                }

                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Command {} failed", cmd);
                source.SetException(e);
            }

            return await source.Task;
        }
    }
}
