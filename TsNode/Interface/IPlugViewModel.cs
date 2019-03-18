﻿using System.ComponentModel;

namespace TsNode.Interface
{
    public interface IPlugViewModel : INotifyPropertyChanged , IConnectTarget
    {
        IConnectionViewModel StartConnection();
    }
}
