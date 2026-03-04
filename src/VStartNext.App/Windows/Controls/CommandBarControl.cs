using System.Drawing;
using System.Windows.Forms;

namespace VStartNext.App.Windows.Controls;

public sealed class CommandBarControl : UserControl
{
    public TextBox InputBox { get; }

    public CommandBarControl()
    {
        Dock = DockStyle.Fill;
        Margin = new Padding(6);
        BackColor = Color.FromArgb(28, 30, 38);

        InputBox = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(22, 23, 30),
            PlaceholderText = "Type command or search..."
        };

        var padding = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        padding.Controls.Add(InputBox);
        Controls.Add(padding);
    }
}
