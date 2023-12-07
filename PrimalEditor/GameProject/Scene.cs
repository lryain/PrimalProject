using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
         * 这里有个坑点 必须按照字母顺序来放，不然提取不到值，改成：
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

        public Scene(Project project, string name) 
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
        }
    }
}
