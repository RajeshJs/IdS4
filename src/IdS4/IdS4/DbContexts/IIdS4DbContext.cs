﻿using IdS4.Logs;
using Microsoft.EntityFrameworkCore;

namespace IdS4.DbContexts
{
    public interface IIdS4DbContext
    {
        DbSet<Log> Logs { get; } 
    }
}
