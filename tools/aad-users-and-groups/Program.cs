using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// ReSharper disable PropertyCanBeMadeInitOnly.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace aad_users_and_groups
{
    internal class Program
    {
        public static async Task<int> Main(bool azclilogin, bool export, bool list, bool sync,
            string datadir, string password = "Password01")
        {
            Log(0, "START");

            AzureCredentials creds;

            if (azclilogin)
            {
                Log(0, "Login from Azure CLI");
                creds = AzureCliCredentials.Create();
            }
            else
            {
                Log(0, "Login from Environment");
                //$env:servicePrincipalId, $env:servicePrincipalKey and $env:tenantId 
                var client = Environment.GetEnvironmentVariable("servicePrincipalId");
                var key = Environment.GetEnvironmentVariable("servicePrincipalKey");
                var tenant = Environment.GetEnvironmentVariable("tenantId");

                creds = new AzureCredentialsFactory().FromServicePrincipal(client,
                    key,
                    tenant,
                    AzureEnvironment.AzureGlobalCloud);
            }

            var auth = Azure.Configure()
                .Authenticate(creds);

            if (export)
            {
                await ExportAsync(auth, datadir);
            }
            else if (list)
            {
                await ListAsync(auth, datadir);
            }
            else if (sync)
            {
                var myUsers = LoadUsersFromDisk(datadir);
                var myGroups = LoadGroupsFromDisk(datadir);

                await SyncUsersAsync(auth, myUsers, password);
                await SyncGroupsAsync(auth, myGroups);

                await SyncGroupMembers(auth, myGroups);
            }
            else
            {
                return 1;
            }

            Log(0, "FERTIG");
            return 0;
        }

        private static async Task ExportAsync(Azure.IAuthenticated authenticated, string dataDir)
        {
            if (string.IsNullOrEmpty(dataDir))
            {
                dataDir = Directory.GetCurrentDirectory();
            }

            var aadUsers = await LoadActiveDirectoryUsersAsync(authenticated);

            Log(0, "===========================================");
            Log(0, "ExportAsync");
            {
                Log(1, $"AAD Users: {aadUsers.Length}");
                var list = aadUsers.Select(MyUser.Create).ToArray();
                var json = JsonSerializer.Serialize(list,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                await File.WriteAllTextAsync(Path.Combine(dataDir, "export-users.json"), json, Encoding.UTF8);
            }
            Log(0, "===========================================");
            {
                var aadGroups = await LoadActiveDirectoryGroupsAsync(authenticated);
                var list = await Task.WhenAll(aadGroups.Select(async aadGroup =>
                {
                    var aadMembers = await LoadPagedCollectionAsync(aadGroup.ListMembersAsync());
                    var g = MyGroup.Create(aadGroup);


                    g.Members = aadMembers.Select(mem =>
                        {
                            var u = aadUsers.FirstOrDefault(x => x.Id == mem.Id);
                            if (u != null)
                                return u.UserPrincipalName;
                            return string.Empty;
                        })
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToArray();

                    return g;
                }));


                Log(1, $"AAD Groups: {aadGroups.Length}");
                //var list = aadGroups.Select(MyGroup.Create).ToArray();

                var json = JsonSerializer.Serialize(list,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                await File.WriteAllTextAsync(Path.Combine(dataDir, "export-groups.json"), json, Encoding.UTF8);
            }
            Log(0, "===========================================");
        }

        private static async Task ListAsync(Azure.IAuthenticated auth, string datadir)
        {
            Log(0, "===========================================");
            Log(0, "ListAsync");
            Log(0, "===========================================");
            var myUsers = LoadUsersFromDisk(datadir);
            Log(1, $"Users FromDisk: {myUsers.Length}");
            foreach (var myUser in myUsers) Log(2, myUser);

            Log(0, "===========================================");
            var myGroups = LoadGroupsFromDisk(datadir);
            Log(1, $"Groups FromDisk: {myGroups.Length}");
            foreach (var myGroup in myGroups) Log(2, myGroup);

            Log(0, "===========================================");
            var aadUsers = await LoadActiveDirectoryUsersAsync(auth);
            Log(1, $"AAD Users: {aadUsers.Length}");
            foreach (var myUser in aadUsers) Log(2, myUser);

            Log(0, "===========================================");
            var aadGroups = await LoadActiveDirectoryGroupsAsync(auth);
            Log(1, $"AAD Groups: {aadGroups.Length}");
            foreach (var myUser in aadGroups) Log(2, myUser);
        }

        private static Task<IActiveDirectoryGroup[]> LoadActiveDirectoryGroupsAsync(
            Azure.IAuthenticated authenticated)
        {
            return LoadPagedCollectionAsync(authenticated.ActiveDirectoryGroups.ListAsync());
        }

        private static Task<IActiveDirectoryUser[]> LoadActiveDirectoryUsersAsync(Azure.IAuthenticated authenticated)
        {
            return LoadPagedCollectionAsync(authenticated.ActiveDirectoryUsers.ListAsync());
        }

        private static T[] LoadFromDisk<T>(string searchPattern, string dataDir)
        {
            if (string.IsNullOrEmpty(dataDir))
            {
                dataDir = Directory.GetCurrentDirectory();
            }

            return Directory
                .GetFiles(dataDir, searchPattern)
                .SelectMany(file =>
                {
                    Log(1, $"{file}");
                    var content = File.ReadAllText(file);
                    return JsonSerializer.Deserialize<T[]>(content);
                })
                .ToArray();
        }

        private static MyGroup[] LoadGroupsFromDisk(string dataDir)
        {
            return LoadFromDisk<MyGroup>("gruppen*.json", dataDir);
        }

        private static async Task<T[]> LoadPagedCollectionAsync<T>(Task<IPagedCollection<T>> listAsync)
        {
            var list = new List<T>();
            var page = await listAsync;

            while (page != null)
            {
                list.AddRange(page);
                page = await page.GetNextPageAsync();
            }

            return list.ToArray();
        }

        private static MyUser[] LoadUsersFromDisk(string dataDir)
        {
            return LoadFromDisk<MyUser>("benutzer*.json", dataDir);
        }

        private static void Log(int level, string text)
        {
            var space = new string(' ', level * 3);
            Console.WriteLine($"{space}{text}");
        }


        private static void Log(int level, MyUser item)
        {
            Log(level, $"{item.Displayname}");
            Log(level + 1, $"{item.Givenname}");
            Log(level + 1, $"{item.Surname}");
        }

        private static void Log(int level, MyGroup item)
        {
            Log(level, $"{item.Name}");
            Log(level + 1, $"{item.MailNickname}");
        }

        private static void Log(int level, IActiveDirectoryUser cur)
        {
            Log(level, $"{cur.Name}");
            Log(level + 1, $"{cur.Inner.GivenName}");
            Log(level + 1, $"{cur.Inner.Surname}");
        }

        private static void Log(int level, IActiveDirectoryGroup item)
        {
            Log(level, $"{item.Name}");
            Log(level + 1, $"{item.Inner.MailNickname}");
        }

        private static async Task<IActiveDirectoryGroup> ProcessGroupAsync(MyGroup item, IActiveDirectoryGroup[] list,
            Azure.IAuthenticated authenticated)
        {
            Log(0, "===========================================");
            Log(0, "Processing Group");
            Log(1, item);

            var cur = list.FirstOrDefault(x => x.Inner.MailNickname == item.MailNickname);
            if (cur == null && !item.Delete)
            {
                Log(2, "Creating Group");

                cur = await authenticated.ActiveDirectoryGroups
                    .Define(item.Name)
                    .WithEmailAlias(item.MailNickname)
                    .CreateAsync();
            }

            if (cur != null)
            {
                Log(2, cur);
                if (item.Delete)
                {
                    Log(2, "Deleting Group");

                    await authenticated.ActiveDirectoryGroups.DeleteByIdAsync(cur.Id);
                    return null;
                }

                // // update
                // if (!string.Equals(cur.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                // {
                //     Log(2, "Recreating Group");
                //     await authenticated.ActiveDirectoryGroups.DeleteByIdAsync(cur.Id);
                //
                //     
                //     cur = await authenticated.ActiveDirectoryGroups
                //         .Define(item.Name)
                //         .WithEmailAlias(item.MailNickname)
                //         .CreateAsync();
                // }

                // cur.Manager.Inner.Groups.GetWithHttpMessagesAsync()
            }

            return cur;
        }

        private static async Task<IActiveDirectoryUser> ProcessUserAsync(MyUser item, IActiveDirectoryUser[] list,
            Azure.IAuthenticated authenticated, string password)
        {
            Log(0, "===========================================");
            Log(0, "Processing User");
            Log(1, item);

            var cur = list.FirstOrDefault(x => x.UserPrincipalName == item.Upn);

            if (cur == null && !item.Delete)
            {
                Log(3, "Creating User");


                cur = await authenticated.ActiveDirectoryUsers
                    .Define(item.Displayname)
                    .WithUserPrincipalName(item.Upn)
                    .WithPassword(password)
                    .CreateAsync();
            }

            if (cur != null)
            {
                if (item.Delete)
                {
                    Log(3, "Deleting User");
                    await authenticated.ActiveDirectoryUsers.DeleteByIdAsync(cur.Id);
                    return null;
                }

                Log(3, "Updating User");
                Log(3, cur);

                await authenticated.ActiveDirectoryUsers.Inner.UpdateWithHttpMessagesAsync(cur.UserPrincipalName,
                    new UserUpdateParameters
                    {
                        Surname = item.Surname,
                        GivenName = item.Givenname,
                        DisplayName = item.Displayname
                    });
            }

            return cur;
        }

        private static async Task SyncGroupMembers(Azure.IAuthenticated authenticated, MyGroup[] myGroups)
        {
            Log(0, "===========================================");
            Log(0, "SyncGroupMembers");

            var aadGroups = await LoadActiveDirectoryGroupsAsync(authenticated);
            var aadUsers = await LoadActiveDirectoryUsersAsync(authenticated);

            foreach (var myGroup in myGroups)
            {
                var group = aadGroups.FirstOrDefault(x => x.Inner.MailNickname == myGroup.MailNickname);
                if (group != null && myGroup.Members != null)
                {
                    Log(1, "Processing Group Membership");
                    Log(2, $"{group.Name}");

                    var aadMembers = await LoadPagedCollectionAsync(group.ListMembersAsync());

                    Log(3, "Checking Members to Remove");
                    foreach (var mem in aadMembers)
                    {
                        Log(4, $"Name: {mem.Name}");

                        var u = aadUsers.FirstOrDefault(x => x.Id == mem.Id);
                        if (u != null)
                        {
                            Log(5, $"UserPrincipalName: {u.UserPrincipalName}");

                            if (!myGroup.Members.Contains(u.UserPrincipalName))
                            {
                                Log(5, "Removing Member from Group");
                                await group.Update()
                                    .WithoutMember(mem.Id)
                                    .ApplyAsync();
                            }
                        }
                        else
                        {
                            Log(5, "Removing Member from Group");
                            await group.Update()
                                .WithoutMember(mem.Id)
                                .ApplyAsync();
                        }
                    }

                    Log(3, "Checking Members to Add");
                    foreach (var upn in myGroup.Members)
                    {
                        Log(4, $"{upn}");
                        var u = aadUsers.FirstOrDefault(x => x.UserPrincipalName == upn);
                        if (u != null)
                        {
                            if (aadMembers.All(x => x.Id != u.Id))
                            {
                                Log(5, "Adding Member to Group");
                                await group.Update()
                                    .WithMember(u)
                                    .ApplyAsync();
                            }
                        }
                    }
                }
            }
        }

        private static async Task SyncGroupsAsync(Azure.IAuthenticated authenticated,
            MyGroup[] myGroups)
        {
            Log(0, "===========================================");
            Log(0, "SyncGroupsAsync");
            var aadGroups = await LoadActiveDirectoryGroupsAsync(authenticated);
            Log(1, $"AAD Groups: {aadGroups.Length}");
            Log(1, $"My Groups: {myGroups.Length}");
            Log(0, "===========================================");
            await Task.WhenAll(myGroups.Select(x => ProcessGroupAsync(x, aadGroups, authenticated)));
        }

        private static async Task SyncUsersAsync(Azure.IAuthenticated authenticated,
            MyUser[] myUsers, string password)
        {
            Log(0, "===========================================");
            Log(0, "SyncUsersAsync");
            var aadUsers = await LoadActiveDirectoryUsersAsync(authenticated);
            Log(1, $"AAD Users: {aadUsers.Length}");
            Log(1, $"My Users: {myUsers.Length}");
            Log(0, "===========================================");
            await Task.WhenAll(myUsers.Select(x => ProcessUserAsync(x, aadUsers, authenticated, password)));
        }

        private class MyGroup
        {
            [JsonPropertyName("delete")]
            public bool Delete { get; set; }

            [JsonPropertyName("nick")]
            public string MailNickname { get; set; }

            [JsonPropertyName("members")]
            public string[] Members { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            public static MyGroup Create(IActiveDirectoryGroup aad)
            {
                return new()
                {
                    MailNickname = aad.Inner.MailNickname,
                    Name = aad.Name
                };
            }
        }

        private class MyUser
        {
            [JsonPropertyName("delete")]
            public bool Delete { get; set; }

            [JsonPropertyName("anzeige")]
            public string Displayname { get; set; }

            [JsonPropertyName("vorname")]
            public string Givenname { get; set; }

            [JsonPropertyName("nachname")]
            public string Surname { get; set; }

            [JsonPropertyName("upn")]
            public string Upn { get; set; }

            public static MyUser Create(IActiveDirectoryUser aad)
            {
                return new()
                {
                    Displayname = aad.Name,
                    Givenname = aad.Inner.GivenName,
                    Surname = aad.Inner.Surname,
                    Upn = aad.UserPrincipalName
                };
            }
        }
    }
}