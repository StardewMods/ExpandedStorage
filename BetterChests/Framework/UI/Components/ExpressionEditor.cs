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
        Color.Red, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Pink,
    ];

    private readonly List<Color> colors = [];
    private readonly List<ClickableComponent> components = [];
    private readonly List<IExpression?> expressions = [];

    private int offsetY;

    public ExpressionEditor(int xPosition, int yPosition, int width, int height)
        : base(xPosition, yPosition, width, height) { }

    /// <summary>Gets or sets the y-offset.</summary>
    public int OffsetY
    {
        get => this.offsetY;
        set => this.offsetY = Math.Min(0, value);
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b) => this.draw(b, -1);

    /// <inheritdoc />
    public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
    {
        var (mouseX, mouseY) = Game1.getMousePosition(true);
        for (var index = 0; index < this.components.Count; index++)
        {
            var component = this.components[index];
            var expression = this.expressions[index];
            var color = component.containsPoint(mouseX, mouseY - this.OffsetY)
                ? ExpressionEditor.Highlighted(this.colors[index])
                : ExpressionEditor.Muted(this.colors[index]);

            if (expression?.ExpressionType is ExpressionType.All or ExpressionType.Any or ExpressionType.Not)
            {
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.mouseCursors,
                    new Rectangle(403, 373, 9, 9),
                    component.bounds.X,
                    this.OffsetY + component.bounds.Y,
                    component.bounds.Width,
                    component.bounds.Height,
                    color,
                    Game1.pixelZoom,
                    false);
            }

            if (expression?.ExpressionType is ExpressionType.All
                or ExpressionType.Any
                or ExpressionType.Not
                or ExpressionType.Comparable)
            {
                continue;
            }

            if (component is ClickableTextureComponent clickableTextureComponent)
            {
                clickableTextureComponent.draw(b, Color.White, 1f, yOffset: this.OffsetY);
                continue;
            }

            if (string.IsNullOrWhiteSpace(component.label))
            {
                continue;
            }

            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(432, 439, 9, 9),
                component.bounds.X,
                this.OffsetY + component.bounds.Y,
                component.bounds.Width,
                component.bounds.Height,
                color,
                Game1.pixelZoom,
                false);

            b.DrawString(
                Game1.smallFont,
                component.label,
                new Vector2(component.bounds.X + 8, this.OffsetY + component.bounds.Y + 2),
                Game1.textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.55f);
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y - this.OffsetY);
        for (var index = 0; index < this.components.Count; index++)
        {
            var component = this.components[index];
            var expression = this.expressions[index];
            var color = this.colors[index];

            if (component is ClickableTextureComponent clickableTextureComponent)
            {
                clickableTextureComponent.tryHover(x, y - this.OffsetY);
            }
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        const int scrollAmount = 40;

        // Scroll down
        if (direction < 0)
        {
            this.OffsetY -= scrollAmount;
            return;
        }

        // Scroll up
        if (direction > 0)
        {
            this.OffsetY += scrollAmount;
        }
    }

    /// <summary>Re-initializes the components of the object with the given initialization expression.</summary>
    /// <param name="initExpression">The initial expression, or null to clear.</param>
    public void ReInitializeComponents(IExpression? initExpression)
    {
        const int lineHeight = 40;
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
                this.components.Count.ToString(CultureInfo.InvariantCulture));

            this.expressions.Add(expression);
            this.components.Add(component);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            var subComponent = new ClickableComponent(
                new Rectangle(component.bounds.X + 8, component.bounds.Y + 8, 64, 32),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                expression.ExpressionType is ExpressionType.All ? "all" : "any");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.Gray);

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
            var subWidth = (int)Game1.smallFont.MeasureString("+term").X + 20;
            var subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, 32),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "+term");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.Gray);

            subComponent = new ClickableComponent(
                new Rectangle(
                    currentX + offsetX + subWidth + tabWidth,
                    currentY,
                    (int)Game1.smallFont.MeasureString("+group").X + 20,
                    32),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "+group");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.Gray);

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

            var subComponent = new ClickableComponent(
                new Rectangle(component.bounds.X + 8, component.bounds.Y + 8, 64, 32),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "not");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.Gray);

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

            var subWidth = ((this.width - tabWidth) / 2) - offsetX - 19;
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
                new Rectangle(component.bounds.Right - 36, component.bounds.Y + 4, 36, 36),
                string.Empty,
                "Remove",
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                3f);

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
            this.colors.Add(Color.White);

            var subWidth = ((this.width - tabWidth) / 2) - offsetX - 19;
            var subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX, currentY, subWidth, lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                "Something");

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            subComponent = new ClickableComponent(
                new Rectangle(currentX + offsetX + subWidth + tabWidth, currentY, subWidth, lineHeight),
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                expression.Term);

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(ExpressionEditor.Colors[offsetX / tabWidth % ExpressionEditor.Colors.Length]);

            subComponent = new ClickableTextureComponent(
                this.components.Count.ToString(CultureInfo.InvariantCulture),
                new Rectangle(component.bounds.Right - 36, component.bounds.Y + 4, 36, 36),
                string.Empty,
                "Remove",
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                3f);

            this.expressions.Add(null);
            this.components.Add(subComponent);
            this.colors.Add(Color.White);

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

    private static Color Highlighted(Color color) => Color.Lerp(color, Color.White, 0.5f);

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