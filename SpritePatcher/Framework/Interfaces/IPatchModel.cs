namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents a conditional patch.</summary>
public interface IPatchModel
{
    /// <summary>Gets the unique identifier for this mod.</summary>
    string Id { get; }

    /// <summary>Gets the content pack associated with this mod.</summary>
    IContentPack? ContentPack { get; }

    /// <summary>Gets the target sprite sheet being patched.</summary>
    string Target { get; }

    /// <summary>Gets the source rectangle of the sprite sheet being patched.</summary>
    Rectangle SourceArea { get; }

    /// <summary>Gets the draw methods where the patch will be applied.</summary>
    List<DrawMethod> DrawMethods { get; }

    /// <summary>Gets the mode that the patch will be applied.</summary>
    PatchMode PatchMode { get; }

    /// <summary>Gets or sets the raw texture data for the patch.</summary>
    IRawTextureData? Texture { get; set; }

    /// <summary>Gets or sets the area of the texture.</summary>
    Rectangle Area { get; set; }

    /// <summary>Gets or sets the tint of the texture.</summary>
    Color? Tint { get; set; }

    /// <summary>Gets or sets the scale of the texture.</summary>
    float Scale { get; set; }

    /// <summary>Gets or sets the number of animation frames.</summary>
    int Frames { get; set; }

    /// <summary>Gets or sets the number of ticks per animation frame.</summary>
    int TicksPerFrame { get; set; }

    /// <summary>Gets or sets the offset for where the patch will be applied.</summary>
    Vector2 Offset { get; set; }

    /// <summary>Retrieves a unique identifier for the current patch.</summary>
    /// <returns>The current ID as a string.</returns>
    string GetCurrentId();

    /// <summary>Runs code necessary to update the texture.</summary>
    /// <param name="managedObject">The managed object requesting the patch.</param>
    /// <returns>True if the texture should be applied.</returns>
    bool Run(IManagedObject managedObject);
}