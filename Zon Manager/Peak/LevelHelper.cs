using System;
using System.Windows;
using System.Windows.Controls;

namespace Zön_Manager.Peak
{
    /// <summary>
    /// All the levels in the peak level meter. G - green bars, Y - yellow bar, R - red bar, and W - white bar.
    /// 32 green, 22 yellow, 9 red and one white - a total of 64
    /// </summary>
    public enum LevelBar
    {
        G1, G2, G3, G4, G5, G6, G7, G8, G9, G10, G11, G12,
        G13, G14, G15, G16, G17, G18, G19, G20, G21, G22,
        G23, G24, G25, G26, G27, G28, G29, G30, G31, G32,
        Y1, Y2, Y3, Y4, Y5, Y6, Y7, Y8, Y9, Y10, Y11, Y12,
        Y13, Y14, Y15, Y16, Y17, Y18, Y19, Y20, Y21, Y22,
        R1, R2, R3, R4, R5, R6, R7, R8, R9,
        W1
    }

    public class LevelHelper : DependencyObject
    {
        /// <summary>
        /// The dp that is fired on the changing of the progress bar's Value property.
        /// </summary>
        public static readonly DependencyProperty LevelCompletionProperty = DependencyProperty.RegisterAttached(
            "LevelCompletion", typeof(double), typeof(LevelHelper),
            new PropertyMetadata(0.0, OnLevelCompletionChanged));

        /// <summary>
        /// The key of the level to display.
        /// </summary>
        private static readonly DependencyPropertyKey LevelPropertyKey = DependencyProperty.
            RegisterAttachedReadOnly(
                "Level", typeof(LevelBar), typeof(LevelHelper), new PropertyMetadata(LevelBar.G1));

        /// <summary>
        /// The level to display.
        /// </summary>
        public static readonly DependencyProperty LevelProperty = LevelPropertyKey.DependencyProperty;

        /// <summary>
        /// Called when the level completion changed.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnLevelCompletionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progress = (double)e.NewValue;
            if (!(d is ProgressBar)) return;
            var bar = d as ProgressBar;

            // calculate the actual level values
            var dist = bar.Maximum - bar.Minimum;
            const int numLevels = (LevelBar.W1 - LevelBar.G1) + 1;  // will be 64!
            var distLevel = dist / numLevels;
            var progLevel = (int)(progress / distLevel);

            var progLevelEnum = (LevelBar)Enum.ToObject(typeof(LevelBar), progLevel);

            bar.SetValue(LevelPropertyKey, progLevelEnum);
        }

        /// <summary>
        /// Sets the level completion dp.
        /// </summary>
        /// <param name="bar">The progress bar.</param>
        /// <param name="progress">The progress value.</param>
        public static void SetLevelCompletion(ProgressBar bar, double progress)
        {
            bar.SetValue(LevelCompletionProperty, progress);
        }

        /// <summary>
        /// Sets the level.
        /// </summary>
        /// <param name="bar">The progress bar.</param>
        /// <param name="stage">The stage to display until.</param>
        public static void SetLevel(ProgressBar bar, LevelBar stage)
        {
            bar.SetValue(LevelPropertyKey, stage);
        }
    }
}