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
using System.Windows.Input;

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

        // 重做撤销静态类
        public static UndoRedo UndoRedo {  get; } = new UndoRedo();

        // 然后添加两个相应的ICommand的方法
        public ICommand AddScene {  get; private set; }
        public ICommand RemoveScene { get; private set; }

        public ICommand Undo {  get; private set; }
        public ICommand Redo {  get; private set; }

        // 添加场景 将这两个方法改成私有的，然后重命名成内部的方法
        private void AddSceneInternal(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }

        private void RemoveSceneInternal(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }

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
            // 在详细序列化后 生成一个添加场景命令？
            AddScene = new RelayCommand<object>(x =>
            {
                AddSceneInternal($"New Scene {_scenes.Count}");
                var newScene = _scenes.Last(); // 记住场景和索引
                var sceneIndex = _scenes.Count - 1;

                UndoRedo.Add(new UndoRedoAction(
                    () => RemoveSceneInternal(newScene),
                    () => _scenes.Insert(sceneIndex, newScene),
                    $"Add {newScene.Name}"));
            });

            RemoveScene = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenes.IndexOf(x);
                RemoveSceneInternal(x);

                UndoRedo.Add(new UndoRedoAction(
                    () => _scenes.Insert(sceneIndex, x),
                    () => RemoveSceneInternal(x),
                    $"Remove {x.Name}"));
            }, x => !x.IsActive);

            // 每当我们点击重做和撤销
            Undo = new RelayCommand<object>(x => UndoRedo.Undo());
            Redo = new RelayCommand<object>(x => UndoRedo.Redo());
        }

        // 此时构造函数已经不再需要，我们需要用其他办法来构造Project，在反序列化后我们需要一个方法来处理后续的场景初始化逻辑
        // 然后在构造函数中 实现自定义的执行命令的方法
        public Project(string name, string path) { 
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
            // 现在转到Scene来看缺少什么
        }
    }
}
