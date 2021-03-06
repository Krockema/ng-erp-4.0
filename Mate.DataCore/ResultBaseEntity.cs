﻿using System.ComponentModel.DataAnnotations;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;

namespace Mate.DataCore
{
    public class ResultBaseEntity : IBaseEntityDbGeneratedId, IId
    {
        [Key]
        public int Id { get; set; }

        protected ResultBaseEntity()
        {
            
        }
        
        public Id GetId()
        {
            return new Id(Id);
        }

        public override bool Equals(object obj)
        {
            ResultBaseEntity other = (ResultBaseEntity)obj;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            string fullName = GetType().FullName; 
            return $"{Id}: {fullName}";
        }
    }

}