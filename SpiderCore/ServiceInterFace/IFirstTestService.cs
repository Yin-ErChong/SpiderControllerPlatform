using SpiderDataBase.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore.ServiceInterFace
{
    public interface IFirstTestService
    {
         Task<User> GetUser(Guid id);

    }
}
