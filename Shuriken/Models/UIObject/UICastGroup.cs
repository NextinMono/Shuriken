using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using XNCPLib.XNCP;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class UICastGroup : INotifyPropertyChanged, ICastContainer
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }
        public bool Visible { get; set; }

        public ObservableCollection<ShurikenUIElement> Casts { get; set; }

        public void AddCast(ShurikenUIElement cast)
        {
            Casts.Add(cast);
        }

        public void RemoveCast(ShurikenUIElement cast)
        {
            Casts.Remove(cast);
        }

        public UICastGroup(CastGroup castGroup, string name = "Group")
        {
            Name = name;
            Visible = true;
            Casts = new ObservableCollection<ShurikenUIElement>();
        }

        public UICastGroup(string name = "Group")
        {
            Name = name;
            Visible = true;
            Casts = new ObservableCollection<ShurikenUIElement>();
        }

        public UICastGroup(UICastGroup g)
        {
            Name = g.name;
            Visible = true;

            Casts = new ObservableCollection<ShurikenUIElement>(g.Casts);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
