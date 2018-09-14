using Reindeer_Hunter.Hunt;
using System.Collections.Generic;
using System.Windows;

namespace Reindeer_Hunter.Subsystems.ToolsCommands.Editor
{
    /// <summary>
    /// Interaction logic for MatchEditor.xaml
    /// </summary>
    public partial class MatchEditor : Window
    {
        public MatchEditor(School school, List<Match> matchesToEdit)
        {
            InitializeComponent();

            ((Editor)DataContext).Setup(school, matchesToEdit, this);
        }
    }
}
