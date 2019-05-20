using System;
using System.Collections.Generic;
using System.Text;
using Peanut.Mappings;

namespace SqlietUserAmount
{
    ///<summary>
    ///Peanut Generator Copyright @ henryfan 2018 email:henryfan@msn.com
    ///website:http://www.ikende.com
    ///</summary>
    [Table()]
    public partial class User : Peanut.Mappings.DataObject
    {
        private string mName;
        public static Peanut.FieldInfo<string> name = new Peanut.FieldInfo<string>("User", "Name");
        private long mAmount;
        public static Peanut.FieldInfo<long> amount = new Peanut.FieldInfo<long>("User", "Amount");
        ///<summary>
        ///Type:string
        ///</summary>
        [ID()]
        public virtual string Name
        {
            get
            {
                return mName;
                
            }
            set
            {
                mName = value;
                EntityState.FieldChange("Name");
                
            }
            
        }
        ///<summary>
        ///Type:long
        ///</summary>
        [Column()]
        public virtual long Amount
        {
            get
            {
                return mAmount;
                
            }
            set
            {
                mAmount = value;
                EntityState.FieldChange("Amount");
                
            }
            
        }
        
    }
    
}
