using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;


namespace NWN_Toolset
{
    public class ItemProperty : IEnumerable<KeyValuePair<string, int>>
    {
        public enum Property
        {
            CastSpell = 15,
            TrueSeeing = 71,
           
        }
        
        public int PropertyName { get; set; }
        public int SubType { get; set; }
        public int CostTable { get; set; }
        public int CostValue { get; set; }
        public int Param1 { get; set; }
        public int Param1Value { get; set; }
        public int ChanceAppear { get; set; }

        public ItemProperty(int propertyName, int subType, int costTable, int costValue, int param1, int param1Value, int chanceAppear)
        {
            PropertyName = propertyName;
            SubType = subType;
            CostTable = costTable;
            CostValue = costValue;
            Param1 = param1;
            Param1Value = param1Value;
            ChanceAppear = chanceAppear;
        }

        public static ItemProperty CreateSeeInvisibility()
        {
            return new ItemProperty(15, 243, 3, 10, 255, 0, 100); // cost value 10 = 3x/day, 12 = 5x day
        }



        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            // Yield return each property name and value as a KeyValuePair
            yield return new KeyValuePair<string, int>(nameof(PropertyName), PropertyName);
            yield return new KeyValuePair<string, int>(nameof(SubType), SubType);
            yield return new KeyValuePair<string, int>(nameof(CostTable), CostTable);
            yield return new KeyValuePair<string, int>(nameof(CostValue), CostValue);
            yield return new KeyValuePair<string, int>(nameof(Param1), Param1);
            yield return new KeyValuePair<string, int>(nameof(Param1Value), Param1Value);
            yield return new KeyValuePair<string, int>(nameof(ChanceAppear), ChanceAppear);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
