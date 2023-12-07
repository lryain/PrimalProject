using PrimalEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEditor.GameProject
{
    [DataContract]
    public class ProjectData
    {
        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string ProjectPath { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        public string FullPath  => $"{ProjectPath}{ProjectName}{Project.Extension}";

        [DataMember]
        public byte[] Icon { get; set; }
        [DataMember]
        public byte[] Screenshot { get; set; }
        // 序列化应该存储图片路径，而不是Blob
        public string IconFilePath { get; set; }
        public string ScreenshotFilePath { get; set; }
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects { get; set; }
    }

    //  : ViewModelBase
    /**
     * 然后在新项目控件的OnCreate_Button_Click方法中调用OpenProject.Open方法处理 项目打开逻辑
     * 然后在控件中绑定项目列表信息
     */
    class OpenProject
    {

        // 我们在这里保存项目清单
        private static readonly string _applicationDtaPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\PrimalEditor\";
        private static readonly string _projectDataPath; // C:\Users\lryain\AppData\Roaming\PrimalEditor

        private static readonly ObservableCollection<ProjectData> _projects = new ObservableCollection<ProjectData>();
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

        // 读取ProjectData.xml文件
        private static void ReadProjectData()
        {
            if(File.Exists(_projectDataPath))
            {
                // 序列化项目数据列表，并按日期降序排序
                var projects = Serializer.FromFile<ProjectDataList>(_projectDataPath).Projects.OrderByDescending(x=> x.Date);
                _projects.Clear(); // 清空原项目列表
                //重新设置项目列表
                foreach (var project in projects)
                {
                    if (File.Exists(project.FullPath))
                    {
                        // 如果项目存在，读取图标和截图，然后放入项目列表中
                        project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                        project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");
                        _projects.Add(project);
                    }
                }
            }
        }

        // 按日期排序，然后将ProjectDataList对象序列化后写入到 ProjectData.xml 文件
        private static void WriteProjectData()
        {
            //然后序列化
            var projects = _projects.OrderBy(x => x.Date).ToList();
            Serializer.ToFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
        }

        public static Project Open(ProjectData data)
        {
            // 在这里再次读取
            ReadProjectData();
            var project = _projects.FirstOrDefault(x=>x.FullPath == data.FullPath);
            if(project != null)
            {
                // 如果我们发现项目已经在列表中只需要更新日期
                project.Date = DateTime.Now;
            }
            else
            {
                //新项目
                project = data;
                project.Date = DateTime.Now;
                _projects.Add(project);
            }
            //每次操作后都要重新保存改动到 ProjectData.xml 文件
            WriteProjectData();

            return null;
        }

        // 静态构造函数
        static OpenProject() 
        {
            try
            {
                // 如果目录不存在，则创建
                if (!Directory.Exists(_applicationDtaPath)) Directory.CreateDirectory(_applicationDtaPath);
                _projectDataPath = $@"{_applicationDtaPath}ProjectData.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                ReadProjectData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // TODO: log errors
            }
        }
    }
}
