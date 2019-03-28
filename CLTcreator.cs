//Only walls drawn after this addin is loaded 

namespace ClassLibrary1
{
    using System;
    using System.Collections.Generic;
    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public class CLTcreator : IExternalCommand
    {
        #region public methods

        class WallSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                return e is Wall;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return true;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (null == commandData)
                {
                    throw new ArgumentNullException("commandData");
                }

                UIApplication uiapp = commandData.Application;
                Application app = uiapp.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                Reference r = null;

                try
                {
                    r = uidoc.Selection.PickObject(
                      ObjectType.Element,
                      new WallSelectionFilter(),
                      "Select a wall to split into panels");
                }
                catch (Autodesk.Revit.Exceptions
                  .OperationCanceledException)
                {
                    return Result.Cancelled;
                }

                Wall wall = (r == null || r.ElementId
                    == ElementId.InvalidElementId)
                  ? null
                  : doc.GetElement(r.ElementId) as Wall;

                if (wall == null)
                {
                    message = "Unable to retrieve wall.";
                    return Result.Failed;
                }

                LocationCurve location
                  = wall.Location as LocationCurve;

                if (null == location)
                {
                    message = "Unable to retrieve wall location curve.";
                    return Result.Failed;
                }

                Line line = location.Curve as Line;

                if (null == location)
                {
                    message = "Unable to retrieve wall location line.";
                    return Result.Failed;
                }

                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Building panels");

                    IList<ElementId> wallList = new List<ElementId>(1);

                    wallList.Add(r.ElementId);

                    if (PartUtils.AreElementsValidForCreateParts(
                      doc, wallList))
                    {
                        PartUtils.CreateParts(doc, wallList);

                        doc.Regenerate();

                        ICollection<ElementId> parts
                          = PartUtils.GetAssociatedParts(
                            doc, wall.Id, false, false);

                        if (PartUtils.ArePartsValidForDivide(
                          doc, parts))
                        {
                            int divisions = 5;

                            XYZ origin = line.Origin;

                            XYZ delta = line.Direction.Multiply(
                              line.Length / divisions);

                            Transform shiftDelta
                              = Transform.CreateTranslation(delta);

                            // Construct a 90 degree rotation in the 
                            // XY plane around the line start point

                            Transform rotation = Transform.CreateRotationAtPoint(
                              XYZ.BasisZ, 0.5 * Math.PI, origin);

                            // A vector perpendicular to the wall with
                            // length equal to twice the wall witdh

                            XYZ wallWidthVector = rotation.OfVector(
                              line.Direction.Multiply(2 * wall.Width));

                            Curve intersectionLine
                              = Line.CreateBound( // Line.CreateBound
                                origin + wallWidthVector,
                                origin - wallWidthVector);

                            IList<Curve> curveArray = new List<Curve>();

                            /*
                            //Jeremy's Modelline Code
                            XYZ v = 2*wallWidthVector;
                            double dxy = Math.Abs(v.X) + Math.Abs(v.Y);
                            XYZ w = (dxy > 1.0e-9)
                              ? XYZ.BasisZ
                              : XYZ.BasisY;
                            XYZ norm = v.CrossProduct(w).Normalize();
                            Plane plane = Plane.CreateByNormalAndOrigin(norm, origin + wallWidthVector);
                            SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                            //Jeremy's code end
                            */

                            for (int i = 1; i < divisions; ++i)
                            {
                                intersectionLine = intersectionLine.CreateTransformed(shiftDelta);

                                //ModelCurve curve = doc.IsFamilyDocument ? doc.FamilyCreate.NewModelCurve(intersectionLine, sketchPlane) : doc.Create.NewModelCurve(intersectionLine, sketchPlane);

                                curveArray.Add(intersectionLine);
                            }
                            
                            SketchPlane divisionSketchPlane
                              = SketchPlane.Create(doc,
                                Plane.CreateByThreePoints(
                                  line.Origin,
                                  XYZ.BasisX,
                                  XYZ.BasisY));
                                  
                            // An empty list of intersecting ElementIds

                            IList<ElementId> intersectionElementIds
                              = new List<ElementId>();

                            PartUtils.DivideParts(doc,parts,intersectionElementIds,curveArray,divisionSketchPlane.Id);

                            /*
                            var wnd = new TaskDialog("info")
                            {
                                MainContent = curveArray[0]
                            };
                            wnd.Show();
                            */
                        }
                        doc.ActiveView.PartsVisibility
                          = PartsVisibility.ShowPartsOnly;
                    }
                    transaction.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        #endregion
    }
}