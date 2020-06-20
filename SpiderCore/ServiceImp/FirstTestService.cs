using Microsoft.EntityFrameworkCore;
using Snai.Mysql.DataAccess.Base;
using SpiderCore.ServiceInterFace;
using SpiderDataBase.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiderCore.ServiceImp
{
    public class FirstTestService : IFirstTestService
    {
        private DataAccess _dataAccess;
        public FirstTestService(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
        public async Task<User> GetUser(Guid id)
        {
            try
            {
                var li = await _dataAccess.User.ToListAsync();
                var result = await _dataAccess.User.Where(n=>n.Id==id.ToString()).FirstOrDefaultAsync();//await _dataAccess.User.Where(n => n.Id == id).FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ee)
            {

                return new User();
            }
        }
    }
}
