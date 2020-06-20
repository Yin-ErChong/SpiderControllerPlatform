using Microsoft.EntityFrameworkCore;
using SpiderDataBase.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snai.Mysql.DataAccess.Base
{
    public class DataAccess : DbContext
    {
        public DataAccess(DbContextOptions<DataAccess> options)
            : base(options)
        { }

        public DbSet<User> User { get; set; }
    }
}
