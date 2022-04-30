using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateRoomsPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateRooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;  //получаем доступ к документу
            List<Level> listLevel = new FilteredElementCollector(doc) //создаем список уровней
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            List<RoomTagType> m_roomTagTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .Cast<RoomTagType>()
                .ToList<RoomTagType>(); //создаем список марок помещений

            PlanTopology planTopology;
            Room room ;
            int roomNumber;
            int levelNumber = 0;

            Transaction transaction = new Transaction(doc, "Создать помещение");
            transaction.Start();
            foreach (Level level in listLevel)
            {
                planTopology = doc.get_PlanTopology(level); //получаем топологию плана для заданного уровня
                levelNumber ++;
                roomNumber = 0;
                foreach (PlanCircuit circuit in planTopology.Circuits)
                {
                    room = doc.Create.NewRoom(level, circuit.GetPointInside());
                                       
                    roomNumber++;
                    room.Number = $"{levelNumber}-{roomNumber}";   //присваиваем марке номер типа "номер этажа-номер помещения"
                   
                    LocationPoint locationPoint = room.Location as LocationPoint;  
                    UV point = new UV(locationPoint.Point.X, locationPoint.Point.Y);
                    RoomTag newTag = doc.Create.NewRoomTag(new LinkElementId(room.Id), point, null);
                }
            }
            transaction.Commit();
            return Result.Succeeded;
        }
    }
}
