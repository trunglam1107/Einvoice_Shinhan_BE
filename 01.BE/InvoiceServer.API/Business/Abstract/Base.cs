using IFRSCaseSearchDBServer.Business.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IFRSCaseSearchDBServer.API.Business.Abstract
{
    public abstract class Base<T> where T : class
    {
        public IBOFactory factory;
        public abstract List<T> GetAll();
        public abstract T GetByID(int Id);

        public virtual bool Update(T t) {
            return true;
        }

        public virtual bool Insert(T t)
        {

            return true;
        }

        public virtual bool Delete(T t)
        {
            
            return true;
        }
    }
}