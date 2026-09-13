namespace LabExtended.Enums;

/// <summary>
/// Specifies the alignment of a hint on the canvas.
/// </summary>
public enum HintAlign
{
    /// <summary>
    /// Aligns the hint to the left of the canvas.
    /// </summary>
    FullLeft,

    /// <summary>
    /// Aligns the hint to the left of the canvas.
    /// </summary>
    Left,

    /// <summary>
    /// Aligns the hint to the center of the canvas.
    /// </summary>
    Center,

    /// <summary>
    /// Aligns the hint to the right of the canvas. Due to limitation of the game's UI system, this will not be fully aligned to the right side of the canvas.
    /// </summary>
    Right
}