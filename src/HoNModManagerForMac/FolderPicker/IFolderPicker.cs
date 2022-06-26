using System;
namespace HonModManagerForMac.Interfaces
{
    public interface IFolderPicker
    {
        Task<string> PickFolder();
    }
}

