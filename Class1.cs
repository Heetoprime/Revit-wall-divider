namespace ClassLibrary1
{
    using System;
    using Autodesk.Revit.UI;
    using System.Reflection;
    using System.Windows.Media.Imaging;

    public class Class1 : IExternalApplication
    {
        #region external application public methods

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "CLT creator";
            string panelAnnotationName = "CLT tab";

            application.CreateRibbonTab(tabName);

            var panelAnnotation = application.CreateRibbonPanel(tabName, panelAnnotationName);

            var TagWallLayersBtnData = new PushButtonData("TagWallLayersBtnData", "Tag Wall\nLayers", Assembly.GetExecutingAssembly().Location, "ClassLibrary1.CLTcreator")
            {
                ToolTipImage = new BitmapImage(new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2019\Dodge.png")),
                ToolTip = "This is some sample tooltip text, replace it later..."
            };

            var TagWallLayersBtn = panelAnnotation.AddItem(TagWallLayersBtnData) as PushButton;
            TagWallLayersBtn.LargeImage = new BitmapImage(new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2019\Cat 32x32.png"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        #endregion
    }
}