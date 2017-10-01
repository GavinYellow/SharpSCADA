using System.Windows.Input;

namespace CoreTest
{
    public static class MyCommands
    {
        public static RoutedCommand StartPowerOff = new RoutedCommand("StartPowerOff", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) });

        public static RoutedCommand Save = new RoutedCommand("Save", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) });

        public static RoutedCommand ComeBack = new RoutedCommand("ComeBack", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) });

        public static RoutedCommand Release = new RoutedCommand("Release", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) });

        public static RoutedCommand Previous = new RoutedCommand("Previous", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Alt) });

        public static RoutedCommand Next = new RoutedCommand("Next", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Alt) });

        public static RoutedCommand First = new RoutedCommand("First", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Alt) });

        public static RoutedCommand Last = new RoutedCommand("Last", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.L, ModifierKeys.Alt) });

        public static RoutedCommand MoveUp = new RoutedCommand("MoveUp", typeof(MyCommands),
          new InputGestureCollection { new KeyGesture(Key.U, ModifierKeys.Alt) });

        public static RoutedCommand MoveDown = new RoutedCommand("MoveDown", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.M, ModifierKeys.Alt) });

        public static RoutedCommand Select = new RoutedCommand("Select", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Alt) });

        public static RoutedCommand Query = new RoutedCommand("Query", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Alt) });

        public static RoutedCommand Search = new RoutedCommand("Search", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Alt) });

        public static RoutedCommand ExpandAll = new RoutedCommand("ExpandAll", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.Z, ModifierKeys.Alt) });

        public static RoutedCommand CollapseAll = new RoutedCommand("CollapseAll", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Alt) });

        public static RoutedCommand Add = new RoutedCommand("Add", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.A, ModifierKeys.Control) });

        public static RoutedCommand Delete = new RoutedCommand("Delete", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.Delete) });

        public static RoutedCommand Edit = new RoutedCommand("Edit", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) });

        public static RoutedCommand Clear = new RoutedCommand("Clear", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.X, ModifierKeys.Control) });

        public static RoutedCommand Commit = new RoutedCommand("Commit", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.C, ModifierKeys.Control) });

        public static RoutedCommand Insert = new RoutedCommand("Insert", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) });

        public static RoutedCommand InsertAt = new RoutedCommand("InsertAt", typeof(MyCommands),
            new InputGestureCollection { new KeyGesture(Key.Z, ModifierKeys.Control) });

        public static RoutedCommand Pause = new RoutedCommand("Pause", typeof(MyCommands),
          new InputGestureCollection { new KeyGesture(Key.P, ModifierKeys.Control) });

        public static RoutedCommand Resume = new RoutedCommand("Resume", typeof(MyCommands),
          new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) });

        public static RoutedCommand Stop = new RoutedCommand("Stop", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) });

        public static RoutedCommand Option = new RoutedCommand("Option", typeof(MyCommands),
           new InputGestureCollection { new KeyGesture(Key.F10) });
    }

}
