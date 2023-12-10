using PrimalEditor.Components;
using PrimalEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    [DataContract]
    public class Scene : ViewModelBase
    {
        [DataMember]
        private string _name;
        public string Name
        {
            get => _name;
            private set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        
        [DataMember]
        public Project Project { get; private set; }
        // 我们需要保存的是 bool标志位而不是这个具体的活动场景
        // 我们需要将它变成可绑定的属性
        //public bool IsActive => Project.ActiveScene == this;
        private bool _isActive;

        /***
         * 这里有个坑点 必须按照字母顺序来放，不然提取不到bool字段的值，改成：
         * <Scene z:Id="i2">
         *    <IsActive>true</IsActive>
         *    <Project z:Ref="i1"/>
         *    <_name>Default Scene</_name>
        * </Scene>
         */
        [DataMember]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        [DataMember(Name = nameof(GameEntities))]
        // 临时去掉只读属性 因为模板文件中还没有GameEntity这个字段
        private readonly ObservableCollection<GameEntity> _gameEntities = new ObservableCollection<GameEntity>();
        public ReadOnlyObservableCollection<GameEntity> GameEntities { get; private set; }

        private void AddGameEntity(GameEntity entity)
        {
            Debug.Assert(!_gameEntities.Contains(entity));
            _gameEntities.Add(entity);
        }

        private void RemoveGameEntity(GameEntity entity)
        {
            Debug.Assert(_gameEntities.Contains(entity));
            _gameEntities.Remove(entity);
        }

        public ICommand AddGameEntityCommand { get; private set; }
        public ICommand RemoveGameEntityCommand { get; private set; }

        // 反序列化后我们需要一个方法来处理后续的场景初始化逻辑
        // 遍历所有场景，然后找到活动的场景，
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // 生成了GameEntity模块属性之后就不用在这里初始化了
            //if(_gameEntities == null) _gameEntities = new ObservableCollection<GameEntity>();
            if (_gameEntities != null)
            {
                GameEntities = new ReadOnlyObservableCollection<GameEntity>(_gameEntities);
                // 绑定属性改变事件
                OnPropertyChanged(nameof(GameEntities));
            }

            AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                AddGameEntity(x);
                var entityIndex =_gameEntities.Count -1;

                Project.UndoRedo.Add(new UndoRedoAction(
                    () => RemoveGameEntity(x),
                    () => _gameEntities.Insert(entityIndex, x),
                    $"Add {x.Name}to{Name}"));
            });

            RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                var entityIndex = _gameEntities.IndexOf(x);
                RemoveGameEntity(x);

                Project.UndoRedo.Add(new UndoRedoAction(
                    () => _gameEntities.Insert(entityIndex, x),
                    () => RemoveGameEntity(x),
                    $"Remove {x.Name}"));
            });

            //// 每当我们点击重做和撤销
            //UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo());
            //RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo());
            //SaveCommand = new RelayCommand<object>(x => Save(this));
        }

        public Scene(Project project, string name) 
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
            OnDeserialized(new StreamingContext());
        }
    }
}
