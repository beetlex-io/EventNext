using System;
using System.Collections.Generic;
using System.Text;
using Peanut.Mappings;
namespace SqlietUserAmount
{
    [Table]
    public interface IUser
    {
        [ID]
        string Name { get; set; }
        [Column]
        long Amount { get; set; }
    }
}
