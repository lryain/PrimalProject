using PrimalEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEditor.GameProject
{
    // 项目模板列表 使用数据合同来序列化
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string ProjectFile { get; set; }
        [DataMember]
        public List<string> Folders { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
        public string IconFilePath { get; set; }
        public string ScreenshotFilePath { get; set; }
        public string ProjectFilePath { get; set; }
    }

    class NewProject : ViewModelBase
    {
        // TODO: 从安装路径取得path
        private readonly string _templatePath = @"..\..\PrimalEditor\ProjectTemplates";

        // 可绑定属性,在set方法中将属性绑定到控件 对应新项目控件的名称
        private string _projectName = "NewProject";

        public string ProjectName { get => _projectName;
            set {
                if (_projectName != value)
                {
                    _projectName = value;
                    //添加验证 方法
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\PrimalProjects\";

        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    //添加验证 方法
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private bool _isValid = false;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string _errorMsg;
        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (_errorMsg != value) { 
                    _errorMsg = value;
                    OnPropertyChanged(nameof(ErrorMsg));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = new ObservableCollection<ProjectTemplate>();
        // 使用只读的来包装它，然后在构造函数中初始化，来达到数据隔离的效果
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }

        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!StringUtils.EndsInDirectorySeparator(path))
            {
                path += @"\";
            }
            path += $@"{ProjectName}\"; // full path for our project

            IsValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMsg = "请输入项目名称。";
            }
            else if (ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ErrorMsg = "无效的项目名称。";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMsg = "请选择正确的项目文件路径。";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMsg = "无效的项目路径。";
            }
            // 如果项目路径有效，但是目录不为空，然让不能使用！
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMsg = "选择的目录已经包含项目，请选择一个空目录。";
            }
            else
            {
                ErrorMsg = string.Empty;
                IsValid = true;
            }
            return IsValid;
        }

        // 创建项目
        public string CreateProject(ProjectTemplate template)
        {
            ValidateProjectPath();
            if (!IsValid)
            {
                return string.Empty;
            }

            if (!StringUtils.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";
            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if(!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach (var folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }
                var dirInfo = new DirectoryInfo(path+ @".Primal\");
                // 设置为隐藏文件
                dirInfo.Attributes |= FileAttributes.Hidden;
                //拷贝模板文件
                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Screenshot.png")));

                // 生成序列化文件，作为模板，现在不需要这段的代码
                /*
                // 创建项目包含一个空的场景
                //var project = new Project(ProjectName, path);
                // 序列化到.primal文件
                //Serializer.ToFile(project, path+ $"{ProjectName}"+Project.Extension);
                */
                // 读取模板文件内容
                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = string.Format(projectXml, ProjectName, ProjectPath);
                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                // 向模板文件内传递参数 {0} {1}
                File.WriteAllText(projectPath, projectXml);

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public NewProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);
            // 初始化项目模板列表
            // 加载文件夹列表
            try
            {
                var templatesFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templatesFiles.Any());
                foreach (var file in templatesFiles)
                {
                    // dummy code 来生成序列化模板，生成后不再需要
                    //var template = new ProjectTemplate()
                    //{
                    //    ProjectType = "Empty Project",
                    //    ProjectFile = "project.primal",
                    //    Folders = new List<string>() { ".Primal", "Content", "GameCode" }
                    //};

                    // 序列化使用dataContract方案，允许我序列化私有字段属性
                    //Serializer.ToFile(template, file);

                    // 现在需要遍历这些文件夹，然后读取他们
                    var template = Serializer.FromFile<ProjectTemplate>(file);

                    template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "screenshot.jpg"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), template.ProjectFile));
                    // 现在我们有了全部数据，接下来告诉控件如何显示即可！

                    // 控件中的背景是一个图像笔刷

                    _projectTemplates.Add(template);
                }
                //添加验证 方法
                ValidateProjectPath();
            }
            catch (Exception ex) 
            { 
                Debug.WriteLine(ex.Message);
                // TODO: log error
                
            }

        }
    }

   
}
