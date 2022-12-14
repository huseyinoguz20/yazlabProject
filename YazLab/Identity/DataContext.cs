using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using YazLab.Models;

namespace YazLab.Identity
{
    public class DataContext : DbContext
    {
        public DataContext() : base("dbConnection")
        {

        }

        public DbSet<BasvuruModel.Staj> Stajs { get; set; }
        public DbSet<BasvuruModel.Ime> Imes { get; set; }
    }
}