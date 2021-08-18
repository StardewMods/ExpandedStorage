using System.Collections.Generic;

namespace XSAlternativeTextures
{
    public interface IExpandedStorageAPI
    {
        /// <summary>Returns all Expanded Storage by name.</summary>
        /// <returns>List of storages</returns>
        IList<string> GetAllStorages();
    }
}