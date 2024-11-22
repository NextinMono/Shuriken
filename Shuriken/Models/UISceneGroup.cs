using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Shuriken.Models
{
    public class ShurikenUISceneGroup
    {
        public string Name { get; set; }
        public bool Visible { get; set; }
        public ObservableCollection<ShurikenUIScene> Scenes { get; set; }
        public ObservableCollection<ShurikenUISceneGroup> Children { get; set; }

        public void Clear()
        {
            Scenes.Clear();
            foreach (var child in Children)
                child.Clear();
        }

        public ShurikenUISceneGroup(string name)
        {
            Name = name;
            Visible = true;
            Scenes = new ObservableCollection<ShurikenUIScene>();
            Children = new ObservableCollection<ShurikenUISceneGroup>();
        }
    }
}
