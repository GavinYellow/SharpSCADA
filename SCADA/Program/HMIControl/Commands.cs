using System.Windows.Input;

namespace HMIControl
{
    public static class DeviceCommand
    {
        #region  手动更换仓指令
        public static RoutedCommand ManualChangeTarget = new RoutedCommand("ManualChangeTarget", typeof(DeviceCommand));
        public static RoutedCommand ManualChangeOrigin = new RoutedCommand("ManualChangeOrigin", typeof(DeviceCommand));
        public static RoutedCommand ManualChangeStore  = new RoutedCommand("ManualChangeStore", typeof(DeviceCommand));

        #endregion


        public static RoutedCommand Abort = new RoutedCommand("Abort", typeof(DeviceCommand));

        public static RoutedCommand Start = new RoutedCommand("Start", typeof(DeviceCommand));

        public static RoutedCommand Pause = new RoutedCommand("Pause", typeof(DeviceCommand));

        public static RoutedCommand Resume = new RoutedCommand("Resume", typeof(DeviceCommand));
        public static RoutedCommand Stop = new RoutedCommand("Stop", typeof(DeviceCommand));

        public static RoutedCommand Hold = new RoutedCommand("Hold", typeof(DeviceCommand));

        public static RoutedCommand PermitTare = new RoutedCommand("PermitTare", typeof(DeviceCommand));

        public static RoutedCommand ChangeOrigin = new RoutedCommand("ChangeOrigin", typeof(DeviceCommand));

        public static RoutedCommand ChangeTarget = new RoutedCommand("ChangeTarget", typeof(DeviceCommand));

        public static RoutedCommand StartProduct = new RoutedCommand("StartProduct", typeof(DeviceCommand));
        public static RoutedCommand StopProduct = new RoutedCommand("StopProduct", typeof(DeviceCommand));


        public static RoutedCommand Properties = new RoutedCommand("Properties", typeof(DeviceCommand));

        public static RoutedCommand IgnoreError = new RoutedCommand("IgnoreError", typeof(DeviceCommand));

    }//#FF646662

}
