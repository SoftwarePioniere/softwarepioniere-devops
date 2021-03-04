using System;
using System.Threading.Tasks;

namespace SoftwarePioniere.DevOps
{
    static class AadCommandHandler
    {
        public static Task<int> ParseCommandHandler(string dataDir, string userFilePattern, string groupFilePattern)
        {
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");

            return Task.FromResult(0);
        }

        public static Task<int> ExportCommandHandler(bool loginAzCli, string dataDir, string userFilePattern, string groupFilePattern)
        {
            Console.WriteLine($"{nameof(loginAzCli)}: {loginAzCli}");
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");
            
            return Task.FromResult(0);
        }

        public static Task<int> DeployCommandHandler(bool loginAzCli, string dataDir, string defaultPassword, string userFilePattern, string groupFilePattern)
        {
            Console.WriteLine($"{nameof(loginAzCli)}: {loginAzCli}");
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(defaultPassword)}: {defaultPassword}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");
            
            return Task.FromResult(0);
        }
    }
}