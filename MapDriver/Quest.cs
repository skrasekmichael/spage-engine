using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    [Serializable]
    public class Quest
    {
        public bool Done { get; private set; } = false;

        private enum QuestType : int
        {
            KillAllUnits, KillUnit, SerchMap, SaveUnit, TransportWagons
        }

        private QuestType type { get; set; }
        private string specific { get; set; }

        private Unit specific_unit;
        private Point position;

        public static Quest KillAllUnits() => new Quest()
        {
            type = QuestType.KillAllUnits,
            specific = ""
        };

        public static Quest KillUnit(Unit specific_unit, string specific = "") => new Quest()
        {
            type = QuestType.KillUnit,
            specific = specific,
            specific_unit = specific_unit
        };

        public static Quest SaveUnit(Unit specific_unit, string specific = "") => new Quest()
        {
            type = QuestType.SaveUnit,
            specific = specific,
            specific_unit = specific_unit
        };

        public static Quest TransportWagons(Unit specific_unit, string specific = "") => new Quest()
        {
            type = QuestType.TransportWagons,
            specific = specific,
            specific_unit = specific_unit
        };

        public static Quest SerchMap(Point pos, string specific = "") => new Quest()
        {
            type = QuestType.SerchMap, 
            specific = specific, 
            position = pos
        };

        public string Name => type.ToString().ToLower() + specific;

        public bool Check(Map map, Engine engine)
        {
            switch (type)
            {
                case QuestType.KillAllUnits:
                    if (map.Units.Values.ToList().Where(u => u.Player != 1).ToList().Count == 0)
                        Done = true;
                    break;
                case QuestType.KillUnit:
                    if (map.Units.Values.ToList().Where(u => u == specific_unit).ToList().Count == 0)
                        Done = true;
                    break;
                case QuestType.SerchMap:
                    if (engine.GetVisibility(position.X, position.Y) != Visibility.Gone)
                        Done = true;
                    break;
            }

            return Done;
        }
    }
}
