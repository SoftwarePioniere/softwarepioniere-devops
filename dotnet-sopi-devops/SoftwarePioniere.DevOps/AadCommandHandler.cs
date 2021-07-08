#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable PropertyCanBeMadeInitOnly.Local
// ReSharper disable MemberCanBePrivate.Local

namespace SoftwarePioniere.DevOps
{
    internal static class AadCommandHandler
    {
        public static async Task<int> HandleDeployCommand(bool loginAzCli, string dataDir, string defaultPassword,
            string userFilePattern, string groupFilePattern, bool dryRun)
        {
            Console.WriteLine($"{nameof(loginAzCli)}: {loginAzCli}");
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(defaultPassword)}: {defaultPassword}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");
            Console.WriteLine($"{nameof(dryRun)}: {dryRun}");

            var auth = Login(loginAzCli);

            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            Log(0, "Deploy");
            Log(0, "===========================================");

            
            
            var myUsers = LoadUsersFromDisk(dataDir, userFilePattern);
            var myGroups = LoadGroupsFromDisk(dataDir, groupFilePattern);

            await SyncUsersAsync(auth, myUsers, dataDir);
          
            await SyncGroupsAsync(auth, myGroups);

            await SyncGroupMembers(auth, myGroups);
            
            
            Log(0, "===========================================");
            {
                var json = JsonSerializer.Serialize(myUsers,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                var file = $"deploy-{userFilePattern.Replace(".json", "").Replace("*", "-all")}.json";
                Log(1, $"Writing Users JSON to {file}");
                await File.WriteAllTextAsync(Path.Combine(dataDir, file), json, Encoding.UTF8);
            }

            return 0;
        }

        public static async Task<int> HandleExportCommand(bool loginAzCli, string dataDir, string userFilePattern,
            string groupFilePattern)
        {
            Console.WriteLine($"{nameof(loginAzCli)}: {loginAzCli}");
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");

            var authenticated = Login(loginAzCli);

            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            Log(0, "Export");
            Log(0, "===========================================");
            var aadUsers = await LoadActiveDirectoryUsersAsync(authenticated);
            Log(1, $"AAD Users: {aadUsers.Length}");
            var aadGroups = await LoadActiveDirectoryGroupsAsync(authenticated);
            Log(1, $"AAD Groups: {aadGroups.Length}");

            var myUsers = aadUsers.Select(MyUser.Create).OrderBy(x => x.Displayname).ToArray();

            Log(0, "===========================================");
            {
                var json = JsonSerializer.Serialize(myUsers,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                var file = $"export-{userFilePattern.Replace(".json", "").Replace("*", "-all")}.json";
                Log(1, $"Writing Users JSON to {file}");
                await File.WriteAllTextAsync(Path.Combine(dataDir, file), json, Encoding.UTF8);
            }

            var myGroups = await Task.WhenAll(aadGroups.Select(async aadGroup =>
            {
                var aadMembers = await LoadPagedCollectionAsync(aadGroup.ListMembersAsync());
                var g = MyGroup.Create(aadGroup);

                g.Members = aadMembers
                    .Where(x => x != null)
                    .OfType<IActiveDirectoryUser>()
                    .Select(mem =>
                    {
                        var u = aadUsers.FirstOrDefault(x => x.Id == mem.Id);
                        if (u != null)
                        {
                            return u.UserPrincipalName;
                        }

                        return string.Empty;
                    })
                    .Where(x => !string.IsNullOrEmpty(x))
                    .OrderBy(x => x)
                    .ToArray();

                g.Groups = aadMembers.Where(x => x != null)
                    .OfType<IActiveDirectoryGroup>()
                    .Select(mem =>
                    {
                        var u = aadGroups.FirstOrDefault(x => x.Id == mem.Id);
                        if (u != null)
                        {
                            return u.Inner.MailNickname;
                        }

                        return string.Empty;
                    })
                    .Where(x => !string.IsNullOrEmpty(x))
                    .OrderBy(x => x)
                    .ToArray();

                return g;
            }));

            Log(0, "===========================================");
            {
                var json = JsonSerializer.Serialize(myGroups,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                var file = $"export-{groupFilePattern.Replace(".json", "").Replace("*", "-all")}.json";
                Log(1, $"Writing Groups JSON to {file}");
                await File.WriteAllTextAsync(Path.Combine(dataDir, file), json, Encoding.UTF8);
            }

            Log(0, "===========================================");

            foreach (var myGroup in myGroups)
            {
                var users = myUsers.Where(x => myGroup.Members != null && myGroup.Members.Contains(x.Upn))
                    .OrderBy(x => x.Displayname)
                    .ToArray();
                if (users.Length > 0 && !string.IsNullOrEmpty(myGroup.Name))
                {
                    var gname = myGroup.Name.Replace(" ", "-");
                    var xname = userFilePattern.Replace(".json", "").Replace("*", $"-{gname}");
                    var file = $"export-{xname}.json";
                    var json = JsonSerializer.Serialize(users,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                    Log(1, $"Writing Users JSON to {file}");
                    await File.WriteAllTextAsync(Path.Combine(dataDir, file), json, Encoding.UTF8);
                }
            }

            Log(0, "===========================================");

            return 0;
        }

        public static async Task<int> HandleShowCommand(bool loginAzCli, string dataDir, string userFilePattern,
            string groupFilePattern)
        {
            Console.WriteLine($"{nameof(loginAzCli)}: {loginAzCli}");
            Console.WriteLine($"{nameof(dataDir)}: {dataDir}");
            Console.WriteLine($"{nameof(userFilePattern)}: {userFilePattern}");
            Console.WriteLine($"{nameof(groupFilePattern)}: {groupFilePattern}");

            if (!Directory.Exists(dataDir))
            {
                Log(0, $"Directory not found: {dataDir}");
                return 1;
            }

            var authenticated = Login(loginAzCli);

            Log(0, "===========================================");
            Log(0, "Show");
            Log(0, "===========================================");
            var myUsers = LoadUsersFromDisk(dataDir, userFilePattern);
            Log(1, $"Users FromDisk: {myUsers.Length}");
            foreach (var myUser in myUsers) Log(2, myUser);

            Log(0, "===========================================");
            var myGroups = LoadGroupsFromDisk(dataDir, groupFilePattern);
            Log(1, $"Groups FromDisk: {myGroups.Length}");
            foreach (var myGroup in myGroups) Log(2, myGroup);

            Log(0, "===========================================");
            var aadUsers = await LoadActiveDirectoryUsersAsync(authenticated);
            Log(1, $"AAD Users: {aadUsers.Length}");
            foreach (var myUser in aadUsers) Log(2, myUser);

            Log(0, "===========================================");
            var aadGroups = await LoadActiveDirectoryGroupsAsync(authenticated);
            Log(1, $"AAD Groups: {aadGroups.Length}");
            foreach (var myUser in aadGroups) Log(2, myUser);

            await Task.CompletedTask;
            return 0;
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
                    return JsonSerializer.Deserialize<T[]>(content) ?? throw new InvalidOperationException();
                })
                .ToArray();
        }

        private static MyGroup[] LoadGroupsFromDisk(string dataDir, string searchPattern)
        {
            return LoadFromDisk<MyGroup>(searchPattern, dataDir);
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

        private static MyUser[] LoadUsersFromDisk(string dataDir, string pattern)
        {
            return LoadFromDisk<MyUser>(pattern, dataDir);
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

        private static Azure.IAuthenticated Login(bool loginAzCli)
        {
            AzureCredentials creds;

            if (loginAzCli)
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

            return auth;
        }

        private static async Task<IActiveDirectoryGroup?> ProcessGroupAsync(MyGroup item, IActiveDirectoryGroup[] list,
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

        private static async Task<IActiveDirectoryUser?> ProcessUserAsync(MyUser item, IActiveDirectoryUser[] list,
            // ReSharper disable once UnusedParameter.Local
            Azure.IAuthenticated authenticated, string password)
        {
            async Task ApplyPassword(IActiveDirectoryUser activeDirectoryUser)
            {
                if (item.UseDefaultPassword)
                {
                    Log(3, $"Updating UseDefaultPassword: {activeDirectoryUser.UserPrincipalName}");

                    var respo = await authenticated.ActiveDirectoryUsers.Inner.UpdateWithHttpMessagesAsync(
                        activeDirectoryUser.UserPrincipalName,
                        new UserUpdateParameters()
                        //{
                        // PasswordProfile = new PasswordProfile(password)
                        // PasswordProfile = new PasswordProfile(SdkContext.RandomResourceName("Pa5$", 15))
                        // {
                        //     ForceChangePasswordNextLogin = false
                        // }
                        //        }
                    );

                    var cont = await respo.Response.Content.ReadAsStringAsync();
                    Log(3,
                        $"Updating UseDefaultPassword Responce: {activeDirectoryUser.UserPrincipalName}: {respo.Response.StatusCode} {cont}");
                }
            }

            Log(0, "===========================================");
            Log(0, "Processing User");
            Log(1, item);

            var cur = list.FirstOrDefault(x => x.UserPrincipalName == item.Upn);

            if (cur == null && !item.Delete)
            {
                Log(3, $"Creating User: {item.Upn}");

                // var userCreateParameters = new UserCreateParameters()
                // {
                //     MailNickname = item.Upn,
                //     Surname = item.Surname,
                //     GivenName = item.Givenname,
                //     DisplayName = item.Displayname,
                //     UserPrincipalName = item.Upn,
                //     UserType = UserType.Member,
                //     PasswordProfile = new PasswordProfile(SdkContext.RandomResourceName("Pa5$", 15))
                //     // {
                //     //     Password = SdkContext.RandomResourceName("Pa5$",
                //     //         15), // Guid.NewGuid().ToString(), // password,
                //     //     ForceChangePasswordNextLogin = false
                //     // }
                // };
                // userCreateParameters.Validate();
                // //
                // var inner = await authenticated.ActiveDirectoryUsers.Inner.CreateWithHttpMessagesAsync(
                //     userCreateParameters
                // );
                // //
                // Policy.Handle<Exception>()
                //     .RetryAsync(5,
                //         async (exception, i) =>
                //         {
                //             cur = await authenticated.ActiveDirectoryUsers.GetByIdAsync(inner.Body.ObjectId);
                //
                //             if (cur == null)
                //                 throw new Exception();
                //         });


                cur = await authenticated.ActiveDirectoryUsers
                    .Define(item.Displayname)
                    .WithUserPrincipalName(item.Upn)
                    //  .WithPassword(password)
                    .WithPassword(SdkContext.RandomResourceName("Pa5$", 15))
                    .WithPromptToChangePasswordOnLogin(false)
                    .CreateAsync();

                await ApplyPassword(cur);
            }

            if (cur != null)
            {
                if (item.Delete)
                {
                    Log(3, $"Deleting User: {cur.UserPrincipalName}");
                    await authenticated.ActiveDirectoryUsers.DeleteByIdAsync(cur.Id);
                    return null;
                }

                Log(3, $"Updating User: {cur.UserPrincipalName}");
                Log(3, cur);

                var parameters = new UserUpdateParameters
                {
                    Surname = item.Surname,
                    GivenName = item.Givenname,
                    DisplayName = item.Displayname
                };

              //   if (item.UseDefaultPassword)
              //   {
              //       parameters.AdditionalProperties = new Dictionary<string, object>();
              //       
              // //      parameters.AdditionalProperties.Add("UseDefaultPassword", item.UseDefaultPassword.ToString());
              //   }

                await authenticated.ActiveDirectoryUsers.Inner.UpdateWithHttpMessagesAsync(cur.UserPrincipalName,
                    parameters);
                //

                await ApplyPassword(cur);
            }

            if (cur != null)
                Log(0, $"Finished Processing: {cur.UserPrincipalName}");
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
                    Log(1, "Processing User Membership");
                    Log(2, $"{group.Name}");
                    var aadMembers = await LoadPagedCollectionAsync(group.ListMembersAsync());

                    Log(3, "Checking Members to Remove");
                    foreach (var mem in aadMembers.OfType<IActiveDirectoryUser>())
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
                            if (aadMembers.OfType<IActiveDirectoryUser>().All(x => x.Id != u.Id))
                            {
                                Log(5, "Adding Member to Group");
                                await group.Update()
                                    .WithMember(u)
                                    .ApplyAsync();
                            }
                        }
                    }
                }

                if (group != null && myGroup.Groups != null)
                {
                    Log(1, "Processing Groups Membership");
                    Log(2, $"{group.Name}");
                    var aadMembers = await LoadPagedCollectionAsync(group.ListMembersAsync());

                    Log(3, "Checking Groups to Remove");
                    foreach (var mem in aadMembers.OfType<IActiveDirectoryGroup>())
                    {
                        Log(4, $"Name: {mem.Name}");

                        var u = aadGroups.FirstOrDefault(x => x.Id == mem.Id);
                        if (u != null)
                        {
                            Log(5, $"MailNickname: {u.Inner.MailNickname}");

                            if (!myGroup.Groups.Contains(u.Inner.MailNickname))
                            {
                                Log(5, "Removing Group from Group");
                                await group.Update()
                                    .WithoutMember(mem.Id)
                                    .ApplyAsync();
                            }
                        }
                        else
                        {
                            Log(5, "Removing Group from Group");
                            await group.Update()
                                .WithoutMember(mem.Id)
                                .ApplyAsync();
                        }
                    }

                    Log(3, "Checking Groups to Add");
                    foreach (var nick in myGroup.Groups)
                    {
                        Log(4, $"{nick}");
                        var u = aadGroups.FirstOrDefault(x => x.Inner.MailNickname == nick);
                        if (u != null)
                        {
                            if (aadMembers.OfType<IActiveDirectoryGroup>().All(x => x.Id != u.Id))
                            {
                                Log(5, "Adding Group to Group");
                                await @group.Update()
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
            public string? MailNickname { get; set; }

            [JsonPropertyName("members")]
            public string[]? Members { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("groups")]
            public string[]? Groups { get; set; }

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
            public string? Displayname { get; set; }

            [JsonPropertyName("vorname")]
            public string? Givenname { get; set; }

            [JsonPropertyName("nachname")]
            public string? Surname { get; set; }

            [JsonPropertyName("upn")]
            public string? Upn { get; set; }

            [JsonPropertyName("default_passsword")]
            public bool UseDefaultPassword { get; set; }

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