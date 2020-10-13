using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConvertMVC5ToCore3.ClassesDef;
using ConvertMVC5ToCore3.Common;
using EnvDTE;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ConvertMVC5ToCore3
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConvertMvc5ToCore3Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d015183e-3cd7-4bd4-be9a-57837ed87e66");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertMvc5ToCore3Command"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ConvertMvc5ToCore3Command(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                Project selectedProject = GetSelectedProject();
                IVsSolution solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

                IVsHierarchy hierarchy;
                solution.GetProjectOfUniqueName(selectedProject.UniqueName, out hierarchy);

                IVsAggregatableProjectCorrected ap;
                ap = hierarchy as IVsAggregatableProjectCorrected;

                string projTypeGuids;
                //ap.GetAggregateProjectTypeGuids(out projTypeGuids); //Cancelled by Gelis at 2019/10/05 (因為 .NET Core 的全新的專案檔格式沒有 Guid)

                //if (projTypeGuids.ToUpper().IndexOf(Constants.WpfProjectGuidString) > 0)
                //{
                menuCommand.Visible = true;
                menuCommand.Enabled = true;
                //}
            }

        }

        private Project GetSelectedProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Project activeProject = null;
            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
            object[] activeSolutionProjects = dte.ActiveSolutionProjects as object[];
            if (activeSolutionProjects != null)
            {
                activeProject = activeSolutionProjects.GetValue(0) as Project;
            }
            return activeProject;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConvertMvc5ToCore3Command Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ConvertMvc5ToCore3Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ConvertMvc5ToCore3Command(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string title = "Convert ASP.NET MVC5 to ASP.NET Core 3";
            string message = "您想要將目前的 ASP.NET MVC5 的專案 Convert 成 .NET Core 3 類型的專案嗎？";

            var result = VsShellUtilities.ShowMessageBox(this.package, message, title, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            if (result == 1)
            {
                try
                {
                    Project activeProject = GetSelectedProject();
                    await ConvertProjectAsync(activeProject);
                }
                catch (Exception ex)
                {
                    VsShellUtilities.ShowMessageBox(this.package, ex.Message, title, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }

            /*
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "ConvertMvc5ToCore3Command";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        */
        }

        async Task ConvertProjectAsync(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await DeleteProjectItemAsync(project, "Web.Config");
            //await DeleteProjectItemAsync(project, "Web.Debug.config");
            //await DeleteProjectItemAsync(project, "Web.Release.config");
            await DeleteProjectItemAsync(project, "packages.config");
            //await DeleteProjectItemAsync(project, "Global.asax.cs");
            await DeleteProjectItemAsync(project, "Global.asax");
            await DeleteProjectItemAsync(project, "App_Start");
            await DeleteProjectItemAsync(project, "Properties");
            await DeleteProjectItemAsync(project, "Views");
            //await DeleteProjectItemAsync(project, "RouteConfig.cs");
            //await DeleteProjectItemAsync(project, "WebApiConfig.cs");

            //var resultAll = prjItems.OfType<ProjectItem>().Select(c => c);
            //var test1 = resultAll.Where(c =>
            //{
            //    Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            //    return c.Name.ToLower() == "packages.config";
            //})
            //.FirstOrDefault();
            //test1.Remove();

            IVsSolution4 solution = await ServiceProvider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution4;

            ProjectRootElement projectRoot = ProjectRootElement.Open(project.FullName);

            //建立 .NET Core 相關檔案
            await CreateProjectItemAsync(project, "startup.cs", projectRoot.PropertyGroups.FirstOrDefault());
            await CreateProjectItemAsync(project, "program.cs", projectRoot.PropertyGroups.FirstOrDefault());
            await CreateProjectItemAsync(project, "appsettings.json", projectRoot.PropertyGroups.FirstOrDefault());

            var projectData = ReadProjectData(projectRoot);
            projectData.FilePath = Path.GetDirectoryName(project.FullName);
            projectData.AssemblyVersion = project.Properties.Item(Constants.AssemblyVersion)?.Value.ToString();

            UnloadProject(solution, projectData.ProjectGuid);

            DeleteCSProjContents(projectRoot);

            UpdateCSProjectFolders(projectRoot);

            UpdateCSProjContents(projectRoot, projectData);

            projectRoot.Save();

            //UpdateAssemblyInfo(projectData.FilePath);

            ReloadProject(solution, projectData.ProjectGuid);
        }

        private void UpdateCSProjectFolders(ProjectRootElement projectRoot)
        {
            var deletePhysicalName = projectRoot.FullPath; //deleteItem.Properties.Item("FullPath");
            string fullPath = deletePhysicalName;
            string projectPath = Path.GetDirectoryName(fullPath);

            ProjectItemGroupElement group01 = null;

            if (Directory.Exists(Path.Combine(projectPath, "Models")))
            {
                group01 = group01??projectRoot.AddItemGroup();
                group01.AddItem("Folder", @"Models\");
            }
            
            if(Directory.Exists(Path.Combine(projectPath, "Views")))
            {
                group01 = group01??projectRoot.AddItemGroup();
                group01.AddItem("Folder", @"Views\");
            }
        }

        private void UpdateCSProjContents(ProjectRootElement projectRoot, ProjectData projectData)
        {
            projectRoot.ToolsVersion = null;
            projectRoot.Sdk = Constants.Sdk;
            var propertyGroup = projectRoot.AddPropertyGroup(); //projectRoot.PropertyGroups.First();

            propertyGroup.AddProperty(Constants.TargetFramework, Constants.NetCoreApp3);
            propertyGroup.AddProperty(Constants.GenerateAssemblyInfo, "false");

            //TODO: check to see if the project type is WPF or WinForms.
            //propertyGroup.AddProperty(Constants.UseWPF, Constants.True);

            if (!string.IsNullOrWhiteSpace(projectData.AssemblyVersion))
                propertyGroup.AddProperty(Constants.Version, projectData.AssemblyVersion);

            //UpdateNuGetPackageReferences(projectRoot, projectData);
        }

        private void DeleteCSProjContents(ProjectRootElement projectRoot)
        {
            projectRoot.ToolsVersion = null;
            projectRoot.RemoveAllChildren();
        }

        void UnloadProject(IVsSolution4 solution, Guid projectGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int hr;
            hr = solution.UnloadProject(ref projectGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
            ErrorHandler.ThrowOnFailure(hr);
        }

        private ProjectData ReadProjectData(ProjectRootElement projectRoot)
        {
            var projectData = new ProjectData();
            var propertyGroup = projectRoot.PropertyGroups.First();
            projectData.ProjectGuid = Guid.Parse(propertyGroup.Properties.FirstOrDefault(x => x.Name == Constants.ProjectGuid)?.Value.ToString());
            projectData.ProjectTypeGuids = propertyGroup.Properties.FirstOrDefault(x => x.Name == Constants.ProjectTypeGuids)?.Value.ToString();
            //projectData.AssemblyName = propertyGroup.Properties.FirstOrDefault(x => x.Name == Constants.AssemblyName)?.Value.ToString();
            //projectData.OutputType = propertyGroup.Properties.FirstOrDefault(x => x.Name == Constants.OutputType)?.Value.ToString();
            return projectData;
        }

        private void ReloadProject(IVsSolution4 solution, Guid projectGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int hr;
            hr = solution.ReloadProject(ref projectGuid);
            ErrorHandler.ThrowOnFailure(hr);
        }

        private async Task CreateProjectItemAsync(
            Project project,
            string createFileName,
            ProjectPropertyGroupElement rootProjectGroup)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ProjectItem proDirectory = project.ProjectItems.OfType<ProjectItem>()
                .Where(c =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return c.Name.ToLower() == "properties";
                })
                .FirstOrDefault();

            var createPhysicalName = proDirectory.Properties.Item("FullPath");
            string fullPath = createPhysicalName.Value.ToString().ToLower().Replace("properties", "");
            string fileTextContent = string.Empty;

            string rootNameSpace = rootProjectGroup != null
                ?
                rootProjectGroup
                    .Properties
                    .FirstOrDefault(x => x.Name == Constants.AssemblyName)?.Value.ToString()
                :
                    string.Empty;

            switch (createFileName.ToLower())
            {
                case "startup.cs":
                    fileTextContent = NetCoreClassesDef.GetStartup.Replace("$(NAMESPACE_DEF)$", rootNameSpace);
                    break;
                case "appsettings.json":
                    fileTextContent = NetCoreClassesDef.GetAppSettings;
                    break;
                case "program.cs":
                    fileTextContent = NetCoreClassesDef.GetProgram.Replace("$(NAMESPACE_DEF)$", rootNameSpace);
                    break;
            }

            FileHelper.WriteTextToFile(Path.Combine(fullPath, createFileName), fileTextContent);
        }
        private async Task DeleteProjectItemAsync(Project project, string deleteFileName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ProjectItems prjItems = project.ProjectItems;
            ProjectItem deleteItem = prjItems.OfType<ProjectItem>()
                .Where(c =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return c.Name.ToLower() == deleteFileName.ToLower();
                })
                .FirstOrDefault();

#pragma warning disable VSTHRD109 // 使用非同步方法時請切換而非判斷提示
            ThreadHelper.ThrowIfNotOnUIThread();
#pragma warning restore VSTHRD109 // 使用非同步方法時請切換而非判斷提示

            try
            {
                deleteItem.Remove();
            }
            catch (Exception ex) { }

            if(deleteItem != null)
            {
                var deletePhysicalName = deleteItem.Properties.Item("FullPath");
                string fullPath = deletePhysicalName.Value.ToString();
                try
                {
                    string deletePath = Path.GetDirectoryName(fullPath);
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch { }
                    //真的移除掉該實體檔案
                    switch (deleteFileName.ToLower())
                    {
                        case "web.config":
                            File.Delete(Path.Combine(deletePath, "Web.Debug.config"));
                            File.Delete(Path.Combine(deletePath, "Web.Release.config"));
                            break;
                        case "global.asax":
                            File.Delete(Path.Combine(deletePath, "global.asax.cs"));
                            break;
                        case "app_start":
                            File.Delete(Path.Combine(deletePath, "RouteConfig.cs"));
                            File.Delete(Path.Combine(deletePath, "WebApiConfig.cs"));
                            Directory.Delete(deletePath);
                            break;
                        case "properties":
                            File.Delete(Path.Combine(deletePath, "AssemblyInfo.cs"));
                            break;
                        case "views":
                            File.Delete(Path.Combine(deletePath, "web.config"));
                            break;
                    }

                }
                catch (Exception ex) { }
            }
        }
    }
}
