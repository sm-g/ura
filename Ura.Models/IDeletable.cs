using System;
using System.Diagnostics;

namespace Ura.Models
{
    public interface IDeletable
    {
        bool Deprecated { get; set; }
    }

    public class DeletableEventArgs : EventArgs
    {
        public IDeletable entity;

        [DebuggerStepThrough]
        public DeletableEventArgs(IDeletable e)
        {
            entity = e;
        }
    }
}