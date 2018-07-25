using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project.Mapping
{
    public enum MapMethod
    {
        Click,
        Frame,
        Input,
        IsElement,
        SelectElement,
        SqlQuery
    }

    public static class MapMethodTyper
    {
        public static
        string jsonConvert(this MapMethod me)
        {
            switch (me)
            {
                case MapMethod.Click:
                    return "click-element";
                case MapMethod.Frame:
                    return "frame";
                case MapMethod.Input:
                    return "input-element";
                case MapMethod.IsElement:
                    return "is-element";
                case MapMethod.SelectElement:
                    return "select-element";
                case MapMethod.SqlQuery:
                    return "sql-query";
                default:
                    return "null";
            }

        }
    }
}
