using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Components
{
    [DataContract]
    public class Transform : Component
    {
        private string _name;

        private Vector3D _position;
        [DataMember]
        public Vector3D Position
        {
            get { return _position; }
            set { 
                _position = value; 
                OnPropertyChanged(nameof(Position));
            }
        }

        private Vector3D _rotation;
        [DataMember]
        public Vector3D Rotaition
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                OnPropertyChanged(nameof(Rotaition));
            }
        }

        private Vector3D _scale;
        [DataMember]
        public Vector3D Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }

        [DataMember(Name = nameof(Components))]
        private ObservableCollection<Component> _components = new ObservableCollection<Component>();
        public ReadOnlyObservableCollection<Component> Components { get; }

        public Transform(GameEntity owner) : base(owner) {
        }
    }
}
