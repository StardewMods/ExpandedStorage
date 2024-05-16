namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI;
using StardewValley.Menus;

internal sealed class DropdownWithIcon : GenericDropdown<int>
{
    private readonly List<(int Key, string Value, IIcon? Icon)> items;

    public DropdownWithIcon(
        ClickableComponent anchor,
        List<(int Key, string Value, IIcon? Icon)> items,
        Action<int?> callback)
        : base(anchor, items.Select(item => (item.Key, item.Value)).ToList(), callback, 10)
    {
        this.items = items;

        // Expand each component and bounds to accommodate an icon
        this.width += 32;

        foreach (var component in this.Components)
        {
            component.bounds.Width += 32;
        }
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch b)
    {
        // Draw items
        var (x, y) = Game1.getMousePosition(true);
        foreach (var component in this.Components)
        {
            var index = this.Offset + int.Parse(component.name, CultureInfo.InvariantCulture);
            var item = this.items.ElementAtOrDefault(index);
            if (string.IsNullOrWhiteSpace(item.Value))
            {
                continue;
            }

            if (component.bounds.Contains(x, y))
            {
                b.Draw(
                    Game1.staminaRect,
                    component.bounds with { Width = component.bounds.Width - 16 },
                    new Rectangle(0, 0, 1, 1),
                    Color.Wheat,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.975f);
            }

            if (item.Icon is not null)
            {
                b.Draw(
                    Game1.content.Load<Texture2D>(item.Icon.Path),
                    new Vector2(component.bounds.X, component.bounds.Y),
                    item.Icon.Area,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    2f,
                    SpriteEffects.None,
                    1f);
            }

            b.DrawString(
                Game1.smallFont,
                item.Value,
                new Vector2(component.bounds.X + 32, component.bounds.Y),
                Game1.textColor);
        }
    }
}