// --------------------------------------------------------------------------------
// <copyright>
// Copyright (C)Nintendo. All rights reserved.
// 
// These coded instructions, statements, and computer programs contain proprietary
// information of Nintendo and/or its licensed developers and are protected by
// national and international copyright laws. They may not be disclosed to third
// parties or copied or duplicated in any form, in whole or in part, without the
// prior written consent of Nintendo.
// 
// The content herein is highly confidential and should be handled accordingly.
// </copyright>
// --------------------------------------------------------------------------------

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TsCore.Mvvm
{
    public class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}