using MaSch.Presentation.Wpf.Common;
using System.Collections.Generic;

namespace TimeTracker.Models
{
    public class GuiSettings
    {
        public List<WindowPosition> WindowPositions { get; set; }

        public GuiSettings()
        {
            WindowPositions = new List<WindowPosition>();
        }
    }
}
