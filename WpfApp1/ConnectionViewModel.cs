﻿using TsGui.Operation;
using TsNode.Preset;

namespace WpfApp1
{
    public class ConnectionViewModel : PresetConnectionViewModel
    {
        public int Name { get; set; }

        public ConnectionViewModel( IOperationController controller )
        {

        }
    }
}
