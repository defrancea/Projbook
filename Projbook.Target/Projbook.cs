using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projbook.Target
{
    public class Projbook : Task
    {
        public override bool Execute()
        {
            Log.LogError(this.Pouet);
            Log.LogWarning(this.Pouet);
            return false;
        }

        [Required]
        public string Pouet { get; set; }
    }
}
