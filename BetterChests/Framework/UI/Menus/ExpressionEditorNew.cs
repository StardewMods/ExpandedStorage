namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Menus;

internal sealed class ExpressionEditorNew : FramedMenu
{
    private readonly IExpressionHandler expressionHandler;
    private readonly Func<string> getSearchText;
    private readonly IIconRegistry iconRegistry;
    private readonly IReflectionHelper reflectionHelper;
    private readonly Action<string> setSearchText;

    private ExpressionGroup? baseComponent;

    /// <summary>Initializes a new instance of the <see cref="ExpressionEditorNew" /> class.</summary>
    /// <param name="expressionHandler"></param>
    /// <param name="iconRegistry"></param>
    /// <param name="inputHelper"></param>
    /// <param name="reflectionHelper"></param>
    /// <param name="getSearchText"></param>
    /// <param name="setSearchText"></param>
    /// <param name="xPosition"></param>
    /// <param name="yPosition"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public ExpressionEditorNew(
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        IReflectionHelper reflectionHelper,
        Func<string> getSearchText,
        Action<string> setSearchText,
        int xPosition,
        int yPosition,
        int width,
        int height)
        : base(inputHelper, reflectionHelper, xPosition, yPosition, width, height)
    {
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.reflectionHelper = reflectionHelper;
        this.getSearchText = getSearchText;
        this.setSearchText = setSearchText;
    }

    /// <inheritdoc />
    protected override Rectangle Frame =>
        new(this.xPositionOnScreen - 4, this.yPositionOnScreen - 8, this.width + 8, this.height + 16);

    /// <inheritdoc />
    protected override int StepSize => 40;

    /// <summary>Re-initializes the components of the object with the given initialization expression.</summary>
    /// <param name="initExpression">The initial expression, or null to clear.</param>
    public void ReInitializeComponents(IExpression? initExpression)
    {
        if (initExpression is null)
        {
            return;
        }

        this.baseComponent = new ExpressionGroup(
            this.iconRegistry,
            this.Input,
            this.reflectionHelper,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            initExpression,
            -1);

        this.MaxOffset = new Point(
            -1,
            Math.Max(0, this.baseComponent.bounds.Bottom - this.height - this.yPositionOnScreen));
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor) =>
        this.baseComponent?.Draw(spriteBatch, cursor, new Point(-this.Offset.X, -this.Offset.Y));

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch b, Point cursor) { }
}