using CommandLine;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzurePipelines.QueueBuild.CmdLine
{
    class Program
    {
        public class Options
        {
            [Option('c', "collectionUrl", Required = false, HelpText = "VSTS Collection URL (https://{name}.visualstudio.com/DefaultCollection)")]
            public string CollectionUrl { get; set; }

            [Option('p', "projectName", Required = false, HelpText = "Project Name")]
            public string ProjectName { get; set; }

            [Option('t', "token", Required = false, HelpText = "Personal Access Token")]
            public string Token { get; set; }

            [Option('b', "buildName", Required = false, HelpText = "Name of the Build to Queue")]
            public string BuildName { get; set; }
        }
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                     .WithParsed(o =>
                     {
                         QueueBuild(o.CollectionUrl, o.ProjectName, o.BuildName, o.Token).GetAwaiter().GetResult();
                     });
        }

        private async static Task QueueBuild(string vstsCollectionUrl, string projectName, string buildName, string pat)
        {
            VssConnection connection = new VssConnection(new Uri(vstsCollectionUrl), new VssBasicCredential(string.Empty, pat));
            var buildServer = connection.GetClient<BuildHttpClient>();

            var builddefs = buildServer.GetDefinitionsAsync(project: projectName, name: buildName).Result;
            foreach (BuildDefinitionReference builddef in builddefs)
            {
                var res = buildServer.QueueBuildAsync(new Build
                {
                    Definition = new DefinitionReference
                    {
                        Id = builddef.Id
                    },
                    Project = builddef.Project
                });
                Console.WriteLine(res.Result);
            }
        }
    }
}
