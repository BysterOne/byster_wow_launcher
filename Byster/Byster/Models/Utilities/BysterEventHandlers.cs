using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;



namespace Byster.Models.Utilities
{
    partial class BysterEventHandlers
    {
        public static Action animationCompetedAction { get; set; }
        public BysterEventHandlers()
        {
        }

        private void animationCompleted(object sender, EventArgs e)
        {
            animationCompetedAction?.Invoke();
        }
    }
}
