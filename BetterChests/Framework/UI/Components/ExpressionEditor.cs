namespace StardewMods.BetterChests.Framework.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

internal sealed class ExpressionEditor : IClickableMenu
{
    private static readonly Color[] Colors =
    [
        ExpressionEditor.Muted(Color.Red),
        ExpressionEditor.Muted(Color.Yellow),
        ExpressionEditor.Muted(Color.Green),
        ExpressionEditor.Muted(Color.Cyan),
        ExpressionEditor.Muted(Color.Blue),
        ExpressionEditor.Muted(Color.Violet),
        ExpressionEditor.Muted(Color.Pink),
    ];

    private readonly List<Color> colors = [];
    private readonly List<ClickableComponent> components = [];
    private readonly List<IExpression?> expressions = [];
    private readonly Texture2D uiTexture;

    public ExpressionEditor(Texture2D uiTexture, int xPosition, int yPosition, int width, int height)
        : base(xPosition, yPosition, width, height) =>
        this.uiTexture = uiTexture;

    /// <inheritdoc />
    public override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        for (var index = 0; index < this.components.Count; index++)
        {
            var component = this.components[index];
            var expression = this.expressions[index];
            var color = this.colors[index];

            if (expression?.ExpressionType is ExpressionType.All
                or ExpressionType.Any
                or ExpressionType.Not
                or ExpressionType.Dynamic
                or ExpressionType.Static)
            {
                // Draw Top-Left
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.X, component.bounds.Y, 4, 4),
                    new Rectangle(128, 128, 4, 4),
                    color);

                // Draw Top-Center
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.X + 4, component.bounds.Y, component.bounds.Width - 8, 4),
                    new Rectangle(132, 128, 56, 4),
                    color);

                // Draw Top-Right
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.Right - 4, component.bounds.Y, 4, 4),
                    new Rectangle(188, 128, 4, 4),
                    color);

                // Draw Middle-Left
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.X, component.bounds.Y + 4, 4, component.bounds.Height - 8),
                    new Rectangle(128, 132, 4, 56),
                    color);

                // Draw Middle-Center
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(
                        component.bounds.X + 4,
                        component.bounds.Y + 4,
                        component.bounds.Width - 8,
                        component.bounds.Height - 8),
                    new Rectangle(64, 128, 64, 64),
                    color);

                // Draw Middle-Right
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.Right - 4, component.bounds.Y + 4, 4, component.bounds.Height - 8),
                    new Rectangle(188, 132, 4, 56),
                    color);

                // Draw Bottom-Left
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.X, component.bounds.Bottom - 4, 4, 4),
                    new Rectangle(128, 188, 4, 4),
                    color);

                // Draw Bottom-Center
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.X + 4, component.bounds.Bottom - 4, component.bounds.Width - 8, 4),
                    new Rectangle(132, 188, 56, 4),
                    color);

                // Draw Bottom-Right
                b.Draw(
                    Game1.uncoloredMenuTexture,
                    new Rectangle(component.bounds.Right - 4, component.bounds.Bottom - 4, 4, 4),
                    new Rectangle(188, 188, 4, 4),
                    color);
            }

            switch (expression?.ExpressionType)
            {
                case ExpressionType.All or ExpressionType.Any:
                    b.Draw(
                        this.uiTexture,
                        new Rectangle(component.bounds.X + 8, component.bounds.Y + 8, 64, 32),
                        new Rectangle(64, 48, 16, 8),
                        Color.White);

                    b.DrawString(
                        Game1.tinyFont,
                        component.label,
                        new Vector2(component.bounds.X + 16, component.bounds.Y + 16),
                        Game1.textColor,
                        0f,
                        Vector2.Zero,
                        0.65f,
                        SpriteEffects.None,
                        0.55f);

                    break;

                case ExpressionType.Not:
                    b.DrawString(
                        Game1.smallFont,
                        "NOT",
                        new Vector2(component.bounds.X + 8, component.bounds.Y + 4),
                        Game1.textColor,
                        0f,
                        Vector2.Zero,
                        1f,
                        SpriteEffects.None,
                        0.55f);

                    break;

                case ExpressionType.Comparable: break;

                case ExpressionType.Dynamic or ExpressionType.Static:
                    b.DrawString(
                        Game1.smallFont,
                        expression.Term,
                        new Vector2(component.bounds.X + 8, component.bounds.Y + 4),
                        Game1.textColor);

                    break;

                default:
                    if (component is ClickableTextureComponent clickableTextureComponent)
                    {
                        clickableTextureComponent.draw(b);
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(component.label))
                    {
                        // Left
                        b.Draw(
                            this.uiTexture,
                            new Rectangle(component.bounds.X, component.bounds.Y, 8, 32),
                            new Rectangle(64, 48, 2, 8),
                            Color.White);

                        // Center
                        b.Draw(
                            this.uiTexture,
                            new Rectangle(component.bounds.X + 8, component.bounds.Y, component.bounds.Width - 16, 32),
                            new Rectangle(66, 48, 12, 8),
                            Color.White);

                        // Right
                        b.Draw(
                            this.uiTexture,
                            new Rectangle(component.bounds.Right - 8, component.bounds.Y, 8, 32),
                            new Rectangle(78, 48, 2, 8),
                            Color.White);

                        b.DrawString(
                            Game1.tinyFont,
                            component.label,
                            new Vector2(component.bounds.X + 10, component.bounds.Y + 10),
                            Game1.textColor,
                            0f,
                            Vector2.Zero,
                            0.65f,
                            SpriteEffects.None,
                            0.55f);
                    }

                    break;
            }
        }
    }

    /// <summary>Re-initializes the components of the object with the given initialization expression.</summary>
    /// <param name="initExpression">The initial expression, or null to clear.</param>
    public void ReInitializeComponents(IExpression? initExpression)
    {
        const int lineHeight = 32;
        const int tabWidth = 12;
        var currentX = this.xPositionOnScreen;
        var currentY = this.yPositionOnScreen;
        this.colors.Clear();
        this.components.Clear();
        this.expressions.Clear();

        if (initExpression is null)
        {
            return;
        }

        var offsetX = -1;
        Enqueue(initExpression);
        this.populateClickableComponentList();
        return;

        void AddGroup(IExpression expression)
        {
            if (offsetX == -1)
            {
                offsetX = 0;
                foreach (var subExpression in expression.Expressions)
                {
                    Enqueue(subExpression);
                }

                AddInsert();
                return;
            }

            var initialY = currentY;
            var component = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, this.width - (offsetX * 2), lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                expression.ExpressionType is ExpressionType.All ? "ALL" : "ANY");

            this.expressions.Add(expression);
            this.components.Add(component);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            currentY += lineHeight + tabWidth;
            offsetX += tabWidth;

            foreach (var subExpression in expression.Expressions)
            {
                Enqueue(subExpression);
            }

            AddInsert();
            offsetX -= tabWidth;

            var newHeight = currentY - initialY - tabWidth;
            if (component.bounds.Height == newHeight)
            {
                return;
            }

            component.bounds.Height = newHeight + tabWidth;
            currentY += tabWidth;
        }

        void AddInsert()
        {
            var subWidth = (int)(Game1.tinyFont.MeasureString("+TERM").X * 0.65f) + 20;
            var subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "+TERM");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.White);

            subComponent = new ClickableComponent(
                new Rectangle(
                    currentX + offsetX + subWidth + tabWidth,
                    currentY,
                    (int)(Game1.tinyFont.MeasureString("+GROUP").X * 0.65f) + 20,
                    lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "+GROUP");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.White);

            currentY += lineHeight + tabWidth;
        }

        void AddNot(IExpression expression)
        {
            var initialY = currentY;
            var component = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, this.width - (offsetX * 2), lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture));

            this.expressions.Add(expression);
            this.components.Add(component);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            currentY += lineHeight + tabWidth;
            offsetX += tabWidth;

            Enqueue(expression.Expressions.First());

            offsetX -= tabWidth;
            var newHeight = currentY - initialY - tabWidth;
            if (component.bounds.Height == newHeight)
            {
                return;
            }

            component.bounds.Height = newHeight + tabWidth;
            currentY += tabWidth;
        }

        void AddComparable(IExpression expression)
        {
            var component = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, this.width - (offsetX * 2), lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture));

            this.expressions.Add(expression);
            this.components.Add(component);
            this.colors.Add(Color.White);

            var leftTerm = expression.Expressions.First();
            var rightTerm = expression.Expressions.Last();

            var subWidth = ((this.width - tabWidth) / 2) - offsetX - 12;
            var subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                leftTerm.Term);

            this.expressions.Add(leftTerm);
            this.components.Add(subComponent);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX + subWidth + tabWidth, currentY, subWidth, lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                rightTerm.Term);

            this.expressions.Add(rightTerm);
            this.components.Add(subComponent);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            subComponent = new ClickableTextureComponent(
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                new Rectangle(component.bounds.Right - 22, component.bounds.Y + 4, 24, 24),
                string.Empty,
                "Remove",
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                2f);

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.White);

            currentY += lineHeight + tabWidth;
        }

        void AddTerm(IExpression expression)
        {
            var component = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, this.width - (offsetX * 2), lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture));

            this.expressions.Add(expression);
            this.components.Add(component);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);
            currentY += lineHeight + tabWidth;
        }

        void Enqueue(IExpression expression)
        {
            switch (expression.ExpressionType)
            {
                case ExpressionType.All or ExpressionType.Any:
                    AddGroup(expression);
                    break;

                case ExpressionType.Not:
                    AddNot(expression);
                    break;

                case ExpressionType.Comparable:
                    AddComparable(expression);
                    break;

                default:
                    AddTerm(expression);
                    break;
            }
        }
    }

    private static Color Muted(Color color)
    {
        color = new Color(
            (int)Utility.Lerp(color.R, Math.Min(255, color.R + 150), 0.65f),
            (int)Utility.Lerp(color.G, Math.Min(255, color.G + 150), 0.65f),
            (int)Utility.Lerp(color.B, Math.Min(255, color.B + 150), 0.65f));

        var hsl = HslColor.FromColor(color);
        hsl.S *= 0.5f;
        return hsl.ToRgbColor();
    }
}