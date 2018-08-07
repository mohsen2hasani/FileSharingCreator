using System;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.AccessControl;

namespace FileSharingCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //You need set domain and admin user and admin password
                using (var context = new PrincipalContext(ContextType.Domain, "Domain Name", "Administrator UserName", "Administrator Password"))
                {
                    using (var searcher = new PrincipalSearcher(new GroupPrincipal(context)))
                    {
                        string folderName = string.Empty;
                        DirectoryInfo info;
                        DirectorySecurity security;
                        //I need to get all the groups that have "_Unit" in their names
                        foreach (GroupPrincipal groupPrincipal in searcher.FindAll().Where(a => a.Name.Contains("_Unit")))
                        {
                            var groupName = groupPrincipal.Name;

                            //Create folder for each groups and set permissions
                            folderName = $@"E:\Share\{groupName}";

                            if (!Directory.Exists(folderName))
                                Directory.CreateDirectory(folderName);

                            info = new DirectoryInfo(folderName);
                            security = info.GetAccessControl();
                            security.AddAccessRule(new FileSystemAccessRule(groupPrincipal.SamAccountName, FileSystemRights.Read, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                            security.AddAccessRule(new FileSystemAccessRule(groupPrincipal.SamAccountName, FileSystemRights.Read, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                            info.SetAccessControl(security);

                            var members = groupPrincipal.Members.Where(a => a is UserPrincipal).ToList();

                            //Create Folder for each memebers of this group and set permissions
                            foreach (UserPrincipal member in members)
                            {
                                folderName = $@"E:\Share\{groupName}\{member.DisplayName}";
                                if (!Directory.Exists(folderName))
                                    Directory.CreateDirectory(folderName);

                                info = new DirectoryInfo(folderName);
                                security = info.GetAccessControl();
                                security.AddAccessRule(new FileSystemAccessRule(member.SamAccountName, FileSystemRights.Modify, InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
                                security.AddAccessRule(new FileSystemAccessRule(member.SamAccountName, FileSystemRights.Modify, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                                info.SetAccessControl(security);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadKey();
            }
        }
    }
}