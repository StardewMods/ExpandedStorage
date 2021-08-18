using AlternativeTextures.Framework.Models;

namespace XSAlternativeTextures
{
    public interface IAlternativeTexturesAPI
    {
        public void AddAlternativeTexture(AlternativeTextureModel model, string owner, string tileSheetPath);
    }
}