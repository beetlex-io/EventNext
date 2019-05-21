using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SqlietUserAmount
{
    public class SqliteDriver : Peanut.DriverTemplate<
SQLiteConnection,
SQLiteCommand,
SQLiteDataAdapter,
SQLiteParameter,
Peanut.SqlitBuilder>
    {
    }

    public class DateTimeConvter : Peanut.Mappings.PropertyCastAttribute
    {
        public override object ToColumn(object value, Type ptype, object source)
        {
            return ((DateTime)value).Ticks;
        }
        public override object ToProperty(object value, Type ptype, object source)
        {
            return new DateTime((long)value);
        }
    }
}
