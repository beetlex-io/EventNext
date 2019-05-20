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
}
