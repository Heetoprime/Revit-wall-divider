namespace RevitClass1
{
    using System;
    using Autodesk.Revit.UI;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using System.IO;

    public class Base : IExternalApplication
    {
        #region external application public methods

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "CLT creator";
            string panelAnnotationName = "CLT tab";

            application.CreateRibbonTab(tabName);

            var panelAnnotation = application.CreateRibbonPanel(tabName, panelAnnotationName);

            var WallDivBtnData = new PushButtonData("WallDivBtnData", "CLT\nButton", Assembly.GetExecutingAssembly().Location, "RevitClass1.CLT1WallVariable")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ProPreview.png", UriKind.Absolute)),
                ToolTip = "This divides a wall according to your input."
            };

            var WallDivBtn = panelAnnotation.AddItem(WallDivBtnData) as PushButton;
            WallDivBtn.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ProIcon.png", UriKind.Absolute));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        #endregion
    }
}