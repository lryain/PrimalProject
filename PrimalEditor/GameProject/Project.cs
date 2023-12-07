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
using System.Windows;

namespace PrimalEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".primal";
        [DataMember]
        public string Name { get; private set; } = "New Project";
        [DataMember]
        public string Path { get; private set; }
        public string FullPath => $"{Path}{Name}{Extension}";

        // 场景
        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene> ();
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }
        // Active活动的场景
        public Scene _activeScene;
        //[DataMember] 我们需要保存的是 bool标志位而不是这个具体的活动场景
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        // 当前项目
        public static Project Current => Application.Current.MainWindow.DataContext as Project;

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }

        // unload project
        public void Unload()
        {

        }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
        }

        // 反序列化后我们需要一个方法来处理后续的场景初始化逻辑
        // 遍历所有场景，然后找到活动的场景，
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if(_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                // 绑定属性改变事件
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene  = Scenes.FirstOrDefault(x => x.IsActive);
        }

        // 此时构造函数已经不再需要，我们需要用其他办法来构造Project，在反序列化后我们需要一个方法来处理后续的场景初始化逻辑
        public Project(string name, string path) { 
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
            // 现在转到Scene来看缺少什么
        }
    }
}
