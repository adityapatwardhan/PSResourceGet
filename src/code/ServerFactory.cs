// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Management.Automation;
using System.Net;
using Microsoft.PowerShell.PSResourceGet.UtilClasses;

namespace Microsoft.PowerShell.PSResourceGet.Cmdlets
{
    internal static class UserAgentInfo
    {
        static UserAgentInfo()
        {
            using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                _psVersion = ps.AddScript("$PSVersionTable").Invoke<Hashtable>()[0]["PSVersion"].ToString();
            }

            _psResourceGetVersion = typeof(UserAgentInfo).Assembly.GetName().Version.ToString();
            _distributionChannel = System.Environment.GetEnvironmentVariable("POWERSHELL_DISTRIBUTION_CHANNEL") ?? "unknown";
        }

        private static string _psVersion;
        private static string _psResourceGetVersion;
        private static string _distributionChannel;

        internal static string UserAgentString()
        {
            string psGetCompat = InternalHooks.InvokedFromCompat ? "true" : "false";
            return $"PSResourceGet/{_psResourceGetVersion} PowerShell/{_psVersion} DistributionChannel/{_distributionChannel} PowerShellGetCompat/{psGetCompat}";
        }
    }

    internal class ServerFactory
    {
        public static ServerApiCall GetServer(PSRepositoryInfo repository, PSCmdlet cmdletPassedIn, NetworkCredential networkCredential)
        {
            PSRepositoryInfo.APIVersion repoApiVersion = repository.ApiVersion;
            ServerApiCall currentServer = null;
            string userAgentString = UserAgentInfo.UserAgentString();

            switch (repoApiVersion)
            {
                case PSRepositoryInfo.APIVersion.V2:
                    currentServer = new V2ServerAPICalls(repository, cmdletPassedIn, networkCredential, userAgentString);
                    break;

                case PSRepositoryInfo.APIVersion.V3:
                    currentServer = new V3ServerAPICalls(repository, cmdletPassedIn, networkCredential, userAgentString);
                    break;

                case PSRepositoryInfo.APIVersion.Local:
                    currentServer = new LocalServerAPICalls(repository, cmdletPassedIn, networkCredential);
                    break;

                case PSRepositoryInfo.APIVersion.NugetServer:
                    currentServer = new NuGetServerAPICalls(repository, cmdletPassedIn, networkCredential, userAgentString);
                    break;

                case PSRepositoryInfo.APIVersion.ContainerRegistry:
                    currentServer = new ContainerRegistryServerAPICalls(repository, cmdletPassedIn, networkCredential, userAgentString);
                    break;

                case PSRepositoryInfo.APIVersion.Unknown:
                    break;
            }

            return currentServer;
        }
    }
}
