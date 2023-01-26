using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models
{
    public enum ScaleType : uint
    {
        [Description("Stretch")]
        Below1,

        [Description("Scale to Aspect Ratio")]
        Above1,
    }
}
